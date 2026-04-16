---
name: testing-conventions
description: Project testing conventions for this repo. Covers Playwright E2E for React/TypeScript frontend and xUnit unit tests for .NET backend.
---

## Frontend — React / TypeScript (Playwright E2E)

- Framework: Playwright
- Test location: `e2e/**/*.spec.ts`
- Run command: `npx playwright test`
- Conventions:
  - Use page object model — one class per page/component in `e2e/pages/`
  - One spec file per feature/user flow
  - Assert on visible UI state, not internal component state

## Backend — .NET (xUnit)

- Framework: xUnit + Moq
- Test location: `tests/**/*Tests.cs`
- Run command: `dotnet test`
- Conventions:
  - One test class per service/controller
  - Arrange/Act/Assert with clear section comments
  - Mock all external dependencies via constructor injection
