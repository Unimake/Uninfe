# Versionamento e build

Preservar .NET Framework 4.8.1, projects clássicos e packages.config. Debug/Beta usam ProjectReference para `C:\projetos\github\Unimake.DFe`; Release conserva NuGet. Não renumerar enums persistidos. Nesta execução, todo build e teste deve usar explicitamente `-c Debug`; build alvo: `dotnet build source/uninfe.sln -c Debug --no-restore` quando suportado. Release e atualização do NuGet não estão autorizadas neste plano.
