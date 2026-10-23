# Relay

Desktop app for Windows and macOS that orchestrates AI models on coding tasks. You pick a project, describe the work, and Relay plans, runs in an isolated sandbox, tests, and returns a reviewable diff or pull request — with a log of every step and its cost.

API keys stay in the OS keychain. Project data stays on disk by default.

## Stack

- **Tauri 2** desktop shell
- **Rust** task engine, providers, sandbox, and storage
- **React + TypeScript + Vite + Zustand** UI
- **SQLite** (WAL) for tasks, checkpoints, and budgets

## Prerequisites

- Node.js 18 or newer
- Rust (stable)
- Git
- Windows: Visual Studio C++ Build Tools and a Windows 10/11 SDK
- macOS: Xcode command-line tools

## Setup

```bash
npm install
```

Frontend only:

```bash
npm run dev
```

Desktop app:

```bash
npm run desktop
```

On Windows you can also run `launcher.bat`, which checks for Node, Rust, and the C++ toolchain before starting Tauri.

Production build:

```bash
npm run desktop:build
```

## Tests

```bash
node tests/phase1_runner.cjs
node tests/phase2_runner.cjs
node tests/phase4_security_runner.cjs
node tests/phase5_6_runner.cjs
node tests/phase7_closed_beta_runner.cjs
```

## Providers

Relay talks to OpenAI, Anthropic, Gemini, Ollama, Groq, DeepSeek, Mistral, and OpenAI-compatible endpoints. Keys are stored with the `keyring` crate, never in the repo.

Do not commit `.env` files, signing keys, or Apple/Windows certificates. GitHub Actions reads those from repository secrets when you tag a release (`v*`).

## Layout

```
src/            React UI
src-tauri/      Rust / Tauri core
fixtures/       sample project used in onboarding
tests/          phase runners
docs/           extension and memory formats
.github/        release workflow
```

## License

MIT. See [LICENSE](LICENSE).
