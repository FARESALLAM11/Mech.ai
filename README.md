<div align="center">

# ⚙️ Mech.ai

### **One Unified AI Team. Not Ten Disconnected Chats.**

An open-source multi-agent orchestration platform and desktop environment that coordinates specialized AI models to plan, code, test, and review your tasks—delivering production-ready Diffs and Pull Requests.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![Built with Tauri](https://img.shields.io/badge/Built_with-Tauri_&_Rust-orange.svg)](#tech-stack)
[![BYOK](https://img.shields.io/badge/API-Bring_Your_Own_Key-success.svg)](#key-features)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

[Features](#-why-mechai) • [Architecture](#-how-it-works) • [Workflow](#-the-execution-pipeline) • [Tech Stack](#-tech-stack) • [Getting Started](#-getting-started)

</div>

---

## 💡 What is Mech.ai?

Managing complex engineering tasks across multiple AI chat windows leads to lost context, broken code, and wasted tokens. **Mech.ai** replaces fragmented workflows with a single, cohesive AI engineering team.

Write your objective once. The **Mech Orchestrator** analyzes your repository, breaks the objective down, and assigns it to specialized agents (**Planner, Executor, Tester, and Reviewer**) that share unified project context. Built with a strict **Bring Your Own Key (BYOK)** philosophy, Mech.ai executes every task inside an isolated `git worktree` sandbox and hands you back a tested, verified Diff or Pull Request—alongside a transparent, step-by-step cost breakdown.

---

## 🏗️ How It Works

```text
                       [ You: Define the Task ]
                                  │
                                  ▼
                   ┌──────────────────────────────┐
                   │       Mech Orchestrator      │
                   │   (Analyzes, Plans, Routes)  │
                   └──────────────┬───────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        ▼                         ▼                         ▼
┌───────────────┐         ┌───────────────┐         ┌───────────────┐
│  📋 Planner   │ ──────► │  💻 Executor  │ ──────► │  🧪 Tester    │
└───────────────┘         └───────────────┘         └───────┬───────┘
                                                            │
                                                            ▼
                                                    ┌───────────────┐
                                                    │  🔍 Reviewer  │
                                                    └───────┬───────┘
                                                            │
                                                            ▼
                                          [ Ready Output: Diff / PR + Cost Log ]
```

---

## 🔄 The Execution Pipeline

1. **Scope & Budget:** Select your repository, describe the task, and set hard limits on budget, steps, and execution time.
2. **Interactive Blueprint:** Mech scans your codebase and proposes a concise execution plan for your approval or adjustment.
3. **Upfront Cost Estimation:** Review token and financial estimates before a single line of code is modified.
4. **Sandboxed Execution:** Agents work inside an isolated `git worktree`—your main working directory remains completely untouched.
5. **Automated Test & Repair Loop:** Mech runs your test suite, automatically debugs failures within a capped retry limit, and conducts a final code review.
6. **Clean Delivery:** Inspect and accept changes file-by-file via an interactive Diff viewer, or let Mech open a Pull Request with a full audit and cost report.

---

## ✨ Why Mech.ai?

- 🔍 **Reviewable Outcomes, Not Chat Logs:** Receive clean code modifications backed by passing tests rather than endless conversation threads you have to manually stitch together.
- 🔑 **True Model Freedom (BYOK & Local):** Connect dozens of cloud providers using your own API keys or run local LLMs. Built-in automatic fallback ensures uninterrupted execution if a provider goes down.
- 💰 **Granular Cost Control:** Route simple subtasks to lightweight, cost-effective models and reserve frontier models for complex reasoning—all governed by strict budget and step ceilings.
- 🛡️ **Security & Isolation First:** Every task runs in a sandboxed environment with mandatory human-in-the-loop approval for sensitive shell commands and automatic secret redaction before context leaves your machine.
- 💾 **Crash-Resilient Checkpoints:** Resume interrupted or failed workflows from the last verified state without burning tokens starting from scratch.
- 🧠 **Open Project Memory:** Architectural decisions, codebase summaries, and context are stored locally in open, human-readable formats—100% portable and zero vendor lock-in.
- 🧩 **Permissioned GitHub Extensions:** Paste any compatible GitHub tool or agent repository URL; Mech installs it behind a clear permission prompt and integrates it directly into your agent pipeline.
- 🌍 **Native RTL & Arabic Support:** Designed from the ground up with full Right-to-Left (RTL) interface support for a seamless Arabic developer experience.

---

## 🎯 Where Mech.ai Fits in the Ecosystem

- **vs. Individual Coding Agents (Cursor, Claude Code, Codex):** Mech doesn't replace your favorite coding tools—it acts as the orchestration layer above them, coordinating multi-step verification and cross-agent review.
- **vs. Developer Frameworks (LangGraph, CrewAI, AG2):** Skip writing boilerplate orchestration code. Mech gives you a ready-to-use desktop application out of the box.
- **vs. Closed Enterprise Platforms (Devin, Cloud Suites):** 100% open-source core, self-hostable, zero black-box execution, and complete ownership of your API keys and source code.

---

## 🛠️ Tech Stack

- **Desktop Core:** [Tauri](https://tauri.app/) & **Rust** (High performance, low memory footprint, OS-level sandboxing)
- **Frontend UI:** **React** & TypeScript (Full LTR & RTL layout support)
- **Isolation Engine:** Native `git worktree` management & process sandboxing
- **Memory & State:** Local open-format checkpoints (JSON / Markdown)

---

## 🚀 Getting Started

### Prerequisites
- **Git** (v2.30+)
- **Node.js** (v20+) & **pnpm**
- **Rust** (stable toolchain for building from source)

### Installation (From Source)

```bash
# 1. Clone the repository
git clone [https://github.com/your-username/mech.ai.git](https://github.com/your-username/mech.ai.git)
cd mech.ai

# 2. Install frontend dependencies
pnpm install

# 3. Launch the Tauri development environment
pnpm tauri dev
```

---

## 🗺️ Roadmap

- [x] Core Orchestrator (Plan → Execute → Test → Review pipeline)
- [x] Isolated `git worktree` execution engine
- [x] Pre-execution cost estimator & real-time token ledger
- [ ] Multi-provider fallback router & local Ollama/vLLM integration
- [ ] Interactive file-by-file Diff approval UI
- [ ] GitHub Plugin & Extension Permissions Manager
- [ ] Full Arabic (RTL) interface localization

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! We are building Mech.ai in the open with a focus on reliability, security, and developer control. Please read our [Contributing Guide](CONTRIBUTING.md) before submitting a Pull Request.

---

## 📄 License

Copyright (c) 2026 Fares Sayed.  
Distributed under the **GNU General Public License v3.0 (GPL-3.0)**. See [`LICENSE`](LICENSE) for more information.
