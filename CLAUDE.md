# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Curator AI Highlights — Project Memory

## What this is
A proof-of-concept exploring AI- and graph-powered intelligent search and highlight
generation for media assets. It reads a structured sports event feed (Opta MA3), lets a
user ask in natural language for highlights ("all the fouls", "the first goal"), and
produces in/out timecodes that drive a video pipeline (FFmpeg) to assemble a reel. It is
a research vehicle for combining our Curator media-asset platform with rich content
metadata and AI. The longer-term differentiator is the round-trip from plain language to
*meaning* to the *actual editable/publishable asset* — something only a system that owns
both the asset layer (Curator) and a rich semantic content layer (a graph) can do.

## Stage and priorities  (READ THIS FIRST)
- This is a POC / learning project. **Favour clean, comprehensible architecture over
  production hardening.** Defer error handling, validation, auth, and performance tuning
  to a later, explicit pass — do not add them pre-emptively.
- The near-term goal is a stable, glitch-free demo for management and customers that
  shows off what AI brings, using static test data. The demo's job is to *provoke
  requirements*, not to be a finished product.

## How to work with me
- When reviewing my code, ask *why* a design choice was made and suggest alternatives.
- Work in small, verifiable increments: one change, confirm it builds/runs, then move on.
- Prefer the smallest abstraction that does the job; explain non-obvious reasoning.

## Current architecture
- **Two cooperating processes (this split is intentional):**
  1. **MCP server (ASP.NET Core)** — the "brain". Hosts a REST API for the UI and an MCP
     endpoint for the LLM. `AiChatClientService` drives the LLM (Claude or local Ollama,
     via Microsoft.Extensions.AI — **keep the AI layer model-agnostic**).
     `SportsFeedProcessorTools` exposes tools that query the Opta MA3 feed.
  2. **WinForms UI + video pipeline** — a thin HTTP client to the server; cuts and plays
     clips locally via FFmpeg/ffplay. To be replaced later by another client over the
     same (or enhanced) API — **this API boundary is where Curator eventually plugs in.**
- Test data: a static Opta MA3 JSON match file plus the corresponding match video.

## Conventions and hard rules
- **Never bulk-read the event array.** Events are always reached through scoped query
  tools (e.g. `ReadSportFeedEvents`) that filter server-side and return only matches.
- **The LLM interprets intent; deterministic code extracts facts.** Timecodes, ordering,
  and filtering happen in code — never by the model reasoning over raw data.
- **Never assume event typeIds.** Resolve them from the EventMap at runtime.
- Parse the feed once into in-memory objects; query the objects, not the raw JSON string.
- Keep this file lean — it loads into context on every session.

## Removed / parked
- The legacy Neo4j code is being removed for now. A graph database is a **known future
  pillar** (relationship- and sequence-based highlights; cross-domain fusion under an
  RDF/OWL ontology, reusing existing semantic-web work). **Do not re-introduce Neo4j
  without discussing it first.**

## Near-term task list
1. Remove legacy and dead code (including Neo4j).
2. Move hardcoded paths to `appsettings.json`; model them as "event source" and "output
   location", not literal file paths.
3. Establish a clean project structure and sensible logging.
4. Build the AI-highlights → FFmpeg pipeline around a clear **highlight-segment
   contract**: `{ label, in-timecode, out-timecode, source-asset reference }`. This seam
   between brain and video pipeline is a miniature of the eventual content-metadata ↔
   asset linkage, so design it deliberately.
5. Polish UI and API last.

## Build / run / test

Solution: `WinFormsAiHighlightsApp.slnx`

```bash
# Build entire solution
dotnet build WinFormsAiHighlightsApp.slnx

# Run the MCP/API server (must be running before the UI)
dotnet run --project AiHighlightsMcpServer/AiHighlightsMcpServer.csproj

# Run the WinForms desktop UI
dotnet run --project AiHighlightsWinFormsUi/AiHighlightsWinFormsUi.csproj

# Run all tests
dotnet test UnitTests/UnitTests.csproj

# Run a single test class
dotnet test UnitTests/UnitTests.csproj --filter "ClassName=MCPSportsFeedToolsUnitTests"

# Run a single test method
dotnet test UnitTests/UnitTests.csproj --filter "TestMethod=<MethodName>"
```

### MCP Server Ports
- `11190` — REST API used by the WinForms UI (`/aiChat/...` endpoints)
- `5252` — MCP protocol endpoint (HTTP transport, stateless)
- Swagger UI: `http://localhost:5252/swagger/ui`

### Key services

| Component | Role |
|-----------|------|
| `AiChatClientService.cs` | Singleton LLM orchestrator; supports Claude and multiple Ollama models; manages chat history and tool invocation |
| `Ma3FeedDataProviderService.cs` | Loads the Opta MA3 soccer match JSON feed; provides event/qualifier data to MCP tools |
| `SportsFeedProcessorTools.cs` | Primary MCP tools exposed to the LLM; extracts event maps, qualifiers, and schemas from the feed |
| `AiChatController.cs` | REST layer; endpoints for `runPrompt`, model selection, system prompt management, and tool enumeration |
| `SystemConfigurationService.cs` | Background hosted service handling async startup initialization |

### Hardcoded paths (must update to run locally)

- **`AiHighlightsWinFormsUi/Program.cs`** — `SourceVideoFilePath`, `OutputClipDirPath`
- **`AiHighlightsMcpServer/Services/Ma3FeedDataProviderService.cs`** — `TheFeedFilePath`

Moving these to `appsettings.json` is task #2 above.

### Testing

Unit tests cover MCP tools and data services. A key fixture assertion: the MA3 feed must deserialize to **1745 events** — use this as a sanity check when modifying feed parsing logic.
