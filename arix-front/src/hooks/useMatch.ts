import { useCallback, useReducer } from "react";
import useWebSocket from "react-use-websocket";
import { ARIX_SERVER_ORIGIN } from "../helpers/queryBuilder";
import { useLocalStorage } from "./useLocalStorage";
import type {
  ClientMsg,
  MatchAction,
  MatchQuestion,
  ServerMsg
} from "../apiTypes/match.types";

type Phase = "idle" | "waiting" | "in_match" | "game_over";

type MatchState = {
  phase: Phase;
  yourHp: number;
  opponentHp: number;
  yourClass: string;
  opponentName: string;
  opponentClass: string;
  currentQuestion: MatchQuestion | null;
  chargePoints: number;
  bleedStacks: number;
  cursedQuestionsRemaining: number;
  actionLog: MatchAction[];
  eloChange: number;
  won: boolean;
};

const INITIAL_STATE: MatchState = {
  phase: "idle",
  yourHp: 0,
  opponentHp: 0,
  yourClass: "",
  opponentName: "",
  opponentClass: "",
  currentQuestion: null,
  chargePoints: 0,
  bleedStacks: 0,
  cursedQuestionsRemaining: 0,
  actionLog: [],
  eloChange: 0,
  won: false
};

type Action =
  | { type: "SET_WAITING" }
  | {
      type: "MATCH_START";
      opponentName: string;
      opponentClass: string;
      yourHp: number;
      opponentHp: number;
      question: MatchQuestion;
      yourClass: string;
    }
  | { type: "NEW_QUESTION"; question: MatchQuestion }
  | {
      type: "HIT";
      yourHp: number;
      opponentHp: number;
      damageDealt: number;
      damageTaken: number;
      effect: string | null;
    }
  | { type: "BLEED_TICK"; yourHp: number; amount: number }
  | { type: "OPPONENT_BLEED"; opponentHp: number; amount: number }
  | { type: "CURSE_APPLIED"; questionsAffected: number }
  | { type: "CURSE_REMOVED" }
  | {
      type: "GAME_OVER";
      won: boolean;
      eloChange: number;
      log: MatchAction[];
    }
  | { type: "SET_YOUR_CLASS"; yourClass: string }
  | { type: "RESET" };

function reducer(state: MatchState, action: Action): MatchState {
  switch (action.type) {
    case "SET_WAITING":
      return { ...state, phase: "waiting" };
    case "MATCH_START":
      return {
        ...state,
        phase: "in_match",
        opponentName: action.opponentName,
        opponentClass: action.opponentClass,
        yourHp: action.yourHp,
        opponentHp: action.opponentHp,
        currentQuestion: action.question,
        yourClass: action.yourClass,
        chargePoints: 0,
        bleedStacks: 0,
        cursedQuestionsRemaining: 0,
        actionLog: []
      };
    case "NEW_QUESTION":
      return { ...state, currentQuestion: action.question };
    case "HIT": {
      const log: MatchAction[] = [
        ...state.actionLog,
        {
          actor: "hit",
          description: `Dealt ${action.damageDealt}, took ${action.damageTaken}${action.effect ? ` (${action.effect})` : ""}`
        }
      ];
      const parsed =
        state.yourClass === "Berserker" && action.effect?.startsWith("charge:")
          ? Number(action.effect.slice("charge:".length))
          : NaN;
      const chargePoints = !isNaN(parsed) ? parsed : state.chargePoints;
      return {
        ...state,
        yourHp: action.yourHp,
        opponentHp: action.opponentHp,
        actionLog: log,
        chargePoints
      };
    }
    case "BLEED_TICK":
      return {
        ...state,
        yourHp: action.yourHp,
        bleedStacks: Math.max(0, state.bleedStacks - 1),
        actionLog: [
          ...state.actionLog,
          { actor: "bleed", description: `Bleed tick: -${action.amount} HP` }
        ]
      };
    case "OPPONENT_BLEED":
      return {
        ...state,
        opponentHp: action.opponentHp,
        actionLog: [
          ...state.actionLog,
          { actor: "bleed", description: `Opponent bleed tick: -${action.amount} HP` }
        ]
      };
    case "CURSE_APPLIED":
      return {
        ...state,
        cursedQuestionsRemaining: action.questionsAffected
      };
    case "CURSE_REMOVED":
      return { ...state, cursedQuestionsRemaining: 0 };
    case "GAME_OVER":
      return {
        ...state,
        phase: "game_over",
        won: action.won,
        eloChange: action.eloChange,
        actionLog: action.log
      };
    case "SET_YOUR_CLASS":
      return { ...state, yourClass: action.yourClass };
    case "RESET":
      return INITIAL_STATE;
    default:
      return state;
  }
}

