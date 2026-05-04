---
name: ASP.NET 9 Pre-PR Production Self Review
description: Production-sensitive self review for ASP.NET 9 branch diffs before a PR is submitted.
on:
  workflow_dispatch:
    inputs:
      target_branch:
        description: Base branch to compare against when no PR is present.
        required: false
        default: main
        type: string
  pull_request:
    types:
      - opened
      - synchronize
      - reopened
      - ready_for_review
permissions:
  contents: read
  pull-requests: read
engine: copilot
strict: true
run-name: ASP.NET 9 Pre-PR Production Self Review
concurrency:
  group: aspnet9-pre-pr-production-self-review-${{ github.event.pull_request.number || github.ref_name }}
  cancel-in-progress: true
timeout-minutes: 30
tools:
  bash: true
safe-outputs:
  create-pull-request-review-comment:
    max: 20
    side: RIGHT
    target: triggering
  submit-pull-request-review:
    max: 1
    target: triggering
    allowed-events:
      - COMMENT
      - REQUEST_CHANGES
    footer: if-body
---

# ASP.NET 9 Pre-PR Production Self Review

Review the current branch diff against its target branch before a PR is submitted.

Treat this as a production-sensitive GitHub code review for an ASP.NET 9 migration or refactor.

## Review Goals

- Identify hidden logic or behavior changes.
- Detect production stability risks, startup failures, deployment failures, and runtime regressions.
- Surface security issues, especially auth, authz, secrets, config, and request pipeline changes.
- Review controller, service, middleware, DI, serialization, validation, mapping, data-access, and hosting changes.
- Detect async, blocking, thread-safety, shared-state, and exception-handling risks.
- Call out legacy carryover and migration anti-patterns.
- Flag dependency and package supportability issues.
- Assess rollback risk and whether the PR scope is too broad for safe review.

## Step-by-Step Setup

1. Determine the comparison target.
   - If the workflow runs on `pull_request`, compare the branch against `github.event.pull_request.base.ref`.
   - If the workflow runs on `workflow_dispatch`, compare the current branch against `inputs.target_branch`, defaulting to `main`.
2. Inspect the changed files and the diff.
   - Run `git diff --stat`, `git diff --name-only`, and `git diff --unified=0` against the merge base.
   - Open each changed file in context, not just the diff hunks, when behavior depends on surrounding code.
3. Prioritize production-impacting ASP.NET 9 areas first.
   - Startup and hosting
   - Middleware and request pipeline ordering
   - Dependency injection and options binding
   - Authentication and authorization
   - Configuration and environment-specific behavior
   - Serialization, model binding, and validation
   - Data access, EF Core, and transaction boundaries
   - Async control flow, blocking calls, and exception handling
4. Compare intended behavior to actual behavior.
   - Look for hidden default changes, reordered execution, changed null/default handling, and altered error paths.
   - Check for migration leftovers such as old patterns, duplicated abstractions, or compatibility shims that no longer fit ASP.NET 9.
5. Validate the evidence for each finding.
   - Prefer exact file and line references from the diff.
   - If you cannot cite exact line evidence, keep the concern in the markdown report only.
6. Produce the final report.
   - Return exactly one markdown report in the required section order.
   - Do not approve the PR, merge code, or modify any files.
   - If exact file and line evidence exists and a PR context is available, also emit the matching inline review comment safe outputs so GitHub can post them.
   - Always include the `review_comments` JSON block described below so the report stays machine-readable.

## Findings Rules

- Use `High`, `Medium`, or `Low` severity.
- Use a precise category such as `stability`, `security`, `behavior`, `validation`, `config`, `startup`, `auth`, `serialization`, `data-access`, `async`, `rollback`, or `scope`.
- Use a classification such as `immediate remediation required`, `should fix before PR`, or `informational`.
- Keep findings concrete, production-oriented, and evidence-based.
- Do not include praise or generic commentary.
- Do not invent line numbers.
- Do not flag speculative issues as inline review comments unless you can point to exact file and line evidence.

## Required Output Sections

Return the report in this exact order:

1. Submission decision
2. Executive summary
3. Blockers before PR submission
4. Findings table
5. Hidden behavior change warnings
6. Legacy carryover concerns
7. Missing validation evidence
8. Rollback concerns
9. Scope reduction suggestions
10. Suggested PR declaration

## Inline Review Comments Payload

When a finding has exact file and line evidence, append a fenced JSON block named `review_comments`.

Each comment object must include:

- `path`
- `line`
- `side` (`RIGHT` unless the comment refers to removed code)
- `severity`
- `category`
- `classification`
- `body`

Use this exact shape:

```json
{
  "review_comments": [
    {
      "path": "Controllers/TasksController.cs",
      "line": 84,
      "side": "RIGHT",
      "severity": "High",
      "category": "stability",
      "classification": "immediate remediation required",
      "body": "This change alters the failure path without preserving the previous guard. Keep the existing guard or document the new runtime behavior."
    }
  ]
}
```

If there are no exact-line findings, emit:

```json
{
  "review_comments": []
}
```

## Agent Instructions

- Do not approve the PR.
- Do not merge code.
- Do not modify repository files.
- Do not claim the change is safe unless the evidence supports that conclusion.
- Prefer blocking findings over vague concern when the issue can affect production behavior.
- If the diff is too broad for a reliable review, say so plainly and recommend a smaller PR.
- If the branch is missing validation evidence, state exactly what evidence is absent and why it matters.
- If the review is running against a pull request, submit the review with `REQUEST_CHANGES` when there are blockers and `COMMENT` otherwise.

## Suggested Review Flow

1. Start with the branch comparison and changed-file list.
2. Read the smallest relevant code path first, then expand outward to callers and adjacent startup or configuration code.
3. Confirm whether the change affects public behavior, request handling, auth, data access, serialization, validation, or operational safety.
4. Separate definite findings from warnings and from missing evidence.
5. End with a clear submission decision that matches the severity of the findings.
