using SupeRPG.Input;

using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SupeRPG.Game
{
    public class EnemyData : IDisposable
    {
        private string m_ability;
        private string m_name;
        private Sprite m_sprite;

        [Order(0)]
        public string Name
        {
            get => this.m_name;
            set => this.m_name = value ?? String.Empty;
        }

        [Order(1)]
        public int Armor { get; set; }

        [Order(2)]
        public float Precision { get; set; }

        [Order(3)]
        public float Evasion { get; set; }

        [Order(4)]
        public float CritChance { get; set; }

        [Order(5)]
        public float CritMultiplier { get; set; }

        [Order(6)]
        public string Ability
        {
            get => this.m_ability;
            set => this.m_ability = value ?? String.Empty;
        }

        [Order(7)]
        public Sprite Sprite
        {
            get => this.m_sprite;
            set => this.m_sprite = value == null ? ResourceManager.DefaultSprite : value;
        }

        public EnemyData()
        {
            this.m_name = String.Empty;
            this.m_ability = String.Empty;
            this.m_sprite = ResourceManager.DefaultSprite;
        }

        public void Dispose()
        {
        }
    }

    public class Enemy : IEntity
    {
        private readonly static string[] ms_cowNames = new string[]
        {
            "GrayRasterCow",
            "RedRasterCow",
            "BlueRasterCow",
            "GreenRasterCow",
        };

        private readonly List<Effect> m_effects;
        private readonly Ability[] m_abilities;
        private readonly EnemyData m_data;
        private readonly int m_baseDamage;
        private readonly int m_baseHealth;
        private EntityStats m_stats;
        private TurnStats m_turn;

        public Sprite Sprite => this.m_data.Sprite;

        public bool IsPlayer => false;

        public bool IsAlive => this.m_stats.CurHealth > 0;

        public bool IsMelee => true;

        public ref readonly EntityStats EntityStats => ref this.m_stats;

        public ref readonly TurnStats TurnStats => ref this.m_turn;

        public IReadOnlyList<Effect> Effects => this.m_effects;

        public IReadOnlyList<Ability> Abilities => this.m_abilities;

        public IReadOnlyList<Potion> EquippedPotions => Array.Empty<Potion>();

        public Enemy(string name, int health, int damage)
        {
            var data = ResourceManager.EnemyDatas.Find(_ => _.Name == name);
            
            if (data is null)
            {
                throw new Exception($"Enemy data named \"{name}\" could not be found");
            }

            var ability = ResourceManager.EnemyAbilityDatas.Find(_ => _.Name == data.Ability);

            if (ability is null)
            {
                throw new Exception($"Enemy Ability data named \"{data.Ability}\" could not be found");
            }

            this.m_data = data;
            this.m_baseHealth = Mathf.Max(0, health);
            this.m_baseDamage = Mathf.Max(0, damage);
            this.m_abilities = new Ability[1] { new Ability(this, ability) };
            this.m_effects = new List<Effect>();

            this.RecalculateStats();
        }

        public Enemy(EnemyData data, AbilityData ability, int health, int damage)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (ability is null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            this.m_data = data;
            this.m_baseHealth = Mathf.Max(0, health);
            this.m_baseDamage = Mathf.Max(0, damage);
            this.m_abilities = new Ability[1] { new Ability(this, ability) };
            this.m_effects = new List<Effect>();

            this.RecalculateStats();
        }

        private void RecalculateStats()
        {
            this.m_stats.MaxMana = 0;
            this.m_stats.MaxHealth = this.m_baseHealth;
            this.m_stats.Armor = this.m_data.Armor;
            this.m_stats.Damage = this.m_baseDamage;
            this.m_stats.Evasion = this.m_data.Evasion;
            this.m_stats.Precision = this.m_data.Precision;
            this.m_stats.CritChance = this.m_data.CritChance;
            this.m_stats.CritMultiplier = this.m_data.CritMultiplier + 1.0f;

            for (int i = 0; i < this.m_effects.Count; ++i)
            {
                this.m_effects[i].ModifyStats(ref this.m_stats, ref this.m_turn);
            }

            this.m_stats.CurMana = 0;
            this.m_stats.CurHealth = Mathf.Clamp(this.m_stats.CurHealth, 0, this.m_stats.MaxHealth);
            this.m_stats.Evasion = Mathf.Clamp01(this.m_stats.Evasion);
            this.m_stats.Precision = Mathf.Clamp(this.m_stats.Precision, 0.0f, Single.PositiveInfinity);
            this.m_stats.CritChance = Mathf.Clamp01(this.m_stats.CritChance);
        }



        public void InitBattle()
        {
            this.FinishBattle();

            this.m_stats.CurHealth = this.m_stats.MaxHealth;
            this.m_stats.CurMana = this.m_stats.MaxMana;
        }

        public void InitTurn()
        {
            this.m_turn = default;
        }

        public void FinishBattle()
        {
            this.m_effects.Clear();

            for (int i = 0; i < this.m_abilities.Length; ++i)
            {
                this.m_abilities[i].Reset();
            }

            this.RecalculateStats();
        }

        public void Regenerate()
        {
        }

        public void Cooldown(out int totalHeal, out int totalMana, out int totalDmgs)
        {
            int count = this.m_effects.Count;

            for (int i = 0; i < this.m_abilities.Length; ++i)
            {
                this.m_abilities[i].Cooldown();
            }

            totalHeal = 0;
            totalMana = 0;
            totalDmgs = 0;

            // positive, then neutral, then negative

            for (int i = this.m_effects.Count - 1; i >= 0; --i)
            {
                var effect = this.m_effects[i];

                if (effect.Side == EffectSide.Positive)
                {
                    int curHealth = this.m_stats.CurHealth;

                    effect.Cooldown(ref this.m_stats, ref this.m_turn);

                    totalHeal += this.m_stats.CurHealth - curHealth;

                    if (!effect.IsLasting)
                    {
                        this.m_effects.RemoveAt(i);
                    }
                }
            }

            for (int i = this.m_effects.Count - 1; i >= 0; --i)
            {
                var effect = this.m_effects[i];

                if (effect.Side == EffectSide.Neutral)
                {
                    effect.Cooldown(ref this.m_stats, ref this.m_turn);

                    if (!effect.IsLasting)
                    {
                        this.m_effects.RemoveAt(i);
                    }
                }
            }

            for (int i = this.m_effects.Count - 1; i >= 0; --i)
            {
                var effect = this.m_effects[i];

                if (effect.Side == EffectSide.Negative)
                {
                    int curHealth = this.m_stats.CurHealth;

                    effect.Cooldown(ref this.m_stats, ref this.m_turn);

                    totalDmgs += curHealth - this.m_stats.CurHealth;

                    if (!effect.IsLasting)
                    {
                        this.m_effects.RemoveAt(i);
                    }
                }
            }

            if (count > this.m_effects.Count)
            {
                this.RecalculateStats();
            }
        }



        public void ApplyDamage(int damage)
        {
            this.m_stats.CurHealth -= damage;

            if (this.m_stats.CurHealth < 0)
            {
                this.m_stats.CurHealth = 0;
            }
        }

        public void AddEffect(Effect effect)
        {
            this.m_effects.Add(effect);

            this.RecalculateStats();
        }

        public AbilityUsage CanUseAbility(int abilityIndex)
        {
            if (abilityIndex >= 0 && abilityIndex < this.m_abilities.Length)
            {
                var ability = this.m_abilities[abilityIndex];

                if (ability.IsOnCooldown)
                {
                    return AbilityUsage.OnCooldown;
                }

                return AbilityUsage.CanUse;
            }

            return AbilityUsage.DoesNotExist;
        }

        public AbilityUsage CanUseAbility(Ability ability)
        {
            if (ability is not null && ability.Owner == this)
            {
                if (ability.IsOnCooldown)
                {
                    return AbilityUsage.OnCooldown;
                }

                return AbilityUsage.CanUse;
            }

            return AbilityUsage.DoesNotExist;
        }

        public void ApplyImmediateEffects()
        {
            for (int i = this.m_effects.Count - 1; i >= 0; --i)
            {
                var effect = this.m_effects[i];

                if (effect.Type == EffectType.IsImmediate)
                {
                    effect.ApplyImmediate(ref this.m_stats, ref this.m_turn);

                    this.m_effects.RemoveAt(i);
                }
            }
        }

        public void RemoveEffectsOfSide(EffectSide side)
        {
            int initial = this.m_effects.Count;

            for (int i = initial - 1; i >= 0; --i)
            {
                if (this.m_effects[i].Side == side)
                {
                    this.m_effects.RemoveAt(i);
                }
            }

            if (initial != this.m_effects.Count)
            {
                this.RecalculateStats();
            }
        }



        public static Enemy CreateDefaultEnemy()
        {
            return new Enemy(new EnemyData()
            {
                Name = "Cow",
                Armor = 10 * Random.Range(4, 9),
                Precision = 70.0f,
                Evasion = 0.10f,
                CritChance = 0.05f,
                CritMultiplier = 1.5f,
                Ability = "Milkdrop",
                Sprite = ResourceManager.LoadSprite("Sprites/Monsters/" + ms_cowNames[Random.Range(0, ms_cowNames.Length)]),
            }, new AbilityData()
            {
                Class = "Enemy",
                Name = "Milkdrop",
                EnemyEffects = new string[]
                {
                    "Weaken",
                    "Poison",
                },
                AllyEffects = new string[]
                {
                    "Healing",
                    "Amplify",
                    "Sharpness",
                },
                EnemyModifiers = new float[]
                {
                    0.20f,
                    0.30f,
                },
                AllyModifiers = new float[]
                {
                    0.15f,
                    0.20f,
                    30.0f,
                },
                EnemyDurations = new int[]
                {
                    2,
                    3,
                },
                AllyDurations = new int[]
                {
                    0,
                    3,
                    3,
                },
                DamageMultiplier = 1.5f,
                ManaCost = 0,
                IsAOE = false,
                CooldownTime = 5,
                Sprite = ResourceManager.DefaultSprite,
                Description = "Spits poisonous milk. The milk applies reduces enemy's damage by 20% for 2 turns and applies " +
                "Poison effect that deals 30% of base damage over 3 turns. The milk also applies positive effects to the entity itself, " +
                "healing it by 15% of its maximum health, increasing its damage by 20% for 3 turns and increasing its precision by 30 " +
                "points for 3 turns.",
            }, 10 * Random.Range(30, 50), Random.Range(10, 20));
        }
    }
}
