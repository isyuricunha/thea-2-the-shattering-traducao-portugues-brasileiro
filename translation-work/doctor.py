"""Read-only portability preflight. No installs, translation or output writes."""
import hashlib
import json
import platform
from pathlib import Path
import sys

from native import require, records, rebuild, build_outputs


def normalized(path):
    data = path.read_bytes()
    return data.replace(b'\r\n', b'\n') if path.suffix in ('.py', '.json', '.jsonl', '.md', '.yml') or path.name in ('.gitattributes', '.gitignore') else data


def doctor(project):
    require(sys.version_info >= (3, 10), 'runtime: install Python 3.10 or newer')
    machine = platform.machine().lower()
    require(machine in ('amd64', 'x86_64', 'arm64', 'aarch64'), 'architecture: unsupported ' + machine)
    receipt = project.path('translation-work/runtime-support.json')
    require(receipt.is_file(), 'dependency: missing runtime-support.json')
    info = json.loads(receipt.read_text(encoding='utf8'))
    require(info.get('schema') == 1 and isinstance(info.get('tooling'), dict), 'malformed-test-data: invalid runtime receipt')
    for name, digest in info['tooling'].items():
        require(project.path(name).is_file(), 'dependency: missing ' + name)
        require(hashlib.sha256(normalized(project.path(name))).hexdigest() == digest, 'checksum: toolkit changed: ' + name)
    manifest = json.loads(project.path('translation-work/translations/manifest.json').read_text(encoding='utf8'))
    for name, digest in manifest['sources'].items():
        require(hashlib.sha256(project.path('EN/' + name).read_bytes()).hexdigest() == digest, 'checksum: original changed: ' + name)
    path = 'Thea2_Data/StreamingAssets/DataBase/DATABASE_UI_LOCALIZATION.xml'
    fixture = b'<DATABASE_UI_LOCALIZATION-NO_HSH><LOC_LIBRARY-UI><Entry Key="SMOKE" Val="Fixture {SCRIPT:id}"/></LOC_LIBRARY-UI></DATABASE_UI_LOCALIZATION-NO_HSH>'
    rows = records(path, fixture)
    require(len(rows) == 1, 'malformed-test-data: fixture parser failed')
    # Synthetic passthrough verifies actual parse/reconstruction, never translates.
    require(rebuild(path, fixture, {rows[0]['id']: rows[0]['EN']}) == fixture, 'malformed-test-data: reconstruction failed')
    output = build_outputs({path: fixture}, {rows[0]['id']: rows[0]['EN']})
    require(len(output) == 2, 'malformed-test-data: mod reconstruction failed')
    module = 'Thea2_Data/StreamingAssets/Modules/Smoke.xml'
    fixture = b'<MODULE xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" name="Smoke" uniqueModuleID="1"><Adventure uniqueID="1" name="Fixture"><nodes xsi:type="NodeAdventure" ID="2"><outputs ownerID="2" targetID="3" name="Fixture&#10;choice"/>Fixture story</nodes></Adventure></MODULE>'
    rows = records(module, fixture)
    require(rebuild(module, fixture, {r['id']: r['EN'] for r in rows}) == fixture,
            'malformed-test-data: module reconstruction failed')
    return {'status': 'ok', 'runtime': platform.python_version(), 'architecture': machine,
            'system': platform.system(), 'dependencies': 'Python standard library only',
            'arm64_execution': 'verified by this invocation' if platform.system() == 'Linux' and machine in ('aarch64', 'arm64') else 'pending first Ubuntu ARM64 preflight',
            'sources': len(manifest['sources']), 'read_only': True}
