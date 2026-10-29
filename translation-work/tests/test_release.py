import base64
import copy
import hashlib
import io
import json
from pathlib import Path
import sys
import unittest
import urllib.error
import urllib.parse
from unittest.mock import patch
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from release import API, publish, next_version, changelog
from consent import consent, STATEMENT


def payload(content=b'fixture'):
    name = 'Thea2_Data/StreamingAssets/Mods/PTBR/info.txt'
    buffer = io.BytesIO()
    with zipfile.ZipFile(buffer, 'w') as z:
        z.writestr(zipfile.ZipInfo(name, date_time=(2020, 1, 1, 0, 0, 0)), content)
    return {name: hashlib.sha256(content).hexdigest()}, buffer.getvalue()


class Response:
    def __init__(self, value): self.value = json.dumps(value).encode()
    def read(self): return self.value
    def __enter__(self): return self
    def __exit__(self, *args): pass


class Server:
    def __init__(self):
        self.calls = []; self.items = []; self.tags = {}; self.before = {}
        self.current = 'commit'; self.fail = None; self.upload_count = 0
        self.after_upload_fail = False

    def __call__(self, req, timeout):
        url = urllib.parse.urlparse(req.full_url)
        path = url.path; method = req.get_method()
        self.calls.append((method, req.full_url))
        self.last_headers = req.headers
        if self.fail:
            code = self.fail; self.fail = None
            raise urllib.error.HTTPError(req.full_url, code, 'fixture', {}, None)
        prefix = '/repos/owner/repo'
        body = json.loads(req.data) if req.data and req.headers.get('Content-type') == 'application/json' else None
        if path == prefix and method == 'GET': return Response({'default_branch': 'main'})
        if path == prefix + '/commits/main': return Response({'sha': self.current})
        if path == prefix + '/releases' and method == 'GET': return Response(copy.deepcopy(self.items))
        if path.startswith(prefix + '/git/ref/tags/'):
            tag = path.rsplit('/', 1)[1]
            if tag not in self.tags: raise urllib.error.HTTPError(req.full_url, 404, 'missing', {}, None)
            return Response({'object': {'sha': self.tags[tag]}})
        if path.startswith(prefix + '/contents/'):
            return Response({'content': base64.b64encode(json.dumps({'outputs': self.before}).encode()).decode()})
        if path == prefix + '/releases' and method == 'POST':
            item = dict(body, id=7, assets=[], upload_url='https://uploads.github.com/repos/owner/repo/releases/7/assets{?name,label}')
            self.items.append(item); self.tags[body['tag_name']] = body['target_commitish']
            return Response(copy.deepcopy(item))
        if path == prefix + '/releases/7/assets' and method == 'POST':
            self.upload_count += 1
            asset = {'id': 8, 'name': 'PTBR.zip', 'size': len(req.data), 'digest': 'sha256:' + hashlib.sha256(req.data).hexdigest()}
            self.items[-1]['assets'].append(asset)
            if self.after_upload_fail:
                self.after_upload_fail = False
                raise urllib.error.HTTPError(req.full_url, 503, 'interrupted', {}, None)
            return Response(asset)
        if path == prefix + '/releases/7/assets' and method == 'GET': return Response(copy.deepcopy(self.items[-1]['assets']))
        if path == prefix + '/releases/7' and method == 'PATCH':
            self.items[-1].update(body); return Response(copy.deepcopy(self.items[-1]))
        if path == prefix + '/releases/7' and method == 'GET': return Response(copy.deepcopy(self.items[-1]))
        raise AssertionError(f'Unexpected actual HTTP request: {method} {req.full_url}')


