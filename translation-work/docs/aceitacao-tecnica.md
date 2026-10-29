# Aceitação técnica inicial

Verificação em 28/10/2026, Windows amd64, Python 3.14.8. A preparação foi feita
somente no repositório; instalação Steam e mod de referência permaneceram em
leitura. Os 48 recursos gerenciados instalados foram comparados por SHA-256 com
EN e continuam idênticos byte a byte.

## Resultado executado

```text
python -B -m unittest discover -s translation-work/tests -v
Ran 31 tests in 317.820s
OK
```

A suíte cobre:

- Leitura dos 48 arquivos reais, preservação byte a byte quando não há mudança
  e reconstrução com acentos de fixture em todos eles, somente em memória.
- XML de módulos, escolhas multilinha, entidades, nomes e ocorrências duplicadas;
  comparação de estrutura não traduzível antes e depois da reconstrução.
- Apply parcial, IDs/fingerprints/geração, placeholders, sobrescrita explícita,
  preservação de progresso, histórico de fontes alteradas e reordenação.
- Importação de instalação validada, procedência desconhecida, dry-run sem
  escrita, fonte ausente, caminhos inseguros e conflito concorrente.
- Build incompleto sem saída parcial, build completo em fixture, recibo obsoleto,
  flags de completude adulteradas, saída modificada e inventário do ZIP.
- Journal interrompido, recuperação, conflito, GC protegido e retenção; 500
  aplicações pequenas em 101 lotes independentes, sem copiar o conjunto inteiro.
- JSON autorado com CRLF e bytes canônicos LF do Git, adulteração real de JSON
  e recurso binário, recibo de build antes/depois de normalização.
- Doctor somente leitura e diagnósticos de runtime, dependência, arquitetura e
  checksum. Reconstrução de fixtures de banco e módulo sem dependência externa.
- Cliente HTTP real com transporte simulado: endpoints canônicos, primeira
  versão v1.0.0, patch v1.0.1, saída idêntica, tag conflitante, histórico inválido,
  commit obsoleto, permissões/404/transientes, draft interrompido, upload, digest,
  anexos inesperados, publicação e confirmação do único PTBR.zip.
- Consentimento afirmativo com identidade e escopo do PR, integridade dos avisos
  legais, ausência de autoria inventada e README permanente entre versões.

O teste de retenção também foi executado separadamente após o ajuste final de
proteção de backups: aprovado em 291.162s. O limiar do teste exige menos de
100.000 bytes de snapshots concluídos, com no máximo dez retidos.

## Checagens de integração

`doctor` e `check` passaram na árvore de trabalho e numa cópia nova extraída do
índice com `git checkout-index`, já com os filtros de `.gitattributes` aplicados.
Nenhum hash foi calculado dependendo do core.autocrlf da máquina. Os arquivos
nativos e referências originais usam bytes exatos; ferramentas/metadados próprios
usam representação LF canônica. `git diff --cached --check` passou; espaços dos
originais e do texto legal oficial são preservados, não corrigidos.

`update-sources --dry-run` passou tanto por descoberta Steam quanto com caminho
explícito, reportando 45.325 entradas inalteradas. `check` confirmou 45.325
pendências, zero traduções e 91 lotes. Não existe PTBR parcial nem recibo de build
concluído de produção. A geração aceita é:

```text
7fcb81e8963755c0dbbb1828b3298e9305b6c6ec8123df1eb13406d7c412fa32
```

O README foi renderizado pela API de Markdown do GitHub e inspecionado em browser
local, em tela larga e viewport de 390px. Banner e badges carregaram; os 14 links
externos retornaram HTTP 200. A prévia local usa estilo simples equivalente,
não uma promessa de identidade visual exata com todas as versões do GitHub.

O briefing usa Steam e os recursos ingleses reais. A referência contextual
identifica 69 eventos com 167 seletores nativos de gênero, distinguindo seleção
de personagens de identidade do falante. Não há decisão linguística aprovada
nem tradução automática aplicada.

## Verificações que continuam separadas

- **Ubuntu Server 24.04 LTS ARM64:** execução real pendente do primeiro doctor
  após clone/pull. A aprovação no Windows não é registrada como execução ARM64.
- **Runtime do jogo:** carregamento do mod, fontes, glifos, encaixe de interface,
  concordância dinâmica, saves e coop não foram testados.
- **GitHub Actions e publicação real:** o cliente e o fluxo foram exercitados
  offline com HTTP simulado; nenhuma Release real foi criada como teste. A
  automação só deve publicar após um build real completo e validado.

Depois do preflight no servidor, o responsável pela tradução deve ler o briefing
já escrito, estabelecer suas diretrizes/glossário e começar pelas pendências de
batch-0001.jsonl. Tradução, revisão, build, publicação e teste em jogo são estados
distintos e não devem ser inferidos uns dos outros.
