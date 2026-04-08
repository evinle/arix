## 1. Data Models & Database

- [ ] 1.1 Update `Player` model in `ArixBack/Models/Player.cs` to include `Elo` (default 1200).
- [ ] 1.2 Create `Match` model for MongoDB persistence in `ArixBack/Models/Match.cs`.
- [ ] 1.3 Add `Matches` collection to `DatabaseService.cs`.

## 2. Matchmaking Service

- [ ] 2.1 Implement `MatchmakingService` singleton to manage the player queue.
- [ ] 2.2 Create a `MatchmakingBackgroundWorker` to process the queue every second.
- [ ] 2.3 Implement the Elo expansion logic: `200 + (5 * waitTime)`.

## 3. Match Session & Engine

- [ ] 3.1 Create `MatchSession` class to handle the game loop for two players.
- [ ] 3.2 Implement `MathProblemGenerator` with tiered difficulty (Add/Sub, Mul/Div, Sqrt/Exp).
- [ ] 3.3 Add logic to `MatchSession` for individual problem delivery and server-side validation.
- [ ] 3.4 Implement attribute-driven scaling (Class/Weapon influence on difficulty and damage).

## 4. WebSocket Communication

- [ ] 4.1 Update `Matchmaking` controller (or create a new one) to handle `JOIN_QUEUE` and `SUBMIT_ANSWER`.
- [ ] 4.2 Implement message broadcasting for `MATCH_FOUND`, `BATTLE_UPDATE`, and `MATCH_TERMINATED`.

## 5. Persistence & Logging

- [ ] 5.1 Implement in-memory `MatchEventLog` within `MatchSession`.
- [ ] 5.2 Add "flush to MongoDB" logic upon match termination.
- [ ] 5.3 Implement a basic endpoint to retrieve match logs for replays.

## 6. Frontend Integration

- [ ] 6.1 Update `Matchmaking.tsx` to handle the new structured WebSocket protocol.
- [ ] 6.2 Implement a basic "Battle" UI that displays unique problems and opponent HP.
