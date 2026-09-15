# Readiness

UAB-000 foi aprovada explicitamente pelo DEV em 2026-09-14 na `attempt-0002`.

UAB-001 foi aprovada explicitamente pelo DEV em 2026-09-15 na `attempt-0001`; o plano está `READY_FOR_EXECUTION` e os IDs UAB-002 a UAB-006 estão congelados.

UAB-002 está `DELIVERED_FOR_REVIEW` na `attempt-0001`, com o handoff da DLL confirmado pelo estado `ABI-006 APPROVED` no checkout irmão, fontes normativas íntegras, build/teste consumidor verdes e revisão independente sem achado material. A aprovação permanece exclusiva do DEV; UAB-003 continua `PLANNED` e não foi iniciada.

O checkout irmão estava limpo durante o gate UAB-002, mas recebeu depois alterações externas relativas ao evento NFe 211120. Elas não tocam o contrato NFeABI revisado e não invalidam a evidência histórica; antes de qualquer compilação futura, o executor deve revalidar o checkout e sua aderência ao handoff aprovado.

Toda validação de produto usará `Debug` e a referência direta ao checkout irmão da Unimake.DFe. `Release` e atualização do pacote NuGet ficam fora deste plano até a publicação conduzida pelo DEV.
