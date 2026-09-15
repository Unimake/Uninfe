# Matriz de decisões — UAB-001

| Decisão | Contrato aprovado pelo DEV | Propagação | Estado |
|---|---|---|---|
| DEC-001 | Consumir o handoff ABI-006 aprovado da DLL irmã | UAB-002 | DECIDED |
| DEC-002 | Transporte somente para Status e Autorização, apenas em homologação | UAB-004 a UAB-006 | DECIDED |
| DEC-003 | Autorização: `-nfeabi.xml`, `-ret-nfeabi.xml`, `-ret-nfeabi.err`, `-procNFeABI.xml`; Status: contrato genérico e raiz `consStatServNFeABI` | UAB-003 a UAB-006 | DECIDED |
| DEC-004 | Prefixo `UAB`; IDs UAB-000 a UAB-006 sem renumeração | plano completo | DECIDED |
| DEC-005 | `TipoAplicativo.NFeABI = 17`; “NF-e ABI” nos seletores/configurações existentes e em `Todos`, seguindo NFGas, sem tela nova | UAB-003 | DECIDED |
| DEC-006 | `infRespTec` preservado quando presente; quando ausente, criar se qualquer campo estiver configurado e copiar os disponíveis; manter ausente se todos estiverem vazios; sem validação adicional | UAB-005 | DECIDED |
| DEC-007 | Todo build e teste de produto somente em Debug com ProjectReference para a DLL irmã | UAB-002 a UAB-006 | DECIDED |

Nenhum contrato ERP, nome de arquivo ou comportamento operacional adicional foi inferido.
