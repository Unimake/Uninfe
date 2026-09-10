# UniNFe - NF-e ABI - Testes UAB-000

| Fase | Comando | Resultado |
|---|---|---|
| Plano | `pwsh -NoProfile -File .agents/skills/plan-linter/scripts/test-plan.ps1 -RepositoryRoot .` | PASS |
| Referências | varredura por raízes antigas fora do snapshot histórico attempt-0001 | PASS |
| Isolamento | verificação de ausência dos sete caminhos migrados em `docs/` e presença em `docsplan/` | PASS |
| Higiene | varredura de caracteres de controle, interpolações inválidas e raízes duplicadas | PASS |

Testes de produto não executados porque a etapa 000 permite somente planejamento.
