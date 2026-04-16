# Matchmaking v1 — Full Feature Spec

## Overview

Real-time 1v1 math combat over WebSockets with JWT authentication, Elo-based matchmaking, class mechanics, and a persistent match log for future replay.

---

## Branch

`feature/ws-match-game`

---

## Existing Codebase Context

- **Backend:** ASP.NET Core 9, MongoDB, JWT auth already wired for WS via `?access_token=` query param. `WebsocketManager` singleton holds player connections. `Matchmaking` controller at `GET /Websocket/ws` accepts WS and currently echoes messages.
- **Frontend:** React + TypeScript (Vite). `react-use-websocket` already installed. `Matchmaking.tsx` connects to WS with JWT and renders a debug echo UI. `useLocalStorage` hook stores JWT. `queryFnBuilder` handles typed API calls.
- **Key files to read before starting:**
  - `ArixBack/Program.cs`
  - `ArixBack/Controllers/Matchmaking.cs`
  - `ArixBack/Services/WebsocketManager.cs`
  - `ArixBack/Services/PlayerService.cs`
  - `ArixBack/Models/Player.cs`
  - `ArixBack/Models/Weapon.cs`
  - `ArixBack/Services/DatabaseService.cs`
  - `ArixBack/ArixBack.csproj`
  - `arix-front/src/components/Game/Matchmaking.tsx`
  - `arix-front/src/App.tsx`
  - `arix-front/src/helpers/queryBuilder.ts`
  - `arix-front/src/hooks/useLocalStorage.ts`
  - `arix-front/src/components/Form/AxInput.tsx`
  - `arix-front/src/components/Menu/MenuItem.tsx`

---

## Game Rules

- Both players answer questions simultaneously in real time — no turns.
- Correct answer → deal damage to opponent immediately.
- Wrong answer → no penalty, receive a new question.
- Skip → player takes 10 flat damage, receives a new question.
- Each player has their own independent question stream.
- When a player's HP reaches 0, the match ends. No draws.
- If a player disconnects mid-match, the opponent wins by default.
- Base damage per correct answer: **20** (before modifiers).
- Skip penalty: **10 flat damage to self**.

---

## Data Model Changes

### `Models/Player.cs` — add fields
- `Elo` (int, default 1000)
- `ClassType` (enum `ClassType`: `Rogue=0, Berserker=1, Juggernaut=2, Wizard=3`)
- `EquippedWeaponId` (string?, nullable)
- `EquippedArmorId` (string?, nullable)

### `Models/Weapon.cs` — add fields
- `DamageModifier` (double, default 1.0) — multiplier on outgoing damage
- `SpecialEffect` (string?, always null for now — reserved for future)

### `Models/Armor.cs` — new
```
Id (BsonId, ObjectId)
Name (string)
DamageReductionModifier (double) — multiplier reducing incoming damage (e.g. 0.8 = 20% reduction)
SpecialEffect (string?, null — reserved for future)
```

### `Models/MatchLog.cs` — new (persisted to DB at match end)
```
Id (BsonId, ObjectId)
Player1Id (string)
Player2Id (string)
StartedAt (DateTime)
EndedAt (DateTime)
WinnerId (string)
Actions (List<MatchAction>)
```
`MatchAction` record:
```
Timestamp (DateTime)
PlayerId (string)
ActionType (string)  // e.g. "correct_answer", "skip", "bleed_tick", "charge_release", "curse_applied", "game_over"
Payload (string?)    // JSON string with extra context
```

---

## WebSocket Message Protocol

All messages are JSON.

### Server → Client
```json
{ "type": "waiting" }

{ "type": "match_start", "opponentName": "string", "opponentClass": "string",
  "yourHp": 100, "opponentHp": 100,
  "question": { "id": "string", "text": "string" }, "skillTier": 0 }

{ "type": "question", "id": "string", "text": "string" }

{ "type": "hit", "yourHp": 0, "opponentHp": 0,
  "damageDealt": 0, "damageTaken": 0, "effect": "string|null" }

{ "type": "bleed_tick", "yourHp": 0, "amount": 0 }

{ "type": "curse_applied", "questionsAffected": 3 }

{ "type": "curse_removed" }

{ "type": "game_over", "won": true, "eloChange": 0, "log": [ /* MatchAction[] */ ] }
```

### Client → Server
```json
{ "type": "queue", "skillTier": 0 }        // sent after WS connect to enter queue

{ "type": "answer", "questionId": "string", "value": 0 }

{ "type": "skip" }

{ "type": "release_charge" }               // Berserker only
```

---

## Skill Tiers (Question Difficulty)

Each tier is an independent class implementing `IQuestionTier`. No coupling between tiers.

| Tier | Label  | Type                                                                 |
|------|--------|----------------------------------------------------------------------|
| 0    | Easy   | Addition / Subtraction (operands 1–20)                               |
| 1    | Medium | Multiplication / Division (whole results, operands 1–12)             |
| 2    | Hard   | Exponents / Roots (a^b, b∈{2,3}, a∈2–10; or square/cube roots of perfect powers) |
| 3    | Expert | Logarithms (log_b(x) where result is whole number, b∈{2,3,5,10})    |
| 4    | Master | Linear equations (ax + b = c) or quadratic (perfect square trinomials), integer solutions |

