"""Verified, serialized GitHub Release publisher. Offline-testable HTTP transport."""
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import time
import urllib.error
import urllib.parse
import urllib.request

from toolkit import Project, ROOT, require, read_json

VERSION = re.compile(r'^v(\d+)\.(\d+)\.(\d+)$')


def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT).decode('utf8').strip()


class API:
    def __init__(self, repository, token, transport=None):
        require(re.fullmatch(r'[\w.-]+/[\w.-]+', repository), 'Invalid GitHub repository')
        self.repository, self.token = repository, token
        self.root = 'https://api.github.com/repos/' + repository
        self.transport = transport or urllib.request.urlopen

    def request(self, method, suffix='', body=None, raw=False):
        url = suffix if suffix.startswith('https://') else self.root + suffix
        host = urllib.parse.urlparse(url).hostname
        require(host in ('api.github.com', 'uploads.github.com'), 'Untrusted GitHub API host')
        headers = {'Authorization': 'Bearer ' + self.token, 'User-Agent': 'thea2-ptbr-release',
                   'Accept': 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28'}
        if raw:
            data = body
            headers['Content-Type'] = 'application/zip'
        else:
            data = None if body is None else json.dumps(body).encode()
            if data is not None:
                headers['Content-Type'] = 'application/json'
        # Only GET is automatically retried. A failed POST is resumed by a fresh
        # workflow run, which discovers the existing draft/tag/asset first.
        for attempt in range(3):
            try:
                req = urllib.request.Request(url, data=data, headers=headers, method=method)
                with self.transport(req, timeout=60) as response:
                    payload = response.read()
                    return json.loads(payload) if payload else None
            except urllib.error.HTTPError as error:
                code = error.code
                error.close()
                if method == 'GET' and code in (429, 500, 502, 503, 504) and attempt < 2:
                    time.sleep(1 + attempt)
                    continue
                raise ValueError(f'GitHub {method} {url}: HTTP {code}; check permissions, repository and rate limits') from error
        raise ValueError('GitHub retry limit')

    def optional(self, suffix):
        # A 404 is allowed ONLY for explicit existence lookups, never metadata.
        try:
            return self.request('GET', suffix)
        except ValueError as error:
            if 'HTTP 404;' in str(error):
                return None
            raise

    def releases(self):
        result = []
        for page in range(1, 101):
            items = self.request('GET', f'/releases?per_page=100&page={page}')
            result.extend(items)
            if len(items) < 100:
                return result
        raise ValueError('Unexpected release pagination limit')


def next_version(previous):
    if previous is None:
        return 'v1.0.0'
    m = VERSION.fullmatch(previous)
    require(m is not None, 'Invalid release version')
    a, b, c = map(int, m.groups())
    return f'v{a}.{b}.{c + 1}'


def rows_at(ref):
    result = {}
    for name in git('ls-tree', '-r', '--name-only', ref, '--', 'translation-work/translations').splitlines():
        if re.fullmatch(r'translation-work/translations/batch-\d{4}\.jsonl', name):
            for line in git('show', ref + ':' + name).splitlines():
                row = json.loads(line)
                require(row['id'] not in result, 'Duplicate release row identity')
                result[row['id']] = row
    return result


def changelog(old, new, before, after, repository, sha):
    added = sorted(set(new) - set(old))
    removed = sorted(set(old) - set(new))
    revised = sorted(k for k in set(old) & set(new) if old[k]['PTBR'] != new[k]['PTBR'] or old[k]['EN'] != new[k]['EN'])
    changed = sorted(k for k in set(before) | set(after) if before.get(k) != after.get(k))
    groups = {}
    for rid in added + revised:
        category = new[rid]['context']['kind']
        groups[category] = groups.get(category, 0) + 1
    labels = {'database': 'Interface e descrições', 'name': 'Nomes', 'story': 'Narrativas', 'choice': 'Escolhas'}
    body = ['## Tradução PT-BR', '', f'Entradas adicionadas: {len(added)}; revisadas: {len(revised)}; removidas: {len(removed)}.', '']
    body += [f'- {labels.get(k, k)}: {v} entradas alteradas.' for k, v in sorted(groups.items())]
    body += ['', f'Arquivos nativos alterados: {len(changed)}.', '']
    body += [f'- `{name}`' for name in changed]
    body += ['', 'Baixe **PTBR.zip** e siga as instruções do README. Validação estrutural concluída; testes em jogo são uma verificação separada.',
             '', f'[Instalação e créditos](https://github.com/{repository}/blob/{sha}/README.md) · '
             f'[Licença](https://github.com/{repository}/blob/{sha}/LICENSE) · '
             f'[Permissão de integração oficial](https://github.com/{repository}/blob/{sha}/GAME-DEVELOPER-PERMISSION.md)']
    return '\n'.join(body)


def publish(api, sha, outputs, zip_bytes, ancestry, notes):
    """No filesystem mutation. Caller verifies native build and committed inputs."""
    import io
    import zipfile
    require(bool(zip_bytes) and bool(outputs), 'Empty/missing ZIP or output inventory')
    with zipfile.ZipFile(io.BytesIO(zip_bytes)) as archive:
        require(len(archive.namelist()) == len(set(archive.namelist())) and archive.testzip() is None,
                'Invalid/duplicate ZIP members')
        require({name: hashlib.sha256(archive.read(name)).hexdigest() for name in archive.namelist()} == outputs,
                'ZIP differs from verified build')
    digest = 'sha256:' + hashlib.sha256(zip_bytes).hexdigest()
    meta = api.request('GET')  # canonical repository root has no trailing slash
    branch = urllib.parse.quote(meta['default_branch'], safe='')
    current = api.request('GET', '/commits/' + branch)['sha']
    require(current == sha, 'Stale workflow commit; default branch has advanced')
    releases = api.releases()
    published = []
    for release in releases:
        if release['draft'] or release.get('prerelease'):
            continue
        m = VERSION.fullmatch(release['tag_name'])
        require(m is not None, 'Ambiguous non-SemVer public release')
        published.append((tuple(map(int, m.groups())), release))
    published.sort(key=lambda p: p[0])
    previous = published[-1][1] if published else None
    before = {}
    if previous:
        prior_tag = previous['tag_name']
        ref = api.request('GET', '/git/ref/tags/' + urllib.parse.quote(prior_tag, safe=''))
        ancestry(prior_tag, ref['object']['sha'])
        prior_receipt = api.request('GET', '/contents/translation-work/build-manifest.json?ref=' + urllib.parse.quote(prior_tag, safe=''))
        import base64
        before = json.loads(base64.b64decode(prior_receipt['content']))['outputs']
        require({x['name'] for x in previous['assets']} == {'PTBR.zip'}, 'Unexpected public release assets; manual correction required')
        if before == outputs:
            require(previous['assets'][0].get('digest') == digest and previous['assets'][0]['size'] == len(zip_bytes),
                    'Existing public ZIP digest differs; manual review required')
            return {'status': 'unchanged', 'tag': prior_tag}
    tag = next_version(previous['tag_name'] if previous else None)
    ref = api.optional('/git/ref/tags/' + tag)
    require(ref is None or ref['object']['sha'] == sha, 'Existing tag points to another commit; refusing retag')
    drafts = [r for r in releases if r['tag_name'] == tag]
    require(len(drafts) <= 1 and all(r['draft'] for r in drafts), 'Conflicting release state')
    body = notes(previous['tag_name'] if previous else None, before)
    release = drafts[0] if drafts else api.request('POST', '/releases', {
        'tag_name': tag, 'target_commitish': sha, 'name': tag + ' — Thea 2 PT-BR', 'body': body, 'draft': True, 'prerelease': False})
    require(release['target_commitish'] == sha, 'Draft targets a different commit')
    assets = api.request('GET', f"/releases/{release['id']}/assets")
    require(all(a['name'] == 'PTBR.zip' for a in assets) and len(assets) <= 1, 'Unexpected draft assets; review before retry')
    if assets:
        require(assets[0].get('digest') == digest and assets[0]['size'] == len(zip_bytes), 'Draft archive differs; review before retry')
    else:
        upload = release['upload_url'].split('{')[0] + '?name=PTBR.zip'
        asset = api.request('POST', upload, zip_bytes, raw=True)
        require(asset['name'] == 'PTBR.zip' and asset.get('digest') == digest and asset['size'] == len(zip_bytes), 'Uploaded asset verification failed')
    # Recheck default branch immediately before publishing a draft.
    require(api.request('GET', '/commits/' + branch)['sha'] == sha, 'Branch advanced during upload; draft preserved')
    api.request('PATCH', f"/releases/{release['id']}", {'draft': False, 'body': body})
    final = api.request('GET', f"/releases/{release['id']}")
    require(not final['draft'] and len(final['assets']) == 1 and final['assets'][0]['name'] == 'PTBR.zip'
            and final['assets'][0].get('digest') == digest and final['assets'][0]['size'] == len(zip_bytes), 'Published asset set/digest mismatch')
    return {'status': 'published', 'tag': tag, 'sha256': digest}


def main():
    project = Project()
    project.verify_output()
    require(not git('status', '--porcelain', '--untracked-files=no'), 'Tracked checkout differs from triggering commit')
    project.package()
    sha = git('rev-parse', 'HEAD')
    require(sha == os.environ['GITHUB_SHA'], 'Unexpected workflow revision')
    receipt = read_json(project.path('translation-work/build-manifest.json'))
    archive = project.path('PTBR.zip').read_bytes()
    require(archive, 'Empty ZIP')
    repository = os.environ['GITHUB_REPOSITORY']
    api = API(repository, os.environ['GH_TOKEN'])
    def ancestry(tag, remote_sha):
        require(git('rev-parse', tag + '^{}') == remote_sha, 'Remote/local tag mismatch')
        require(subprocess.run(['git', 'merge-base', '--is-ancestor', tag, sha], cwd=ROOT).returncode == 0, 'Prior release is not an ancestor')
    def notes(tag, before):
        return ('Versão nativa de referência: ' + receipt['source_version'] + '\n\n' +
                changelog(rows_at(tag) if tag else {}, rows_at('HEAD'), before, receipt['outputs'], repository, sha))
    print(json.dumps(publish(api, sha, receipt['outputs'], archive, ancestry, notes), indent=2))


if __name__ == '__main__':
    main()
