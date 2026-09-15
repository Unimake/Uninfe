# Catálogo de integrações e fontes

| Integração/fonte | Contrato | Estado | Estratégia |
|---|---|---|---|
| Unimake.DFe irmã | Debug por ProjectReference; Release permanece por NuGet | ABI-006 aprovada no commit `3f8811253`; somente Debug será compilado neste plano | gate UAB-002; Release/NuGet fora do escopo atual |
| `C:\Users\Wandrey\OneDrive\Downloads\NFeAbi` | 2 MOCs e 20 XSDs; agregado `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8` | fonte obrigatória recorrente, 22/22 conferidos em 2026-09-15 | reler no Plan/Check de toda etapa 002+ segundo [SOURCE-REVIEW](../plans/evidence/UAB-001/evidence/SOURCE-REVIEW.md) |
| `C:\Users\Wandrey\OneDrive\Downloads\NFeAbi\PL_NFeABI_1.00` | 20 XSDs; agregado `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40` | pacote 1.00 conferido em 2026-09-15 | comparar nomes, tamanhos e hashes pela regra registrada no [SOURCE-REVIEW](../plans/evidence/UAB-001/evidence/SOURCE-REVIEW.md) |
| Portal Serviços | https://dfe-portal.svrs.rs.gov.br/NFABI/Servicos | status/autorização homologação | não duplicar endpoint na lógica UniNFe |
| ERP/pastas | nomes, retornos, .err, EmProcessamento/Autorizados/Originais | contrato público existente | aprovação DEC-003 e testes de contexto |
| Produção | não publicada | indisponível | fail-closed e etapa futura |
