## Context

Arix currently has basic WebSocket connectivity but lacks structured matchmaking and real-time game engine logic. For the "Mathletics" competitive mode, we need to transition from a simple "echo" to a server-authoritative engine that manages pairing, real-time math problems, and persistent history.

## Goals / Non-Goals

**Goals:**
- Implement an Elo-based matchmaking queue with wait-time expansion.
- Create a server-authoritative `MatchSession` that handles game state, problem generation, and damage calculation.
- Support persistent storage of match event logs for replay functionality.
- Keep the implementation simple and in-memory for the current scale.

**Non-Goals:**
- Distributed matchmaking (Redis-backed) is out of scope for this phase.
- Social features like friends lists or direct challenges.
- Real-time spectating of active matches.

## Decisions

### 1. In-Memory Singleton Services
Instead of external infrastructure (Redis), we will use ASP.NET Core Singleton services (`MatchmakingService`, `MatchSessionManager`).
- **Rationale**: Minimal latency and zero infrastructure cost for the initial prototype.
- **Alternatives**: Redis was considered but dismissed to keep the project "lean" and reduce AWS costs.

### 2. Matchmaking Loop (Background Worker)
A `BackgroundService` will run every second to scan the `ConcurrentQueue` of waiting players.
- **Elo Expansion**: `MaxAllowedGap = 200 + (5 * WaitSeconds)`.
- **Rationale**: Ensures players find matches even if the player pool is small, while prioritizing fair play for new entries.

### 3. Server-Authoritative Engine
The server will not just relay messages; it will be the source of truth for:
- Problem generation (e.g., `12 + 5 * 2`).
- Answer validation.
- HP calculation and damage application.
- **Rationale**: Prevents cheating (e.g., reporting a win without solving) and ensures a consistent experience.

### 4. Buffer-and-Flush Persistence
Match events will be collected in a `List<MatchEvent>` within the `MatchSession`. Upon termination, the entire list is flushed to a new `Matches` collection in MongoDB.
- **Rationale**: Reduces database I/O during the critical real-time battle phase.

### 5. Structured JSON Protocol
WebSockets will use a standard format: `{ "type": "EVENT_NAME", "payload": { ... } }`.
- **Rationale**: Makes it easy to extend and debug both frontend and backend logic.

## Risks / Trade-offs

- **[Risk] Memory Exhaustion** -> [Mitigation] Strict 5-minute timeout per match and automatic cleanup of disconnected sessions.
- **[Risk] Server Restart Data Loss** -> [Mitigation] Active matches are lost on restart. This is accepted for the current scale; persistence occurs only at the end of a match.
- **[Risk] Latency in Math Generation** -> [Mitigation] Pre-generate the next problem while the player is solving the current one.
