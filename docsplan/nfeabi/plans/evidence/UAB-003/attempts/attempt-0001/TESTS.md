# Testes UAB-003

| Verificação | Resultado |
|---|---|
| Fonte normativa no Plan e Check | PASS — 22/22; agregado esperado reproduzido |
| Schemas `PL_NFeABI_1.00` | PASS — 20/20; agregado esperado reproduzido |
| Teste focado `UniNFe.Test.NFeABI` em Debug | PASS — 20/20 |
| Reexecução independente em Debug com `--no-build` | PASS — 20/20 |
| Build `NFe.UI` em Debug | PASS — exit 0, 0 avisos, 0 erros |
| Layout da combo no cadastro de nova empresa | PASS — descrição `Todos` com 587 px; área útil de 680 px na combo de 700 px |
| Associação raiz+sufixo e negativos | PASS |
| Seletor de configuração, exclusão da consulta e modo `Todos` | PASS |
| Mapeamento de extensões de erro consumido por `GravaErroERP` | PASS |
| `git diff --check` | PASS |
| Plan linter | PASS |

Não houve teste físico de gravação do arquivo `.err`, interação visual manual WinForms ou transporte fiscal online. O ajuste visual foi verificado pelas dimensões do Designer, pela medição com a fonte efetiva Segoe UI 12 pt e pela compilação Debug.
