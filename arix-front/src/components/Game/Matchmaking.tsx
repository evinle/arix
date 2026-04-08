import { useEffect, useState } from "react";
import useWebSocket, { ReadyState } from "react-use-websocket";
import { ARIX_SERVER_ORIGIN } from "../../helpers/queryBuilder";
import MenuItem from "../Menu/MenuItem";
import { useNavigate } from "react-router";
import { useLocalStorage } from "../../hooks/useLocalStorage";
import AxInput from "../Form/AxInput";
import { useForm, useWatch } from "react-hook-form";

interface GameState {
  matchId: string;
  opponentName: string;
  playerHP: number;
  opponentHP: number;
  currentProblem: string;
  status: "QUEUE" | "MATCH_FOUND" | "BATTLE" | "FINISHED";
  winnerId?: string;
  playersInQueue?: number;
}

const Matchmaking = () => {
  const { value: jwt } = useLocalStorage<string>("jwt");
  const wsUrl = `${ARIX_SERVER_ORIGIN.replace(/^http/, "ws")}/Websocket/ws?access_token=${jwt}`;

  const [gameState, setGameState] = useState<GameState>({
    matchId: "",
    opponentName: "",
    playerHP: 100,
    opponentHP: 100,
    currentProblem: "",
    status: "QUEUE",
  });

  const { control, setValue } = useForm<{
    answer: string | null;
  }>();
  const [answerToSend] = useWatch({
    control,
    name: ["answer"],
  });

  const { sendMessage, readyState } = useWebSocket(wsUrl, {
    shouldReconnect: () => true,
    onOpen: () => {
      // Auto-join queue for now
      sendMessage(JSON.stringify({ type: "JOIN_QUEUE", payload: {} }));
    },
    onMessage: (m) => {
      const data = JSON.parse(m.data);
      const { type, payload } = data;

      switch (type) {
        case "JOINED_QUEUE":
          setGameState((prev) => ({
            ...prev,
            playersInQueue: payload.playersInQueue,
          }));
          break;

        case "MATCH_FOUND":
          setGameState((prev) => ({
            ...prev,
            matchId: payload.matchId,
            opponentName: payload.playerB.username, // Simplified
            status: "BATTLE",
          }));
          break;

        case "NEW_PROBLEM":
          setGameState((prev) => ({
            ...prev,
            currentProblem: payload.text,
          }));
          break;

        case "BATTLE_UPDATE":
          // Need to know which player we are to update HP correctly. 
          // For simplicity, let's assume we update both.
          setGameState((prev) => ({
            ...prev,
            playerHP: payload.hpA,
            opponentHP: payload.hpB,
          }));
          break;

        case "MATCH_TERMINATED":
          setGameState((prev) => ({
            ...prev,
            status: "FINISHED",
            winnerId: payload.winnerId,
          }));
          break;
      }
    },
  });

  const submitAnswer = () => {
    sendMessage(JSON.stringify({
      type: "SUBMIT_ANSWER",
      payload: { answer: String(answerToSend) }
    }));
    setValue("answer", "");
  };

  const connectionStatus = {
    [ReadyState.CONNECTING]: "Connecting",
    [ReadyState.OPEN]: "Open",
    [ReadyState.CLOSING]: "Closing",
    [ReadyState.CLOSED]: "Closed",
    [ReadyState.UNINSTANTIATED]: "Uninstantiated",
  }[readyState];

  const navigate = useNavigate();

  return (
    <div style={{ padding: "20px", textAlign: "center", color: "white" }}>
      <h1>Mathletics</h1>
      <div>Status: {connectionStatus}</div>
      <hr />

      {gameState.status === "QUEUE" && (
        <div>
          <h2>Searching for opponent...</h2>
          {gameState.playersInQueue !== undefined && (
            <h3>Players in queue: {gameState.playersInQueue}</h3>
          )}
          <div className="loader"></div>
        </div>
      )}

      {gameState.status === "BATTLE" && (
        <div>
          <div style={{ display: "flex", justifyContent: "space-around", marginBottom: "20px" }}>
            <div>
              <h3>You</h3>
              <div style={{ fontSize: "24px", color: gameState.playerHP < 30 ? "red" : "green" }}>
                HP: {gameState.playerHP}
              </div>
            </div>
            <div style={{ fontSize: "40px" }}>VS</div>
            <div>
              <h3>{gameState.opponentName}</h3>
              <div style={{ fontSize: "24px", color: gameState.opponentHP < 30 ? "red" : "green" }}>
                HP: {gameState.opponentHP}
              </div>
            </div>
          </div>

          <div style={{ margin: "40px 0" }}>
            <h2 style={{ fontSize: "48px" }}>{gameState.currentProblem}</h2>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                submitAnswer();
              }}
              style={{ display: "flex", justifyContent: "center", alignItems: "center", gap: "10px" }}
            >
              <AxInput control={control} name="answer" label="Your Answer" />
              <MenuItem
                config={{
                  id: "Submit",
                  label: "Solve",
                  onClick: submitAnswer,
                }}
              />
            </form>
          </div>
        </div>
      )}

      {gameState.status === "FINISHED" && (
        <div>
          <h2>Game Over!</h2>
          <h1>{gameState.winnerId ? "Winner Decided!" : "Match Ended"}</h1>
          <MenuItem
            config={{
              id: "rematch",
              label: "Play Again",
              onClick: () => window.location.reload(),
            }}
          />
        </div>
      )}

      <div style={{ marginTop: "40px" }}>
        <MenuItem
          config={{
            id: "back",
            label: "Back to Menu",
            onClick: () => navigate("/"),
          }}
        />
      </div>
    </div>
  );
};

export default Matchmaking;
