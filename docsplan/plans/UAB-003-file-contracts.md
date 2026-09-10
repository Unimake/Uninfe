# UniNFe - NF-e ABI - Etapa UAB-003: registro e contratos públicos de arquivo

> **Tipo:** EXECUTION
> **Dependências:** UAB-002 APPROVED
> **Ambientes:** ENV-UNI-DEBUG
> **Decisões necessárias:** DEC-003, DEC-004 e DEC-005
> **Manifesto:** docsplan/plans/manifests/UAB-003.json
> **Orquestrador:** $uab-003-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

UniNFe reconhece XML NFeABI e os contratos de entrada/retorno/erro aprovados sem enviá-lo.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

Servicos, TipoEnvio, ExtRetorno, TpcnResources/chave, namespace, tipo aplicativo quando aprovado, DefinirTipoServico/ValidarExtensao/GravaErroERP e csproj.

## 5. Fora de escopo

Envio SOAP, status task, procNFeABI e eventos/consulta.

## 6. Subtrees permitidos

Components/enums/extensões, detecção Processar, settings/UI somente se DEC-005 aprovar, e testes. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Sufixos e casing são API do ERP; testes devem congelar valores exatos.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-003-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-003-P02 | Do | BALANCED | executar somente o escopo autorizado | incremento da etapa |
| UAB-003-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-003-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Registrar enums/constants sem renumerar IDs persistidos. 2. Adicionar extensões aprovadas. 3. Detectar raízes publicadas. 4. Testar roteamento/erro sem transporte.

## 10. Compatibilidade, migration e rollback

Remover somente registros NFABI e preservar valores existentes.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | build projetos afetados + testes de detecção/extensões | ENV-UNI-DEBUG | 15 min | raízes roteadas e contratos literais congelados | docsplan/plans/evidence/UAB-003/evidence/validation.txt |

## 12. Testes

Positivos para NFeABI/consStatServNFeABI; negativos para raiz desconhecida, sufixo errado e case sensível.

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

Registro completo, sem chamadas remotas, compatibilidade preservada.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-003/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
