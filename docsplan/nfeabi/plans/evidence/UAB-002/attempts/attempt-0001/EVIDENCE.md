# UniNFe - NF-e ABI - Evidência UAB-002

- Etapa: UAB-002
- Status: DELIVERED_FOR_REVIEW
- AttemptId: attempt-0001
- Estado-base: UAB-001 `APPROVED`; ABI-006 `APPROVED`; alterações pendentes da UAB-000/UAB-001 preservadas na árvore UniNFe
- Início: 2026-09-15T07:05:00-03:00
- Término: 2026-09-15T07:19:29-03:00
- Próxima etapa iniciada: NÃO

## Escopo e checkpoint

- Dois MOCs integralmente relidos e vinte XSDs integralmente parseados.
- Snapshot agregado da fonte normativa e do pacote de schemas reproduzido no Plan e no Check, sem divergência.
- Checkout irmão e API pública NFeABI comparados ao dossiê ABI-006 aprovado.
- Todo build e teste será executado exclusivamente com `-c Debug`.
- O teste consumidor será restrito a `source/UniNFe.Test/NFeABI`; nenhum fluxo, serviço, enum ou contrato de arquivo será implementado nesta etapa.
- Fonte no Plan: 22/22 e 20/20, agregados iguais ao catálogo.
- Handoff: checkout irmão limpo em `3f8811253`; ABI-006 e REQ-010 `APPROVED`.
- MSBuild Debug resolveu o ProjectReference da DLL irmã com `Configuration=Debug` e `TargetFramework=netstandard2.0`.
- DLL copiada para o consumidor e DLL compilada no checkout irmão têm o mesmo SHA-256 `626AC2A0EB712E97BA48474F90F6522768C09AB0B5324F68A2E8752545E4F138`.
- Smoke test criado em `source/UniNFe.Test/NFeABI/DllHandoffTests.cs`: raízes/namespace públicos, modelo 77 e construtores tipados de Status/Autorização.
- Build do projeto de testes Debug: PASS, 0 erros e 0 avisos na execução incremental final.
- Teste focado inicial: PASS, 6/6, sem rede ou certificado.
- Alteração externa preservada fora do escopo: exclusão pendente de `exemplos xml/NFe e NFCe 4.00/Eventos/Evento_211120-ped-eve.xml` não foi produzida nem tocada nesta etapa.

## ACT/P04 e limitações

- CHECK/P03 independente: sem achado material; fontes 22/22 e schemas 20/20 conferidos.
- Build Debug: exit 0, 0 erros/avisos; teste focado: 6/6.
- Limitação não bloqueante: `*Undefined*Build.bat` apareceu no stdout, embora o comando tenha terminado com exit 0.
- O checkout irmão estava limpo durante o build, o teste e a comparação dos binários. Após o CHECK/ACT, foram detectadas alterações externas relativas à remoção do evento NFe 211120; revisão complementar confirmou que não tocam NFeABI nem invalidam o resultado histórico, mas uma nova compilação exige revalidação do checkout.
- UAB-002 entregue para revisão humana; não aprovada. UAB-003 permanece `PLANNED`.

## Ambiente

Windows win-x64; PowerShell 7.6.5; .NET SDK 10.0.401. Credenciais e certificado não foram acessados.

## Rollback e cleanup

Reverter testes/docs desta etapa.
