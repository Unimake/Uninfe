---
name: uninfe-test-creator
description: 'Create and evolve automated tests in UniNFe.Test for UniNFe services, reusing TaskTestFixtureBase and domain fixtures to avoid duplicated setup and global-state handling.'
---

# UniNFe test creator

## Goal

Create test files for UniNFe with minimal duplication, following current project patterns in `source/UniNFe.Test`.

Use the existing abstractions as the default approach:

- `UniNFe.Test.Abstractions.TaskTestFixtureBase`
- `UniNFe.Test.Abstractions.TaskTestContextBase`
- domain fixture pattern (example: `eBoleto/EBoletoTestFixture.cs`)

## Scope

Apply this skill when the user asks to:

- create new tests for a `Task...` service class;
- add bugfix regression tests;
- extract duplicated setup from multiple test classes;
- add reusable fixture/context for a feature domain.

## Mandatory project assumptions

- Solution: `source/uninfe.sln`
- Test project: `source/UniNFe.Test/UniNFe.Test.csproj`
- Runtime compatibility: .NET Framework 4.8.1
- Keep tests compatible with C# 7.3 style used by the repo.

## Required structure and conventions

1. Use `xUnit` attributes (`Fact`, `Theory`, `InlineData`).
2. Prefer domain fixture + context over inline setup in each test class.
3. Reuse `TaskTestFixtureBase` for:
   - thread-based execution;
   - wait/polling for generated files;
   - XML loading helpers.
4. Reuse `TaskTestContextBase` for:
   - temporary folders;
   - setup/restore of global UniNFe static state;
   - input file creation.
5. For domains with static/global side effects, define serial test collection:
   - `[CollectionDefinition("<Domain> Serial", DisableParallelization = true)]`
   - `[Collection("<Domain> Serial")]` in test classes.
6. Keep assertion style objective and behavior-oriented.
7. Keep Portuguese messages for user-facing assertions/log expectations when applicable.

## File placement pattern

- Abstractions:
  - `source/UniNFe.Test/Abstractions/*.cs`
- Domain fixtures:
  - `source/UniNFe.Test/<Domain>/<Domain>TestFixture.cs`
- Domain tests:
  - `source/UniNFe.Test/<Domain>/*Tests.cs`
- Bugfix tests:
  - `source/UniNFe.Test/<Domain>/BugFixes/<id>/BugFix<id>.cs` (or existing bugfix layout already in use)

## Implementation workflow

1. Identify existing domain fixture and collection.
2. If no fixture exists, create one inheriting from `TaskTestFixtureBase`.
3. If setup is repeated, move it to a context class inheriting from `TaskTestContextBase`.
4. Create/adjust tests to consume fixture context via `using (...)` to guarantee cleanup.
5. Build and run targeted tests.
6. If shared static behavior causes flaky tests, force serial collection for that domain.

## XML/file-based task testing checklist

When testing service tasks that process input XML and generate output files:

- Create realistic input XML based on schema constraints used by the task.
- Set expected extensions using `Propriedade.Extensao(...)` contracts.
- Execute task in a named thread if `Empresas.FindEmpresaByThread()` is used.
- Assert output file existence.
- Assert input file deletion/move behavior expected by the task.
- Validate key return XML nodes (`Status`, `Motivo`, and task-specific fields).

## Guardrails

- Do not introduce new test frameworks.
- Do not refactor production code unless explicitly requested.
- Do not break external file naming contracts.
- Do not duplicate fixture setup already available in abstractions.
- Do not rely on interactive scripts.

## Validation commands (reference)

```powershell
dotnet build source/uninfe.sln --no-restore
```

Run only the impacted test types when possible.

## Output expectations

When using this skill, produce:

- fixture/context updates (if needed);
- test class(es) using the shared abstractions;
- build and targeted test execution results;
- short summary of what was created and where.
