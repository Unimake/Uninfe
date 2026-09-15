# UniNFe - NF-e ABI - Evidência UAB-001

- Etapa: UAB-001
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-000 `APPROVED`; ABI-006 `APPROVED`; árvore Git do UniNFe limpa antes da tentativa
- Início: 2026-09-14T22:29:16-03:00
- Próxima etapa iniciada: NÃO

## Checkpoint Plan

- Instruções, plano, manifesto, catálogos e fontes normativas lidos.
- Fonte normativa: 22 arquivos; snapshot agregado de referência `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8`.
- Schemas: 20 arquivos em `PL_NFeABI_1.00`; snapshot agregado de referência `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40`.
- Handoff: checkout `C:\projetos\github\Unimake.DFe` limpo; PDCA e commit `3f8811253` confirmam `ABI-006 APPROVED`.
- Migração solicitada concluída: plano movido para `docsplan/nfeabi`; referências internas, orquestradores e linter adaptados.
- Build futuro: somente Debug com referência direta à DLL irmã; Release/NuGet fora do escopo atual por decisão do DEV.
- Rodada 1 concluída pelo DEV em 2026-09-15: prefixo `UAB` mantido; transporte limitado a Status e Autorização em homologação; produção, consulta de protocolo e eventos sem transporte.
- Rodada 2 concluída pelo DEV em 2026-09-15: contratos literais de autorização congelados; Status reutiliza o contrato genérico existente e é roteado pela raiz `consStatServNFeABI`.
- Rodada 3 concluída pelo DEV em 2026-09-15: `TipoAplicativo.NFeABI = 17`; seletores/configurações e modo `Todos` seguem NFGas sem tela nova; `infRespTec` replica exatamente a regra permissiva atual da NFGas.
- Gate decisório concluído: nenhuma decisão material do proprietário permanece aberta.
- CHECK/P03 independente concluído em 2026-09-15: PASS, sem achado material; catálogo de hashes e validações N/A de build conferidos.
- ACT/P04 concluído: dossiê fechado, snapshot `attempt-0001` imutável após os hashes finais; aprovação humana ainda pendente.

## Ambiente

Windows win-x64; PowerShell 7.6.5; .NET SDK 10.0.401. Credenciais e certificado não foram acessados. Os perfis executados e seus contextos estão registrados no manifesto final: `DEEP` para Plan/Do, `INDEPENDENT_REVIEW` para Check e `ECONOMY` para Act.

## Rollback e cleanup

Restaurar revisão anterior do planejamento.
