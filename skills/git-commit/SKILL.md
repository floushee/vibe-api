---
name: git-commit
description: 'Run tests, then stage ALL changed files and commit with an auto-generated Conventional Commit message derived from the diff. Use when user asks to commit changes, create a git commit, or mentions "/commit". Safety: never commit if tests fail; never commit secrets.'
license: MIT
allowed-tools: Bash
---

# Git Commit (Test-Gated) with Conventional Commits

## Overview

Create standardized, semantic git commits using the Conventional Commits specification.

Default behavior:

1) Run tests (`dotnet test`)
2) If tests pass, stage all changed files (`git add -A`)
3) Generate a Conventional Commit message based on the staged diff
4) Create the commit

If tests fail (or there are no changes), do not commit.

## Conventional Commit Format

```
<description>
```


## Workflow

### 0. Pre-flight checks

```bash
# Must be inside a git repo
git rev-parse --is-inside-work-tree

# See what's currently changed
git status --porcelain
```

If there are no changes (`git status --porcelain` outputs nothing), stop.

### 1. Run tests (gate)

```bash
dotnet test
```

If tests fail, stop and report failures. Do not stage or commit.

### 2. Stage all changes

Stage everything that’s changed (tracked and untracked). This is intentionally non-interactive.

```bash
git add -A
```

Safety: do a quick scan before committing.

```bash
git status --porcelain
```

**Never commit secrets** (.env, credentials.json, private keys).

### 3. Analyze staged diff

```bash
git diff --staged
```

### 4. Generate commit message

Analyze the diff to determine:

- **Description**: One-line summary of what changed (present tense, imperative mood, <72 chars)

### 5. Execute commit

```bash
git commit -m "<description>"
```

## Best Practices

- One logical change per commit
- Present tense: "add" not "added"
- Imperative mood: "fix bug" not "fixes bug"
- Reference issues: `Closes #123`, `Refs #456`
- Keep description under 72 characters

## Git Safety Protocol

- NEVER update git config
- NEVER run destructive commands (--force, hard reset) without explicit request
- NEVER skip hooks (--no-verify) unless user asks
- NEVER force push to main/master
- If commit fails due to hooks, fix and create NEW commit (don't amend)