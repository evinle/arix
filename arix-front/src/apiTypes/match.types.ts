export type MatchQuestion = { id: string; text: string };

// Server → Client
export type ServerMsg =
  | { type: "waiting" }
  | {
      type: "match_start";
      opponentName: string;
      opponentClass: string;
      yourClass: string;
      yourHp: number;
      opponentHp: number;
      question: MatchQuestion;
      skillTier: number;
    }
  | { type: "question"; id: string; text: string }
  | {
      type: "hit";
      yourHp: number;
      opponentHp: number;
      damageDealt: number;
      damageTaken: number;
      effect: string | null;
    }
  | { type: "bleed_tick"; yourHp: number; amount: number }
  | { type: "opponent_bleed"; opponentHp: number; amount: number }
  | { type: "curse_applied"; questionsAffected: number }
  | { type: "curse_removed" }
  | {
      type: "game_over";
      won: boolean;
      eloChange: number;
      log: MatchAction[];
    };

// Client → Server
export type ClientMsg =
  | { type: "queue"; skillTier: number }
  | { type: "answer"; questionId: string; value: number }
  | { type: "skip" }
  | { type: "release_charge" };

export type MatchAction = {
  actor: string;
  description: string;
};

export type Armor = {
  id: string;
  armorName: string;
};

export type EquipRequest = {
  weaponId: string | null;
  armorId: string | null;
  playerClass: string;
};

export type EquippedResponse = {
  weaponId: string | null;
  armorId: string | null;
  playerClass: string;
};
