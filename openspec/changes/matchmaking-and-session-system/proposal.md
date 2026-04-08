## Why

To enable competitive multiplayer in the Mathletics game, we need a server-authoritative matchmaking and match session system that ensures fair play, real-time interaction, and persistent match history for replays.

## What Changes

- **Player Model Update**: Add Elo rating to the `Player` model.
- **Matchmaking Service**: Implement a singleton service that manages a queue and pairs players based on Elo and wait time.
- **Match Session**: Create a server-authoritative engine that generates math problems, validates answers, and calculates damage/HP in real-time.
- **WebSocket Protocol**: Update the communication layer to handle match-found events, action submission, and match termination.
- **Match Persistence**: Implement a "buffer and flush" system to store every action of a match into a `Matches` collection in MongoDB for future replays.

## Capabilities

### New Capabilities
- `matchmaking`: Handles player queueing, Elo-based matching with time-proportional discrepancy, and session initiation.
- `match-session`: Manages the real-time game loop, including problem generation, server-side validation, and state synchronization.
- `match-persistence`: Records and stores ordered match events for replayability and audit.

### Modified Capabilities
- `player-management`: Updated to include and persist Elo ratings.

## Impact

- **ArixBack**: Significant additions to Services and Models. New logic for WebSocket handling.
- **arix-front**: UI updates for matchmaking status and the real-time battle interface.
- **MongoDB**: New `Matches` collection and schema updates for the `Player` collection.
