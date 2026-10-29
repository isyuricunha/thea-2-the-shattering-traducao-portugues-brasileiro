# Trabalho de tradução — Thea 2: The Shattering

## Estado inicial e ambiente

Referência: Steam AppID **606230**, build **20265356**, versão nativa
**2.0528.0681**, coletada em 28/10/2026. São 48 arquivos originais: quatro bancos
XML e 44 módulos. O catálogo inicial tem **45.325 entradas**, em **91 lotes**:
90 com 500 e o último com 325. Todos os campos PTBR começam vazios.

Categorias: 6.311 textos não vazios de banco, 4.240 nomes, 11.754 narrativas e
23.020 opções. A contagem textual simples do XML inclui um exemplo comentado e
valores vazios; não representa o catálogo traduzível. O mod russo foi observado
apenas como referência estrutural e não é fonte linguística nem dependência.

Use **Python 3.10+**, somente biblioteca padrão. Desenvolvimento e testes no
Windows; o código é independente de arquitetura. Execução real no **Ubuntu
Server 24.04 LTS ARM64 ainda depende do primeiro preflight**. Não é necessário
instalar .NET, Mono, bibliotecas nativas, compiladores ou pacotes Python.
A ferramenta de inspeção usada no desenvolvimento não integra o runtime.

## Primeiros passos do responsável pela tradução

Após o clone/pull normal, na raiz do repositório:

```bash
python3 -B translation-work/toolkit.py doctor
python3 -B translation-work/toolkit.py status
```

Se `doctor` falhar, pare antes de traduzir e reporte a mensagem exata. Ele verifica
arquitetura, Python, recibo de integridade, originais e reconstrução de fixture,
sem alterar dados ou instalar nada. `-B` também evita criação de bytecode.

O briefing já foi pesquisado e escrito na preparação técnica: leia
[briefing-do-jogo.md](docs/briefing-do-jogo.md). Em seguida, crie e mantenha as
decisões em [diretrizes-de-traducao.md](docs/diretrizes-de-traducao.md) e
[glossario-ptbr.md](docs/glossario-ptbr.md). Não há terminologia aprovada de antemão.
Traduza e revise manualmente. Os 500 registros são a organização do arquivo;
grupos menores podem ser aplicados a qualquer momento.

## Organização e mapeamento

```text
EN/Thea2_Data/StreamingAssets/{DataBase,Modules}/  originais intactos
translation-work/accepted-sources/               geração aceita para comparação
translation-work/translations/batch-*.jsonl      trabalho manual UTF-8
translation-work/translations/manifest.json      catálogo e geração
translation-work/reports/                        diffs duráveis, inclusive removidos
translation-work/.state/                         recuperação local, ignorada no Git
PTBR/Thea2_Data/StreamingAssets/Mods/PTBR/         somente mod nativo completo
```

**Os caminhos de entrada e saída diferem por exigência do sistema de mods.**
Cada XML de DataBase é registrado como `<DB>` em `info.txt`; cada XML de Modules
é registrado como `<STORY_MODULE>`. O nome do módulo é preservado. Nenhum XML é
instalado por cima dos bancos originais. `info.txt` é gerado pela toolkit.
O ZIP contém `Thea2_Data/...` diretamente, sem uma pasta externa `PTBR/`.
Consulte [formato-localizacao.md](docs/formato-localizacao.md).

## Comandos

Todos resolvem o repositório pela localização do script, mesmo quando chamados
de outro diretório. Relatórios de terminal são JSON UTF-8; `--json` é aceito.

