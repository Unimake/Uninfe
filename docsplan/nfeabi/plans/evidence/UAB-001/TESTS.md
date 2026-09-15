# UniNFe - NF-e ABI - Testes UAB-001

| Verificação | Resultado |
|---|---|
| Fonte normativa: 22 arquivos e snapshot agregado | PASS — `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8` |
| Schemas: 20 XSDs e snapshot agregado | PASS — `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40` |
| JSON dos sete manifests UAB | PASS |
| Linter PDCA no estado final `DELIVERED_FOR_REVIEW` | PASS — 7 etapas, prefixo UAB, 7 manifests |
| Referências antigas, raiz duplicada e caminho normativo escapado | PASS — zero ocorrência ativa indevida |
| Estado das etapas | PASS — UAB-000 APPROVED; UAB-001 DELIVERED_FOR_REVIEW; UAB-002 a UAB-006 PLANNED |
| Escopo Git | PASS — somente planejamento/governança; nenhum arquivo de produto |
| Build/testes do produto | N/A justificado — UAB-001 é exclusivamente de planejamento; etapas UAB-002+ usarão somente Debug |

| Revisão CHECK/P03 independente | PASS — sem achado material; catálogo de hashes e N/A de build conferidos |
| Build/testes do produto | N/A justificado — etapa exclusivamente de planejamento; Debug somente nas etapas futuras |
| Plan linter final | PASS — exit code 0 |
