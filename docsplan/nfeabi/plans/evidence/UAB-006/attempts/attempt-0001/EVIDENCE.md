# UniNFe - NF-e ABI - Evidência UAB-006

- Etapa: UAB-006
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-005 `APPROVED`; ABI-006 `APPROVED`; árvore UniNFe limpa em `9ce50bbfb`
- Início: 2026-09-15T14:26:16-03:00
- Término: 2026-09-15T14:50:31-03:00
- Próxima etapa iniciada: NÃO

## Escopo entregue

- Documentação operacional pública de autorização síncrona e consulta de status da NF-e ABI, com limites de homologação, contratos ERP e recuperação segura.
- Exemplo integral com dados sintéticos e teste que o prepara e assina pela DLL real sem transporte.
- Índice, catálogo e índices do viewer regenerados.
- Gates conjuntos Debug/offline e revisão independente.

## Gate

- Plan/Check 22/22: `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8`.
- XSD 20/20: `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40`.
- MOCs: `FE9868E5E5280EFE518C37AC70AABB833B197B1DE2BEA11C7B90322FE65B3E94` e `8DEF3134279864662CDE7D8A45CFF4665E9534AE8A890909410817893149104D`.
- DLL irmã limpa em `308864d94`, descendente do handoff aprovado `3f8811253`.
- Build NFe.Service Debug controlado: PASS; suíte UniNFe NFeABI: PASS 64/64; testes offline DLL: PASS 71/71; exemplo público: PASS 1/1; viewer 107 documentos; P03 independente, diff e linter: PASS.

## Limitações

A build completa Debug foi tentada, mas o ambiente negou a cópia de `System.Net.Http.WinHttpHandler.dll` para o checkout irmão. O gate foi completado com build controlado de NFe.Service sem recompilar referências, testes UniNFe Debug e a allowlist offline da DLL já compilada em Debug. O projeto clássico grava a saída Debug de NFe.Service em `bin/Release`; isso é apenas o `OutputPath` legado. Nenhum comando Release/NuGet, publicação ou transporte fiscal online foi executado.

## Escopo e checkpoint

Execução autorizada exclusivamente para UAB-006. Fontes normativas 22/22 e schemas 20/20 reproduziram os hashes aprovados no Plan e no Check. Checkout irmão limpo em `308864d94`, descendente do handoff `3f8811253`. Decisões `DEC-002` e `DEC-007` fechadas pelo DEV. A aprovação continua exclusiva do DEV.

## Ambiente

Windows win-x64; PowerShell 7.6.5; .NET SDK 10.0.401. Credenciais e certificado não foram acessados.

## Rollback e cleanup

Reverter ajustes finais sem apagar dossiês anteriores.