| Comando | Uso |
| --- | --- |
| `extract` | Cria o catálogo inicial a partir de EN; recusa sobrescrever progresso |
| `sync` | Compara EN com a geração aceita; também é o comando padrão |
| `update-sources` | Localiza uma instalação Steam e importa somente os XML gerenciados |
| `check` | Valida originais, campos imutáveis, IDs e tokens; pendências são permitidas |
| `status` / `resume` | Mostra progresso, última atualização, saída obsoleta e armazenamento |
| `diff` | Exibe último relatório ou proposta de sincronização, sem escrita |
| `apply --payload ARQUIVO` | Aplica somente PTBR, por ID e fingerprint |
| `build` | Exige todas as entradas; reconstrói e publica PTBR em transação |
| `verify-output` | Verifica recibo, fontes, traduções, inventário e hashes, sem reconstruir |
| `package` | Verifica e produz PTBR.zip determinístico |
| `doctor` | Preflight somente leitura para o clone/pull |
| `recover` | Restaura a geração anterior de uma operação interrompida |
| `rollback --snapshot ID` | Reverte snapshot retido, somente se não houver progresso posterior |
| `gc --dry-run` / `gc` | Inspeciona / remove somente snapshots concluídos elegíveis |

### Atualização do jogo

```bash
python translation-work/toolkit.py update-sources --dry-run
python translation-work/toolkit.py update-sources --game-root "G:/SteamLibrary/steamapps/common/Thea 2 The Shattering" --dry-run
python translation-work/toolkit.py update-sources --game-root "G:/SteamLibrary/steamapps/common/Thea 2 The Shattering"
```

A descoberta é limitada aos manifests Steam, bibliotecas registradas e último
caminho local verificado. Não varre discos. No servidor sem o jogo, atualize EN
no Windows e envie pelo Git. Para fontes já copiadas use `sync`; há também
`--source-root` para uma árvore completa ou `--source-file` para um XML gerenciado
cujo nome seja inequívoco. Nunca use o mod traduzido como original.

Um hash ainda não conhecido exige inspeção de procedência. A ferramenta informa
o fingerprint; depois de confirmar que são originais ingleses, forneça
`--approve-source FINGERPRINT`. Essa confirmação não é automática. Novos módulos
e bancos de localização reconhecidos são descobertos; ausência de um arquivo
antes aceito bloqueia a operação para evitar interpretar cópia incompleta como
remoção intencional. Uma retirada real de arquivo exige migração explícita do
catálogo pela manutenção. Remoções de entradas dentro de um arquivo são reportadas
e arquivadas normalmente.

O mesmo motor atende `sync` e `update-sources`. Texto e contexto idênticos mantêm
PTBR e lote. Mudança material esvazia o PTBR ativo e guarda os valores anteriores
em `history`. Reordenação conserva traduções; renomeação sem prova é removido +
adicionado. Relatórios JSON/Markdown identificam gerações, IDs, lotes e diferenças.
Uma execução sem mudanças não regrava dados; `--dry-run` não cria cache ou relatório.

### Aplicar uma revisão manual

O payload JSON tem exatamente este contrato; os identificadores e fingerprints
devem ser copiados do catálogo real, não inventados:

```json
{
  "generation": "fingerprint da geração aceita",
  "entries": [
    {"id": "ID copiado do lote", "source_fingerprint": "fingerprint do registro", "PTBR": "texto revisado"}
  ]
}
```

IDs desconhecidos, duplicados ou de geração errada invalidam o payload antes de
qualquer escrita. Erros de conteúdo são separados por entrada: as válidas são
salvas e as rejeitadas são reportadas, com saída não zero. Reenvio idêntico é
idempotente. Para substituir PTBR existente, revise e use `--replace`; o anterior
vai para `history`. Só os lotes realmente alterados são escritos e copiados para
recuperação; os outros preservam bytes e mtime. Edição direta de PTBR também é
validada por `check`, mas prefira `apply` para garantir gravação transacional.

## Integridade, armazenamento e recuperação

`.gitattributes` fixa LF para scripts/JSON e `-text` para originais e arquivos
nativos. O hash de ferramentas usa os mesmos bytes canônicos LF que o Git;
originais usam SHA-256 dos bytes exatos. O recibo de runtime lista as ferramentas
sem depender de si próprio; o manifesto lista as fontes; o recibo do build liga
fontes, lotes, ferramentas e saídas. Alterar conteúdo invalida o recibo relevante;
não regenere hashes apenas para esconder erro.