function parseServerMsg(raw: string): ServerMsg | null {
  try {
    return JSON.parse(raw) as ServerMsg;
  } catch {
    return null;
  }
}

export function useMatch(): MatchState & {
  sendAnswer: (questionId: string, value: number) => void;
  sendSkip: () => void;
  releaseCharge: () => void;
  joinQueue: (skillTier: number) => void;
  reset: () => void;
} {
  const { value: jwt } = useLocalStorage<string>("jwt");
  const wsUrl = `${ARIX_SERVER_ORIGIN.replace(/^http/, "ws")}/Websocket/ws?access_token=${jwt ?? ""}`;

  const [state, dispatch] = useReducer(reducer, INITIAL_STATE);

  const { sendMessage } = useWebSocket(wsUrl, {
    shouldReconnect: () => true,
    onMessage: (event: MessageEvent<string>) => {
      const msg = parseServerMsg(event.data);
      if (!msg) return;
      switch (msg.type) {
        case "waiting":
          dispatch({ type: "SET_WAITING" });
          break;
        case "match_start":
          dispatch({
            type: "MATCH_START",
            opponentName: msg.opponentName,
            opponentClass: msg.opponentClass,
            yourHp: msg.yourHp,
            opponentHp: msg.opponentHp,
            question: msg.question,
            yourClass: msg.yourClass
          });
          break;
        case "question":
          dispatch({
            type: "NEW_QUESTION",
            question: { id: msg.id, text: msg.text }
          });
          break;
        case "hit":
          dispatch({
            type: "HIT",
            yourHp: msg.yourHp,
            opponentHp: msg.opponentHp,
            damageDealt: msg.damageDealt,
            damageTaken: msg.damageTaken,
            effect: msg.effect
          });
          break;
        case "bleed_tick":
          dispatch({
            type: "BLEED_TICK",
            yourHp: msg.yourHp,
            amount: msg.amount
          });
          break;
        case "opponent_bleed":
          dispatch({
            type: "OPPONENT_BLEED",
            opponentHp: msg.opponentHp,
            amount: msg.amount
          });
          break;
        case "curse_applied":
          dispatch({
            type: "CURSE_APPLIED",
            questionsAffected: msg.questionsAffected
          });
          break;
        case "curse_removed":
          dispatch({ type: "CURSE_REMOVED" });
          break;
        case "game_over":
          dispatch({
            type: "GAME_OVER",
            won: msg.won,
            eloChange: msg.eloChange,
            log: msg.log
          });
          break;
      }
    }
  });

  const send = useCallback(
    (msg: ClientMsg) => sendMessage(JSON.stringify(msg)),
    [sendMessage]
  );

  const joinQueue = useCallback(
    (skillTier: number) => send({ type: "queue", skillTier }),
    [send]
  );

  const sendAnswer = useCallback(
    (questionId: string, value: number) =>
      send({ type: "answer", questionId, value }),
    [send]
  );

  const sendSkip = useCallback(
    () => send({ type: "skip" }),
    [send]
  );

  const releaseCharge = useCallback(
    () => send({ type: "release_charge" }),
    [send]
  );

  const reset = useCallback(() => dispatch({ type: "RESET" }), []);

  return {
    ...state,
    sendAnswer,
    sendSkip,
    releaseCharge,
    joinQueue,
    reset
  };
}
