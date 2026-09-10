# Decisões

| ID | Decisão | Estado | Autoridade | Fonte/data | Necessária antes | Alternativas/recomendação | Impacto/fallback |
|---|---|---|---|---|---|---|---|
| DEC-001 | UniNFe depende do handoff ABI-006 da DLL | DECIDED | DEV | pedido 2026-09-10 | UAB-002 | executar planos em sequência | bloqueia se DLL não estiver pronta |
| DEC-002 | Integrar remotamente só status/autorização; produção ausente | PROPOSED | DEV | portal/pedido 2026-09-10 | UAB-004 | recomendado: somente publicados | futuros serviços exigem replanejamento |
| DEC-003 | Sufixos/casing ERP NFABI | PROPOSED | DEV | padrões NFGas/NFCom | UAB-003 | recomendar -nfeabi.xml, -ret-nfeabi.xml, -procNFeABI.xml e erro correspondente | contrato público; sem decisão bloqueia |
| DEC-004 | Prefixo UAB | PROPOSED | DEV | planejador 2026-09-10 | UAB-001 | UAB recomendado | renomeável uma vez na 001 |
| DEC-005 | Incluir NFeABI nos seletores/tipo aplicativo existentes, sem tela nova | PROPOSED | DEV | padrão UniNFe | UAB-003 | recomendado: sim | se não, integração apenas no modo Todos |
| DEC-006 | Injetar gRespTec configurado quando ausente | PROPOSED | DEV | XSD/padrão NFGas | UAB-005 | recomendado: sim | sem decisão, preservar XML sem injeção |
