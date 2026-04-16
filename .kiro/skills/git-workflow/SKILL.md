---
name: git-workflow
description: Git branching and commit conventions for the coder agent. Use when starting a new feature, making a checkpoint commit, or finishing a feature.
---

## Starting a Feature

1. Make sure you're on `main` (or the agreed base branch) and it's up to date:
   ```bash
   git checkout main && git pull
   ```
2. Create a branch named with 2–5 lowercase words describing the change, separated by hyphens:
   ```bash
   git checkout -b <short-description>
   # examples: add-weapon-damage-field, fix-null-player-id, lock-down-forgot-password
   ```

## Checkpoint Commits

Commit whenever a logical unit of work is in a **building, non-broken state** — not just at the end.

```bash
git add <relevant files>
git commit -m "<imperative summary under 72 chars>"
# example: "Add damage field to Weapon model and service"
```

Rules:
- Never commit if `dotnet build` fails.
- Never commit commented-out dead code unless it's intentional scaffolding.
- One concern per commit — don't bundle unrelated changes.

## Finishing a Feature

1. Verify the build and tests are green:
   ```bash
   dotnet build ArixBack --nologo -v q
   dotnet test --nologo -v q   # if tests exist
   ```
2. Push the branch:
   ```bash
   git push -u origin <branch-name>
   ```
3. Proceed to the `review-cycle` skill.
