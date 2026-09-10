# UniNFe - NF-e ABI - Etapa UAB-004: fluxo de consulta de status

> **Tipo:** EXECUTION
> **Dependências:** UAB-003 APPROVED
> **Ambientes:** ENV-UNI-DEBUG
> **Decisões necessárias:** DEC-002
> **Manifesto:** docsplan/plans/manifests/UAB-004.json
> **Orquestrador:** $uab-004-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

Arquivo de status é processado pela DLL, retorna arquivo ERP correto, registra diagnóstico passivo e é removido conforme padrão.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

TaskConsultaStatusNFeABI baseada em NFGas, configuração certificado/proxy/TLS, retorno/erro, dispose, diagnóstico e testes de contexto de arquivo.

## 5. Fora de escopo

Autorização, proc, consulta de protocolo, eventos e UI adicional.

## 6. Subtrees permitidos

Task/status NFeABI, roteamento mínimo, testes e exemplo sintético. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Telemetria passiva; StatusServico é a única sonda fiscal explícita permitida e deve respeitar cache existente.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-004-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-004-P02 | Do | BALANCED | executar somente o escopo autorizado | incremento da etapa |
| UAB-004-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-004-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Criar task. 2. Integrar switch. 3. Preservar retorno .err e cleanup. 4. Testar sucesso/falhas determinísticas.

## 10. Compatibilidade, migration e rollback

Desregistrar task/status mantendo contratos de arquivo da UAB-003.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | build NFe.Service + testes focados de status/contexto | ENV-UNI-DEBUG | 15 min | retorno/erro/cleanup corretos sem secret | docsplan/plans/evidence/UAB-004/evidence/validation.txt |

## 12. Testes

Sucesso mockado, certificado/config/transporte falhos, retorno vazio, diagnóstico sem XML e arquivo de entrada removido/retido conforme contrato.

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

Status integrado e comprovado; autorização não iniciada.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-004/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
