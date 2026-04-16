using ArixBack.Models;

namespace ArixBack.Services
{
    public record EffectResult(int DamageToOpponent, int DamageToSelf, int HealSelf, string? EffectMessage);

    public class ClassEffectService
    {
        private static readonly Random _rng = Random.Shared;
        private const int MaxBleedStacks = 3;
        private const int BleedTicksPerStack = 4;
        private const int BleedDamagePerTick = 5;

        public EffectResult ApplyOnCorrectAnswer(PlayerMatchState attacker, PlayerMatchState defender, int baseDamage)
        {
            int dmgToOpponent = baseDamage;
            int dmgToSelf = 0;
            int healSelf = 0;
            string? effect = null;

            switch (attacker.ClassType)
            {
                case ClassType.Rogue:
                    attacker.CorrectStreak++;
                    if (attacker.CorrectStreak >= 3 && defender.BleedStacks < MaxBleedStacks)
                    {
                        defender.BleedStacks++;
                        defender.BleedTicksRemaining += BleedTicksPerStack;
                        effect = $"bleed_applied:{defender.BleedStacks}";
                    }
                    break;

                case ClassType.Berserker:
                    attacker.ChargePoints += 15;
                    dmgToOpponent = 0; // damage only on release
                    effect = $"charge:{attacker.ChargePoints}";
                    break;

                case ClassType.Wizard:
                    healSelf = 5;
                    if (_rng.NextDouble() < 0.30 && defender.CursedQuestionsRemaining == 0)
                    {
                        defender.CursedQuestionsRemaining = 3;
                        effect = "curse_applied";
                    }
                    break;
            }

            return new EffectResult(dmgToOpponent, dmgToSelf, healSelf, effect);
        }

        public EffectResult ApplyOnHit(PlayerMatchState defender, int incomingDamage)
        {
            int dmgToOpponent = 0;
            int reducedDamage = incomingDamage;
            string? effect = null;

            if (defender.ClassType == ClassType.Juggernaut)
            {
                reducedDamage = (int)(incomingDamage * 0.8);
                dmgToOpponent = 5; // reflect
                effect = "juggernaut_reflect";
            }

            return new EffectResult(dmgToOpponent, reducedDamage, 0, effect);
        }

        public int TickBleed(PlayerMatchState player)
        {
            if (player.BleedStacks <= 0 || player.BleedTicksRemaining <= 0) return 0;
            int damage = player.BleedStacks * BleedDamagePerTick;
            player.BleedTicksRemaining--;
            if (player.BleedTicksRemaining <= 0)
                player.BleedStacks = 0;
            return damage;
        }

        public int ReleaseCharge(PlayerMatchState berserker)
        {
            int charge = berserker.ChargePoints;
            berserker.ChargePoints = 0;
            return charge;
        }
    }
}
