# Testes UAB-006

| Verificação | Resultado |
|---|---|
| Build completa `uninfe.sln` Debug | LIMITAÇÃO AMBIENTAL — acesso negado ao copiar dependência no checkout irmão |
| Build NFe.Service Debug controlado | PASS — 0 erros/0 avisos |
| Suíte UniNFe NFeABI Debug | PASS — 64/64 |
| Exemplo público preparado e assinado sem transporte | PASS — 1/1 |
| Allowlist offline NFeABI da DLL Debug | PASS — 71/71 |
| Índices do viewer | PASS — 107 documentos |
| Links das páginas NF-e ABI | PASS — 0 links Markdown internos falhos; exemplo usa link público absoluto |
| Fontes normativas no Plan e Check | PASS — 22/22 fontes e 20/20 XSDs |
| CHECK/P03 independente | PASS |
| `git diff --check` | PASS |
| Plan linter | PASS no fechamento |

Não executados: transporte fiscal online, produção, Release/NuGet, publish/deploy, consulta de protocolo, eventos ou etapa sucessora.
