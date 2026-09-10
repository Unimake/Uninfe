# UniNFe - NF-e ABI - Etapa UAB-001: revisão de contratos ERP e congelamento

> **Tipo:** PLAN_REVIEW
> **Dependências:** UAB-000 APPROVED
> **Ambientes:** ENV-PLAN
> **Decisões necessárias:** todas as propostas do DEV
> **Manifesto:** docsplan/plans/manifests/UAB-001.json
> **Orquestrador:** $uab-001-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

Fechar nomes/extensões públicas, escopo de UI e comportamento do responsável técnico antes do código.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

Rodadas com DEV, propagação, readiness do handoff ABI e congelamento.

## 5. Fora de escopo

Source, exemplos, binários ou DLL.

## 6. Subtrees permitidos

`docs/`, `.agents/` e seção DevPlanner do README/AGENTS. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Nenhum contrato ERP é selecionado pelo agente sem aprovação.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-001-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-001-P02 | Do | DEEP | executar somente o escopo autorizado | incremento da etapa |
| UAB-001-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-001-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Confirmar prefixo. 2. Aprovar sufixos e casing. 3. Aprovar status/autorização apenas. 4. Decidir seletor/UI e gRespTec. 5. Propagar/lint.

## 10. Compatibilidade, migration e rollback

Restaurar plano 0.1.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | linter + revisão independente | ENV-PLAN | 15 min | zero decisão material aberta e UAB-002 pronta | docsplan/plans/evidence/UAB-001/evidence/validation.txt |

## 12. Testes

Verificar rastreabilidade, dependências e contratos.

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

Plano só fica READY_FOR_EXECUTION após aprovação humana.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-001/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
