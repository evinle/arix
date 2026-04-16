---
name: review-cycle
description: How to request a code review and apply feedback. Use after finishing a feature branch and before merging.
---

## Requesting a Review

After pushing your branch, ask the orchestrator to review the diff:

> "Please review the changes on branch `<branch-name>`. Focus on correctness, security, and .NET conventions."

Provide context:
- What the feature/fix does
- Any known trade-offs or TODOs you left intentionally
- Files changed (a `git diff main...<branch>` output is helpful)

## Applying Feedback

For each piece of feedback:

1. **Critical / security issues** — fix immediately, no discussion.
2. **Correctness bugs** — fix and make a new checkpoint commit:
   ```bash
   git commit -m "Fix: <what was wrong>"
   ```
3. **Suggestions / style** — use your judgement. If the suggestion clearly improves the code, apply it. If it's ambiguous or out of scope, note it as a follow-up rather than blocking the merge.
4. **Disagreements** — explain your reasoning. Don't silently ignore feedback.

After addressing all critical and correctness items, confirm with the reviewer that the branch is ready.

## Definition of "Ready to Merge"

- `dotnet build` passes with no warnings introduced by your changes.
- No critical or correctness issues outstanding from the review.
- All checkpoint commits are clean (no "WIP" or broken-state commits).
