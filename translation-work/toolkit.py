#!/usr/bin/env python3
"""Offline manual-localization workflow. All writes confined to this repository."""
import argparse
import contextlib
import hashlib
import json
import os
import re
from pathlib import Path, PurePosixPath
import shutil
import sys
import time
import uuid
import zipfile

sys.dont_write_bytecode = True

from native import PATHS, CATALOG, records, rebuild, require, validate_translation, build_outputs, output_paths

ROOT = Path(__file__).resolve().parent.parent
WORK = 'translation-work/'
TRANSLATIONS = WORK + 'translations'
SNAPSHOT = WORK + 'accepted-sources'
REPORTS = WORK + 'reports'
BUILD = WORK + 'build-manifest.json'
STATE = WORK + '.state'
SCHEMA = 1
TX_RETENTION_COUNT = 10
TX_RETENTION_BYTES = 512 * 1024 * 1024


def blob(value):
    return (json.dumps(value, ensure_ascii=False, sort_keys=True, indent=2) + '\n').encode('utf8')


def sha(data):
    return hashlib.sha256(data).hexdigest()


def fingerprint(value):
    return sha(blob(value))


def read_json(path):
    return json.loads(path.read_text(encoding='utf-8'))


def safe(root, rel):
    p = PurePosixPath(rel)
    require(rel and not p.is_absolute() and '\\' not in rel and ':' not in rel
            and all(x not in ('', '.', '..') for x in rel.split('/')), f'Unsafe path: {rel}')
    root = root.absolute()
    require(not root.is_symlink() and not (hasattr(root, 'is_junction') and root.is_junction()), f'Linked root: {root}')
    target = root / rel
    require(target.resolve().is_relative_to(root.resolve()), f'Escaping path: {target}')
    for part in [target, *target.parents]:
        if part == root.parent:
            break
        require(not part.is_symlink() and not (hasattr(part, 'is_junction') and part.is_junction()),
                f'Linked path: {part}')
    return target


def inventory(root):
    if not root.exists():
        return {}
    require(root.is_dir() and not root.is_symlink(), f'Invalid root: {root}')
    found, folded = {}, set()
    for parent, dirs, files in os.walk(root, followlinks=False):
        for name in dirs + files:
            p = Path(parent) / name
            rel = p.relative_to(root).as_posix()
            safe(root, rel)
            require(rel.casefold() not in folded, f'Case collision: {rel}')
            folded.add(rel.casefold())
        for name in files:
            p = Path(parent) / name
            require(p.is_file(), f'Not a regular file: {p}')
            found[p.relative_to(root).as_posix()] = sha(p.read_bytes())
    return dict(sorted(found.items()))


def read_sources(root):
    inv = inventory(root)
    require(set(PATHS) <= set(inv), 'Incomplete original source tree; required resources missing')
    require(all(re.fullmatch(r'Thea2_Data/StreamingAssets/(?:Modules/[^/]+\.xml|DataBase/DATABASE_[^/]+(?:LOCALIZATION|NAMES)\.xml)', name) for name in inv), 'Unknown source resource path')
    return {p: safe(root, p).read_bytes() for p in sorted(inv)}


def source_records(sources):
    result = []
    for path, data in sources.items():
        result.extend(records(path, data))
    require(len({r['id'] for r in result}) == len(result), 'Duplicate native identity')
    for r in result:
        r['source_fingerprint'] = fingerprint({k: v for k, v in r.items() if k not in ('PTBR', 'row')})
    return result


def source_inventory(sources):
    return {p: sha(data) for p, data in sorted(sources.items())}


