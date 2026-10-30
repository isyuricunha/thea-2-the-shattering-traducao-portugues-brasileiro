# Análise inicial dos glifos

As capturas mostram falhas em “Saudações”, “Configurações” e “Informações”, mas “Expedição” e textos de tutorial exibem acentos. O XML gerado contém a grafia correta.

Inspeção somente leitura de resources.assets encontrou Apex Lake Regular SDF (475), PRECIOSA SDF (476), Roycroft SDF9 (477), GentiumPlusCompact (480), variantes regionais e LiberationSans SDF (493).

O código TMP_FontAsset declara listas de glifos e fontes alternativas (fallbackFontAssets). A leitura genérica da árvore de tipos falha nesta versão; uma inspeção binária inicial identificou registros de 36 bytes pela sequência A/B/C. Nessa sequência, LiberationSans contém ç, Ç, ã, Ã, õ e Õ; as tabelas principais das fontes decorativas examinadas não contêm esses códigos. Isso é evidência preliminar, não um parser completo nem prova do encadeamento de fallback de cada componente da interface.

Próxima etapa: decodificar integralmente as tabelas e referências de fallback e mapear as fontes dos componentes afetados. Preferir completar os glifos da fonte original ou configurar uma alternativa compatível, preservando as palavras e o estilo. Nenhuma correção de fonte foi aplicada ao jogo ou incluída no pacote nesta análise. A validação visual final depende de execução dentro do jogo.
