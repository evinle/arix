---
name: tester-cycle
description: How to request testing from the tester agent and handle the bug reports it returns. Use after a feature is implemented and building cleanly.
---

## Requesting Testing

Ask the orchestrator to test the branch, providing a full brief:

```
Branch: <branch-name>

Feature summary:
<2–4 sentences on what was built and why>

Changed files:
<list of files modified>

Critical paths (must work):
- <e.g. POST /LoginController/Login returns a JWT on valid credentials>
- <e.g. unauthenticated requests to /players are rejected with 401>

Edge cases to probe:
- <e.g. login with wrong password>
- <e.g. registering a duplicate username>
- <e.g. missing required fields in request body>

Known limitations / out of scope:
- <anything you intentionally didn't handle>
```

The more context you give, the more targeted the test report will be.

## Handling Bug Reports

For each bug the tester returns:

1. **Reproduce it locally** before touching code — confirm you can hit the same failure.
2. **Fix it** with a focused commit per bug:
   ```bash
   git commit -m "Fix: <what was broken>"
   ```
3. **Re-request testing** after all fixes are applied, noting which bugs were addressed.

Repeat until the tester confirms no outstanding bugs.

## When to Stop

Move on to the `review-cycle` skill once the tester signs off. Don't request a review while known bugs are open.
