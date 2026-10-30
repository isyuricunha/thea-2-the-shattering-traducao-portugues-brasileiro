# Análise inicial dos glifos

As capturas mostram falhas em “Saudações”, “Configurações” e “Informações”, mas “Expedição” e textos de tutorial exibem acentos. O XML gerado contém a grafia correta.

Inspeção somente leitura de resources.assets encontrou Apex Lake Regular SDF (475), PRECIOSA SDF (476), Roycroft SDF9 (477), GentiumPlusCompact (480), variantes regionais e LiberationSans SDF (493).

O código TMP_FontAsset declara listas de glifos e fontes alternativas (fallbackFontAssets). A leitura genérica da árvore de tipos falha nesta versão; uma inspeção binária inicial identificou registros de 36 bytes pela sequência A/B/C. Nessa sequência, LiberationSans contém ç, Ç, ã, Ã, õ e Õ; as tabelas principais das fontes decorativas examinadas não contêm esses códigos. Isso é evidência preliminar, não um parser completo nem prova do encadeamento de fallback de cada componente da interface.

Próxima etapa: decodificar integralmente as tabelas e referências de fallback e mapear as fontes dos componentes afetados. Preferir completar os glifos da fonte original ou configurar uma alternativa compatível, preservando as palavras e o estilo. Nenhuma correção de fonte foi aplicada ao jogo ou incluída no pacote nesta análise. A validação visual final depende de execução dentro do jogo.

## Tabelas e cadeia de fallback verificadas

A leitura binária da lista de glifos foi aprofundada usando o layout declarado pelas classes TMP_FontAsset, TMP_Asset, FaceInfo e KerningPair do assembly desta instalação. Os registros de glifos têm 36 bytes; contagem, unicidade e limites dos códigos foram conferidos. As listas de fallback foram lidas após as tabelas de kerning. O inventário encontra-se em fontes-glifos.json. A árvore genérica do UnityPy continua incompatível; não se deve usá-la para regravar estes objetos sem um adaptador validado.

- GentiumPlusCompact: 99 glifos básicos; usa onze alternativas regionais. As variantes FR e TR têm ç/Ç; Mix tem ã. Nenhuma das onze alternativas examinadas fornece õ/Õ ou Ã. As variantes CN/JP/KR foram conferidas separadamente.
- GentiumPlusCompact180, em sharedassets2.assets: 98 glifos; não contém diretamente ç/Ç/ã/Ã/õ/Õ. Sua cadeia de fallback específica ainda precisa ser mapeada.
- Apex Lake Regular SDF, PRECIOSA SDF e Roycroft SDF9: respectivamente 66, 53 e 28 glifos; nenhum dos seis caracteres examinados, e nenhuma fonte alternativa local configurada.
- LiberationSans SDF: 250 glifos; inclui os seis caracteres examinados.

Isso explica por que ç e ã podem aparecer enquanto õ desaparece nos textos Gentium. Não prova ainda qual fonte cada componente das capturas utiliza: falta ligar as referências serializadas dos componentes à fonte, além de examinar o fallback global e o código de busca do TMP antes de definir a intervenção.

A correção preferível é completar a cobertura portuguesa mantendo o visual Gentium, ou registrar uma alternativa já existente que tenha os glifos. Usar Liberation Sans apenas como fallback pode misturar estilos; isso precisa ser avaliado visualmente. Não é necessário remover acentos nem modificar as traduções. Esta etapa permanece somente análise: nenhum asset original foi alterado e nenhum patch de fonte foi produzido.
