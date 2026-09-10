# UniNFe - NF-e ABI - Etapa UAB-000: base do planejamento

> **Tipo:** PLANNING_BASELINE
> **Dependências:** nenhuma
> **Ambientes:** ENV-PLAN
> **Decisões necessárias:** DEC-001, DEC-002 e DEC-003
> **Manifesto:** docsplan/plans/manifests/UAB-000.json
> **Orquestrador:** $uab-000-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

Gerar plano independente do UniNFe, dependente do handoff validado da DLL.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

Descoberta de fluxos NFGas/NFCom, arquivo ERP, etapas, riscos, manifests e linter.

## 5. Fora de escopo

Código funcional, exemplos, commit, push e publicação.

## 6. Subtrees permitidos

`docs/`, `.agents/` e seção DevPlanner do README/AGENTS. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Não duplicar XML/transporte já pertencente à Unimake.DFe.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-000-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-000-P02 | Do | DEEP | executar somente o escopo autorizado | incremento da etapa |
| UAB-000-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-000-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Inspecionar padrões. 2. Gerar pacote. 3. Validar e entregar.

## 10. Compatibilidade, migration e rollback

Reverter somente planejamento.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | `pwsh -NoProfile -File .agents/skills/plan-linter/scripts/test-plan.ps1 -RepositoryRoot .` | ENV-PLAN | 15 min | linter retorna 0 | docsplan/plans/evidence/UAB-000/evidence/validation.txt |

## 12. Testes

Somente linter; produto proibido.

## 13. Segurança, privacidade e observabilidade

- Transporte com certificado e proxy segue infraestrutura existente; nenhum secret entra em source, plano, log ou screenshot.
- Evidência externa deve ser sanitizada. Falhas distinguem rejeição fiscal de DNS, TLS, proxy, certificado e configuração.
- Testes online fiscais só ocorrem com autorização e ambiente preparado; nenhuma autorização real é repetida como sonda.

## 14. Critérios mensuráveis

| Critério | Gate | Como medir |
|---|---|---|
| Escopo | PASS | diff somente nos subtrees permitidos |
| Contrato XML/ERP | PASS | fixture representativa preserva estrutura, ordem, namespace e nomes |
| Regressão | PASS | build e testes focados verdes; limitações ambientais registradas |
| Fontes | PASS | hashes comparados e divergências analisadas antes de codificar |

## 15. Stop/Blocked

- Bloquear se a documentação externa mudar, se um contrato público depender de decisão aberta, se o endpoint necessário não estiver publicado ou se o ambiente obrigatório faltar.
- Não enfraquecer validação, certificado ou teste para obter verde. Não iniciar a sucessora.

## 16. Definition of Done

Plano entregue e UAB-001 não iniciada.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-000/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