Interface:
```csharp
interface IQuestionTier {
    Question Generate();
    bool Validate(Question q, string answer);
}

record Question(string Id, string Text, string Answer);
```

---

## Class Mechanics

### Rogue
- Low base damage (base 20, no bonus).
- Tracks correct answer streak per player.
- On streak ≥ 3: apply 1 bleed stack to opponent.
- Bleed: 5 dmg/tick, ticks every 5 seconds, max 3 stacks, 4 ticks each stack.
- Streak resets on wrong answer or skip.

### Berserker
- Accumulates +15 charge per correct answer.
- Player manually sends `release_charge` to burst all charge as damage.
- Charge resets to 0 after release.
- No passive bonus otherwise.

### Juggernaut
- 20% flat damage reduction (stacks with armor modifier).
- Reflects 5 flat damage back to attacker on every hit received.

### Wizard
- 30% chance to apply curse to opponent on each correct answer.
- Curse: opponent's next 3 questions are bumped up one difficulty tier.
- Cursed question answered wrong → opponent loses 15 HP.
- Heals self 5 HP per correct answer.

---

## Elo System

- Standard Elo formula, K=32.
- Called at match end: `(int newWinnerElo, int newLoserElo) Calculate(int winnerElo, int loserElo)`.
- Both players' Elo updated in DB after match.

## Matchmaking Queue

- Singleton with background loop running every 2 seconds.
- `QueueEntry`: `PlayerId`, `PlayerName`, `Elo`, `SkillTier`, `ClassType`, `WeaponDamageModifier`, `ArmorDamageReductionModifier`, `EnqueuedAt`.
- Allowed Elo gap: `100 + (secondsInQueue / 10) * 50` (grows over time).
- On pair found: create `MatchSession`, send `match_start` to both, assign first questions.

---

## Backend — Files to Create / Modify

### New files
| File | Purpose |
|------|---------|
| `Models/Armor.cs` | Armor model |
| `Models/MatchLog.cs` | Match log + MatchAction record |
| `Services/Questions/IQuestionTier.cs` | Interface + Question record |
| `Services/Questions/AddSubtractTier.cs` | Tier 0 |
| `Services/Questions/MultiplyDivideTier.cs` | Tier 1 |
| `Services/Questions/ExponentRootTier.cs` | Tier 2 |
| `Services/Questions/LogTier.cs` | Tier 3 |
| `Services/Questions/EquationTier.cs` | Tier 4 |
| `Services/QuestionService.cs` | Holds tier array, `GetTier(int)` |
| `Services/EloService.cs` | Elo calculation |
| `Services/ClassEffectService.cs` | Stateless class effect logic |
| `Services/MatchSessionStore.cs` | Singleton dict of active sessions + `MatchSession` / `PlayerMatchState` definitions |
| `Services/MatchmakingQueue.cs` | Queue + background pairing loop |
| `Services/MatchLogService.cs` | Persists `MatchLog` to MongoDB |
| `Controllers/ArmorController.cs` | `GET /Armor/GetAllArmors` |

### Modified files
| File | Changes |
|------|---------|
| `Models/Player.cs` | Add `Elo`, `ClassType`, `EquippedWeaponId`, `EquippedArmorId` |
| `Models/Weapon.cs` | Add `DamageModifier`, `SpecialEffect` |
| `Services/DatabaseService.cs` | Add `GetArmorCollection()`, `GetMatchLogCollection()` |
| `Services/WebsocketManager.cs` | Add `SendToPlayer(string playerId, object message)` — serialises to JSON and sends |
| `Controllers/Matchmaking.cs` | Full rewrite — see game loop below |
| `Controllers/PlayerController.cs` | Add `GET /Player/GetEquipped`, `POST /Player/Equip` |
| `Program.cs` | Register all new singletons; start `MatchmakingQueue` background loop |

### `PlayerMatchState` fields (defined in MatchSessionStore.cs)
```
PlayerId, PlayerName, ClassType, Hp (int),
WeaponDamageModifier (double), ArmorDamageReductionModifier (double),
SkillTier (int), CurrentQuestion (Question),
ChargePoints (int), BleedStacks (int), BleedTicksRemaining (int),
CursedQuestionsRemaining (int), CorrectStreak (int)
```

### `ClassEffectService` methods
```csharp
EffectResult ApplyOnCorrectAnswer(PlayerMatchState attacker, PlayerMatchState defender, int baseDamage)
EffectResult ApplyOnHit(PlayerMatchState defender, int incomingDamage)   // Juggernaut reflect
int TickBleed(PlayerMatchState player)                                    // called every 5s
int ReleaseCharge(PlayerMatchState berserker)

record EffectResult(int DamageToOpponent, int DamageToSelf, int HealSelf, string? EffectMessage)
```

