# Versionamento e build

Preservar .NET Framework 4.8.1, projects clássicos e packages.config. Debug/Beta usam ProjectReference para `C:\projetos\github\Unimake.DFe`; Release conserva NuGet. Não renumerar enums persistidos. Build alvo: `dotnet build source/uninfe.sln --no-restore` quando suportado; testes principais em `source/UniNFe.Test` Debug. Release não está autorizada.
