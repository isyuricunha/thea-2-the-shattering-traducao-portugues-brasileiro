# Fluxo de revisão manual

1. Faça pull e rode `python3 -B translation-work/toolkit.py doctor` no Ubuntu ARM64.
   Se falhar, preserve a mensagem e interrompa a tradução.
2. Leia o briefing já pesquisado e os exemplos técnicos. Estabeleça o glossário
   e as diretrizes sem alterar os campos de origem dos lotes.
3. Rode `status`/`resume`; leia `reports/sync-latest.md` e o JSON para mudanças.
4. Escolha entradas pendentes e consulte outras linhas do mesmo evento, chaves
   relacionadas e os XML originais. Diferencie narrador, fala citada e referente.
5. Traduza manualmente em grupos revisáveis. Aplique somente PTBR com geração,
   IDs e fingerprints. Verifique a saída do apply; corrija somente as rejeitadas.
6. Rode `check`. Salve progresso em commits; uma linha preenchida exige revisão
   linguística antes de ser considerada aceita. Esta versão usa PTBR não vazio
   como aprovação explícita do responsável: não há segundo marcador automático.
7. Após atualizações, traduza somente entradas novas/invalidadas e use history
   como contexto. Nunca restaure traduções antigas sem conferir o novo inglês.
8. Com todas as entradas revisadas, rode build, verify-output e package. Revise
   o ZIP, salve o build completo e seu recibo no mesmo commit. O push pode acionar
   a publicação automática; progresso parcial não cria saída instalável.
9. Valide apresentação, glifos, concordância dinâmica e carregamento de eventos
   no jogo. Registre versão e alcance real do teste; não extrapole para outros
   saves, mods, plataformas ou versões posteriores.

## Concordância e contexto

Tags como CharacterFemale indicam a categoria dos nomes gerados, não o gênero de
quem diz toda frase próxima. O protagonista e o grupo variam. Um texto em segunda
pessoa pode tratar o grupo, e diálogos citados dentro de story podem alternar voz.
Consulte identificadores, nomes de eventos, lógica, descrições e pronomes. Registre
incertezas nas diretrizes; não codifique uma suposição em IDs ou condições do jogo.

## Retomada

Uma aplicação interrompida pode ter journal pendente. Rode recover e status antes
de recomeçar. Reenvio idêntico é seguro. Se houver edição concorrente, a ferramenta
recusa sobrescrevê-la: guarde uma cópia e peça revisão técnica. Para desfazer um
apply concluído sem trabalho posterior, use o ID da transação retida com rollback.
O histórico Git é a proteção durável depois da retenção local.
