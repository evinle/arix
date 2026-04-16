import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { queryFnBuilder, ARIX_SERVER_ORIGIN } from "../../helpers/queryBuilder";
import type { Armor, EquipRequest, EquippedResponse } from "../../apiTypes/match.types";
import type { Weapon } from "../../apiTypes/weapons.type";

type EquipModalProps = {
  onConfirm: (skillTier: number) => void;
  onClose: () => void;
};

const CLASSES = ["Rogue", "Berserker", "Juggernaut", "Wizard"] as const;
const SKILL_TIERS = ["Easy", "Medium", "Hard", "Expert", "Master"] as const;

export function EquipModal({ onConfirm, onClose }: EquipModalProps): React.ReactElement {
  const { data: weapons } = useQuery({
    queryKey: ["weapons"],
    queryFn: queryFnBuilder<Weapon[]>("/Weapons/GetAllWeapons")
  });
  const { data: armors } = useQuery({
    queryKey: ["armors"],
    queryFn: queryFnBuilder<Armor[]>("/Armor/GetAllArmors")
  });
  const { data: equipped } = useQuery({
    queryKey: ["equipped"],
    queryFn: queryFnBuilder<EquippedResponse>("/Player/GetEquipped")
  });

  const [weaponId, setWeaponId] = useState<string | null>(null);
  const [armorId, setArmorId] = useState<string | null>(null);
  const [playerClass, setPlayerClass] = useState<string>(CLASSES[0]);
  const [skillTier, setSkillTier] = useState(0);

  // Sync defaults from equipped once loaded
  const [synced, setSynced] = useState(false);
  if (equipped && !synced) {
    setWeaponId(equipped.weaponId);
    setArmorId(equipped.armorId);
    setPlayerClass(equipped.playerClass || CLASSES[0]);
    setSynced(true);
  }

  const handleConfirm = async (): Promise<void> => {
    const body: EquipRequest = { weaponId, armorId, playerClass };
    await fetch(`${ARIX_SERVER_ORIGIN}/Player/Equip`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + localStorage.getItem("jwt")
      },
      credentials: "include",
      body: JSON.stringify(body)
    });
    onConfirm(skillTier);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="flex w-96 flex-col gap-4 rounded-xl bg-gray-800 p-6 outline-2 outline-blue-200">
        <h2 className="text-2xl font-bold">Prepare for Battle</h2>

        <section>
          <p className="mb-1 font-semibold">Weapon</p>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              name="weapon"
              checked={weaponId === null}
              onChange={() => setWeaponId(null)}
            />
            None (no modifier)
          </label>
          {weapons?.map((w) => (
            <label key={w.id} className="flex items-center gap-2">
              <input
                type="radio"
                name="weapon"
                checked={weaponId === (w.id ?? null)}
                onChange={() => setWeaponId(w.id ?? null)}
              />
              {w.weaponName}
            </label>
          ))}
        </section>

        <section>
          <p className="mb-1 font-semibold">Armor</p>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              name="armor"
              checked={armorId === null}
              onChange={() => setArmorId(null)}
            />
            None (no modifier)
          </label>
          {armors?.map((a) => (
            <label key={a.id} className="flex items-center gap-2">
              <input
                type="radio"
                name="armor"
                checked={armorId === a.id}
                onChange={() => setArmorId(a.id)}
              />
              {a.armorName}
            </label>
          ))}
        </section>

        <section>
          <p className="mb-1 font-semibold">Class</p>
          <div className="flex gap-2">
            {CLASSES.map((c) => (
              <button
                key={c}
                type="button"
                className={`rounded px-2 py-1 text-sm ${playerClass === c ? "bg-blue-600" : "bg-gray-700"}`}
                onClick={() => setPlayerClass(c)}
              >
                {c}
              </button>
            ))}
          </div>
        </section>

        <section>
          <p className="mb-1 font-semibold">Skill Tier</p>
          <div className="flex gap-2">
            {SKILL_TIERS.map((label, i) => (
              <button
                key={label}
                type="button"
                className={`rounded px-2 py-1 text-sm ${skillTier === i ? "bg-blue-600" : "bg-gray-700"}`}
                onClick={() => setSkillTier(i)}
              >
                {label}
              </button>
            ))}
          </div>
        </section>

        <div className="flex justify-end gap-3">
          <button
            type="button"
            className="rounded-xl bg-gray-700 px-4 py-2 font-bold uppercase"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-xl bg-blue-900 px-4 py-2 font-bold uppercase"
            onClick={() => void handleConfirm()}
          >
            Play
          </button>
        </div>
      </div>
    </div>
  );
}
