# Briefing de contexto — Thea 2: The Shattering

Pesquisa preparada durante o desenvolvimento da toolkit, em 28/10/2026. Este
documento oferece contexto factual; não é um glossário de traduções aprovadas.
O responsável pela localização deve registrar as escolhas linguísticas nos
documentos próprios e acrescentar evidências quando descobrir contexto novo.

## Identidade e fontes

Thea 2: The Shattering é a continuação de Thea: The Awakening. A página oficial
na Steam identifica MuHa Games e Eerie Forest Studio como desenvolvedoras e MuHa
Games como publicadora; informa lançamento em 13 de maio de 2019. O jogo combina
estratégia por turnos, exploração, sobrevivência, RPG e desafios com cartas em um
cenário inspirado no folclore eslavo. [Steam, jogo base](https://store.steampowered.com/app/606230/).

Fontes usadas, por prioridade:

1. Os próprios XML ingleses em `EN/Thea2_Data/StreamingAssets/DataBase/` e
   `EN/Thea2_Data/StreamingAssets/Modules/`: a autoridade para nomes, falas e
   contexto desta versão. O catálogo registra cada caminho e fingerprint.
2. A página oficial do jogo acima: identidade, proposta e enquadramento comercial.
3. [The Awakening](https://store.steampowered.com/app/1212890/): natureza da
   atualização e distinção entre conteúdo incorporado ao jogo e pacote de artes.
4. [Wrath of the Sea](https://store.steampowered.com/app/1240020/): contexto dos
   encontros marítimos, expansão de sistemas e retorno de Mokosh.
5. Documentação nativa `reference/how_to.txt`, exemplo de módulo e
   `reference/UIScripts.cs`: mecanismo de mods e composição dinâmica da interface.

A instalação de referência é Steam build 20265356 / versão 2.0528.0681. O mod
russo 1824477540 ajudou a identificar a organização de um pacote, mas não foi
usado como fonte de significado, lore ou texto para tradução. Não houve teste
de gameplay nesta preparação.

## Premissa, mundo e estrutura narrativa

O jogador atua como uma divindade que orienta um grupo de seguidores. As decisões
de sobrevivência e os encontros formam o eixo da experiência. A exposição não é
apenas uma campanha linear: módulos independentes contêm aventuras, condições,
opções, desafios e consequências. A mesma viagem pode ter encontros de culturas,
ambientes e registros diferentes. Isso pede continuidade terminológica sem
uniformizar todas as vozes.

O inglês local contém nomes de biomas e referências a ruínas, cavernas, aldeias,
florestas, regiões geladas e mar. Esses ambientes são evidência para o tom e para
objetos descritos, não uma cronologia histórica completa do mundo. O jogo mistura
personagens mortais e seres sobrenaturais; narradores tratam o grupo em segunda
pessoa, e a mesma área de story pode incluir falas diretas de outras pessoas.

Uma escolha de saída pode indicar uma classe ou condição entre colchetes, trazer
uma ação ou pular uma parte da história. Não confunda essas alternativas com
fala de um personagem fixo. No módulo KS Events, por exemplo, o evento Glass King
inclui exploração de uma cidade congelada e saídas condicionais a Zerca. O nome
do evento e os filtros da lógica ajudam a localizar o contexto, mas não devem
ser traduzidos dentro dos identificadores técnicos.

## Panteão e vozes verificáveis

As chaves `UI_<NOME>` e `UI_<NOME>_DES` no banco de interface contêm apresentações
em primeira pessoa. As descrições são a evidência de personalidade e domínio;
não se deve substituir a ficção do jogo por uma descrição de mitologia externa.

| Nome de origem | Evidência e contexto no inglês local |
| --- | --- |
| Dzievanna | `UI_DZIEVANNA_DES`: senhora da floresta e caçadora; proteção da natureza acompanhada de ameaça contra quem a viola |
| Horz | `UI_HORZ_DES`: domínio da noite e da lua, controle de criaturas e equilíbrio com a luz; voz solitária e imperativa |
| Lada | `UI_LADA_DES`: esperança, liderança do panteão e harmonia sem passividade; contrapõe beleza e força |
| Marovit | `UI_MAROVIT_DES`: conhecimento, sabedoria e outros mundos; voz reflexiva, com metáforas de correntes e oceanos |
| Stribog | `UI_STRIBOG_DES`: jornada, ventos e liberdade; apresentação como guia dos viajantes |
| Svarog | `UI_SVAROG_DES`: fogo celeste e doméstico, ferramentas, proteção e resistência; discurso de dever |
| Triglav | `UI_TRIGLAV_DES`: lava, ira, pluralidade de corpos/mentes e ordens de combate; alternância singular/plural tem valor contextual |
| Zorya | `UI_ZORYA_DES`: estrelas e auroras, linguagem bélica e alternância entre eu/nós; não eliminar essa pluralidade por conveniência |
| Mokosh | `UI_MOKOSH_DES`: maternidade, natureza, cuidado e ira; retorno relacionado ao chamado do mar |
| Nyia | `UI_NYIA_DES`: morte, passagem das almas pelas terras dos mortos (navyas), solidão e sofrimento; voz amarga e provocadora, com autoidentificação feminina explícita |

“Mistress”, “huntress” e a linguagem maternal de Mokosh fornecem pistas explícitas
para essas autoapresentações. Isso não estabelece o gênero de uma pessoa descrita
em outra frase. Termos como “god” ou “master” não substituem uma análise contextual
de pronomes e referente. Relações de parentesco mencionadas nas apresentações
devem permanecer ligadas à passagem que as afirma, sem inventar uma genealogia.

## Culturas, facções e seres

O banco de descrições e os módulos oferecem ligações concretas para pesquisa:
Slavyan, Scavengers, Woodlandkin, Earthbound, Alphaclan, SpiritTalkers e
Lightbringers possuem módulos próprios. Forestdemons, Nightdemons, Water demons,
Icedemons, Stingers e Shadowkin também aparecem em nomes de módulos. Esses nomes
servem como ponto de entrada, não como equivalências PT-BR já aprovadas.

Procure as famílias de chaves `DES_FACTIONS_*` e `DES_SUBRACE_*` para distinguir
facção, raça, sub-raça e classe. Um adjetivo cultural, um nome coletivo e a
denominação de um indivíduo podem exigir tratamentos diferentes. “Kin”, “clan”,
“spirit” e “demon” têm funções recorrentes, mas não devem receber substituição
global automática. Algumas descrições são curtas ou genéricas: contexto ausente
deve ser registrado como desconhecido, não completado com inferência de lore.

`DATABASE_DES_NAMES.xml` separa coleções e categorias CharacterMale/CharacterFemale.
Esse dado orienta nomes gerados e pode ajudar a analisar um personagem encontrado
no jogo; não resolve de antemão a identidade de quem fala em cada story. Raça e
sub-raça são referências técnicas preservadas, não campos livres de tradução.

## Sistemas e terminologia de interface

O banco de descrições define atributos em três domínios. Wits afeta a rapidez de
ação; Strength e Perception se ligam ao domínio físico; Intelligence e Wisdom ao
mental; Mysticism e Destiny ao espiritual. A leitura cruzada das chaves
`DES_TAG_*` é necessária antes de fixar formas PT-BR.

Health, Sanity e Faith não são sinônimos intercambiáveis de pontos de vida.
As descrições explicam recuperação, riscos e funções diferentes. Shielding e suas
variações por domínio também devem ser distinguidos dos atributos e da vida.
Uma escolha feita no glossário precisa funcionar nas fichas, equipamentos,
desafios e descrições longas, sem conflitar com a função efetiva de cada termo.

Gathering, Crafting, Research, Luck e Rituals aparecem como conceitos de sistema,
às vezes próximos de usos comuns da mesma palavra. Compare rótulo, descrição e
construções de frases antes de decidir capitalização e forma nominal/verbal.
Nomes de classes como Zerca e Inventor pedem leitura das descrições e das opções
que dependem de classe; não são nomes genéricos de profissão por definição.

O código de interface fornecido pelo jogo constrói fragmentos com UI_OF, números
de alvos e intervalos, além de nomes de perfil e variáveis globais. O resultado
final pode ter concordância e preposição que não aparecem em uma única entrada.
Uma tradução tecnicamente válida de um fragmento pode falhar quando concatenada.
Essas situações devem ir para revisão contextual e teste em jogo.

## Conteúdo das atualizações

The Awakening reaproveita aventuras do primeiro jogo e acrescenta elementos ao
sistema, incluindo Inventor. A página Steam do DLC explica que o download separado
é de artes; o conteúdo do jogo foi incorporado à atualização base. A revisão deve
considerar callbacks a personagens e terminologia já usada nos demais módulos,
sem presumir que exista uma tradução anterior autorizada a reaproveitar.

Wrath of the Sea amplia o conteúdo marítimo, com encontros, personagens e recursos,
e relaciona o retorno de Mokosh à nova trama. O módulo correspondente é incluído
no catálogo porque existe na instalação atual. O mesmo vale para Magic DLC, que
não estava registrado no mod russo observado. A existência do arquivo não prova
que cada ramo foi visto em gameplay; revisão deve respeitar as condições do XML.

## Tom, humor e pontos de revisão

Os textos misturam fantasia sombria, humor, ameaças, vozes cerimoniais e linguagem
cotidiana. Um exemplo local é a descrição dos jovens trolls de lava, que aproxima
o temperamento de um adolescente de imagens de calor e explosões. Preserve a
função do humor sem “corrigir” a mitologia ou tornar toda voz solene. Trechos
arcaizantes e inscrições podem diferir da narração de uma escolha cotidiana.

Erros ou ambiguidades do inglês existem. A chave duplicada de troll de lava é
um problema estrutural documentado; não é autorização para alterar IDs. Uma
descrição estranha deve ser comparada com o referente e os termos próximos antes
de produzir uma interpretação definitiva. Conteúdo adulto ou violento deve manter
o sentido e o registro da cena; revisão não deve acrescentar detalhes ausentes.

Para cada caso difícil, distinga:

- **Quem fala:** narrador, personagem citado, divindade ou origem desconhecida.
- **Quem é abordado:** indivíduo, grupo, jogador ou seleção variável.
- **Quem é descrito:** pode ser uma terceira pessoa com concordância própria.
- **O que muda em runtime:** perfil, seleção de personagens, item, classe ou quantidade.

O contexto dos registros inclui evento, nó, imagem, destino e lógica. Use os XML
para ler saídas vizinhas e condições. Não há tabela universal de speakers nem
gênero fixo confiável para todos os eventos; o campo de evidência sinaliza essa
limitação. O trabalho linguístico deve registrar dúvidas e voltar a elas no jogo.

### Seletores de gênero encontrados na lógica

A busca por Speaker_, speaker e gender não encontrou uma tabela universal de
falantes, mas encontrou **filtros nativos explícitos** `TAG-GENDER_MALE`,
`TAG-GENDER_FEMALE` e descrições `DES_TAG_GENDER_*`, inclusive OTHER para personagens
não humanoides. O índice [contexto-genero.json](contexto-genero.json) vincula os
eventos e seus filtros aos prefixos dos IDs de tradução, com fonte e certeza.
Ele é uma referência da geração inicial identificada no arquivo; após atualização,
confirme as informações nos XML atuais. A lógica do próprio nó também permanece
no contexto imutável dos registros JSONL.

`reference/EditorScripts.cs`, método FwT_AllPlayersAllGroupsCharacters, confirma
que o filtro reúne personagens dos grupos disponíveis e seleciona por tag, raça
ou sub-raça. FLP_AandB calcula a interseção de listas. Portanto, uma tag feminina
em uma condição demonstra **seleção de personagens**, não que toda a narração
tenha uma mulher como falante ou destinatária.

Exemplo verificável: `Children.xml`, evento StorkHum, uniqueID 1, nó inicial 1,
combina listas male/female com human e exclui child; o nó 2 seleciona integrantes
dessas listas. A narração trata duas pessoas e a chegada de uma criança. Isso
oferece evidência contextual específica para os referentes desse evento, sem
transformar uma condição de população em identidade fixa do narrador. Consulte
também os nós anteriores quando o texto não contiver o filtro diretamente.

## Limites desta pesquisa

Este briefing não reconstrói uma cronologia completa nem revela os desfechos da
campanha. Não valida saves, coop, fontes ou ordem efetiva de mods em execução.
A equivalência com outras edições depende de comparação dos recursos. O registro
de fatos verificados, nomes e ambiguidades já permite começar a revisão manual,
mas não substitui acompanhar uma cadeia de eventos quando a cena exigir.
