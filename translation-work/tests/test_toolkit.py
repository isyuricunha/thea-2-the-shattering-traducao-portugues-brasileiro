import copy
import hashlib
import importlib
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import toolkit as t
import native as n
from doctor import doctor, normalized

DB = 'Thea2_Data/StreamingAssets/DataBase/DATABASE_UI_LOCALIZATION.xml'
MODULE = 'Thea2_Data/StreamingAssets/Modules/Fixture.xml'


def db(items):
    import html
    return ('<DATABASE_UI_LOCALIZATION-NO_HSH><LOC_LIBRARY-UI>' + ''.join(
        f'<Entry Key="{k}" Val="{html.escape(v, quote=True)}"/>' for k, v in items) + '</LOC_LIBRARY-UI></DATABASE_UI_LOCALIZATION-NO_HSH>').encode()


class ProjectTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.p = t.Project(self.root)
        self.patcher = patch.object(t, 'PATHS', (DB,)); self.patcher.start()
        for name in ('toolkit.py', 'native.py', 'catalog.json'):
            dest = self.root / 'translation-work' / name
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_bytes((t.ROOT / 'translation-work' / name).read_bytes())
        self.p.sync({DB: db([('A', 'Fixture {SCRIPT:id}'), ('B', 'Other')])}, initial=True)

    def tearDown(self):
        self.patcher.stop(); self.temp.cleanup()

    def rows(self):
        return [r for batch in self.p.load()[2].values() for r in batch]

    def apply(self, rows, replace=False):
        return self.p.apply({'generation': self.p.load()[0]['generation'], 'entries': [
            {k: r[k] for k in ('id', 'source_fingerprint', 'PTBR')} for r in rows]}, replace)

    def test_partial_tokens_stale_and_immutable(self):
        rows = self.rows()
        rows[0]['PTBR'] = 'Broken'
        rows[1]['PTBR'] = 'Synthetic fixture'
        result = self.apply(rows)
        self.assertEqual(len(result['accepted']), 1)
        self.assertEqual(len(result['rejected']), 1)
        rows[0]['source_fingerprint'] = 'wrong'
        with self.assertRaisesRegex(ValueError, 'Stale source'):
            self.apply([rows[0]])
        with self.assertRaisesRegex(ValueError, 'Duplicate/unknown'):
            self.apply([rows[1], rows[1]])

    def test_incremental_sync_history_reorder_and_missing(self):
        rows = self.rows()
        for r in rows: r['PTBR'] = r['EN']
        self.apply(rows)
        report = self.p.sync({DB: db([('B', 'Other'), ('A', 'Changed {SCRIPT:id}'), ('C', 'Added')])})
        self.assertEqual(report['counts']['modified'], 1)
        active = {r['context']['native_key']: r for r in self.rows()}
        self.assertEqual(active['B']['PTBR'], 'Other')
        self.assertEqual(active['A']['PTBR'], '')
        self.assertEqual(active['A']['history'][0]['PTBR'], 'Fixture {SCRIPT:id}')
        with self.assertRaises(ValueError): self.p.sync({})
        self.assertFalse((self.root / 'PTBR').exists())

    def test_noop_dryrun_and_batch_mtime(self):
        before = self.p.state_inventory()
        mt = (self.root / t.TRANSLATIONS / 'batch-0001.jsonl').stat().st_mtime_ns
        self.assertEqual(self.p.sync()['status'], 'unchanged')
        self.p.sync({DB: db([('A', 'New'), ('B', 'Other')])}, dry_run=True)
        self.assertEqual(before, self.p.state_inventory())
        self.assertEqual(mt, (self.root / t.TRANSLATIONS / 'batch-0001.jsonl').stat().st_mtime_ns)

    def test_update_sources_validated_installation_and_readonly_dryrun(self):
        with tempfile.TemporaryDirectory(prefix='Thea fixture ') as folder:
            game = Path(folder)
            source = game / DB; source.parent.mkdir(parents=True)
            source.write_bytes(self.p.path('EN/' + DB).read_bytes())
            (game / 'Thea2.exe').write_bytes(b'fixture-identity')
            (game / 'Thea2_Data/StreamingAssets/ver.txt').write_text(n.CATALOG['game_version'])
            digest = t.fingerprint(t.source_inventory({DB: source.read_bytes()}))
            self.p.path('translation-work/source-policy.json').write_bytes(t.blob({'trusted': [digest], 'rejected': []}))
            args = SimpleNamespace(source_file=None, source_root=None, game_root=folder, command='update-sources',
                                   dry_run=True, approve_source=None)
            before = t.inventory(self.root)
            self.assertEqual(self.p.sync(self.p.candidate(args), dry_run=True)['status'], 'unchanged')
            self.assertEqual(before, t.inventory(self.root))
            source.write_bytes(db([('A', 'Updated {SCRIPT:id}'), ('B', 'Other')]))
            with self.assertRaisesRegex(ValueError, 'Unrecognized original'): self.p.candidate(args)
            args.approve_source = t.fingerprint(t.source_inventory({DB: source.read_bytes()}))
            self.assertEqual(self.p.sync(self.p.candidate(args))['counts']['modified'], 1)
            self.assertEqual(source.read_bytes(), self.p.path('EN/' + DB).read_bytes())
            self.assertFalse((self.root / 'PTBR').exists())

    def test_incomplete_complete_package_and_staleness(self):
        with self.assertRaisesRegex(ValueError, 'Incomplete translation'): self.p.build()
        self.assertFalse((self.root / 'PTBR').exists())
        rows = self.rows()
        for r in rows: r['PTBR'] = r['EN']
        self.apply(rows); self.p.build(); self.p.verify_output(); self.p.package()
        before = t.inventory(self.root / 'PTBR')
        with zipfile.ZipFile(self.root / 'PTBR.zip') as z:
            self.assertEqual(set(z.namelist()), set(before))
            self.assertTrue(all(x.startswith(n.MOD) for x in z.namelist()))
            self.assertNotIn('LICENSE', z.namelist())
        rows = self.rows(); rows[1]['PTBR'] = 'Revised fixture'; self.apply([rows[1]], replace=True)
        with self.assertRaisesRegex(ValueError, 'Stale translation'): self.p.verify_output()
        self.assertEqual(before, t.inventory(self.root / 'PTBR'))

    def test_forged_complete_and_changed_native_refused(self):
        rows = self.rows()
        for r in rows: r['PTBR'] = r['EN']
        self.apply(rows); self.p.build()
        receipt = t.read_json(self.root / t.BUILD)
        file = self.root / t.TRANSLATIONS / 'batch-0001.jsonl'
        rows[0]['PTBR'] = ''
        file.write_text(''.join(json.dumps(r) + '\n' for r in rows), encoding='utf8')
        receipt['translation_fingerprint'] = self.p.generation_fingerprint()
        receipt['receipt_hash'] = t.fingerprint({k: v for k, v in receipt.items() if k != 'receipt_hash'})
        (self.root / t.BUILD).write_bytes(t.blob(receipt))
        with self.assertRaisesRegex(ValueError, 'Incomplete/duplicate'): self.p.verify_output()

    def test_crash_recovery_gc_and_conflict(self):
        name = t.TRANSLATIONS + '/batch-0001.jsonl'
        original = self.p.path(name).read_bytes()
        with self.assertRaises(RuntimeError):
            self.p.transaction({name: original + b'\n'}, [name], self.p.state_inventory(), fail_after=0)
        with self.assertRaisesRegex(ValueError, 'interrupted'): self.p.gc()
        self.p.recover()
        self.assertEqual(self.p.path(name).read_bytes(), original)
        with self.assertRaises(RuntimeError):
            self.p.transaction({name: original + b'\n'}, [name], self.p.state_inventory(), fail_after=0)
        self.p.path(name).write_bytes(b'user edit')
        with self.assertRaisesRegex(ValueError, 'Recovery conflict'): self.p.recover()

    def test_unsafe_paths_and_concurrent_change(self):
        for path in ('../escape', '/absolute', 'C:/drive', 'a\\b'):
            with self.assertRaises(ValueError): self.p.path(path)
        expected = self.p.state_inventory()
        self.p.path('EN/' + DB).write_bytes(db([('A', 'External edit')]))
        with self.assertRaisesRegex(ValueError, 'Concurrent edits'):
            self.p.transaction({'fixture': b'x'}, ['fixture'], expected)

    def test_readonly_doctor_diagnostics_and_normalization(self):
        meta = self.root / 'translation-work/meta.json'
        meta.write_bytes(b'{"fixture":1}\r\n')
        support = self.root / 'translation-work/runtime-support.json'
        support.write_bytes(t.blob({'schema': 1, 'tooling': {'translation-work/meta.json': t.sha(normalized(meta))}}))
        before = t.inventory(self.root)
        self.assertEqual(doctor(self.p)['status'], 'ok')
        self.assertEqual(before, t.inventory(self.root))
        meta.write_bytes(b'{"fixture":1}\n')
        self.assertEqual(doctor(self.p)['status'], 'ok')
        meta.write_bytes(b'{"fixture":2}\n')
        with self.assertRaisesRegex(ValueError, 'checksum: toolkit'): doctor(self.p)
        meta.unlink()
        with self.assertRaisesRegex(ValueError, 'dependency: missing'): doctor(self.p)
        with patch('doctor.platform.machine', return_value='unsupported'):
            with self.assertRaisesRegex(ValueError, 'architecture:'): doctor(self.p)
        with patch('doctor.sys.version_info', (3, 9)):
            with self.assertRaisesRegex(ValueError, 'runtime:'): doctor(self.p)
        support.unlink()
        with self.assertRaisesRegex(ValueError, 'dependency: missing runtime'): doctor(self.p)

    def test_build_receipt_survives_git_lf_normalization(self):
        rows = self.rows()
        for r in rows: r['PTBR'] = r['EN']
        self.apply(rows)
        batch = self.root / t.TRANSLATIONS / 'batch-0001.jsonl'
        batch.write_bytes(batch.read_bytes().replace(b'\n', b'\r\n'))
        self.p.build()
        batch.write_bytes(batch.read_bytes().replace(b'\r\n', b'\n'))
        self.p.verify_output()
        output = self.root / 'PTBR' / (n.MOD + 'info.txt')
        output.write_bytes(output.read_bytes() + b'tampered')
        with self.assertRaisesRegex(ValueError, 'Missing/extra/modified'): self.p.verify_output()

    def test_500_tiny_updates_across_101_batches(self):
        # Sparse batches are a supported post-update state; exercise the actual
        # apply/load/journal/retention path, with 101 independently owned files.
        self.p.sync({DB: db([(f'K{i}', f'Fixture {i}') for i in range(101)])})
        manifest, _, batches = self.p.load()
        rows = [r for b in batches.values() for r in b]
        for name in manifest['batches']: self.p.path(t.TRANSLATIONS + '/' + name).unlink()
        manifest['batches'] = []
        for i, row in enumerate(rows):
            name = f'batch-{i+1:04d}.jsonl'; manifest['batches'].append(name)
            self.p.path(t.TRANSLATIONS + '/' + name).write_text(json.dumps(row) + '\n', encoding='utf8')
        self.p.path(t.TRANSLATIONS + '/manifest.json').write_bytes(t.blob(manifest))
        before = {name: self.p.path(t.TRANSLATIONS + '/' + name).stat().st_mtime_ns for name in manifest['batches']}
        for i in range(500):
            row = dict(rows[i % 101], PTBR=f'Synthetic revision {i}')
            self.p.apply({'generation': manifest['generation'], 'entries': [
                {k: row[k] for k in ('id', 'source_fingerprint', 'PTBR')}]}, replace=True)
        status = self.p.gc(dry_run=True)
        self.assertLessEqual(status['kept'], 10)
        self.assertLess(status['total_bytes'], 100_000)
        self.assertFalse(self.p.interrupted())
        self.assertTrue(all(self.p.path(t.TRANSLATIONS + '/' + name).stat().st_mtime_ns != mt for name, mt in before.items()))
        self.assertEqual(len(self.rows()), 101)


