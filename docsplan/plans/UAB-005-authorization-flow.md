# UniNFe - NF-e ABI - Etapa UAB-005: autorização síncrona e persistência

> **Tipo:** EXECUTION
> **Dependências:** UAB-004 APPROVED
> **Ambientes:** ENV-UNI-DEBUG
> **Decisões necessárias:** DEC-002, DEC-003 e DEC-006
> **Manifesto:** docsplan/plans/manifests/UAB-005.json
> **Orquestrador:** $uab-005-orchestrator
> **Regra:** execute somente esta etapa e pare após dossiê/PDCA.

## 1. Objetivo e valor executável

NFeABI é assinada/enviada sincronicamente, gera procNFeABI antes de mover o original e devolve retorno/erro ERP compatível.

## 2. Definition of Ready

- Predecessora aprovada e manifesto coerente com o PDCA.
- Árvore de trabalho inspecionada; alterações preexistentes preservadas.
- Ler integralmente os dois MOCs e os XSDs aplicáveis em $SourceRoot; recalcular SHA-256 e comparar com docsplan/architecture/INTEGRATION-CATALOG.md.
- Decisões listadas fechadas ou contingência explicitamente autorizada.

## 3. Skills e instructions

- Ler .agents/instructions/pdca-execution.instructions.md, .agents/instructions/model-routing.instructions.md e .agents/instructions/nfabi-execution.instructions.md.
- Usar o orquestrador da etapa, o plan-linter no encerramento e as instruções AGENTS.md do subtree tocado.

## 4. Escopo

TaskNFeABIRecepcaoSinc, injeção gRespTec aprovada, XML assinado em processamento, Result/cStat, procNFeABI, fluxo/cleanup, rejeição e diagnóstico.

## 5. Fora de escopo

Consulta/eventos remotos, URL de produção, instalador e publicação.

## 6. Subtrees permitidos

Task autorização, leitura/geração/processamento NFeABI, testes e exemplos sintéticos. O dossiê, o PDCA e as evidências desta etapa ficam restritos a `docsplan/`.

## 7. Contratos, invariantes e arquitetura

- Namespace oficial http://www.portalfiscal.inf.br/nfeabi, versão de schema 1.00, modelo fiscal 77 e processamento síncrono.
- Preservar compatibilidade binária, contratos públicos de arquivo e padrões do repositório; não modernizar stack nem introduzir dependência.
- Não cadastrar endpoint de produção enquanto a autoridade fiscal não o publicar. Nunca copiar senha, certificado ou XML fiscal real para logs/evidence.
- Mover primeiro proc autorizado e depois original; falha intermediária deve ser recuperável e não perder documento.

## 8. Partes PDCA e roteamento

| Parte | Fase | Perfil | Responsabilidade | Saída |
|---|---|---|---|---|
| UAB-005-P01 | Plan | DEEP | reler fontes, inventariar call sites e fechar readiness | checkpoint e matriz de impacto |
| UAB-005-P02 | Do | BALANCED | executar somente o escopo autorizado | incremento da etapa |
| UAB-005-P03 | Check | INDEPENDENT_REVIEW | testes, diff, contratos e revisão independente quando crítica | relatório de gate |
| UAB-005-P04 | Act | ECONOMY | dossiê, hashes, PDCA e parada | DELIVERED_FOR_REVIEW |

## 9. Execução detalhada

1. Ler XML tipado. 2. Configurar DLL/certificado/proxy. 3. Enviar e persistir assinado. 4. Gerar proc e mover em ordem. 5. Retorno ERP/rejeição/cleanup/testes.

## 10. Compatibilidade, migration e rollback

Desabilitar roteamento de autorização e reverter task/helpers; preservar arquivos do usuário e não apagar EmProcessamento.

## 11. Validação específica

| Ordem | Comando/cenário | Ambiente | Timeout | Resultado esperado | Artefato |
|---:|---|---|---:|---|---|
| 1 | build + testes de contexto para sucesso, rejeição e falhas entre movimentos | ENV-UNI-DEBUG | 15 min | proc/original/retorno/erro exatamente nos destinos aprovados | docsplan/plans/evidence/UAB-005/evidence/validation.txt |

## 12. Testes

Mockar DLL/retornos; falhas antes/depois de proc; digest/chave, cStat 100 e rejeição; gRespTec conforme decisão; nenhuma rede.

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

Fluxo autorizado recuperável, contratos ERP verdes e serviços não publicados ausentes.

## 17. Dossiê, evidence e retomada

- Pasta: docsplan/plans/evidence/UAB-005/.
- Criar no início com IN_PROGRESS, AttemptId e checkpoint.
- Arquivar o dossiê completo em attempts/AttemptId/; retrabalho usa novo AttemptId.
