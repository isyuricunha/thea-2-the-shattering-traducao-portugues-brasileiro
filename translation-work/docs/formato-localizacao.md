# Formato e decisões técnicas

## Evidência nativa

`reference/how_to.txt` é a documentação do jogo: DB substitui registros do banco;
STORY_MODULE substitui o módulo de mesmo nome; LOCALIZATION aplica exportações
TXT de um módulo. `reference/TestsModule.*` é o exemplo distribuído pelo jogo.

A inspeção local de `Assembly-CSharp.dll` confirmou:
`Thea2.EventEditor.EventEditorModules.ExportDialogForModule` escreve EVENT por
índice, NODE por ID e OUT por posição. `ApplyLocalization` percorre eventos na
ordem, procura o nó por ID e lê uma única linha após cada OUT. `NodeAdventure.story`
é XmlText. `ModManager` oferece os caminhos de substituição de módulos.
A inspeção usou ilspycmd 11.2.0.10050 somente como ferramenta de desenvolvimento;
nenhum assembly, decompilação ou executável entra na entrega pública.

Existem **24 opções com LF** no inglês. Portanto, a saída usa exclusivamente
**STORY_MODULE XML**, não o TXT do mod russo. A documentação nativa permite isso.
Preserva-se o módulo inteiro, alterando apenas story e outputs/name dos nós
NodeAdventure. A reconstrução compara recursivamente todos os demais atributos,
elementos, lógica e ordenação. XML de origem idêntico ao solicitado retorna os
mesmos bytes. Texto modificado produz XML UTF-8 com declaração e entidades.
Não há dependência do editor, hash do .NET ou reimportação manual.

## IDs e atualizações

- Bancos: caminho + chave nativa + ocorrência. Existem chaves duplicadas no
  original; cada ocorrência é preservada, sem corrigir a lógica do jogo.
- Nomes: caminho + coleção + categoria + nome inglês + ocorrência. O nome não
  possui ID próprio; mudança de grafia é removido + adicionado, sem reaproveitamento
  automático. A categoria CharacterMale/CharacterFemale é contexto do nome.
- Eventos: caminho + uniqueModuleID + uniqueID do evento + ID do nó + story ou
  índice de output. O destino do output e a lógica do nó são contexto imutável.
  Inserção/reordenação de opções pode invalidar entradas; nunca se casa só por inglês.

O campo row é localizador atual, não identidade. Event/node names, image e logic
ajudam na revisão; mudança de contexto invalida conservadoramente o PTBR. Dois
textos iguais com IDs diferentes continuam sendo duas decisões linguísticas.

## Escopo traduzível

Entram somente Entry/Val não vazios, CharacterMale/CharacterFemale/Value e
NodeAdventure story/outputs/name. Não entram nomes técnicos de módulos/eventos,
scripts, tags de raça, caminhos de imagem, IDs, condições, resultados, números,
coordenadas ou nós de lógica. Valores vazios são preservados como vazios nativos.
Um Entry de exemplo comentado é ignorado.

`DES_SUBRACE_TROLL_YOUNG_LAVA` aparece duas vezes com sentidos distintos no
original. A toolkit mantém ambas as ocorrências; não inventa chave `_DES`.
A precedência da chave duplicada dentro do runtime continua sendo a do jogo e
precisa ser observada em teste, sem prometer correção por tradução.

## Sintaxe e codificação

EN e accepted-sources preservam BOM, declaração e bytes. Módulos incluem declaração
windows-1250; a leitura respeita essa declaração. A saída modificada é UTF-8,
aceita pelo leitor XML e sem restrição do alfabeto CP1250. Acentos de PT-BR são
testados em fixture; disponibilidade de glifos na fonte Unity requer teste visual.

São protegidos por multiplicidade comandos entre chaves, tags de formatação TMP
conhecidas, cores, printf e escapes literais de newline. Reordenação de tokens é
permitida para gramática; alterar seu conteúdo é proibido. XML é novamente lido
para detectar caracteres inválidos. A verificação técnica não prova concordância,
voz, semântica nem encaixe na tela. Marcadores do antigo protocolo TXT são
recusados em início de linha por cautela contra migrações acidentais de formato.

`reference/UIScripts.cs` mostra construção dinâmica com UI_OF entre fragmentos,
nome de perfil, limites de alvo e variáveis globais. Não há tabela universal de
gênero: a pessoa que fala pode diferir do referente ou da pessoa abordada.
Informações de nome/categoria não autorizam assumir concordância para eventos.

`docs/contexto-genero.json` indexa os eventos com filtros nativos de gênero da
geração inicial, inclusive filtros em nós anteriores ao texto. A semântica da
seleção é verificável em `reference/EditorScripts.cs`: uma lista de personagens
selecionados não identifica automaticamente falante, destinatário ou referente.
Depois de mudar a geração, use o XML atual e revise essa referência antes de
tratá-la como contexto vigente.
