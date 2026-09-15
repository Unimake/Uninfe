# UniNFe - NF-e ABI - Evidência UAB-005

- Etapa: UAB-005
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-004 `APPROVED`; ABI-006 `APPROVED`; árvore UniNFe limpa
- Início: 2026-09-15T12:19:17-03:00
- Término: 2026-09-15T13:39:00-03:00
- Próxima etapa iniciada: NÃO

## Escopo entregue

- Autorização síncrona NF-e ABI com `infRespTec`, assinatura, retorno bruto, XML processado e persistência recuperável.
- Retomada sem retransmissão, produção fail-closed, diagnóstico passivo/sanitizado e dispatch coberto por testes.

## Gate

- Plan/Check 22/22: `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8`.
- XSD 20/20: `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40`.
- MOCs: `FE9868E5E5280EFE518C37AC70AABB833B197B1DE2BEA11C7B90322FE65B3E94` e `8DEF3134279864662CDE7D8A45CFF4665E9534AE8A890909410817893149104D`.
- DLL irmã limpa em `308864d94`, descendente do handoff aprovado `3f8811253`.
- Build Debug controlado: PASS, 0 erros/0 avisos; suíte NFeABI: PASS 63/63; P03 independente: PASS; `git diff --check`: PASS.

## Limitações

A primeira build completa falhou ao copiar `System.Net.Http.WinHttpHandler.dll` no checkout irmão por acesso negado. A validação final usou `BuildProjectReferences=false`. O OutputPath clássico grava em `bin/Release` mesmo sob Debug; nenhum comando Release/NuGet ou online foi executado.

## Escopo e checkpoint

Execução autorizada exclusivamente para UAB-005. Fontes normativas 22/22 e schemas 20/20 reproduziram os hashes aprovados. Checkout irmão limpo em `308864d94`, descendente do handoff `3f8811253`.

## Ambiente

Windows win-x64; PowerShell 7.6.5; .NET SDK 10.0.401. Credenciais e certificado não foram acessados.

## Rollback e cleanup

Reverter código; preservar/mover com segurança fixtures e arquivos de teste.
