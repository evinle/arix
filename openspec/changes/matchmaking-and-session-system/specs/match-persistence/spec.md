## ADDED Requirements

### Requirement: Persistent match event log
The system SHALL record every action and state change in a `MatchSession` in chronological order.

#### Scenario: Event recording
- **WHEN** a problem is generated, an answer is submitted, or a match starts/ends
- **THEN** the system adds the event with a timestamp to the in-memory log for that match.

### Requirement: Match flush to persistent storage
The system SHALL persist the complete event log of a `MatchSession` to the `Matches` collection in MongoDB upon match termination.

#### Scenario: Match termination persistence
- **WHEN** the `MATCH_TERMINATED` event is triggered
- **THEN** the system writes the entire event log into a single document in MongoDB.

### Requirement: Replayable match data
The system SHALL provide a way to retrieve the `eventLog` of a finished match from MongoDB.

#### Scenario: Replay data retrieval
- **WHEN** a client requests match history with a `matchId`
- **THEN** the system returns the corresponding document from the `Matches` collection.
