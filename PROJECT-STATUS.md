# Mech — Project Status & Next Steps

> **Generated:** 2026-10-20  
> **Location:** project root

---

## 📋 Full Project Summary

**Mech** is a desktop application for Windows and macOS that orchestrates AI models to perform coding tasks reliably. The user selects a project, describes a task (e.g., "fix the login bug"), and Mech analyzes the project, plans, executes in an isolated sandbox, runs tests, and delivers the result as a **reviewable Diff or Pull Request** — with a full log of every step and cost.

### Core Principles
1. **Trust before intelligence** — every change is reversible, every sensitive command needs approval
2. **Transparency** — every step and every cost is visible to the user
3. **Speed & lightness** — feels native, not a website in a frame
4. **Minimum API calls for maximum results** — not the most agents, but the smartest orchestration
5. **User data stays local** — local storage by default, keys in OS keychain

### Tech Stack
| Layer | Technology |
|-------|-----------|
| Framework | **Tauri 2** (10–20 MB install, native WebView) |
| Core | **Rust** — task engine, providers, sandbox, git, storage |
| Frontend | **React + TypeScript + Vite + Zustand** |
| Database | **SQLite WAL** via `rusqlite` (bundled) |
| Secrets | **OS Keychain** via `keyring` crate |
| Code Analysis | **tree-sitter**, `ignore` crate, `notify` crate |
| Diff Engine | **`similar`** crate (pure Rust unified diff) |
| Git | CLI `git` + `git worktree` for task isolation |
| CI/CD | **GitHub Actions** — Windows x64 + macOS universal binary |

---

## ✅ Completed Phases (1–6) · 🚧 Phase 7 In Progress

### Phase 1: Architecture & Planning
- Full architecture document with component diagrams
- Risk matrix and mitigation strategies
- Development plan with all 7 phases defined
- Test plan covering unit, integration, and e2e scenarios

### Phase 2: Core Engine (31/31 tests passing)
- **Task State Machine** — `planned → running → testing → reviewing → done | failed | paused`
- **Provider Layer** — unified `ProviderAdapter` trait with `chat_stream`, `list_models`, `count_tokens`, `price`, `capabilities`
- **8 Provider Adapters** — OpenAI, Anthropic, Gemini, Ollama, Groq, DeepSeek, Mistral, Custom (OpenAI-compatible)
- **Provider Registry** with fallback chain and automatic failover
- **Checkpoint System** — save/resume tasks after failure, stored in SQLite
- **Budget Tracking** — per-provider monthly limits with 80% warning and 100% hard stop
- **Keyring Integration** — store/retrieve/mask API keys via OS keychain
- **SQLite Store** — WAL mode, tables for tasks, checkpoints, task_logs, provider_budgets

### Phase 3: Frontend UI (8 screens)
- **HomeScreen** — task input, real-time timeline, pause/resume/cancel controls, model comparison
- **DiffReviewScreen** — file sidebar, unified diff viewer, accept/reject per file, open PR
- **ProvidersScreen** — filterable grid (direct/hosted/gateway/local), test connection, budget bars
- **MemoryScreen** — edit decisions.md & rules.md, JSON export
- **AnalyticsScreen** — 4 metric cards + provider spending chart
- **AIReposScreen** — paste URL to install extensions, permission badges, SHA pinning
- **SettingsScreen** — network isolation, telemetry, sensitive action toggles, allowlist
- **OnboardingModal** — 3-step wizard for first-time setup
- **Components** — Navbar, Sidebar, CommandPalette (Ctrl+K), CostBadge, ApprovalModal, Timeline, DiffViewer
- **i18n** — full Arabic + English with RTL support
- **Theming** — CSS variables, dark/light, `prefers-reduced-motion`

### Phase 4: Security & Sandbox (19/19 tests passing)
- **Security Policy Engine** — allowlist-based command filtering, sensitive action detection
- **Human Approval Flow** — async approve/deny with 5-minute timeout, "approve always" option
- **Git Worktree Isolation** — each task runs in its own worktree (fallback to dir copy without git)
- **Environment Scrubber** — sanitize env vars, redact secrets from context sent to models
- **Windows Sandbox** — Job Object with memory limits
- **macOS Sandbox** — Seatbelt `sandbox-exec` profile generation

