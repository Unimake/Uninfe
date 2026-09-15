# Riscos

| ID | Risco | Prob. | Impacto | Indicador | Mitigação | Contingência | Owner | Deadline/etapa | Verificação | Estado |
|---|---|---|---|---|---|---|---|---|---|---|
| RISK-001 | DLL entregue diverge do plano/checkout | média | alto | API/hash não coincide | gate UAB-002 concluído com árvore limpa: commit aprovado, ProjectReference Debug, API mínima e hash do binário conferidos; revalidar mudanças externas antes de nova compilação | bloquear integração e corrigir DLL | DEV | toda etapa 003+ | dossiê ABI-006, smoke test 6/6 e conferência pré-build | MITIGATED |
| RISK-002 | contrato público de arquivo escolhido incorretamente | média | alto | ERP não detecta retorno | contratos literais aprovados pelo DEV na UAB-001 e testes literais previstos | replanejar antes UAB-003 | DEV | UAB-003 | tests de extensão | MITIGATION_DEFINED |
| RISK-003 | perda/duplicação ao gerar proc e mover arquivos | média | crítico | falha entre operações | ordem proc antes do original e fault injection | manter EmProcessamento e retorno .err | executor | UAB-005 | testes de contexto | MITIGATION_DEFINED |
| RISK-004 | documentação oficial muda/serviço produção surge | média | alto | hash/portal diverge | reler toda etapa e fail-closed | replanejamento aprovado | executor/DEV | toda etapa 002+ | catálogo/hash | PRESENT |
