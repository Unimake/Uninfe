# UniNFe - NF-e ABI - Etapa UAB-006: validação integrada e documentação operacional

> **Tipo:** EXECUTION
> **Dependências:** UAB-005 APPROVED
> **Ambientes:** ENV-INTEGRATED
> **Decisões necessárias:** DEC-002
> **Manifesto:** docsplan/plans/manifests/UAB-006.json
> **Orquestrador:** $uab-006-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

Entregar integração UniNFe pronta para revisão/release posterior, com documentação ERP e gates conjuntos.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

Testes focados UniNFe + DLL, exemplos sintéticos, docs de integração/serviço, índices do viewer e auditoria de diff/contratos.

## 5. Fora de escopo

Instalador, publish, commit/push, produção, consulta/eventos.

## 6. Subtrees permitidos

Correções NFABI estritas, testes, exemplos e documentação/indexes. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Atualizar docs/index, catálogo e manifestos do viewer ao criar/renomear páginas, conforme AGENTS.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-006-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-006-P02 | Do | BALANCED | executar somente o escopo autorizado | incremento da etapa |
| UAB-006-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-006-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Executar builds/testes conjuntos. 2. Revisar arquivos ERP e recuperação. 3. Documentar configuração/uso/limites. 4. Regenerar índices. 5. Entregar dossiê go/no-go sem publicar.

## 10. Compatibilidade, migration e rollback

Reverter somente docs/correções finais; nenhuma publicação.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | build uninfe.sln quando ambiente permitir + UniNFe.Test Debug focado + testes DLL NFeABI | ENV-INTEGRATED | 15 min | gates determinísticos verdes e docs/index consistentes | docsplan/plans/evidence/UAB-006/evidence/validation.txt |

## 12. Testes

Status/autorização end-to-end com doubles, negativos de arquivo/ambiente, regression de DFe vizinhos impactados e linter do plano.

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

Integração documentada, dossiê completo, limitações online explícitas e nenhuma release iniciada.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-006/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
