# Readiness

UAB-000 foi aprovada explicitamente pelo DEV em 2026-09-14 na `attempt-0002`.

UAB-001 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`; o plano está `READY_FOR_EXECUTION` e os IDs UAB-002 a UAB-006 estão congelados.

UAB-002 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`, após confirmação do handoff `ABI-006 APPROVED`, fontes normativas íntegras, build/teste consumidor verdes e revisão independente sem achado material.

UAB-003 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`.

UAB-004 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`.

UAB-005 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`.

UAB-006 está `DELIVERED_FOR_REVIEW` na `attempt-0001`. A fonte normativa reproduziu os agregados aprovados no Plan e no Check; fontes 22/22 e schemas 20/20 foram lidos, com os XSDs parseados integralmente. O checkout irmão permanece limpo em `308864d94`, descendente do handoff aprovado `3f8811253`. Build Debug controlado, suíte UniNFe NFeABI 64/64, testes offline da DLL 71/71, exemplo público assinado sem transporte, índices do viewer, revisão independente e linter estão verdes. A build completa ficou limitada por acesso negado ao copiar uma dependência no checkout irmão; não houve Release, NuGet ou transporte fiscal online. A aprovação permanece exclusiva do DEV.

Toda validação de produto usará `Debug` e a referência direta ao checkout irmão da Unimake.DFe. `Release` e atualização do pacote NuGet ficam fora deste plano até a publicação conduzida pelo DEV.
