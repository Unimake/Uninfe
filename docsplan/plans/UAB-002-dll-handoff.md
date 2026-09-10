# UniNFe - NF-e ABI - Etapa UAB-002: gate do handoff da DLL e baseline

> **Tipo:** EXECUTION
> **Dependências:** UAB-001 APPROVED
> **Ambientes:** ENV-UNI-DEBUG
> **Decisões necessárias:** DEC-001
> **Manifesto:** docsplan/plans/manifests/UAB-002.json
> **Orquestrador:** $uab-002-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

Provar que o UniNFe Debug consome a DLL irmã com NFeABI e que o baseline está verde antes da integração.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

Validar commit/hash e dossiê ABI-006, ProjectReference Debug/Beta, APIs mínimas e build/testes base focados.

## 5. Fora de escopo

Registro de serviço, tarefas, extensões e release.

## 6. Subtrees permitidos

Testes de contrato consumidor e docs/evidence; correção de referência apenas se estritamente necessária e aprovada. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Release continua NuGet; não alterar estratégia de referência.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-002-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-002-P02 | Do | BALANCED | executar somente o escopo autorizado | incremento da etapa |
| UAB-002-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-002-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Ler handoff ABI-006. 2. Conferir APIs NFeABI. 3. Build/teste Debug focado. 4. Registrar mapa de integração.

## 10. Compatibilidade, migration e rollback

Remover apenas testes/checkpoints novos.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | `dotnet test source/UniNFe.Test/UniNFe.Test.csproj -c Debug --no-restore` com filtro focado | ENV-UNI-DEBUG | 15 min | consumer compila e baseline passa | docsplan/plans/evidence/UAB-002/evidence/validation.txt |

## 12. Testes

Smoke de referência/tipos sem rede/certificado.

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

Handoff aceito por hash, baseline verde e nenhuma tarefa NFABI criada.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-002/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