def durable(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('wb') as f:
        f.write(data)
        f.flush()
        os.fsync(f.fileno())


def atomic_json(path, obj):
    tmp = path.with_suffix('.tmp')
    durable(tmp, blob(obj))
    os.replace(tmp, path)


class Project:
    def __init__(self, root=ROOT):
        self.root = Path(root).resolve()

    def path(self, rel):
        return safe(self.root, rel)

    def state_inventory(self):
        result = {}
        for rel in ('EN', TRANSLATIONS, SNAPSHOT, REPORTS, 'PTBR'):
            result[rel] = inventory(self.path(rel))
        p = self.path(BUILD)
        result[BUILD] = sha(p.read_bytes()) if p.exists() else None
        return result

    def interrupted(self):
        p = self.path(STATE + '/journal.json')
        return p.exists()

    @contextlib.contextmanager
    def lock(self):
        p = self.path(STATE + '/operation.lock')
        p.parent.mkdir(parents=True, exist_ok=True)
        f = p.open('a+b')
        try:
            if os.name == 'nt':
                import msvcrt
                f.seek(0)
                if f.read(1) == b'':
                    f.write(b'0')
                    f.flush()
                f.seek(0)
                msvcrt.locking(f.fileno(), msvcrt.LK_NBLCK, 1)
            else:
                import fcntl
                fcntl.flock(f, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except OSError:
            f.close()
            raise ValueError('Another toolkit operation holds the lock')
        try:
            yield
        finally:
            f.close()

    def recover(self):
        journal = self.path(STATE + '/journal.json')
        if not journal.exists():
            return {'status': 'no-interrupted-transaction'}
        j = read_json(journal)
        base = self.path(STATE + '/transactions/' + j['transaction'])
        # Inspect every affected live tree before recovery, never overwrite edits.
        for rel in j['targets']:
            target = self.path(rel)
            current = self.tree_digest(target)
            require(current in (j['before'][rel], j['after'][rel], None),
                    f'Recovery conflict: {rel}; preserve edits before recovery')
        for rel in j['targets']:
            target, old = self.path(rel), safe(base / 'old', rel)
            if old.exists():
                require(self.tree_digest(old) == j['before'][rel], f'Damaged recovery backup: {rel}')
                if target.exists():
                    saved = safe(base / 'aborted', rel)
                    saved.parent.mkdir(parents=True, exist_ok=True)
                    if saved.exists():
                        require(self.tree_digest(target) == j['before'][rel], 'Recovery conflict')
                        continue
                    os.replace(target, saved)
                target.parent.mkdir(parents=True, exist_ok=True)
                os.replace(old, target)
            elif j['before'][rel] is None and target.exists():
                saved = safe(base / 'aborted', rel)
                saved.parent.mkdir(parents=True, exist_ok=True)
                os.replace(target, saved)
        journal.unlink()
        return {'status': 'rolled-back-interrupted-transaction', 'snapshot': str(base)}

    @staticmethod
    def tree_digest(path):
        if not path.exists():
            return None
        return fingerprint(inventory(path)) if path.is_dir() else sha(path.read_bytes())

    def transaction(self, files, targets, expected, fail_after=None):
        require(not self.interrupted(), 'Interrupted transaction; run recover')
        # File-level copy-on-write: unchanged resources/batches keep bytes and mtimes.
        targets = [t for t in targets if (self.path(t).is_dir() or any(name.startswith(t + '/') for name in files) or
                   (sha(files[t]) if files.get(t) is not None else None) != self.tree_digest(self.path(t)))]
        if not targets:
            return 'unchanged'
        files = {name: data for name, data in files.items() if any(name == t or name.startswith(t + '/') for t in targets)}
        need = sum(len(data) for data in files.values() if data is not None)
        old_size = sum(self.path(t).stat().st_size if self.path(t).is_file() else
                       sum(self.path(t + '/' + f).stat().st_size for f in inventory(self.path(t))) for t in targets)
        require(need + old_size < TX_RETENTION_BYTES, 'Transaction exceeds 512 MiB budget; split operation')
        usage = self.gc(dry_run=True)
        require(usage.get('protected_bytes', 0) + need + old_size < TX_RETENTION_BYTES,
                'Protected recovery data plus operation exceeds 512 MiB; inspect recovery storage before writing')
        require(shutil.disk_usage(self.root).free > need + old_size + 16 * 1024 * 1024, 'Insufficient free space for durable staging')
        tx = uuid.uuid4().hex
        base = self.path(STATE + '/transactions/' + tx)
        stage = base / 'new'
        for name, data in files.items():
            require(any(name == t or name.startswith(t + '/') for t in targets), 'Unowned transaction file')
            if data is not None:
                durable(safe(stage, name), data)
        # Empty target directories (e.g. zero records) are supported explicitly.
        # A missing staged path means a journaled deletion, never an empty directory.
        require(self.state_inventory() == expected, 'Concurrent edits detected; nothing was promoted')
        j = {'transaction': tx, 'targets': targets,
             'before': {t: self.tree_digest(self.path(t)) for t in targets},
             'after': {t: self.tree_digest(safe(stage, t)) for t in targets}}
        atomic_json(self.path(STATE + '/journal.json'), j)
        # Deliberately leave the journal on failure: recovery rolls back all roots.
        for index, rel in enumerate(targets):
            target, old = self.path(rel), safe(base / 'old', rel)
            if target.exists():
                old.parent.mkdir(parents=True, exist_ok=True)
                os.replace(target, old)
            target.parent.mkdir(parents=True, exist_ok=True)
            if safe(stage, rel).exists():
                os.replace(safe(stage, rel), target)
            if fail_after == index:
                raise RuntimeError('Injected interrupted promotion')
        for rel in targets:
            require(self.tree_digest(self.path(rel)) == j['after'][rel], 'Promotion mismatch; run recover')
        atomic_json(base / 'receipt.json', j)
        self.path(STATE + '/journal.json').unlink()
        # Cleanup happens only after the transaction has completed durably.
        # A cleanup failure cannot roll back or misreport a successful write.
        try:
            self.gc()
        except (OSError, ValueError) as error:
            print(f'Warning: transaction {tx} committed; retention cleanup deferred: {error}', file=sys.stderr)
        return tx


    def gc(self, dry_run=False, keep_count=TX_RETENTION_COUNT, max_bytes=TX_RETENTION_BYTES):
        """Bound completed rollback history without touching pending recovery."""
        require(not self.interrupted(), 'Cannot prune an interrupted transaction; recover first')
        require(keep_count >= 1 and max_bytes > 0, 'Invalid retention limits')
        root = self.path(STATE + '/transactions')
        if not root.exists():
            return dict(status='dry-run' if dry_run else 'clean', kept=0,
                        removed=0, reclaimed_bytes=0, protected=0, total_bytes=0)
        require(root.is_dir() and not root.is_symlink(), 'Unsafe transaction storage')
        completed, protected, protected_bytes = [], [], 0
        for item in root.iterdir():
            require(re.fullmatch(r'[0-9a-f]{32}', item.name) and item.is_dir()
                    and not item.is_symlink(), 'Unexpected transaction entry')
            receipt = item / 'receipt.json'
            if not receipt.exists():
                # Staging from a failed pre-journal operation is never a
                # completed snapshot. Do not automatically destroy it.
                protected.append(item.name)
                protected_bytes += sum(safe(item, name).stat().st_size for name in inventory(item))
                continue
            require(receipt.is_file() and not receipt.is_symlink(), 'Unsafe transaction receipt')
            info = read_json(receipt)
            require(isinstance(info, dict)
                    and set(info) == {'transaction', 'targets', 'before', 'after'}
                    and info['transaction'] == item.name
                    and isinstance(info['targets'], list) and bool(info['targets'])
                    and len(info['targets']) == len(set(info['targets']))
                    and isinstance(info['before'], dict) and isinstance(info['after'], dict)
                    and set(info['before']) == set(info['after']) == set(info['targets']),
                    f'Invalid transaction receipt: {item.name}')
            for rel in info['targets']:
                require(self.tree_digest(safe(item / 'old', rel)) == info['before'][rel],
                        f'Damaged snapshot backup: {item.name}/{rel}')
            # First validate ALL entries before removing ANY snapshots.
            size = 0
            for parent, dirs, names in os.walk(item, followlinks=False):
                for name in dirs + names:
                    entry = Path(parent) / name
                    require(not entry.is_symlink() and not
                            (hasattr(entry, 'is_junction') and entry.is_junction()),
                            f'Unsafe snapshot path: {entry}')
                for name in names:
                    entry = Path(parent) / name
                    require(entry.is_file(), f'Unsafe snapshot file: {entry}')
                    size += entry.stat().st_size
            completed.append((receipt.stat().st_mtime_ns, item.name, item, size))
        completed.sort(reverse=True)
        kept, removable, used = [], [], 0
        for record in completed:
            if len(kept) < keep_count and used + record[3] <= max_bytes:
                kept.append(record)
                used += record[3]
            else:
                removable.append(record)
        # Keep the latest completed rollback even if it exceeds the budget.
        require(not completed or (kept and kept[0] == completed[0]),
                'Newest transaction exceeds snapshot byte budget; adjust retention')
        reclaimed = sum(row[3] for row in removable)
        if not dry_run:
            # CLI holds the operation lock; transaction() calls this before
            # releasing that same lock. Never auto-delete unreceipted staging.
            for _, name, item, _ in removable:
                require(not self.interrupted(), 'Interrupted transaction journal appeared')
                require(item.is_dir() and not item.is_symlink()
                        and (item / 'receipt.json').is_file(), 'Snapshot changed during prune')
                shutil.rmtree(item)
        return dict(status='dry-run' if dry_run else 'clean',
                    kept=len(kept), removed=len(removable), reclaimed_bytes=reclaimed,
                    total_bytes=sum(row[3] for row in completed),
                    kept_bytes=used, protected=len(protected), protected_bytes=protected_bytes,
                    eligible=[dict(transaction=row[1], bytes=row[3], status='completed',
                                   age_seconds=max(0, int(time.time() - row[0] / 1e9))) for row in removable],
                    protected_ids=protected,
                    retention_count=keep_count, retention_bytes=max_bytes)

    def load(self, allow_en_changed=False):
        require(not self.interrupted(), 'Interrupted transaction: run toolkit.py recover before reading state')
        manifest = read_json(self.path(TRANSLATIONS + '/manifest.json'))
        require(manifest['schema'] == SCHEMA, 'Unsupported manifest schema')
        sources = read_sources(self.path(SNAPSHOT))
        require(source_inventory(sources) == manifest['sources'], 'Accepted snapshot fingerprints mismatch')
        require(fingerprint(manifest['sources']) == manifest['generation'], 'Invalid generation')
        if not allow_en_changed:
            require(inventory(self.path('EN')) == manifest['sources'], 'EN differs from accepted generation: run sync')
        canonical = {r['id']: r for r in source_records(sources)}
        batches, seen = {}, set()
        expected_names = set(manifest['batches']) | {'manifest.json'}
        require(set(inventory(self.path(TRANSLATIONS))) == expected_names, 'Unexpected/missing batch files')
        for name in manifest['batches']:
            require(name.startswith('batch-') and name.endswith('.jsonl') and '/' not in name, 'Unsafe batch name')
            rows = [json.loads(line) for line in self.path(TRANSLATIONS + '/' + name).read_text(encoding='utf8').splitlines()]
            require(len(rows) <= 500, 'Batch exceeds 500 entries')
            for r in rows:
                rid = r['id']
                require(rid in canonical and rid not in seen, f'Unknown/duplicate ID: {rid}')
                seen.add(rid)
                require({k: v for k, v in r.items() if k not in ('PTBR', 'history')}
                        == {k: v for k, v in canonical[rid].items() if k != 'PTBR'},
                        f'Immutable source fields edited: {rid}')
                require(isinstance(r.get('history', []), list), 'Invalid history')
                validate_translation(r)
            batches[name] = rows
        require(seen == set(canonical) and len(seen) == manifest['records'], 'Missing/extra records')
        return manifest, sources, batches

    def generation_fingerprint(self):
        # Same bytes as Git's text eol=lf filters, including CRLF-authored edits.
        return fingerprint({name: sha(self.path(TRANSLATIONS + '/' + name).read_bytes().replace(b'\r\n', b'\n'))
                            for name in inventory(self.path(TRANSLATIONS))})

    def sync(self, sources=None, initial=False, dry_run=False):
        expected = self.state_inventory()
        exists = self.path(TRANSLATIONS + '/manifest.json').exists()
        require(not initial or not exists, 'Extraction refuses to overwrite existing progress; use sync')
        require(initial or exists, 'No accepted generation; run extract first')
        oldm, old_sources, old_batches = ({}, {}, {}) if initial else self.load(allow_en_changed=True)
        sources = sources if sources is not None else read_sources(self.path('EN'))
        require(set(PATHS) <= set(sources), 'Incomplete source import')
        require(set(oldm.get('sources', {})) <= set(sources), 'Previously accepted source file missing; explicit catalog migration required')
        canonical = source_records(sources)
        sources_inv = source_inventory(sources)
        generation = fingerprint(sources_inv)
        policy = self.path(WORK + 'source-policy.json')
        if policy.exists():
            require(generation not in read_json(policy).get('rejected', []), 'Known translated source; supply original English')
        old_records = {r['id']: (name, r) for name, rows in old_batches.items() for r in rows}
        new_ids = {r['id'] for r in canonical}
        batches = {name: [] for name in old_batches}
        changes = []
        counts = dict(added=0, modified=0, removed=0, moved=0, unchanged=0, preserved_translations=0)
        new = []
        for record in canonical:
            prior = old_records.get(record['id'])
            if prior is None:
                new.append(record)
                counts['added'] += 1
                changes.append(dict(kind='added', id=record['id'], source_file=record['source_file'],
                                    previous_EN=None, EN=record['EN'], previous_PTBR=''))
                continue
            batch, old = prior
            kind = 'unchanged'
            record['history'] = old.get('history', [])
            if record['source_fingerprint'] == old['source_fingerprint']:
                record['PTBR'] = old['PTBR']
                counts['preserved_translations'] += bool(old['PTBR'])
                if record['row'] != old['row']:
                    kind = 'moved'
            else:
                kind = 'modified'
                record['history'] = record['history'] + [dict(generation=oldm['generation'],
                                                            EN=old['EN'], PTBR=old['PTBR'])]
            counts[kind] += 1
            if kind != 'unchanged':
                changes.append(dict(kind=kind, id=record['id'], source_file=record['source_file'],
                                    previous_EN=old['EN'], EN=record['EN'], previous_PTBR=old['PTBR'], batch=batch))
            batches[batch].append(record)
        for rid, (batch, old) in old_records.items():
            if rid not in new_ids:
                counts['removed'] += 1
                changes.append(dict(kind='removed', id=rid, source_file=old['source_file'],
                                    previous_EN=old['EN'], EN=None, previous_PTBR=old['PTBR'], batch=batch,
                                    archived_record=old))
        # Preserve physical batch order, even if native rows have moved.
        for name, rows in batches.items():
            order = {r['id']: i for i, r in enumerate(old_batches[name])}
            rows.sort(key=lambda r: order[r['id']])
        for record in new:
            last = next(reversed(batches), None)
            if last is None or len(batches[last]) >= 500:
                last = f'batch-{len(batches) + 1:04d}.jsonl'
                batches[last] = []
            batches[last].append(record)
        manifest = dict(schema=SCHEMA, game='Thea 2: The Shattering', native='XML database + STORY_MODULE XML overrides',
                        sources=sources_inv, generation=generation, records=len(canonical), batch_size=500,
                        batches=list(batches), deployment='EN/Thea2_Data/StreamingAssets/{DataBase,Modules} -> PTBR/Thea2_Data/StreamingAssets/Mods/PTBR',
                        profile='single-English-baseline', provenance='Explicit verified original resource catalog')
        manifest['source_version'] = getattr(self, '_candidate_version',
            oldm.get('source_version', CATALOG['game_version']) if initial or oldm.get('generation') == generation
            else 'unconfirmed: imported XML generation ' + generation)
        assignments = {r['id']: name for name, rows in batches.items() for r in rows}
        for c in changes:
            c.setdefault('batch', assignments.get(c['id']))
        source_changes = {p: dict(before=oldm.get('sources', {}).get(p), after=h)
                          for p, h in sources_inv.items() if oldm.get('sources', {}).get(p) != h}
        report = dict(status='proposed' if dry_run else 'accepted', previous_generation=oldm.get('generation'),
                      previous_version=oldm.get('source_version'), source_version=manifest['source_version'],
                      generation=generation, counts=counts, source_changes=source_changes, changes=changes,
                      affected_batches=sorted({c['batch'] for c in changes}),
                      existing_pending=sum(not r['PTBR'] for _, r in old_records.values()),
                      pending=sum(not r['PTBR'] for rows in batches.values() for r in rows),
                      warnings=['Original duplicate keys retain occurrence identity; multiline choices use native STORY_MODULE XML overrides.'],
                      next_action='Translator: read briefing, review diff, translate pending entries.')
        if oldm.get('sources') == sources_inv and oldm.get('source_version') == manifest['source_version']:
            if expected['EN'] == sources_inv:
                return dict(status='unchanged', generation=generation, counts=counts)
        if dry_run:
            return report
        files = {}
        for path, data in sources.items():
            files['EN/' + path] = data
            files[SNAPSHOT + '/' + path] = data
        files[TRANSLATIONS + '/manifest.json'] = blob(manifest)
        for name, rows in batches.items():
            files[TRANSLATIONS + '/' + name] = b''.join(
                (json.dumps(r, ensure_ascii=False, sort_keys=True) + '\n').encode('utf8') for r in rows)
        # Keep removed records and diffs versioned beyond the latest sync.
        for path in inventory(self.path(REPORTS)):
            files[REPORTS + '/' + path] = self.path(REPORTS + '/' + path).read_bytes()
        report['recovery'] = 'translation-work/.state/transactions (local); accepted-sources and history are versioned'
        files[REPORTS + '/sync-latest.json'] = blob(report)
        files[REPORTS + '/history/' + generation + '.json'] = blob(report)
        files[REPORTS + '/sync-latest.md'] = (
            '# Sincronização aceita\n\nGeração: `' + generation + '`\n\n' +
            '\n'.join(f'- {k}: {v}' for k, v in counts.items()) +
            '\n\nDetalhes e IDs: sync-latest.json. A pessoa responsável pela tradução deve revisar os registros pendentes.\n').encode('utf8')
        # Validate staged catalog and all translations before publishing anything.
        require(len(assignments) == len(canonical), 'Staged membership mismatch')
        for rows in batches.values():
            for r in rows:
                validate_translation(r)
        targets = sorted(set(files) | {prefix + '/' + name for prefix in ('EN', SNAPSHOT, TRANSLATIONS, REPORTS) for name in expected[prefix]})
        files.update({name: None for name in targets if name not in files})
        report['transaction'] = self.transaction(files, targets, expected)
        return report

    def apply(self, payload, replace=False):
        expected = self.state_inventory()
        manifest, _, batches = self.load()
        require(set(payload) == {'generation', 'entries'}, 'Payload requires only generation and entries')
        require(payload['generation'] == manifest['generation'], 'Wrong source generation')
        require(isinstance(payload['entries'], list), 'entries must be an array')
        by_id = {r['id']: r for rows in batches.values() for r in rows}
        batch_by_id = {r['id']: name for name, rows in batches.items() for r in rows}
        seen, accepted, rejected, dirty_batches = set(), [], [], set()
        # Identity errors invalidate the complete payload before accepting partial text updates.
        for item in payload['entries']:
            require(set(item) == {'id', 'source_fingerprint', 'PTBR'}, 'Only ID, fingerprint and PTBR may be supplied')
            rid = item['id']
            require(rid in by_id and rid not in seen, f'Duplicate/unknown ID: {rid}')
            require(item['source_fingerprint'] == by_id[rid]['source_fingerprint'], f'Stale source: {rid}')
            seen.add(rid)
        for item in payload['entries']:
            old = by_id[item['id']]
            try:
                require(isinstance(item['PTBR'], str) and item['PTBR'] != '', 'PTBR cannot be empty')
                require(not old['PTBR'] or old['PTBR'] == item['PTBR'] or replace, 'Existing PTBR: use --replace after review')
                validate_translation(dict(old, PTBR=item['PTBR']))
                if old['PTBR'] != item['PTBR']:
                    if old['PTBR']:
                        old.setdefault('history', []).append(dict(generation=manifest['generation'], EN=old['EN'], PTBR=old['PTBR']))
                    old['PTBR'] = item['PTBR']
                    accepted.append(item['id'])
                    dirty_batches.add(batch_by_id[item['id']])
            except (ValueError, UnicodeError) as error:
                rejected.append({'id': item['id'], 'reason': str(error)})
        if accepted:
            # This is a file-level transaction, not a full translations/ tree.
            # The immutable source manifest and all untouched JSONL batches
            # keep their exact bytes and modification times.
            files = {}
            for name in sorted(dirty_batches):
                files[TRANSLATIONS + '/' + name] = b''.join(
                    (json.dumps(r, ensure_ascii=False, sort_keys=True) + '\n').encode('utf8')
                    for r in batches[name])
            self.transaction(files, sorted(files), expected)
        return dict(accepted=accepted, rejected=rejected, status='partial' if rejected else 'ok')

    def status(self):
        manifest, _, batches = self.load(allow_en_changed=True)
        rows = [r for batch in batches.values() for r in batch]
        try:
            self.verify_output()
            output = 'current'
        except (ValueError, OSError, KeyError) as error:
            output = 'absent/stale: ' + str(error)
        latest = self.path(REPORTS + '/sync-latest.json')
        counts = read_json(latest)['counts'] if latest.exists() else {}
        return dict(generation=manifest['generation'], files=len(manifest['sources']), records=len(rows),
                    batches=len(batches), completed=sum(bool(r['PTBR']) for r in rows),
                    pending=sum(not r['PTBR'] for r in rows), output=output,
                    next_batch=next((name for name, entries in batches.items() if any(not r['PTBR'] for r in entries)), None),
                    latest_changes=counts, storage=self.gc(dry_run=True),
                    en_changed=inventory(self.path('EN')) != manifest['sources'])

    def build(self):
        expected = self.state_inventory()
        manifest, sources, batches = self.load()
        rows = [r for batch in batches.values() for r in batch]
        require(all(r['PTBR'] for r in rows), f'Incomplete translation: {sum(not r["PTBR"] for r in rows)} pending; PTBR unchanged')
        translations = {r['id']: r['PTBR'] for r in rows}
        outputs = build_outputs(sources, translations)
        # Deep native validation is performed above, not in CI.
        output_hashes = source_inventory(outputs)
        receipt = dict(schema=SCHEMA, producer='thea2-toolkit-build-v1', generation=manifest['generation'],
                       sources=manifest['sources'], translation_fingerprint=self.generation_fingerprint(),
                       records=len(rows), completed=len(rows), pending=0, outputs=output_hashes,
                       native_validation='parse-rebuild-parse-all-records', tooling=self.tooling_fingerprint())
        receipt['source_version'] = manifest['source_version']
        receipt['receipt_hash'] = fingerprint(receipt)
        if self.path(BUILD).exists() and read_json(self.path(BUILD)) == receipt and inventory(self.path('PTBR')) == output_hashes:
            return {'status': 'unchanged', 'outputs': output_hashes}
        files = {'PTBR/' + p: data for p, data in outputs.items()}
        files[BUILD] = blob(receipt)
        self.transaction(files, ['PTBR', BUILD], expected)
        self.verify_output()
        return dict(status='built', outputs=output_hashes, records=len(rows))

    def tooling_fingerprint(self):
        return fingerprint({name: sha(self.path(WORK + name).read_bytes() if name.endswith('.png') or name == 'mod-assets/modid.txt' else self.path(WORK + name).read_bytes().replace(b'\r\n', b'\n')) for name in ('native.py', 'toolkit.py', 'catalog.json', 'mod-assets/description.txt', 'mod-assets/image.png', 'mod-assets/PTBRFontFallback.cs', 'mod-assets/modid.txt')})

    def verify_output(self):
        require(not self.interrupted(), 'Interrupted transaction; recover first')
        require(self.path(BUILD).is_file(), 'No successful build manifest')
        receipt = read_json(self.path(BUILD))
        keys = {'schema', 'producer', 'generation', 'sources', 'translation_fingerprint', 'records',
                'completed', 'pending', 'outputs', 'native_validation', 'tooling', 'receipt_hash', 'source_version'}
        require(set(receipt) == keys, 'Invalid/incomplete build receipt')
        require(receipt['schema'] == SCHEMA and receipt['producer'] == 'thea2-toolkit-build-v1', 'Invalid producer')
        require(receipt['receipt_hash'] == fingerprint({k: v for k, v in receipt.items() if k != 'receipt_hash'}), 'Build receipt mismatch')
        manifest = read_json(self.path(TRANSLATIONS + '/manifest.json'))
        require(receipt['generation'] == manifest['generation'] == fingerprint(manifest['sources']), 'Stale source generation')
        require(receipt['source_version'] == manifest['source_version'], 'Stale source version metadata')
        require(receipt['sources'] == manifest['sources'] == inventory(self.path('EN')) == inventory(self.path(SNAPSHOT)), 'Stale/missing original sources')
        require(receipt['translation_fingerprint'] == self.generation_fingerprint(), 'Stale translation generation')
        require(receipt['tooling'] == self.tooling_fingerprint(), 'Tooling changed: rebuild required')
        require(receipt['records'] == receipt['completed'] == manifest['records'] and receipt['pending'] == 0, 'Incomplete build')
        require(receipt['native_validation'] == 'parse-rebuild-parse-all-records', 'Missing native validation')
        rows = [json.loads(line) for name in manifest['batches'] for line in self.path(TRANSLATIONS + '/' + name).read_text(encoding='utf8').splitlines()]
        require(len(rows) == manifest['records'] and len({r['id'] for r in rows}) == len(rows) and all(isinstance(r.get('PTBR'), str) and r['PTBR'] for r in rows), 'Incomplete/duplicate translation rows')
        for r in rows:
            validate_translation(r)
        require(set(receipt['outputs']) == set(output_paths(manifest['sources'])), 'Invalid output inventory')
        require(inventory(self.path('PTBR')) == receipt['outputs'], 'Missing/extra/modified output files')
        return dict(status='verified', files=len(receipt['outputs']), generation=receipt['generation'])

    def package(self):
        self.verify_output()
        approved = inventory(self.path('PTBR'))
        target = self.path('PTBR.zip')
        temp = self.path(STATE + '/PTBR.zip')
        temp.parent.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(temp, 'w', zipfile.ZIP_DEFLATED) as archive:
            for name in approved:
                info = zipfile.ZipInfo(name, date_time=(2020, 1, 1, 0, 0, 0))
                info.compress_type = zipfile.ZIP_DEFLATED
                archive.writestr(info, safe(self.path('PTBR'), name).read_bytes())
        with zipfile.ZipFile(temp) as archive:
            require(archive.namelist() == list(approved) and archive.testzip() is None, 'Invalid ZIP inventory')
            require({name: sha(archive.read(name)) for name in archive.namelist()} == approved, 'ZIP content mismatch')
        self.verify_output()
        os.replace(temp, target)
        return dict(status='packaged', file='PTBR.zip', sha256=sha(target.read_bytes()), members=list(approved))

    def candidate(self, args):
        if args.source_file:
            source = Path(args.source_file)
            matches = [name for name in PATHS if Path(name).name == source.name]
            require(len(matches) == 1, 'Ambiguous/unknown managed source filename')
            sources = read_sources(self.path('EN'))
            sources[matches[0]] = source.read_bytes()
            return sources
        if args.source_root:
            require(not Path(args.source_root).is_symlink() and not
                    (hasattr(Path(args.source_root), 'is_junction') and Path(args.source_root).is_junction()), 'Linked source root')
            p = Path(args.source_root).resolve()
            require(not p.is_relative_to(self.path('PTBR')), 'PTBR cannot be an original source')
            return read_sources(p)
        if args.game_root or args.command == 'update-sources':
            candidates = []
            if args.game_root:
                candidates = [Path(args.game_root)]
            else:
                local = self.path(STATE + '/machine.json')
                if local.exists():
                    candidates = [Path(read_json(local)['game_root'])]
                else:
                    # Bounded metadata search: Steam AppID 606230, never entire-drive scanning.
                    steam = Path(os.environ.get('ProgramFiles(x86)', '/nonexistent')) / 'Steam/steamapps'
                    libraries = [steam]
                    vdf = steam / 'libraryfolders.vdf'
                    if vdf.exists():
                        import re
                        libraries += [Path(p.replace('\\\\', '\\')) / 'steamapps' for p in re.findall(r'"path"\s+"([^"]+)"', vdf.read_text(encoding='utf8'))]
                    for lib in libraries:
                        acf = lib / 'appmanifest_606230.acf'
                        if acf.exists():
                            import re
                            match = re.search(r'"installdir"\s+"([^"]+)"', acf.read_text(encoding='utf8'))
                            if match:
                                candidates.append(lib / 'common' / match[1])
            require(len(set(p.resolve() for p in candidates)) == 1,
                    'No unique verified installation. Use update-sources --game-root "GAME ROOT" (untouched English originals)')
            p = candidates[0].resolve()
            require(not candidates[0].is_symlink() and not
                    (hasattr(candidates[0], 'is_junction') and candidates[0].is_junction()), 'Linked game root')
            require(p != self.root and not p.is_relative_to(self.root), 'Expected external game installation')
            require((p / 'Thea2.exe').is_file(), 'Not Thea 2: missing Thea2.exe')
            # Read ONLY managed resources, not an entire installation subtree.
            names = set(PATHS)
            modules = safe(p, 'Thea2_Data/StreamingAssets/Modules')
            names.update(f'Thea2_Data/StreamingAssets/Modules/{f.name}' for f in modules.glob('*.xml'))
            names.update(f'Thea2_Data/StreamingAssets/DataBase/{f.name}' for f in safe(p, 'Thea2_Data/StreamingAssets/DataBase').glob('DATABASE_*LOCALIZATION.xml'))
            sources = {name: safe(p, name).read_bytes() for name in sorted(names)}
            source_records(sources)
            approved = read_json(self.path(WORK + 'source-policy.json'))
            digest = fingerprint(source_inventory(sources))
            require(digest not in approved.get('rejected', []), 'Known translated installation; use original resources')
            accepted_manifest = self.path(TRANSLATIONS + '/manifest.json')
            accepted_digest = read_json(accepted_manifest)['generation'] if accepted_manifest.exists() else None
            require(digest in approved['trusted'] or digest == accepted_digest or args.approve_source == digest,
                    f'Unrecognized original/modded source; cannot prove English provenance. Inspect it, then explicitly attest with --approve-source {digest}')
            version = safe(p, 'Thea2_Data/StreamingAssets/ver.txt')
            self._candidate_version = version.read_text(encoding='utf8').strip() if version.is_file() else 'unconfirmed native version'
            if not args.dry_run:
                self._verified_game_root = str(p)
            return sources
        return None


def main(argv=None):
    # Windows consoles and redirected streams may default to cp1252; reports are UTF-8.
    for stream in (sys.stdout, sys.stderr):
        if hasattr(stream, 'reconfigure'):
            stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', nargs='?', default='sync', choices=['extract', 'sync', 'update-sources', 'check', 'diff', 'status', 'resume', 'doctor',
                                          'apply', 'build', 'verify-output', 'package', 'recover', 'rollback', 'gc'])
    parser.add_argument('--source-root')
    parser.add_argument('--source-file')
    parser.add_argument('--game-root')
    parser.add_argument('--approve-source')
    parser.add_argument('--dry-run', action='store_true')
    parser.add_argument('--json', action='store_true')
    parser.add_argument('--payload')
    parser.add_argument('--replace', action='store_true')
    parser.add_argument('--snapshot')
    args = parser.parse_args(argv)
    project = Project()
    try:
        require(sum(bool(v) for v in (args.source_root, args.source_file, args.game_root)) <= 1, 'Choose a single source input')
        require(not (args.source_root or args.source_file or args.game_root or args.approve_source)
                or args.command in ('extract', 'sync', 'update-sources', 'diff'), 'Source flags do not apply to this command')
        require(not args.dry_run or args.command in ('extract', 'sync', 'update-sources', 'diff', 'gc'), '--dry-run requires a source command or gc')
        require(not args.replace or args.command == 'apply', '--replace is only supported by apply')
        require(not args.payload or args.command == 'apply', '--payload is only supported by apply')
        mutates = args.command in ('extract', 'sync', 'update-sources', 'apply', 'build', 'package', 'recover', 'rollback', 'gc') and not args.dry_run
        with project.lock() if mutates else contextlib.nullcontext():
            if project.interrupted() and mutates and args.command != 'recover':
                project.recover()
            if args.command in ('extract', 'sync', 'update-sources'):
                result = project.sync(project.candidate(args), initial=args.command == 'extract' or (args.command == 'update-sources' and not project.path(TRANSLATIONS + '/manifest.json').exists()), dry_run=args.dry_run)
                if hasattr(project, '_verified_game_root') and not args.dry_run:
                    atomic_json(project.path(STATE + '/machine.json'), {'game_root': project._verified_game_root})
            elif args.command == 'check':
                project.load()
                result = project.status()
            elif args.command == 'doctor':
                from doctor import doctor
                result = doctor(project)
            elif args.command in ('status', 'resume'):
                result = project.status()
            elif args.command == 'diff':
                candidate = project.candidate(args)
                result = {'mode': 'read-only', 'report': project.sync(candidate, dry_run=True) if candidate is not None else read_json(project.path(REPORTS + '/sync-latest.json'))}
            elif args.command == 'apply':
                require(args.payload, 'apply requires --payload FILE.json')
                result = project.apply(read_json(Path(args.payload)), args.replace)
            elif args.command == 'build':
                result = project.build()
            elif args.command == 'verify-output':
                result = project.verify_output()
            elif args.command == 'package':
                result = project.package()
            elif args.command == 'recover':
                result = project.recover()
            elif args.command == 'gc':
                result = project.gc(dry_run=args.dry_run)
            else:
                require(args.snapshot and len(args.snapshot) == 32 and all(c in '0123456789abcdef' for c in args.snapshot), 'rollback requires --snapshot TRANSACTION_ID')
                base = project.path(STATE + '/transactions/' + args.snapshot)
                receipt = read_json(base / 'receipt.json')
                require(all(project.tree_digest(project.path(t)) == receipt['after'][t] for t in receipt['targets']), 'Rollback would overwrite newer progress; preserve it first')
                files = {}
                require(any(receipt['before'][t] is not None for t in receipt['targets']), 'Initial extraction has no earlier generation to restore')
                for rel in receipt['targets']:
                    old = safe(base / 'old', rel)
                    require(project.tree_digest(old) == receipt['before'][rel], 'Damaged rollback snapshot')
                    if receipt['before'][rel] is None:
                        files[rel] = None
                        continue
                    if old.is_file():
                        files[rel] = old.read_bytes()
                    else:
                        for name in inventory(old):
                            files[rel + '/' + name] = safe(old, name).read_bytes()
                result = {'status': 'rolled-back', 'transaction': project.transaction(files, receipt['targets'], project.state_inventory())}
        print(json.dumps(result, ensure_ascii=False, indent=2))
        return 2 if result.get('rejected') else 0
    except (OSError, ValueError, KeyError, TypeError, RecursionError) as error:
        print(json.dumps({'status': 'error', 'message': str(error)}, ensure_ascii=False), file=sys.stderr)
        return 1


if __name__ == '__main__':
    sys.exit(main())