As operações de escrita usam lock do sistema, staging no mesmo volume, fsync,
journal e renomeação de arquivos. O journal permite restaurar uma geração lógica
inteira após interrupção, mas **não oferece uma transação multiarquivo visível
atomicamente para editores externos**: não edite durante uma promoção. Alterações
concorrentes detectadas abortam; recuperação com conflito exige preservar a edição.
Entradas antigas e histórico das fontes são versionados; backups locais não
substituem commits Git.

Retenção automática: até **10 transações concluídas / 512 MiB**. Cada operação
estima espaço e recusa staging acima do orçamento; preserva margem livre de
16 MiB. Operações parciais copiam somente arquivos sujos. Builds finais guardam
o conjunto de saída necessário à reversão. `gc --dry-run` mostra contagem e
bytes recuperáveis. Journal pendente bloqueia poda; staging sem recibo fica
protegido para inspeção. A transação mais recente não é descartada para caber
silenciosamente no orçamento. Os limites são constantes documentadas em
`toolkit.py`, ajustáveis pela manutenção com revisão e novos testes.

Ao encontrar interrupção, a próxima escrita tenta recuperar; `recover` explícito
permite fazê-lo antes. `status`/`check` recusam uma geração incompleta. Rollback
exige snapshot íntegro e estado atual igual ao posterior registrado, evitando
sobrescrever novas traduções. A extração inicial não possui geração anterior.

## Build e publicação automática

```bash
python translation-work/toolkit.py check
python translation-work/toolkit.py build
python translation-work/toolkit.py verify-output
python translation-work/toolkit.py package
```

Pendências impedem o build e preservam a saída anterior. Apenas `build` cria
PTBR; as traduções em andamento nunca o atualizam. Ao concluir, inclua PTBR e
`translation-work/build-manifest.json` no mesmo commit. Alterações em PTBR ou no
recibo acionam `.github/workflows/release.yml`; commits apenas de JSONL/README
não publicam. O primeiro pacote validado recebe `v1.0.0`; mudanças seguintes
incrementam patch, comparando arquivos e entradas com o último tag publicado.
Saída idêntica não cria versão. O changelog é factual e não afirma teste em jogo.

A API é usada com token restrito `contents: write`, concorrência serializada,
rechecagem do commit da branch padrão, tag imutável, draft recuperável e validação
de nome/tamanho/SHA-256 do único anexo permitido: **PTBR.zip**. Erros de permissão,
histórico não ancestral ou assets inesperados interrompem. O job não altera README
nem cria commits. Licenças e avisos públicos ficam no corpo/links da Release, sem
anexos extras; não há fonte ou biblioteca de terceiros redistribuída no mod.

O fluxo de publicação é testado com HTTP simulado. **Execução no GitHub e Release
real não foram usadas como teste.** Para um teste autorizado futuro: complete e
valide uma fixture em branch/repositório descartável, configure a branch padrão,
faça push do build completo, verifique tag, único ZIP e rerun sem duplicação.
Neste projeto, não despache o workflow sem um build completo real.

O README público é permanente: números de progresso, diagnóstico, versão atual
e evidências ficam aqui, nos manifestos e nas Releases. A publicação não exige
editar texto de disponibilidade nem marcador no README. Mude a página pública
somente quando houver mudança relevante para jogadores.

## Verificação e limites

```bash
python -B -m unittest discover -s translation-work/tests -v
python -B translation-work/toolkit.py doctor
python -B translation-work/toolkit.py check
```

Os testes cobrem formato real, fixtures de acentos/multilinha, atualização,
incompletude, adulteração, Git LF, recuperação, publicação HTTP e 500 aplicações
pequenas distribuídas em 101 lotes. O relatório de aceitação registra o resultado
executado. Testes do runtime Unity, renderização dos glifos, saves e coop precisam
ser feitos no jogo. O preflight ARM64 é independente desse teste de gameplay.

Licença, autorização comercial oficial e consentimento de contribuições estão
na raiz. O texto inglês e a documentação técnica original do jogo mantêm seus
titulares; a licença do projeto não concede direitos sobre esses materiais.
