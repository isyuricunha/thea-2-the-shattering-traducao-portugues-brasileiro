import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]


def editorial_errors(text):
    forbidden = ('El' + 'la', 'Sa' + 'yuri', 'Co' + 'dex', 'Chat' + 'GPT')
    errors = [name for name in forbidden if re.search(r'\b' + name + r'\b', text, re.I)]
    if re.search(r'(?:traduzid[ao]|testad[ao]|concluíd[ao]) por Yuri', text, re.I):
        errors.append('third-person narration')
    return errors


class DocumentTests(unittest.TestCase):
    def test_public_readme_evergreen_across_release_versions(self):
        text = (ROOT / 'README.md').read_text(encoding='utf8')
        for version in (None, 'v1.0.0', 'v1.0.1', 'v1.0.9'):
            self.assertIn('/releases', text)
            self.assertIn('quando disponível', text)
            self.assertNotIn('/releases/latest', text)
            self.assertFalse(re.search(r'0 traduzid|em preparação|não publicado|100%|release-state', text, re.I))
        self.assertEqual(editorial_errors(text), [])
        self.assertFalse(re.search(r'JSONL|ARM64|fingerprint|batch-\d|45.325|91 lotes', text))
        self.assertEqual(len(re.findall('img.shields.io', text)), 3)
        self.assertIn('apps/606230/header.jpg', text)
        self.assertIn('width="650"', text)
        self.assertNotIn('TODO', text)

    def test_negative_editorial_fixtures_and_legal_name_allowed(self):
        self.assertTrue(editorial_errors('Tradução feita por ' + 'El' + 'la'))
        self.assertTrue(editorial_errors('Tradução concluída por Yuri'))
        self.assertFalse(editorial_errors('Coordenação e manutenção: Yuri Cunha'))
        self.assertFalse(editorial_errors('Validação estrutural concluída.'))

    def test_legal_full_code_and_internal_docs(self):
        license = (ROOT / 'LICENSE').read_text(encoding='utf8')
        self.assertGreater(len(license), 20000)
        self.assertIn('Section 8', license)
        permission = (ROOT / 'GAME-DEVELOPER-PERMISSION.md').read_text(encoding='utf8')
        self.assertIn('Thea 2: The Shattering', permission)
        self.assertIn('MuHa Games', permission)
        self.assertNotIn('Playable Archive', permission)
        for path in (ROOT / 'translation-work').rglob('*.md'):
            if '.state' not in path.parts and 'reference' not in path.parts:
                self.assertEqual(editorial_errors(path.read_text(encoding='utf8')), [], str(path))
        for name in ('LICENSE', 'GAME-DEVELOPER-PERMISSION.md', 'CONTRIBUTING.md'):
            self.assertIn('./' + name, (ROOT / 'README.md').read_text(encoding='utf8'))

    def test_release_workflow_output_gate_and_token(self):
        text = (ROOT / '.github/workflows/release.yml').read_text(encoding='utf8')
        self.assertIn("'PTBR/**'", text)
        self.assertNotIn('translations/**', text)
        self.assertIn('contents: write', text)
        self.assertIn('concurrency:', text)
        self.assertIn('GH_TOKEN: ${{ github.token }}', text)
        self.assertLess(text.index('verify-output'), text.index('translation-work/release.py'))
        self.assertNotIn('toolkit.py build', text)


if __name__ == '__main__': unittest.main()
