---
name: uninfe-test-creator
description: "Create or evolve automated tests in source/UniNFe.Test for UniNFe services. Use when Codex needs to add tests for Task... service classes, regression tests, fixtures, contexts, or reusable test setup while preserving .NET Framework 4.8.1 and existing xUnit patterns."
---

# UniNFe test creator

## Goal

Create test files for UniNFe with minimal duplication, following current project patterns in `source/UniNFe.Test`.

Use existing abstractions as the default approach:

- `UniNFe.Test.Abstractions.TaskTestFixtureBase`
- `UniNFe.Test.Abstractions.TaskTestContextBase`
- domain fixture pattern, for example `eBoleto/EBoletoTestFixture.cs`

## Mandatory project assumptions

- Solution: `source/uninfe.sln`
- Test project: `source/UniNFe.Test/UniNFe.Test.csproj`
- Runtime compatibility: .NET Framework 4.8.1
- Keep tests compatible with the C# style used by the repository.

## Required structure and conventions

1. Use xUnit attributes: `[Fact]`, `[Theory]`, and `[InlineData]`.
2. Prefer domain fixture plus context over inline setup in each test class.
3. Reuse `TaskTestFixtureBase` for thread-based execution, polling/waiting for generated files, and XML-loading helpers.
4. Reuse `TaskTestContextBase` for temporary folders, setup/restore of global UniNFe static state, and input-file creation.
5. For domains with static/global side effects, define a serial test collection:

```csharp
[CollectionDefinition("<Domain> Serial", DisableParallelization = true)]
[Collection("<Domain> Serial")]
```

6. Keep assertions objective and behavior-oriented.
7. Keep Portuguese messages for user-facing assertions/log expectations when applicable.

## File placement pattern

- Abstractions: `source/UniNFe.Test/Abstractions/*.cs`
- Domain fixtures: `source/UniNFe.Test/<Domain>/<Domain>TestFixture.cs`
- Domain tests: `source/UniNFe.Test/<Domain>/*Tests.cs`
- Bugfix tests: `source/UniNFe.Test/<Domain>/BugFixes/<id>/BugFix<id>.cs`, or the existing bugfix layout already in use

## Implementation workflow

1. Identify the existing domain fixture and collection.
2. If no fixture exists, create one inheriting from `TaskTestFixtureBase`.
3. If setup is repeated, move it to a context class inheriting from `TaskTestContextBase`.
4. Create or adjust tests to consume fixture context via `using (...)` to guarantee cleanup.
5. Build and run targeted tests.
6. If shared static behavior causes flaky tests, force a serial collection for that domain.

## XML/file-based task testing checklist

When testing service tasks that process input XML and generate output files:

- Create realistic input XML based on schema constraints used by the task.
- Set expected extensions using `Propriedade.Extensao(...)` contracts.
- Execute the task in a named thread if `Empresas.FindEmpresaByThread()` is used.
- Assert output-file existence.
- Assert input-file deletion or move behavior expected by the task.
- Validate key return XML nodes: `Status`, `Motivo`, and task-specific fields.

## Guardrails

- Do not introduce new test frameworks.
- Do not refactor production code unless explicitly requested.
- Do not break external file-naming contracts.
- Do not duplicate fixture setup already available in abstractions.
- Do not rely on interactive scripts.

## Validation commands

Build the solution or the impacted project when possible:

```powershell
dotnet build source/uninfe.sln --no-restore
```

Run only impacted test types when possible.

## Output expectations

When using this skill, produce:

- fixture/context updates, if needed;
- test classes using shared abstractions;
- build and targeted test execution results;
- short summary of what was created and where.
