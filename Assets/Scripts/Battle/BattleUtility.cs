using SupeRPG.Game;

using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SupeRPG.Battle
{
    public struct PlayerOutcomeInfo
    {
        public struct EnemyInfo
        {
            public int DamageDealt;
            public bool Engaged;
            public bool Missed;
            public bool Killed;
            public string[] Status;
        }

        public int HealReceived;
        public string[] Status;
        public EnemyInfo[] EnemyInfos;

        public PlayerOutcomeInfo(int enemyCount)
        {
            this.Status = null;
            this.HealReceived = 0;
            this.EnemyInfos = enemyCount == 0 ? Array.Empty<EnemyInfo>() : new EnemyInfo[enemyCount];
        }
    }

    public struct EnemyOutcomeInfo
    {
        public int HealReceived;
        public string[] EnemyStatus;
        public string[] PlayerStatus;
        public bool EngagedPlayer;
        public bool MissedPlayer;
        public bool KilledPlayer;
        public int DamageDealt;
    }

    public static class BattleUtility
    {
        public static bool CheckIfHits(float precision, float evasion)
        {
            const float kPrecisionAdditive = 2.0f;

            // evasion is 0.0f to 1.0f
            // precision is 0.0f to infinity

            if (evasion <= 0.0f)
            {
                return true; // if dodge is 0%, always hits
            }

            if (evasion >= 1.0f)
            {
                return false; // if dodge is 100%, never hits (should never happen though?)
            }

            var chance = kPrecisionAdditive * precision * (1.0f - evasion);

            if (chance >= 100.0f)
            {
                return true; // if too much precision, always hits
            }

            return Random.Range(0, 100) < (int)chance;
        }

        public static int CalculateDamage(int damage, int armor, float critChance, float critMultiplier)
        {
            float applied = damage;

            if (critChance >= 1.0f || (critChance > 0.0f && Random.Range(0, 100) < (int)(critChance * 100.0f)))
            {
                applied *= critMultiplier; // if crit chance is 100% or if it applies in general
            }

            return (int)(applied * (100.0f / (100.0f + armor)));
        }

        public static PlayerOutcomeInfo Attack(Player player, Enemy[] enemies, int enemyIndex, int abilityUsed)
        {
            if (player is null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (enemies is null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            var outcome = new PlayerOutcomeInfo(enemies.Length);

            if (abilityUsed < 0 || abilityUsed > player.Abilities.Count)
            {
                var enemy = enemies[enemyIndex];

                Debug.Assert(enemy is not null && enemy.IsAlive);

                ref readonly var playerStats = ref player.EntityStats;
                ref readonly var enemysStats = ref enemy.EntityStats;

                ref var info = ref outcome.EnemyInfos[enemyIndex];

                info.Engaged = true;

                if (CheckIfHits(playerStats.Precision, enemysStats.Evasion))
                {
                    int damage = CalculateDamage(playerStats.Damage, enemysStats.Armor, playerStats.CritChance, playerStats.CritMultiplier);

                    enemy.ApplyDamage(damage);

                    info.DamageDealt = damage;

                    info.Killed = !enemy.IsAlive;
                }
                else
                {
                    info.Missed = true;
                }
            }
            else
            {
                if (player.CanUseAbility(abilityUsed) != AbilityUsage.CanUse)
                {
                    throw new Exception($"Cannot use ability at index {abilityUsed}");
                }

                var ability = player.Abilities[abilityUsed];

                ability.PutOnCooldown();

                player.RemoveMana(ability.ManaCost);

                int currentHealth = player.EntityStats.CurHealth;
                int currentLength = player.Effects.Count;

                ability.ApplyAllyEffects(player);

                if (currentLength < player.Effects.Count)
                {
                    outcome.Status = GetNewStatusNames(player.Effects, currentLength);

                    player.ApplyImmediateEffects();

                    if (player.TurnStats.RemoveNegativeEffects)
                    {
                        player.RemoveEffectsOfSide(EffectSide.Negative);
                    }

                    if (currentHealth < player.EntityStats.CurHealth)
                    {
                        outcome.HealReceived = player.EntityStats.CurHealth - currentHealth;
                    }
                }

                if (ability.DoesDamage)
                {
                    Debug.Assert(enemyIndex >= 0);

                    for (int i = 0; i < enemies.Length; ++i)
                    {
                        var enemy = enemies[i];

                        if (enemy is not null && enemy.IsAlive)
                        {
                            int count = enemy.Effects.Count;

                            ability.ApplyEnemyEffects(player, enemy, i == enemyIndex);

                            if (count < enemy.Effects.Count)
                            {
                                outcome.EnemyInfos[i].Status = GetNewStatusNames(enemy.Effects, count);

                                enemy.ApplyImmediateEffects();

                                if (enemy.TurnStats.RemovePositiveEffects)
                                {
                                    enemy.RemoveEffectsOfSide(EffectSide.Positive);
                                }
                            }
                        }
                    }

                    ref readonly var stats = ref player.EntityStats;

                    int trueDamage = (int)(stats.Damage * ability.DamageMultiplier);

                    if (ability.IsAreaOfEffect)
                    {
                        for (int i = 0; i < enemies.Length; ++i)
                        {
                            var enemy = enemies[i];

                            if (enemy is not null && enemy.IsAlive)
                            {
                                ref var info = ref outcome.EnemyInfos[i];

                                int damage = CalculateDamage(trueDamage, enemy.EntityStats.Armor, stats.CritChance, stats.CritMultiplier);

                                enemy.ApplyDamage(damage);

                                info.Engaged = true;

                                info.DamageDealt = damage;

                                info.Killed = !enemy.IsAlive;
                            }
                        }
                    }
                    else
                    {
                        var enemy = enemies[enemyIndex];

                        Debug.Assert(enemy is not null && enemy.IsAlive);

                        ref var info = ref outcome.EnemyInfos[enemyIndex];

                        int damage = CalculateDamage(trueDamage, enemy.EntityStats.Armor, stats.CritChance, stats.CritMultiplier);

                        enemy.ApplyDamage(damage);

                        info.Engaged = true;

                        info.DamageDealt = damage;

                        info.Killed = !enemy.IsAlive;
                    }
                }
            }

            return outcome;
        }

        public static EnemyOutcomeInfo Attack(Enemy enemy, Player player, int abilityUsed)
        {
            if (enemy is null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (player is null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var outcome = new EnemyOutcomeInfo();

            if (abilityUsed < 0)
            {
                ref readonly var playerStats = ref player.EntityStats;
                ref readonly var enemysStats = ref enemy.EntityStats;

                outcome.EngagedPlayer = true;

                if (CheckIfHits(enemysStats.Precision, playerStats.Evasion))
                {
                    int damage = CalculateDamage(enemysStats.Damage, playerStats.Armor, enemysStats.CritChance, enemysStats.CritMultiplier);

                    player.ApplyDamage(damage);

                    outcome.DamageDealt = damage;

                    outcome.KilledPlayer = !player.IsAlive;
                }
                else
                {
                    outcome.MissedPlayer = true;
                }
            }
            else
            {
                var ability = enemy.Abilities[abilityUsed];

                ability.PutOnCooldown();

                int currentHealth = enemy.EntityStats.CurHealth;
                int currentLength = enemy.Effects.Count;

                ability.ApplyAllyEffects(enemy);

                if (currentLength < enemy.Effects.Count)
                {
                    outcome.EnemyStatus = GetNewStatusNames(enemy.Effects, currentLength);

                    enemy.ApplyImmediateEffects();

                    if (enemy.TurnStats.RemoveNegativeEffects)
                    {
                        enemy.RemoveEffectsOfSide(EffectSide.Negative);
                    }

                    if (currentHealth < enemy.EntityStats.CurHealth)
                    {
                        outcome.HealReceived = enemy.EntityStats.CurHealth - currentHealth;
                    }
                }

                if (ability.DoesDamage && player.IsAlive)
                {
                    int count = player.Effects.Count;

                    ability.ApplyEnemyEffects(enemy, player, true);

                    if (count < player.Effects.Count)
                    {
                        outcome.PlayerStatus = GetNewStatusNames(player.Effects, count);

                        player.ApplyImmediateEffects();

                        if (player.TurnStats.RemovePositiveEffects)
                        {
                            player.RemoveEffectsOfSide(EffectSide.Positive);
                        }
                    }

                    ref readonly var playerStats = ref player.EntityStats;
                    ref readonly var enemysStats = ref enemy.EntityStats;

                    int damage = CalculateDamage((int)(enemysStats.Damage * ability.DamageMultiplier), playerStats.Armor, enemysStats.CritChance, enemysStats.CritMultiplier);

                    player.ApplyDamage(damage);

                    outcome.EngagedPlayer = true;

                    outcome.DamageDealt = damage;

                    outcome.KilledPlayer = !player.IsAlive;
                }
            }

            return outcome;
        }

        public static int GetAbilityUsedByEnemy(Enemy enemy)
        {
            var abilities = enemy.Abilities;
            int available = 0;

            for (int i = abilities.Count - 1; i >= 0; --i)
            {
                if (!abilities[i].IsOnCooldown)
                {
                    available++;
                }
            }

            if (available == 0)
            {
                return -1;
            }

            Span<int> indices = stackalloc int[available + 1];

            indices[0] = -1;

            for (int i = abilities.Count - 1; i >= 0; --i)
            {
                if (!abilities[i].IsOnCooldown)
                {
                    indices[available--] = i;
                }
            }

            return indices[Random.Range(0, indices.Length)];
        }

        private static string[] GetNewStatusNames(IReadOnlyList<Effect> effects, int start)
        {
            int count = 0;

            for (int i = effects.Count - 1; i >= start; --i)
            {
                if (!String.IsNullOrEmpty(effects[i].Status))
                {
                    count++;
                }
            }

            var result = new string[count];

            for (int i = effects.Count - 1; i >= start; --i)
            {
                if (!String.IsNullOrEmpty(effects[i].Status))
                {
                    result[--count] = effects[i].Status;
                }
            }

            return result;
        }
    }
}
