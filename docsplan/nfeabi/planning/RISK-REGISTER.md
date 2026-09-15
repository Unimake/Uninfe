# Riscos

| ID | Risco | Prob. | Impacto | Indicador | Mitigação | Contingência | Owner | Deadline/etapa | Verificação | Estado |
|---|---|---|---|---|---|---|---|---|---|---|
| RISK-001 | DLL entregue diverge do plano/checkout | média | alto | API/hash não coincide | gate UAB-002 | bloquear integração e corrigir DLL | DEV | UAB-002 | dossiê ABI-006 | PRESENT |
| RISK-002 | contrato público de arquivo escolhido incorretamente | média | alto | ERP não detecta retorno | contratos literais aprovados pelo DEV na UAB-001 e testes literais previstos | replanejar antes UAB-003 | DEV | UAB-003 | tests de extensão | MITIGATION_DEFINED |
| RISK-003 | perda/duplicação ao gerar proc e mover arquivos | média | crítico | falha entre operações | ordem proc antes do original e fault injection | manter EmProcessamento e retorno .err | executor | UAB-005 | testes de contexto | MITIGATION_DEFINED |
| RISK-004 | documentação oficial muda/serviço produção surge | média | alto | hash/portal diverge | reler toda etapa e fail-closed | replanejamento aprovado | executor/DEV | toda etapa 002+ | catálogo/hash | PRESENT |