### Matchmaking controller game loop
1. WS connect → validate JWT → accept → register in `WebsocketManager` → wait for `queue` message.
2. `queue` message → build `QueueEntry` from player DB data + message payload → enqueue → send `waiting`.
3. `answer` message → validate against `CurrentQuestion` → if correct: compute damage (base 20 × weapon modifier, reduced by opponent armor modifier) → `ClassEffectService.ApplyOnCorrectAnswer` → `ApplyOnHit` on defender → update HPs → log action → send `hit` to both → assign new question to answerer. If wrong: send new question only.
4. `skip` message → apply 10 dmg to self → log → send new question → send `hit` to both.
5. `release_charge` message → `ClassEffectService.ReleaseCharge` → apply damage → log → send `hit` to both.
6. HP ≤ 0 → `EloService.Calculate` → update both players in DB → `MatchLogService.SaveLog` → send `game_over` to both → clean up session.
7. WS disconnect → if in active match → opponent wins → same end-match flow as step 6.

### New endpoints
```
GET  /Armor/GetAllArmors
GET  /Player/GetEquipped     → { weaponId, armorId, classType, elo }
POST /Player/Equip           body: { weaponId?: string, armorId?: string, classType: int }
```

---

## Frontend — Files to Create / Modify

### New files
| File | Purpose |
|------|---------|
| `src/apiTypes/match.types.ts` | All WS message types (server→client, client→server), `MatchAction`, `Armor`, `EquipRequest` |
| `src/hooks/useMatch.ts` | Custom hook wrapping `react-use-websocket`, manages all match state |
| `src/components/Game/EquipModal.tsx` | Pre-queue equip + class + skill tier selection modal |

### Modified files
| File | Changes |
|------|---------|
| `src/components/Game/Matchmaking.tsx` | Full rewrite using `useMatch` hook |
| `src/helpers/queryBuilder.ts` | Add new endpoints: `/Armor/GetAllArmors`, `/Player/GetEquipped`, `/Player/Equip` |

### `useMatch` hook — state exposed
```ts
phase: 'idle' | 'waiting' | 'in_match' | 'game_over'
yourHp: number
opponentHp: number
yourClass: string
opponentName: string
opponentClass: string
currentQuestion: { id: string; text: string } | null
chargePoints: number
bleedStacks: number
cursedQuestionsRemaining: number
actionLog: MatchAction[]
eloChange: number
won: boolean
// methods
sendAnswer(questionId: string, value: number): void
sendSkip(): void
releaseCharge(): void
joinQueue(skillTier: number): void
```

### `EquipModal` props
```ts
onConfirm(skillTier: number): void
onClose(): void
```
- Fetches weapons (`GET /Weapons/GetAllWeapons`), armors (`GET /Armor/GetAllArmors`), current equipped (`GET /Player/GetEquipped`).
- Shows weapon list (radio), armor list (radio), class selector (Rogue/Berserker/Juggernaut/Wizard), skill tier selector (0–4, labelled Easy/Medium/Hard/Expert/Master).
- "None (no modifier)" option when player has no weapon/armor.
- On confirm: `POST /Player/Equip` → calls `onConfirm(skillTier)`.

### `Matchmaking.tsx` UI states
| Phase | UI |
|-------|----|
| `idle` | "Play" button → opens `EquipModal` |
| `waiting` | "Waiting for opponent…" + cancel button |
| `in_match` | Your HP bar (labelled with class), opponent HP bar, active effects row (bleed stacks, curse indicator, charge bar for Berserker), question text, number input + Submit + Skip, "Release Charge" button (Berserker only, when chargePoints > 0), scrollable action log (auto-scrolls to bottom) |
| `game_over` | "Victory!" or "Defeat", Elo change (`+X` / `-X`), Back button |

### Frontend constraints
- No new npm packages.
- Use existing `AxInput` and `MenuItem` components where they fit.
- Strict TypeScript — no `any`, no `!` assertions.
- Auto-focus answer input when a new question arrives.
- Answer field is a number input.

---

## Constraints (both sides)

- Use `System.Text.Json` throughout (backend).
- No new NuGet packages beyond what's already in `ArixBack.csproj`.
- Match state is in-memory only — no DB persistence except `MatchLog` at match end and Elo updates.
- Leave `SpecialEffect` fields on Weapon and Armor as nullable strings — do not implement any special effect logic yet, just preserve the field.
- Skill tree is per-player choice at queue time (the `skillTier` in the `queue` message) — not per-match negotiation.

---

## Orchestrator Instructions

1. Spawn `backend-coder` and `frontend-coder` **in parallel** on branch `feature/ws-match-game`.
2. Give `backend-coder` the backend section of this spec. Give `frontend-coder` the frontend section. Both should read the "Existing Codebase Context" and "WebSocket Message Protocol" sections.
3. Frontend can be built against the message protocol contract without waiting for backend to finish.
4. When both coders report done and builds are green, fan out to `tester` and `code-reviewer` in parallel.
5. Collect results and relay any issues back to the relevant coder. Repeat until both sign off.
