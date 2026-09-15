# Ambientes

| EnvironmentId | Ambiente | Finalidade | Pré-condições | Etapas |
|---|---|---|---|---|
| ENV-PLAN | Windows/pwsh 7.6.5 | plano/linter | checkout e pasta NFABI legível | UAB-000, UAB-001 |
| ENV-UNI-DEBUG | UniNFe Debug + DLL irmã | implementação/testes | ABI-006 aprovado, restore prévio | UAB-002 a UAB-005 |
| ENV-INTEGRATED | dois repositórios em Debug | regressão/handoff | checkouts alinhados e ProjectReference ativo; sem dependência do NuGet NFeABI atualizado | UAB-006 |

`Release` não é ambiente de gate neste plano. O DEV publicará e atualizará o pacote NuGet após o encerramento das etapas UniNFe.