class ReleaseTests(unittest.TestCase):
    def setUp(self):
        self.server = Server(); self.api = API('owner/repo', 'fixture-token', self.server)
        self.outputs, self.archive = payload()
        self.ancestors = []

    def publish(self, **kwargs):
        return publish(self.api, kwargs.get('sha', 'commit'), kwargs.get('outputs', self.outputs),
                       kwargs.get('archive', self.archive),
                       kwargs.get('ancestry', lambda *a: self.ancestors.append(a)),
                       lambda prior, before: 'Fixture factual notes')

    def test_complete_actual_http_sequence_first_release(self):
        result = self.publish()
        self.assertEqual(result['tag'], 'v1.0.0')
        self.assertEqual(self.server.calls[0], ('GET', 'https://api.github.com/repos/owner/repo'))
        self.assertEqual(self.server.upload_count, 1)
        self.assertEqual([x['name'] for x in self.server.items[-1]['assets']], ['PTBR.zip'])
        self.assertFalse(self.server.items[-1]['draft'])
        self.assertIn('Authorization', self.server.last_headers)

    def test_idempotent_rerun_no_new_version(self):
        self.publish(); self.server.before = self.outputs
        self.assertEqual(self.publish()['status'], 'unchanged')
        self.assertEqual(self.server.upload_count, 1)

    def test_changed_output_patch_version(self):
        self.publish(); self.server.before = self.outputs
        new, archive = payload(b'new fixture')
        result = self.publish(outputs=new, archive=archive)
        self.assertEqual(result['tag'], 'v1.0.1')
        self.assertEqual(len(self.server.items), 2)
        self.assertTrue(self.ancestors)

    def test_interrupted_upload_resumes_draft_without_duplicate(self):
        self.server.after_upload_fail = True
        with self.assertRaisesRegex(ValueError, 'HTTP 503'): self.publish()
        self.assertTrue(self.server.items[0]['draft'])
        self.assertEqual(self.publish()['status'], 'published')
        self.assertEqual(self.server.upload_count, 1)
        self.assertEqual(len(self.server.items), 1)

    def test_stale_commit_and_tag_conflict(self):
        self.server.current = 'newer'
        with self.assertRaisesRegex(ValueError, 'Stale workflow'): self.publish()
        self.assertFalse(self.server.items)
        self.server.current = 'commit'; self.server.tags['v1.0.0'] = 'other'
        with self.assertRaisesRegex(ValueError, 'another commit'): self.publish()

    def test_permissions_404_and_transient_retry(self):
        for code in (401, 403, 404):
            self.server.fail = code
            with self.assertRaisesRegex(ValueError, 'HTTP ' + str(code)): self.publish()
        self.server.fail = 503
        with patch('release.time.sleep') as sleep:
            self.assertEqual(self.publish()['status'], 'published')
            sleep.assert_called_once()

    def test_unexpected_assets_and_nonancestor(self):
        self.publish(); self.server.before = self.outputs
        self.server.items[0]['assets'].append({'name': 'LICENSE'})
        with self.assertRaisesRegex(ValueError, 'Unexpected public release assets'): self.publish()
        self.server.items[0]['assets'].pop()
        def refuse(*args): raise ValueError('non-ancestor fixture')
        with self.assertRaisesRegex(ValueError, 'non-ancestor'): self.publish(ancestry=refuse)

    def test_missing_zip_and_content_tampering(self):
        with self.assertRaisesRegex(ValueError, 'Empty/missing'): self.publish(archive=b'')
        wrong, archive = payload(b'tampered')
        with self.assertRaisesRegex(ValueError, 'differs'): self.publish(archive=archive)
        self.assertFalse(self.server.calls)

    def test_invalid_version_untrusted_host_and_meaningful_changelog(self):
        with self.assertRaises(ValueError): next_version('invalid')
        with self.assertRaises(ValueError): self.api.request('POST', 'https://attacker.invalid/upload', b'x', raw=True)
        old = {'a': {'PTBR': 'a', 'EN': 'A', 'context': {'kind': 'story'}}}
        new = {'a': {'PTBR': 'b', 'EN': 'A', 'context': {'kind': 'story'}}, 'b': {'PTBR': 'c', 'EN': 'B', 'context': {'kind': 'choice'}}}
        text = changelog(old, new, {'file': 'old'}, {'file': 'new'}, 'owner/repo', 'commit')
        self.assertIn('adicionadas: 1; revisadas: 1', text)
        self.assertIn('Narrativas: 1', text)
        self.assertIn('Escolhas: 1', text)

    def test_contribution_requires_affirmative_record(self):
        pr = {'number': 1, 'user': {'login': 'contributor'}, 'head': {'sha': 'fixture'}, 'body': '- [ ] ' + STATEMENT}
        self.assertFalse(consent(pr))
        pr['body'] = '- [x] ' + STATEMENT
        self.assertTrue(consent(pr))
        pr['user'] = {}
        self.assertFalse(consent(pr))


if __name__ == '__main__': unittest.main()