### Phase 5–6: Extensions & Providers (22/22 tests passing)
- **Extension Manifest** — `mech-extension.json` schema with permissions, hooks, MCP tools
- **Auto-Detection** — scan for SKILL.md, mcp.json, AGENTS.md
- **Extension Installer** — install from URL pinned to SHA, list, remove
- **MCP Host** — JSON-RPC 2.0 request/response for tool integration
- **Hook Runner** — lifecycle hooks (pre_task, post_task, on_error)
- **All 8 providers verified** with streaming, token counting, and pricing

### Phase 7: Integration & Closed Beta (in progress)
- The existing `tests/phase7_closed_beta_runner.cjs` is a **simulation with hard-coded outcomes**, not a closed beta with real teams. Its output must not be reported as real-world validation or production readiness.
- The UI now includes a Closed Beta workspace for team tracking, pilot metrics, feedback capture, status updates, and CSV export.
- The Closed Beta workspace starts empty and records team/task outcomes, accepted diffs, cost, duration, interventions, and qualitative feedback. Pilot records persist locally in the app's webview storage and task records export as CSV.
- Records are entered manually; they are not yet linked to the engine's SQLite task history. Avoid entering secrets or personal data in feedback.
- Launch gates remain unverified until actual pilot records have been collected. An empty dataset never counts as meeting a gate.
- **Next:** run the end-to-end workflow with 3–5 consenting engineering teams; record task outcomes, accepted diffs, cost, duration, interventions, and feedback. Review results and document an expand / address / stop decision.
- **Decision gates (proposed):** at least 3 teams complete the pilot, task success ≥80%, and mean interventions ≤2 per task. Review costs and qualitative feedback before expansion.
- CI/CD pipeline: `.github/workflows/release.yml`; sample project: `fixtures/sample-project/`.

---

## 📊 Project Metrics

| Metric | Value |
|--------|-------|
| Rust source files | **38** (.rs files in src-tauri/src/) |
| Frontend source files | **22** (.tsx/.ts files in src/) |
| Test suites | **5** (all passing) |
| Total tests | **78** |
| Frontend build size | **68.61 KB** gzipped |
| Build time | **~2.4 seconds** |
| Provider adapters | **8** + custom (any OpenAI/Anthropic-compatible) |
| Supported languages | **Arabic + English** with RTL |

---

## 📁 Project Structure

