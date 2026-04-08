## ADDED Requirements

### Requirement: Individual math problem generation
The system SHALL generate unique mental math problems for each player in a `MatchSession`. The problems MUST be different for each opponent.

#### Scenario: Unique problem delivery
- **WHEN** a match session starts or a problem is solved
- **THEN** each player receives their own math problem, distinct from their opponent's.

### Requirement: Difficulty-based problem categorization
The system SHALL categorize math problems into difficulty tiers based on operations:
- Tier 1: Addition and Subtraction.
- Tier 2: Multiplication and Division.
- Tier 3: Square roots and Exponentials.

#### Scenario: Problem tier selection
- **WHEN** the system generates a problem
- **THEN** it MUST select a problem from a specific tier based on the player's current attributes and match state.

### Requirement: Attribute-driven difficulty and damage
The system SHALL use a player's attributes (class, weapon, and active buffs) to determine the difficulty tier of the problems they receive and the damage multiplier applied to correct answers.

#### Scenario: Weapon-based difficulty scaling
- **WHEN** a player is equipped with a high-tier weapon (e.g., "Mage's Staff")
- **THEN** the system generates Tier 2 or 3 problems for that player, but correct answers deal significantly higher damage.

### Requirement: Server-side answer validation and HP tracking
The system SHALL validate answers from each player. The system MUST track HP for both players and terminate the match when a player's HP reaches zero.

#### Scenario: Correct answer deals damage
- **WHEN** Player A submits the correct answer to their specific problem
- **THEN** the system calculates damage (based on problem difficulty and attributes) and applies it to the opponent, broadcasting the new HP state.

#### Scenario: Player defeat
- **WHEN** a player's HP reaches 0
- **THEN** the system sends a `MATCH_TERMINATED` message declaring the winner.
