# Ledger

| QuestionId | Tema | Pergunta/decisão | Resposta atual | Fonte | Estado | Owner | Deadline | Impacto |
|---|---|---|---|---|---|---|---|---|
| Q-DEST-001 | Destino | Onde gerar o plano UniNFe? | C:\projetos\github\UniNFe | pedido 2026-09-10 | ANSWERED | DEV | UAB-000 | material |
| Q-SCOPE-001 | Escopo | Integrar somente status/autorização publicados? | sim; somente em homologação, sem transporte de produção, consulta de protocolo ou eventos | DEV 2026-09-15 | ANSWERED | DEV | UAB-001 | material alto |
| Q-FILE-001 | ERP | Quais sufixos/casing públicos usar? | autorização: `-nfeabi.xml`, `-ret-nfeabi.xml`, `-ret-nfeabi.err`, `-procNFeABI.xml`; Status: `-ped-sta.xml`, `-sta.xml`, `-sta.err`, roteado por `consStatServNFeABI` | DEV 2026-09-15 | ANSWERED | DEV | UAB-001 | material alto |
| Q-UI-001 | UI | Expor NFeABI como tipo de aplicativo/configuração? | sim: `NFeABI = 17`, seletores e configurações existentes, modo `Todos`, sem tela nova e seguindo NFGas | DEV 2026-09-15 | ANSWERED | DEV | UAB-001 | material |
| Q-RESP-001 | XML | Como tratar `infRespTec` ausente? | replicar NFGas: preservar quando presente; quando ausente, criar se qualquer campo configurado existir; se nenhum existir, deixar ausente, sem validação adicional no UniNFe | DEV 2026-09-15 | ANSWERED | DEV | UAB-001 | material |
| Q-PFX-001 | Governança | Confirmar prefixo UAB | manter `UAB` e IDs `UAB-000` a `UAB-006` sem renumeração | DEV 2026-09-15 | ANSWERED | DEV | UAB-001 | baixo |
| Q-BUILD-001 | Build | Qual configuração usar antes da publicação do NuGet NFeABI? | somente Debug com referência direta ao checkout irmão; Release fora do escopo atual | DEV 2026-09-14 | ANSWERED | DEV | UAB-001 | material |
