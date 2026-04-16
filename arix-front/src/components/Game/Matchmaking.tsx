import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { useMatch } from "../../hooks/useMatch";
import { EquipModal } from "./EquipModal";
import MenuItem from "../Menu/MenuItem";

const Matchmaking = (): React.ReactElement => {
  const navigate = useNavigate();
  const [showEquip, setShowEquip] = useState(false);
  const [yourClass, setYourClass] = useState("Rogue");

  const match = useMatch(yourClass);

  const [answer, setAnswer] = useState("");
  const answerRef = useRef<HTMLInputElement>(null);
  const logRef = useRef<HTMLDivElement>(null);

  // Auto-focus answer input on new question
  useEffect(() => {
    if (match.currentQuestion) {
      answerRef.current?.focus();
      setAnswer("");
    }
  }, [match.currentQuestion?.id]);

  // Auto-scroll action log
  useEffect(() => {
    if (logRef.current) {
      logRef.current.scrollTop = logRef.current.scrollHeight;
    }
  }, [match.actionLog]);

  const handleConfirm = (skillTier: number, playerClass: string): void => {
    setYourClass(playerClass);
    setShowEquip(false);
    match.joinQueue(skillTier);
  };

  const handleSubmitAnswer = (): void => {
    if (!match.currentQuestion || answer === "") return;
    match.sendAnswer(match.currentQuestion.id, Number(answer));
    setAnswer("");
  };

  if (match.phase === "idle") {
    return (
      <div className="flex flex-col items-center gap-4">
        {showEquip && (
          <EquipModal
            onConfirm={handleConfirm}
            onClose={() => setShowEquip(false)}
          />
        )}
        <MenuItem
          config={{
            id: "play",
            label: "Play",
            onClick: () => setShowEquip(true)
          }}
        />
      </div>
    );
  }

  if (match.phase === "waiting") {
    return (
      <div className="flex flex-col items-center gap-4">
        <p className="text-xl">Waiting for opponent…</p>
        <MenuItem
          config={{
            id: "cancel",
            label: "Cancel",
            onClick: () => match.reset()
          }}
        />
      </div>
    );
  }

  if (match.phase === "game_over") {
    const eloSign = match.eloChange >= 0 ? "+" : "";
    return (
      <div className="flex flex-col items-center gap-4">
        <p className="text-4xl font-bold">
          {match.won ? "Victory!" : "Defeat"}
        </p>
        <p className="text-xl">
          Elo: {eloSign}{match.eloChange}
        </p>
        <MenuItem
          config={{
            id: "back",
            label: "Back",
            onClick: () => {
              match.reset();
              navigate("/");
            }
          }}
        />
      </div>
    );
  }

  // in_match
  const { yourHp, opponentHp, currentQuestion } = match;

  return (
    <div className="flex w-full max-w-2xl flex-col gap-4 p-4">
      {/* HP bars */}
      <div className="flex flex-col gap-2">
        <div>
          <p className="text-sm font-semibold">
            You ({match.yourClass}) — {yourHp} HP
          </p>
          <div className="h-4 w-full rounded bg-gray-700">
            <div
              className="h-4 rounded bg-green-500 transition-all"
              style={{ width: `${Math.max(0, yourHp)}px`, maxWidth: "100%" }}
            />
          </div>
        </div>
        <div>
          <p className="text-sm font-semibold">
            {match.opponentName} ({match.opponentClass}) — {opponentHp} HP
          </p>
          <div className="h-4 w-full rounded bg-gray-700">
            <div
              className="h-4 rounded bg-red-500 transition-all"
              style={{ width: `${Math.max(0, opponentHp)}px`, maxWidth: "100%" }}
            />
          </div>
        </div>
      </div>

      {/* Active effects */}
      <div className="flex gap-3 text-sm">
        {match.bleedStacks > 0 && (
          <span className="rounded bg-red-900 px-2 py-1">
            🩸 Bleed ×{match.bleedStacks}
          </span>
        )}
        {match.cursedQuestionsRemaining > 0 && (
          <span className="rounded bg-purple-900 px-2 py-1">
            💀 Cursed ({match.cursedQuestionsRemaining})
          </span>
        )}
        {match.yourClass === "Berserker" && match.chargePoints > 0 && (
          <span className="rounded bg-orange-900 px-2 py-1">
            ⚡ Charge: {match.chargePoints}
          </span>
        )}
      </div>

      {/* Question */}
      {currentQuestion && (
        <div className="rounded-xl bg-gray-800 p-4">
          <p className="text-lg">{currentQuestion.text}</p>
          <div className="mt-3 flex gap-2">
            <input
              ref={answerRef}
              type="number"
              value={answer}
              onChange={(e) => setAnswer(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleSubmitAnswer();
              }}
              className="w-32 rounded-md bg-gray-900 px-2 text-lg outline-1 outline-pink-300"
              aria-label="Answer"
            />
            <button
              type="button"
              className="rounded-xl bg-blue-900 px-4 py-2 font-bold uppercase"
              onClick={handleSubmitAnswer}
            >
              Submit
            </button>
            <button
              type="button"
              className="rounded-xl bg-gray-700 px-4 py-2 font-bold uppercase"
              onClick={() => match.sendSkip()}
            >
              Skip
            </button>
          </div>
        </div>
      )}

      {/* Release Charge (Berserker only) */}
      {match.yourClass === "Berserker" && match.chargePoints > 0 && (
        <button
          type="button"
          className="rounded-xl bg-orange-700 px-4 py-2 font-bold uppercase"
          onClick={() => match.releaseCharge()}
        >
          Release Charge ({match.chargePoints})
        </button>
      )}

      {/* Action log */}
      <div
        ref={logRef}
        className="h-40 overflow-y-auto rounded-xl bg-gray-800 p-3 text-sm"
      >
        {match.actionLog.map((entry, i) => (
          <p key={i} className="leading-5">
            <span className="font-semibold text-pink-300">{entry.actor}:</span>{" "}
            {entry.description}
          </p>
        ))}
      </div>
    </div>
  );
};

export default Matchmaking;
