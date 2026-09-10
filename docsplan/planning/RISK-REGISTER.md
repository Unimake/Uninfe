# Riscos

| ID | Risco | Prob. | Impacto | Indicador | Mitigação | Contingência | Owner | Deadline/etapa | Verificação | Estado |
|---|---|---|---|---|---|---|---|---|---|---|
| RISK-001 | DLL entregue diverge do plano/checkout | média | alto | API/hash não coincide | gate UAB-002 | bloquear integração e corrigir DLL | DEV | UAB-002 | dossiê ABI-006 | PRESENT |
| RISK-002 | contrato público de arquivo escolhido incorretamente | média | alto | ERP não detecta retorno | aprovação na 001 e testes literais | replanejar antes UAB-003 | DEV | UAB-001 | tests de extensão | DECISION_PENDING |
| RISK-003 | perda/duplicação ao gerar proc e mover arquivos | média | crítico | falha entre operações | ordem proc antes do original e fault injection | manter EmProcessamento e retorno .err | executor | UAB-005 | testes de contexto | MITIGATION_DEFINED |
| RISK-004 | documentação oficial muda/serviço produção surge | média | alto | hash/portal diverge | reler toda etapa e fail-closed | replanejamento aprovado | executor/DEV | toda etapa 002+ | catálogo/hash | PRESENT |
