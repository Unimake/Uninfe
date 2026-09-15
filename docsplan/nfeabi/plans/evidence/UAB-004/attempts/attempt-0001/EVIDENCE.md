# UniNFe - NF-e ABI - Evidência UAB-004

- Etapa: UAB-004
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-003 `APPROVED`; ABI-006 `APPROVED`
- Início: 2026-09-15T11:15:43-03:00
- Término: 2026-09-15T11:54:49-03:00
- Próxima etapa iniciada: NÃO

## Escopo entregue

- Nova `TaskConsultaStatusNFeABI` integrada ao roteamento mínimo de `Processar`.
- Pedido `consStatServNFeABI` versão 1.00 enviado à DLL irmã pelo construtor de string, preservando o conteúdo para validação XSD.
- Configuração reutiliza certificado, UF, ambiente, proxy e preparação TLS existentes; nenhum endpoint foi cadastrado no UniNFe.
- Produção falha antes do transporte enquanto o endpoint não estiver publicado.
- Retornos fiscais 107, 108 e 109 são preservados integralmente em `-sta.xml`; retorno vazio gera `-sta.err`.
- Falhas são sanitizadas e classificadas como DNS, TLS, Proxy, Certificado, Configuração, Retorno ou Transporte.
- Diagnóstico é estritamente passivo: projeta somente o resultado da operação já executada, sem nova chamada de rede, endpoint, XML ou segredo.
- O pedido é apagado somente após sucesso. Em falha, fica no diretório `Erro` pelo helper legado ou permanece na origem caso o arquivamento falhe.
- Certificado ausente ou vencido é rejeitado antes do transporte; o fluxo real de `ProcessaArquivo` foi coberto.

## Gate

- Fontes normativas: 22/22; agregado `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8`.
- Schemas: 20/20; agregado `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40`.
- DLL irmã: checkout limpo em `308864d94`, descendente do handoff aprovado `3f8811253`.
- Build `NFe.Service` Debug controlado: PASS, 0 erros e 22 avisos legados.
- Suíte NFeABI Debug: PASS, 34/34.
- `git diff --check`: PASS.
- CHECK/P03 independente: PASS após correção de três achados materiais e de um achado no dispatch.
- Plan linter: PASS no fechamento.

## Limitações registradas

- A primeira recompilação completa tentou regravar um artefato Debug no checkout irmão e recebeu `MSB3021` por acesso negado. A validação final compilou o consumidor com `BuildProjectReferences=false`, usando o binário Debug já produzido pelo ProjectReference.
- O projeto clássico grava Debug em `bin\\Release` por configuração histórica, mas recebeu `Configuration=Debug` e `DEBUG`; nenhum comando Release foi executado.
- Não houve teste fiscal online nem uso de certificado ou credencial real.

## Rollback e parada

Reverter somente a task, o registro no projeto, o roteamento e os testes listados no manifesto desta tentativa. UAB-004 aguarda revisão e aprovação exclusiva do DEV; UAB-005 permanece `PLANNED` e não foi iniciada.