class NativeTests(unittest.TestCase):
    def test_original_native_roundtrip_all_48_files(self):
        sources = t.read_sources(t.ROOT / 'EN')
        rows = t.source_records(sources)
        outputs = n.build_outputs(sources, {r['id']: r['EN'] for r in rows})
        self.assertEqual(len(outputs), 49)
        for name, data in sources.items(): self.assertEqual(outputs[n.MOD + Path(name).name], data)

    def test_xml_multiline_accent_and_logic_preserved(self):
        data = b'<MODULE xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" name="Fixture" uniqueModuleID="9"><Adventure name="E" uniqueID="3"><nodes xsi:type="NodeAdventure" ID="2"><logic><Filter x="1"/></logic><outputs ownerID="2" targetID="4" name="First&#10;Second"/>Story {SCRIPT:id}</nodes></Adventure></MODULE>'
        rows = n.records(MODULE, data)
        values = {r['id']: r['EN'].replace('Story', 'Fixture \u00e7\u00e3\u00e9') for r in rows}
        output = n.build_outputs({MODULE: data}, values)
        self.assertIn(b'<STORY_MODULE>Fixture.xml</STORY_MODULE>', output[n.MOD + 'info.txt'])
        self.assertIn('Fixture \u00e7\u00e3\u00e9', output[n.MOD + 'Fixture.xml'].decode())
        self.assertEqual([r['EN'] for r in n.records(MODULE, output[n.MOD + 'Fixture.xml'])], list(values.values()))

    def test_actual_48_files_rewrite_accent_fixture_in_memory(self):
        sources = t.read_sources(t.ROOT / 'EN')
        rows = t.source_records(sources)
        values = {r['id']: r['EN'] for r in rows}
        first = {}
        for row in rows: first.setdefault(row['source_file'], row)
        for row in first.values(): values[row['id']] += ' [fixture \u00e7\u00e3\u00e9]'
        outputs = n.build_outputs(sources, values)
        for source, row in first.items():
            rebuilt = n.records(source, outputs[n.MOD + Path(source).name])
            self.assertEqual(rebuilt[0]['EN'], values[row['id']])
        self.assertEqual(len(first), 48)

    def test_duplicate_native_key_occurrences_and_comments(self):
        data = b'<D><L><!-- <Entry Key="NO" Val="Ignored"/> --><Entry Key="K" Val="One"/><Entry Key="K" Val="Two"/></L></D>'
        rows = n.records(DB, data)
        self.assertEqual(len({r['id'] for r in rows}), 2)
        self.assertEqual(n.rebuild(DB, data, {r['id']: r['EN'] for r in rows}), data)

    def test_no_external_entities(self):
        with self.assertRaises(ValueError): n.records(DB, b'<!DOCTYPE x [<!ENTITY x SYSTEM "file:///secret">]><D/>')

    def test_git_canonical_crlf_and_real_tampering(self):
        with tempfile.TemporaryDirectory() as d:
            root = Path(d)
            subprocess.run(['git', 'init', '-q', d], check=True)
            (root / '.gitattributes').write_text('*.json text eol=lf\n*.bin -text\n')
            (root / 'a.json').write_bytes(b'{"fixture":1}\r\n')
            (root / 'native.bin').write_bytes(b'a\r\nb')
            subprocess.run(['git', '-C', d, 'add', '.'], check=True, capture_output=True)
            canonical = subprocess.check_output(['git', '-C', d, 'show', ':a.json'])
            self.assertEqual(canonical, normalized(root / 'a.json'))
            raw = subprocess.check_output(['git', '-C', d, 'show', ':native.bin'])
            self.assertEqual(raw, (root / 'native.bin').read_bytes())
            (root / 'a.json').write_bytes(b'{"fixture":2}\n')
            self.assertNotEqual(canonical, normalized(root / 'a.json'))
            (root / 'native.bin').write_bytes(b'modified')
            self.assertNotEqual(raw, (root / 'native.bin').read_bytes())


if __name__ == '__main__': unittest.main()
