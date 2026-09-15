# UniNFe - NF-e ABI - Evidência UAB-003

- Etapa: UAB-003
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-002 `APPROVED`; ABI-006 `APPROVED`; árvore UniNFe limpa
- Início: 2026-09-15T09:43:13-03:00
- Término: 2026-09-15T11:00:33-03:00
- Próxima etapa iniciada: NÃO

## Escopo entregue

- `Servicos`, `TipoEnvio` e `TpcnResources` receberam somente membros anexados ao fim, preservando valores existentes.
- `TipoAplicativo.NFeABI = 17`, descrição “NF-e ABI” e inclusão no texto de `Todos`.
- Autorização: `-nfeabi.xml`, `-ret-nfeabi.xml`, `-ret-nfeabi.err` e constante `-procNFeABI.xml`.
- Status: reutilização de `-ped-sta.xml` e `-sta.err`, distinguido pela raiz `consStatServNFeABI`.
- Detecção exige o par raiz+sufixo aprovado e mantém o casing exato das raízes.
- Configuração segue os call sites da NFGas; o seletor de configuração inclui NF-e ABI, mas o seletor de consulta não a expõe antes da UAB-004.
- Ajuste solicitado pelo DEV durante a revisão: no cadastro de nova empresa, a largura da combo de serviço passou de 514 px para 700 px e a janela fixa foi ampliada na mesma proporção, sem mudança funcional.
- Nenhuma task, dispatch, chamada remota, geração de processado ou endpoint foi criado.

## Gate

- Fonte Plan/Check: 22/22, hash `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8`.
- Schemas: 20/20, hash `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40`.
- Teste final Debug focado: 20/20 PASS.
- Build NFe.UI Debug: PASS, 0 warnings/0 errors.
- Verificação objetiva do layout: a descrição de `Todos` mede 587 px com a fonte efetiva Segoe UI 12 pt; a combo oferece 680 px úteis após reservar 20 px para a seta.
- `git diff --check`: PASS; após a entrega inicial, somente o ajuste visual solicitado pelo DEV e seu dossiê foram alterados.
- CHECK/P03 independente: dois achados P2 iniciais foram corrigidos; parecer final sem achado material.
- Checkout irmão: limpo em `308864d94`, descendente do handoff aprovado `3f8811253`; nenhum arquivo NFeABI divergiu.

## Limitações

- `*Undefined*Build.bat` apareceu no stdout sem alterar o exit code.
- Projetos clássicos imprimem `bin\Release` como `OutputPath` mesmo quando o MSBuild recebe `Configuration=Debug`; nenhum comando Release foi executado.
- Uma recompilação adicional do revisor encontrou `MSB3021` por restrição de escrita no checkout irmão; o build Debug do executor e o build Debug final do agente principal passaram, e o revisor reproduziu 20/20 com `--no-build`.
- Não houve interação visual manual com o WinForms; o ajuste foi verificado pelas dimensões do Designer, pela medição com a fonte efetiva do MetroFramework e pela compilação Debug. Também não houve gravação física do `.err` ou teste fiscal online.
- A sessão de teste 87916 do revisor permaneceu sem stdout após tentativa de interrupção e não foi encerrada à força para não afetar processos compartilhados.

## Rollback e parada

Reverter somente os registros, testes e documentos listados no manifesto desta tentativa. UAB-004 não foi iniciada; a aprovação desta entrega continua exclusiva do DEV.
