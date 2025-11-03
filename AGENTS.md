# Repository Guidelines

## Project Structure & Module Organization
- Build outputs live under `bin/` and `obj/`; these folders are transient and should stay untracked.


## Coding Style & Naming Conventions
- Follow conventional C# layout: 4-space indentation, braces on new lines, and `PascalCase` for classes/interfaces (`WordsStatisticsImpl`), `camelCase` for locals/fields, and `UPPER_CASE` only for constants.
- Prefer expression clarity over cleverness; lean on `FluentAssertions` to keep tests declarative.
- Keep namespaces aligned with folder structure; update file headers if you relocate code.

## Testing Guidelines
- NUnit is the primary runner; stick to `[Test]` and `[TestCase]` attributes and assert via `FluentAssertions` for consistent failure messages.
- Name tests as `MethodUnderTest_State_Expectation`, e.g., `AddWord_TruncatesLongInput()`.
- House fixtures next to the code they verify, and add regression cases mirroring the edge scenarios in `Samples/Antipatterns`.
- Aim to touch new logic with at least one positive and one defensive test; run `dotnet test` before pushing.

## Commit & Pull Request Guidelines
- Recent history favors short, descriptive commit subjects (often in Russian); keep them imperative and scoped to a single concern.
- Rebase onto the latest `main`, ensure `dotnet test` passes, and attach coverage notes if behavior is risky.
- When opening a PR, include the problem statement, summary of changes, testing evidence (`dotnet test` output or coverage screenshot), and reference related homework/classwork tasks.
