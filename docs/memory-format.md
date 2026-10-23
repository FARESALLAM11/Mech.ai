# Mech Project Memory Specification (.mech/)

Mech stores project memory locally inside the target repository under the `.mech/` directory. This format is 100% open, documented, and human-readable, guaranteeing zero vendor lock-in.

## Directory Structure

```
<project-root>/
  .mech/
    decisions.md      # Architectural decisions record (ADR format)
    rules.md          # Coding standards, style guides, and agent directives
    tasks.jsonl       # Append-only history of completed tasks, costs, and diff summaries
    manifest.json     # Project memory schema version and metadata
```

---

## 1. `decisions.md` (Architectural Decisions)

Written in standard GitHub Flavored Markdown following the ADR (Architecture Decision Record) structure:

```markdown
# Architectural Decisions

## ADR-001: SQLite WAL Mode for Persistent Engine
- **Status:** Accepted
- **Context:** Mech requires sub-second checkpoint writes with concurrent reading without UI stalls.
- **Decision:** Use SQLite in WAL (Write-Ahead Logging) mode with normal synchronization.
- **Consequences:** Maximum concurrency, crash resistance, and instant checkpoint restores.

## ADR-002: Git Worktree for Task Isolation
- **Status:** Accepted
- **Context:** AI models must not modify the user's active branch or working tree during execution.
- **Decision:** Every task is dispatched into an ephemeral `git worktree` and discarded if rejected.
```

---

## 2. `rules.md` (Project Guidelines)

Guidelines injected into the model's system prompt prior to code generation:

```markdown
# Project Rules & Style Guidelines

1. Documentation Integrity: Preserve all existing comments and docstrings.
2. Security: Never hardcode API keys or secrets in source files or git commits.
3. Code Formatting: 2 spaces indentation, explicit types in TypeScript, no `any`.
4. Testing: Every bugfix must be accompanied by a reproducing test case.
```

---

## 3. `tasks.jsonl` (Execution Log)

JSON Lines format tracking task metadata and costs:

```json
{"task_id": "mech-101", "prompt": "Fix login validation", "status": "done", "cost_usd": 0.0003, "tokens": 340, "diff_stat": "+2 -2", "timestamp": "2026-10-20T19:38:00Z"}
```

---

## 4. Portability & Synchronization

- **Export:** Memory can be exported as a single portable `.json` bundle via the Mech UI or CLI.
- **Git Commit:** Teams can check `.mech/decisions.md` and `.mech/rules.md` into git version control to synchronize knowledge across their entire engineering team.
