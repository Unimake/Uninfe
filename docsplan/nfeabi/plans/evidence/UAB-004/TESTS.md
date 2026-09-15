# Testes UAB-004

| Verificação | Resultado |
|---|---|
| Leitura/parsing integral das fontes | PASS — 22/22 |
| XSD `PL_NFeABI_1.00` | PASS — 20/20 |
| Build `NFe.Service` com `Configuration=Debug` | PASS — 0 erros; 22 avisos legados |
| Testes novos de `ConsultaStatusNFeABITests` | PASS — 14 casos |
| Suíte NFeABI completa em Debug | PASS — 34/34 |
| Revisão independente após correções | PASS |
| `git diff --check` | PASS |
| Plan linter | PASS |

Os testes cobrem 107/108/109, XML bruto, configuração UF/ambiente/proxy/TLS, retorno vazio, DNS/TLS/proxy com segredo sintético, certificado ausente/vencido, produção e divergência de ambiente sem transporte, diagnóstico sanitizado, preservação/movimentação do pedido e dispatch real.

Não houve execução Release, transporte fiscal online ou uso de segredo/certificado real.

