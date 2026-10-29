"""Require affirmative, attributable PR consent; never infer it from submission."""
import json
import os
from pathlib import Path

STATEMENT = 'Autorizo minha contribuição sob CC BY-NC-SA 4.0 e a permissão adicional de integração oficial comercial, sem royalties e com créditos, conforme CONTRIBUTING.md; detenho os direitos necessários.'


def consent(pr):
    body = pr.get('body') or ''
    return bool(pr.get('number') and pr.get('user', {}).get('login') and pr.get('head', {}).get('sha')
                and any(line.strip() in ('- [x] ' + STATEMENT, '- [X] ' + STATEMENT) for line in body.splitlines()))


if __name__ == '__main__':
    event = json.loads(Path(os.environ['GITHUB_EVENT_PATH']).read_text(encoding='utf8'))
    pr = event['pull_request']
    if not consent(pr):
        raise SystemExit('Consentimento afirmativo ausente. Leia CONTRIBUTING.md e complete a declaração no PR.')
    print(f"Consentimento registrado no PR #{pr['number']}: {pr['user']['login']} / {pr['head']['sha']}; revisão de direitos ainda necessária.")
