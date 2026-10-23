# Mech Extension Manifest Specification (`mech-extension.json`)

Any repository hosted on GitHub or locally can become a Mech AI Repo by placing a `mech-extension.json` file in its root directory. If this file is omitted, Mech will automatically infer the configuration from repository contents (e.g. `SKILL.md`, `mcp.json`, or `AGENTS.md`).

---

## Schema Reference

```json
{
  "$schema": "https://mech.dev/schemas/extension-v1.json",
  "name": "smart-context-compressor",
  "version": "1.2.0",
  "description": "Reduces prompt token consumption by 45% by compressing large command tool outputs.",
  "extension_type": "tool",
  "author": "Mech Team",
  "license": "MIT",
  "repository": "https://github.com/mech-org/smart-context-compressor",
  "entry_point": "scripts/compress.py",
  "permissions": {
    "allow_network": false,
    "allow_file_write": false,
    "allow_exec_commands": true,
    "allow_read_secrets": false
  },
  "hooks": {
    "pre_step": "scripts/validate.sh",
    "post_step": "scripts/compress.py"
  }
}
```

---

## Supported Extension Types (`extension_type`)

| Type | Description | Primary File Detected |
|---|---|---|
| `mcp_server` | Model Context Protocol server exposing tools and resources over JSON-RPC. | `mcp.json` / `server.py` |
| `skill` | Step-by-step instructions, specialized knowledge, and helper scripts. | `SKILL.md` |
| `rule` | Code style guidelines, linting rules, and agent instructions. | `AGENTS.md` / `rules.md` |
| `hook` | Lifecycle scripts triggered before/after agent steps. | `hooks/` / `hook.sh` |
| `tool` | CLI utilities assisting the agent loop (e.g. repo map builders). | `scripts/` / executable binary |
| `subagent` | Specialized subagent system prompt and tool bindings. | `subagent.json` |

---

## Permission Scopes (`permissions`)

Mech guarantees that extensions run under principle of least privilege:
- `allow_network`: Permits HTTP/TCP traffic (disabled by default; required for external MCP APIs).
- `allow_file_write`: Permits writing files inside the project sandbox (disabled by default).
- `allow_exec_commands`: Permits spawning authorized child processes.
- `allow_read_secrets`: **Strictly restricted**. Extensions cannot inspect the user's OS keyring or API keys.

---

## Installation & Security

1. **Commit SHA Pinning:** Mech clones extensions and checks out an exact 40-character commit SHA to protect against upstream supply chain compromise.
2. **Zero Auto-Install Scripts:** No arbitrary `postinstall` or `npm install` scripts execute without explicit interactive confirmation from the user.