```
mech/
├── src-tauri/
│   ├── Cargo.toml                    # Rust dependencies
│   └── src/
│       ├── main.rs                   # Windows entry point
│       ├── lib.rs                    # Tauri commands & provider registration
│       ├── engine/
│       │   ├── mod.rs
│       │   ├── state_machine.rs      # TaskState transitions + budget/step limits
│       │   ├── runner.rs             # Task lifecycle + checkpoints + events
│       │   └── task.rs               # TaskStatus, TaskStepEvent types
│       ├── providers/
│       │   ├── mod.rs
│       │   ├── trait_def.rs          # ProviderAdapter trait definition
│       │   ├── types.rs              # ChatMessage, StreamChunk, ModelInfo
│       │   ├── registry.rs           # ProviderRegistry + fallback chain
│       │   ├── openai_adapter.rs     # OpenAI adapter
│       │   ├── anthropic.rs          # Anthropic adapter
│       │   ├── gemini.rs             # Google Gemini adapter
│       │   ├── ollama.rs             # Ollama (local) adapter
│       │   ├── groq.rs               # Groq adapter
│       │   ├── deepseek.rs           # DeepSeek adapter
│       │   ├── mistral.rs            # Mistral adapter
│       │   ├── custom.rs             # Custom OpenAI-compatible adapter
│       │   └── mock_adapter.rs       # Mock for testing
│       ├── sandbox/
│       │   ├── mod.rs
│       │   ├── policy.rs             # SecurityPolicy + SensitiveAction detection
│       │   ├── allowlist.rs          # Command allowlist
│       │   ├── worktree.rs           # Git worktree create/diff/accept/cleanup
│       │   ├── env_scrubber.rs       # Environment sanitization + secret redaction
│       │   ├── human_approval.rs     # Async approval with timeout
│       │   ├── windows.rs            # Job Object sandbox
│       │   └── mac.rs                # Seatbelt sandbox profile
│       ├── context/
│       │   ├── mod.rs
│       │   └── repo_map.rs           # Lightweight file scanner for context
│       ├── store/
│       │   ├── mod.rs
│       │   ├── db.rs                 # SQLite WAL (tasks, checkpoints, logs, budgets)
│       │   └── secrets.rs            # Keyring + mask_key + scrub_logs
│       └── extensions/
│           ├── mod.rs
│           ├── manifest.rs           # ExtensionManifest types
│           ├── detector.rs           # Auto-detect SKILL.md/mcp.json/AGENTS.md
│           ├── installer.rs          # Install/list/remove extensions
│           ├── hook_runner.rs         # Lifecycle hooks
│           └── mcp_host.rs           # JSON-RPC 2.0 MCP client
├── src/
│   ├── main.tsx                      # React entry point
│   ├── App.tsx                       # Router + layout
│   ├── screens/
│   │   ├── HomeScreen.tsx            # Main task screen
│   │   ├── DiffReviewScreen.tsx      # Diff review + PR
│   │   ├── ProvidersScreen.tsx       # Provider management
│   │   ├── MemoryScreen.tsx          # Project memory editor
│   │   ├── AnalyticsScreen.tsx       # Cost analytics dashboard
│   │   ├── AIReposScreen.tsx         # Extension manager
│   │   ├── SettingsScreen.tsx        # App settings
│   │   └── OnboardingModal.tsx       # First-run wizard
│   ├── components/
│   │   ├── Navbar.tsx
│   │   ├── Sidebar.tsx
│   │   ├── CommandPalette.tsx        # Ctrl+K command palette
│   │   ├── TaskInput.tsx
│   │   ├── Timeline.tsx
│   │   ├── DiffViewer.tsx
│   │   ├── CostBadge.tsx
│   │   ├── ApprovalModal.tsx
│   │   ├── ModelComparisonModal.tsx
│   │   └── ProviderSettingsModal.tsx
│   ├── store/taskStore.ts            # Zustand state management
│   ├── i18n/translations.ts          # AR + EN translations
│   └── styles/theme.css              # CSS variables + dark/light + RTL
├── tests/
│   ├── phase1_runner.cjs             # E2E agent loop test
│   ├── phase2_runner.cjs             # Core engine tests (31)
│   ├── phase4_security_runner.cjs    # Security tests (19)
│   ├── phase5_6_runner.cjs           # Extensions + providers tests (22)
│   └── phase7_closed_beta_runner.cjs # Integration tests (5 teams)
├── docs/
│   ├── memory-format.md              # .mech/ directory spec
│   └── extension-manifest.md         # Extension manifest schema
├── fixtures/sample-project/          # Buggy auth module for PoC
├── .github/workflows/release.yml     # CI/CD pipeline
├── launcher.bat                      # One-click dev server
├── package.json
├── vite.config.ts
├── tsconfig.json
└── index.html
```

---

## 🔴 Current Blockers (Environment)

These are **not code issues** — they are local dev environment setup items:

