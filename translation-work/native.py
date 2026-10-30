"""Thea 2 native adapter. Python standard library only; no game writes."""
from collections import Counter
import hashlib
import html
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

CATALOG = json.loads(Path(__file__).with_name('catalog.json').read_text(encoding='utf8'))
PATHS = tuple(CATALOG['paths'])
MOD = 'Thea2_Data/StreamingAssets/Mods/PTBR/'
XSI = '{http://www.w3.org/2001/XMLSchema-instance}type'


def require(condition, message):
    if not condition:
        raise ValueError(message)


def decode(data):
    match = re.search(br'encoding=[\"\x27]([^\"\x27]+)', data[:160])
    return data.decode(match[1].decode() if match else 'utf-8-sig')


def xml(data):
    try:
        text = decode(data)
    except (LookupError, UnicodeError) as error:
        raise ValueError('Unsupported or malformed native encoding: ' + str(error)) from error
    require('<!DOCTYPE' not in text and '<!ENTITY' not in text, 'External XML declarations prohibited')
    try:
        return ET.fromstring(text), text
    except ET.ParseError as error:
        raise ValueError('Malformed native XML: ' + str(error)) from error


def module_nodes(data):
    root, _ = xml(data)
    require(root.tag == 'MODULE' and root.get('uniqueModuleID'), 'Invalid module identity')
    seen = set()
    for ei, event in enumerate(root.findall('Adventure')):
        eid = event.get('uniqueID')
        require(eid is not None and eid not in seen, 'Duplicate/missing event ID')
        seen.add(eid)
        ns = set()
        for node in event.findall('nodes'):
            nid = node.get('ID')
            require(nid is not None and nid not in ns, 'Duplicate/missing node ID')
            ns.add(nid)
            if node.get(XSI) == 'NodeAdventure':
                yield root, ei, event, node


def module_units(path, data):
    result = []
    for root, ei, event, node in module_nodes(data):
        prefix = f"module:{root.get('uniqueModuleID')}/event:{event.get('uniqueID')}/node:{node.get('ID')}"
        story = ''.join(t for t in [node.text or '', *(c.tail or '' for c in node)] if t.strip())
        # XmlSerializer ignores indentation-only text; preserve real narrative whitespace.
        if not story.strip():
            story = ''
        context = dict(module=root.get('name'), event=event.get('name'), event_id=event.get('uniqueID'),
                       node_id=node.get('ID'), image=node.get('image'),
                       identity_evidence='Narrator/player group; speaker, addressee and referent may vary. No fixed gender inferred.',
                       logic=ET.tostring(node.find('logic'), encoding='unicode') if node.find('logic') is not None else '')
        if story:
            result.append((prefix + '/story', story, dict(context, kind='story'), (ei, node.get('ID'), 'story')))
        for oi, out in enumerate(node.findall('outputs')):
            value = out.get('name', '')
            if value:
                result.append((prefix + f'/out:{oi}', value,
                               dict(context, kind='choice', output_index=oi, target=out.get('targetID'), owner=out.get('ownerID')),
                               (ei, node.get('ID'), oi)))
    return result


def database_units(path, data):
    root, text = xml(data)
    result, seen = [], Counter()
    if path.endswith('DATABASE_DES_NAMES.xml'):
        counts = Counter()
        # Value-bearing name elements only: Race/Subrace are DB references.
        for collection in root:
            for child in collection:
                if child.tag in ('Race', 'Subrace') or 'Value' not in child.attrib:
                    continue
                value = child.get('Value')
                if not value:
                    continue
                key = f"{collection.tag}/{child.tag}/{value}"
                slot = counts[key]; counts[key] += 1
                key += f'/{slot}'
                result.append((key, value, dict(kind='name', collection=collection.tag,
                    name_category=child.tag, identity_evidence='Category applies to the generated name; does not identify the speaker of an event.'), None))
    else:
        for entry in root.iter('Entry'):
            require(set(entry.attrib) == {'Key', 'Val'}, 'Unexpected localization entry shape')
            key, value = entry.get('Key'), entry.get('Val')
            require(key, 'Missing localization key')
            occurrence = seen[key]; seen[key] += 1
            native_key = key
            key += f'/occurrence:{occurrence}'
            if value:
                result.append((key, value, dict(kind='database', library=root.tag, native_key=native_key, occurrence=occurrence,
                    identity_evidence='Localization key is context, not proof of speaker or referent gender.'), None))
    return result


def units(path, data):
    return module_units(path, data) if '/Modules/' in path else database_units(path, data)


def records(path, data):
    return [dict(id=path + '#' + key, source_file=path, key=key, row=i, EN=value, PTBR='', context=context)
            for i, (key, value, context, locator) in enumerate(units(path, data))]


TOKEN = re.compile(r'\{[^{}]*\}|</?(?:color|size|b|i|u|s|sprite|font|material|link|align|alpha|voffset|space|cspace|mspace|indent|line-height|nobr|style|mark|sup|sub|br)(?:=[^<>]*|\s[^<>]*)?>|<#[0-9a-fA-F]+>|\\[nr]|%(?:\d+\$)?[sdif]', re.I)
MARKERS = ('-- [EVENT] --', '+[NODE]', '[STORY]', '[/STORY]', '[OUT]', '[/NODE]', '[/EVENT]')


def tokens(text):
    return Counter(TOKEN.findall(text))


