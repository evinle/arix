## ADDED Requirements

### Requirement: Player can join a matchmaking queue
The system SHALL provide a WebSocket endpoint for players to join a matchmaking queue.

#### Scenario: Joining the queue
- **WHEN** a player sends a `JOIN_QUEUE` message via WebSocket
- **THEN** the system adds the player to the in-memory queue with their current Elo and join timestamp

### Requirement: Elo-based matching with time-proportional expansion
The system SHALL attempt to match two players in the queue whose Elo difference is less than or equal to the maximum allowed discrepancy. The maximum allowed discrepancy MUST start at 200 and increase by 5 for every second the player has been waiting.

#### Scenario: Immediate match with similar Elo
- **WHEN** Player A (1200 Elo) and Player B (1250 Elo) are in the queue
- **THEN** the system matches them immediately because the 50 Elo difference is within the 200 base range

#### Scenario: Match after waiting
- **WHEN** Player A (1000 Elo) has waited 30 seconds and Player B (1300 Elo) joins
- **THEN** the system matches them because the 300 Elo difference is within the expanded range (200 + 5 * 30 = 350)

### Requirement: Match initialization
The system SHALL remove matched players from the queue and notify both players of the match.

#### Scenario: Match found notification
- **WHEN** two players are matched
- **THEN** both players receive a `MATCH_FOUND` message containing the opponent's name and initial match state
