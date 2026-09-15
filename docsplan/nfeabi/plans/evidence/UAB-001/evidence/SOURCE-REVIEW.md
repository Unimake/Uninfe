# Revisão da fonte normativa — UAB-001

- Fonte: `C:\Users\Wandrey\OneDrive\Downloads\NFeAbi`
- Método: leitura integral dos dois MOCs, parsing/leitura integral dos vinte XSDs e reprodução do snapshot SHA-256 segundo a regra aprovada na ABI-000.
- Regra do agregado: SHA-256 do texto UTF-8 com linhas `caminho/relativo|tamanho|SHA256`, ordenadas por caminho, separadas por LF e com LF final.

| Conjunto | Arquivos | Esperado | Obtido em 2026-09-15 | Resultado |
|---|---:|---|---|---|
| Fonte normativa completa | 22 | `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8` | `0383F95D81140925C4EF91046C2D723CA9D94F477E138069F7202D79D9CCCAD8` | PASS |
| Pacote `PL_NFeABI_1.00` | 20 | `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40` | `BA44B39981C4DF3860A6BE9AA7C0739904D664377EABE963AB52AC8C2E74AE40` | PASS |

Não houve divergência de conteúdo, quantidade ou estrutura da fonte normativa.