def validate_translation(record):
    value = record.get('PTBR')
    require(isinstance(value, str), 'PTBR must be text')
    if not value:
        return
    require('\x00' not in value, 'NUL not allowed')
    require(tokens(record['EN']) == tokens(value), f"Protected token mismatch: {record['id']}")
    def balanced(text):
        stack = []
        for close, name in re.findall(r'<(/?)(b|i|u|s|link|nobr|sup|sub)(?:=[^<>]*|\s[^<>]*)?>', text, re.I):
            name = name.lower()
            if close:
                if not stack or stack.pop() != name:
                    return False
            else:
                stack.append(name)
        return not stack
    require(not balanced(record['EN']) or balanced(value), 'Broken rich-text nesting')
    if record['context']['kind'] in ('story', 'choice'):
        require(not any(line.startswith(MARKERS) for line in value.splitlines()), 'Native event marker injection')
    if record['context']['kind'] == 'choice':
        pass  # XML story-module overrides preserve multiline choices.
    # Verify XML-compatible Unicode even when the output is TXT.
    try:
        ET.fromstring('<x>' + html.escape(value) + '</x>')
    except ET.ParseError as error:
        raise ValueError('Invalid XML text: ' + str(error)) from error


def rebuild(path, data, translations):
    """Byte-preserving DB rewrite; unchanged values retain original entities/format."""
    if '/Modules/' in path:
        return rebuild_module(path, data, translations)
    rows = records(path, data)
    _, text = xml(data)
    spans = []
    if path.endswith('DATABASE_DES_NAMES.xml'):
        pattern = r'<(?!Race\b|Subrace\b)[\w-]+\s+Value="([^"]*)"\s*/>'
        matches = [m for m in re.finditer(pattern, text) if html.unescape(m[1])]
        attr = 1
    else:
        matches = list(re.finditer(r'<Entry\s+Key="[^"]*"\s+Val="([^"]*)"\s*/>', text))
        comments = [(m.start(), m.end()) for m in re.finditer(r'<!--.*?-->', text, re.S)]
        matches = [m for m in matches if html.unescape(m[1]) and not any(a <= m.start() < b for a, b in comments)]
        attr = 1
    require(len(matches) == len(rows), f'Unrecognized DB formatting: {path}')
    for row, match in zip(rows, matches):
        value = translations[row['id']]
        validate_translation(dict(row, PTBR=value))
        if value != row['EN']:
            spans.append((match.start(attr), match.end(attr), html.escape(value, quote=True).replace('\r', '&#13;').replace('\n', '&#10;').replace('\t', '&#9;')))
    if not spans:
        return data
    for a, b, value in reversed(spans):
        text = text[:a] + value + text[b:]
    # Native source DB has no declaration; output UTF-8 explicitly if a declaration exists.
    text = re.sub(r'encoding=["\x27][^"\x27]+["\x27]', 'encoding="utf-8"', text, count=1)
    output = text.encode('utf8')
    rebuilt = records(path, output)
    require([r['EN'] for r in rebuilt] == [translations[r['id']] for r in rows], 'DB reconstruction mismatch')
    return output


def rebuild_module(path, data, translations):
    rows = records(path, data)
    if all(translations[r['id']] == r['EN'] for r in rows):
        return data
    root, _ = xml(data)
    loc = {locator: path + '#' + key for key, value, ctx, locator in module_units(path, data)}
    for ei, event in enumerate(root.findall('Adventure')):
        for node in event.findall('nodes'):
            if node.get(XSI) != 'NodeAdventure':
                continue
            rid = loc.get((ei, node.get('ID'), 'story'))
            if rid:
                node.text = None
                for child in node:
                    child.tail = None
                if len(node):
                    node[-1].tail = translations[rid]
                else:
                    node.text = translations[rid]
            for oi, out in enumerate(node.findall('outputs')):
                rid = loc.get((ei, node.get('ID'), oi))
                if rid:
                    out.set('name', translations[rid])
    output = ET.tostring(root, encoding='utf-8', xml_declaration=True).replace(b'\r', b'&#13;')
    require([r['EN'] for r in records(path, output)] == [translations[r['id']] for r in rows], 'Module text roundtrip mismatch')
    # Compare every nonlocalizable attribute, element, logic subtree and ordering.
    def shape(data):
        tree, _ = xml(data)
        for ei, event in enumerate(tree.findall('Adventure')):
            for node in event.findall('nodes'):
                if node.get(XSI) != 'NodeAdventure':
                    continue
                node.text = None
                for child in node:
                    child.tail = None
                for out in node.findall('outputs'):
                    out.attrib.pop('name', None)
        def structure(e):
            return (e.tag, sorted(e.attrib.items()), (e.text or '').strip(), (e.tail or '').strip(), [structure(c) for c in e])
        return structure(tree)
    require(shape(data) == shape(output), 'Nonlocalizable module structure changed')
    return output


def output_paths(sources):
    return [MOD + Path(p).name for p in sources] + [MOD + 'info.txt', MOD + 'image.png']


def build_outputs(sources, translations):
    result = {}
    info = ['<?xml version="1.0" encoding="utf-8"?>', '<ModInfo Name="Português Brasileiro" Graphic="image.png" Visibility="0">']
    for path, data in sources.items():
        for record in records(path, data):
            validate_translation(dict(record, PTBR=translations[record['id']]))
        module = '/Modules/' in path
        name = Path(path).name
        require(MOD + name not in result, 'Output path collision')
        result[MOD + name] = rebuild(path, data, translations)
        tag = 'STORY_MODULE' if module else 'DB'
        info.append(f'<{tag}>{html.escape(name)}</{tag}>')
    info.extend(['<TAG>Portuguese</TAG>', '<TAG>Translation</TAG>', html.escape(Path(__file__).with_name('mod-assets').joinpath('description.txt').read_text(encoding='utf8')), '</ModInfo>', ''])
    result[MOD + 'image.png'] = Path(__file__).with_name('mod-assets').joinpath('image.png').read_bytes()
    result[MOD + 'info.txt'] = '\n'.join(info).encode('utf8')
    ET.fromstring(result[MOD + 'info.txt'])
    return result
