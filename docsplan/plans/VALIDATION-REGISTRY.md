# Registro de validação

| Etapa | Runner/cenário reproduzível | EnvironmentId | Resultado esperado |
|---|---|---|---|
| UAB-000 | `pwsh -NoProfile -File .agents/skills/plan-linter/scripts/test-plan.ps1 -RepositoryRoot .` | ENV-PLAN | linter retorna 0 |
| UAB-001 | linter + revisão independente | ENV-PLAN | zero decisão material aberta e UAB-002 pronta |
| UAB-002 | `dotnet test source/UniNFe.Test/UniNFe.Test.csproj -c Debug --no-restore` com filtro focado | ENV-UNI-DEBUG | consumer compila e baseline passa |
| UAB-003 | build projetos afetados + testes de detecção/extensões | ENV-UNI-DEBUG | raízes roteadas e contratos literais congelados |
| UAB-004 | build NFe.Service + testes focados de status/contexto | ENV-UNI-DEBUG | retorno/erro/cleanup corretos sem secret |
| UAB-005 | build + testes de contexto para sucesso, rejeição e falhas entre movimentos | ENV-UNI-DEBUG | proc/original/retorno/erro exatamente nos destinos aprovados |
| UAB-006 | build uninfe.sln quando ambiente permitir + UniNFe.Test Debug focado + testes DLL NFeABI | ENV-INTEGRATED | gates determinísticos verdes e docs/index consistentes |