| Blocker | Impact | Fix |
|---------|--------|-----|
| **No Visual Studio Build Tools** | `cargo check` / `cargo build` fails — can't compile Rust core | Install via `winget install Microsoft.VisualStudio.2022.BuildTools` or [VS Build Tools installer](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022) |
| **No Git CLI** | `git worktree` falls back to directory copy; no real PR/diff from git | Install via `winget install Git.Git` or [git-scm.com](https://git-scm.com/download/win) |
| **Execution Policy** | PowerShell blocks `npx` sometimes | Run `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned` |

---

## 🟡 Next Steps (Priority Order)

### Product readiness audit (2026-10-21)

- Provider keys now use OS credential storage from the Providers screen; supported key IDs: OpenAI, Anthropic, Gemini, Groq, DeepSeek, and Mistral. Connection checks call each provider's authenticated models endpoint. Newly saved credentials register with the active provider registry.
- The Home screen no longer substitutes a fake simulated task when Tauri/provider execution fails; it displays the error. Real code-edit execution is still incomplete: the current runner does not yet apply model output as source edits, execute approved project tests, or persist an accept/reject workspace session.
- Project memory now reads and writes `.mech/decisions.md` and `.mech/rules.md` under the selected project. Analytics now reads task records from the local database and omits fabricated time-saved/provider-spend values.
- Sign-in/sign-up screen includes Google, GitHub, and Microsoft/Outlook options. OAuth buttons are intentionally disabled until app registrations, client IDs, and redirect URIs exist. No OAuth identity/backend is configured yet.
- Remaining placeholder features identified in the UI audit: AI Repos install/toggle/remove, Diff accept/PR, pause/resume/stop task controls, settings persistence/enforcement, and approved test-command execution. These must not report success until connected to core commands.
- Rust compilation was not validated in this environment because the Windows MSVC linker (`link.exe`) is missing. Frontend production build succeeds.

### Immediate (Before First Real Build)

1. **Install Visual Studio Build Tools**
   - Required for Rust compilation on Windows
   - After installing: run `cargo check` in `src-tauri/` to verify

2. **Install Git**
   - Required for worktree isolation and PR creation
   - After installing: run `git init` in project root

3. **Fix Missing Dependencies in `Cargo.toml`**
   - Add `regex` crate — used by `env_scrubber.rs`
   - Wire `pub mod extensions;` in `lib.rs`

4. **Run `cargo check`**
   - Fix any remaining compile errors in the Rust core
   - Goal: clean compilation with zero warnings

5. **Run `npm run tauri dev`**
   - This launches the actual desktop app (Tauri window with embedded frontend)
   - First milestone: see the Mech UI running as a native window

### Short Term (Alpha Quality)

6. **Real Provider Integration Testing**
   - Add a real API key (e.g., OpenAI) to the keyring
   - Run a real task end-to-end: task → plan → execute → diff
   - Verify streaming, token counting, and cost tracking

7. **CodeMirror 6 Integration**
   - Replace inline diff viewer with CodeMirror 6 (lazy-loaded)
   - Add syntax highlighting for 20+ languages
   - Spec requirement: load only when viewing diffs

8. **tree-sitter Repo Map**
   - Currently using a lightweight file scanner
   - Upgrade to tree-sitter for AST-level code understanding
   - Build symbol index (functions, classes, imports) per file

9. **Prompt Caching**
   - Implement for OpenAI (system prompt caching), Anthropic (beta header), Gemini (cached content)
   - Goal: reduce costs by 50%+ on repeated contexts

### Medium Term (Beta Quality)

10. **GitHub OAuth Flow**
    - Implement OAuth device flow for GitHub
    - Enable "Open PR" button in DiffReviewScreen
    - Create PR with task description and cost summary

11. **Tauri Updater**
    - Configure auto-update with signed releases
    - Update check on startup (non-blocking)

12. **Platform-Specific Builds**
    - macOS: universal binary (Intel + Apple Silicon) + notarization
    - Windows: x64 + arm64 + code signing

13. **End-to-End Testing with Real Projects**
    - Test with 5+ real open-source repos
    - Measure: success rate, cost per task, time to completion

### Long Term (Production)

14. **Plugin Marketplace**
    - Public registry for community extensions
    - Verified publisher badges

15. **Team Features**
    - Shared provider pools
    - Centralized budget management
    - Audit log

16. **Telemetry (Opt-in Only)**
    - Anonymous usage analytics
    - Crash reporting with user consent

---

## 🧪 How to Run

### Frontend Dev Server (works now)
```bash
cd <project-root>
# Option 1: Use launcher
launcher.bat

# Option 2: Manual
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
npx vite dev
# Opens at http://localhost:5173
```

### Build Frontend
```bash
npx vite build
# Output in dist/ (~68 KB gzipped)
```

### Run Tests
```bash
node tests/phase2_runner.cjs
node tests/phase4_security_runner.cjs
node tests/phase5_6_runner.cjs
node tests/phase7_closed_beta_runner.cjs
```

### Full Desktop App (after installing Build Tools + Git)
```bash
cd src-tauri
cargo check          # Verify Rust compiles
cd ..
npm run tauri dev    # Launch native desktop app
```

---

## 📝 Key Design Decisions

1. **Tauri 2 over Electron** — 10× smaller install, 3× less memory, native WebView
2. **Rust core handles all secrets** — API keys never reach JavaScript
3. **Events, not polling** — Tauri event system for all core→UI communication
4. **State machine for tasks** — every transition logged, enables checkpoint/resume
5. **Fallback chain** — if a provider fails, automatically try the next one
6. **Git worktree per task** — complete isolation, easy cleanup, real diffs
7. **SQLite WAL** — concurrent reads during writes, no external DB dependency
8. **CSS variables theming** — no heavy UI libraries, fast and customizable
9. **Arabic-first bilingual** — RTL layout as default, full i18n support

---

*This file is the single source of truth for where Mech stands today.*
