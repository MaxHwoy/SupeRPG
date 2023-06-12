using SupeRPG.Items;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SupeRPG.Game
{
    public class Player : IEntity
    {
        public class Data
        {
            public int Money;
            public string Race;
            public string Class;

            public int EquippedHelmet;
            public int EquippedChestplate;
            public int EquippedLeggings;
            public int EquippedWeapon;

            public int[] EquippedPotions;
            public int[] EquippedHelmetTrinkets;
            public int[] EquippedChestplateTrinkets;
            public int[] EquippedLeggingsTrinkets;
            public int[] EquippedWeaponTrinkets;

            public string[] Armors;
            public string[] Weapons;
            public string[] Potions;
            public string[] Trinkets;
        }

        private static Player ms_instance;

        private readonly List<TrinketData> m_trinkets;
        private readonly List<PotionData> m_potions;
        private readonly List<WeaponData> m_weapons;
        private readonly List<ArmorData> m_armors;
        private readonly List<Effect> m_effects;

#if DEBUG || DEVELOPMENT_BUILD
        private Ability[] m_abilities;
        private ClassInfo m_classInfo;
        private Sprite m_sprite;
        private RaceInfo m_raceInfo;
#else
        private readonly Ability[] m_abilities;
        private readonly ClassInfo m_classInfo;
        private readonly Sprite m_sprite;
        private readonly RaceInfo m_raceInfo;
#endif

        private readonly Potion[] m_equippedPotions;

        private readonly WeaponTrinket[] m_weaponTrinkets;
        private Weapon m_weapon;

        private readonly ArmorTrinket[] m_leggingsTrinkets;
        private Armor m_leggings;

        private readonly ArmorTrinket[] m_chestplateTrinkets;
        private Armor m_chestplate;

        private readonly ArmorTrinket[] m_helmetTrinkets;
        private Armor m_helmet;

        private EntityStats m_stats;
        private TurnStats m_turn;
        private int m_money;

        public const int MaxPotionSlots = 3;

        public const int ManaRegeneration = 5;

        public const int HealthRegeneration = 2;

        public const int RewardForDefeat = 5;

        public const string CharacterSpriteDB = "Sprites/Characters/";

        public static readonly float SellMultiplier = 0.8f;

        public static readonly int InitialPlayerBank = 0;

        public static bool IsPlayerLoaded => ms_instance is not null;

        public static Player Instance => ms_instance ?? throw new Exception("Cannot access player when not in game");

        public Sprite Sprite => this.m_sprite;

        public bool IsPlayer => true;

        public bool IsAlive => this.m_stats.CurHealth > 0;

        public bool IsMelee => this.m_classInfo.IsMelee;

        public ref readonly EntityStats EntityStats => ref this.m_stats;

        public ref readonly TurnStats TurnStats => ref this.m_turn;

        public RaceInfo Race => this.m_raceInfo;

        public ClassInfo Class => this.m_classInfo;

        public int Money => this.m_money;

        public Armor Helmet => this.m_helmet;

        public Armor Chestplate => this.m_chestplate;

        public Armor Leggings => this.m_leggings;

        public Weapon Weapon => this.m_weapon;

        public IReadOnlyList<ArmorTrinket> HelmetTrinkets => this.m_helmetTrinkets;

        public IReadOnlyList<ArmorTrinket> ChestplateTrinkets => this.m_chestplateTrinkets;

        public IReadOnlyList<ArmorTrinket> LeggingsTrinkets => this.m_leggingsTrinkets;

        public IReadOnlyList<WeaponTrinket> WeaponTrinkets => this.m_weaponTrinkets;

        public IReadOnlyList<Potion> EquippedPotions => this.m_equippedPotions;

        public IReadOnlyList<ArmorData> Armors => this.m_armors;

        public IReadOnlyList<WeaponData> Weapons => this.m_weapons;

        public IReadOnlyList<PotionData> Potions => this.m_potions;

        public IReadOnlyList<TrinketData> Trinkets => this.m_trinkets;

        public IReadOnlyList<Effect> Effects => this.m_effects;

        public IReadOnlyList<Ability> Abilities => this.m_abilities;

        private Player(Data data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.m_raceInfo = ResourceManager.Races.Find(_ => _.Name == data.Race);
            this.m_classInfo = ResourceManager.Classes.Find(_ => _.Name == data.Class);

            this.m_armors = new();
            this.m_weapons = new();
            this.m_potions = new();
            this.m_effects = new();
            this.m_trinkets = new();
            this.m_abilities = ResourceManager.Abilities.Where(_ => _.Class == data.Class).Select(_ => new Ability(this, _)).ToArray();

            this.m_equippedPotions = new Potion[Player.MaxPotionSlots];
            this.m_helmetTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_chestplateTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_leggingsTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_weaponTrinkets = new WeaponTrinket[Weapon.MaxTrinketSlots];

            this.m_money = data.Money;
            this.m_sprite = Player.GetSpriteForRaceClass(data.Race, data.Class);

            if (data.Armors is not null)
            {
                this.m_armors.Capacity = data.Armors.Length;

                for (int i = 0; i < data.Armors.Length; ++i)
                {
                    this.m_armors.Add(ResourceManager.Armors.Find(_ => _.Name == data.Armors[i]));
                }
            }

            if (data.Weapons is not null)
            {
                this.m_weapons.Capacity = data.Weapons.Length;

                for (int i = 0; i < data.Weapons.Length; ++i)
                {
                    this.m_weapons.Add(ResourceManager.Weapons.Find(_ => _.Name == data.Weapons[i]));
                }
            }

            if (data.Potions is not null)
            {
                this.m_potions.Capacity = data.Potions.Length;

                for (int i = 0; i < data.Potions.Length; ++i)
                {
                    this.m_potions.Add(ResourceManager.Potions.Find(_ => _.Name == data.Potions[i]));
                }
            }

            if (data.Trinkets is not null)
            {
                this.m_trinkets.Capacity = data.Trinkets.Length;

                for (int i = 0; i < data.Trinkets.Length; ++i)
                {
                    this.m_trinkets.Add(ResourceManager.Trinkets.Find(_ => _.Name == data.Trinkets[i]));
                }
            }

            if (data.EquippedHelmet >= 0)
            {
                this.m_helmet = new Armor(this.m_armors[data.EquippedHelmet]);
            }

            if (data.EquippedChestplate >= 0)
            {
                this.m_chestplate = new Armor(this.m_armors[data.EquippedChestplate]);
            }

            if (data.EquippedLeggings >= 0)
            {
                this.m_leggings = new Armor(this.m_armors[data.EquippedLeggings]);
            }

            if (data.EquippedWeapon >= 0)
            {
                this.m_weapon = new Weapon(this.m_weapons[data.EquippedWeapon]);
            }

            if (data.EquippedPotions is not null && data.EquippedPotions.Length == this.m_equippedPotions.Length)
            {
                for (int i = 0; i < this.m_equippedPotions.Length; ++i)
                {
                    if (data.EquippedPotions[i] >= 0)
                    {
                        this.m_equippedPotions[i] = new Potion(this.m_potions[data.EquippedPotions[i]]);
                    }
                }
            }

            if (data.EquippedHelmetTrinkets is not null && data.EquippedHelmetTrinkets.Length == this.m_helmetTrinkets.Length)
            {
                for (int i = 0; i < this.m_helmetTrinkets.Length; ++i)
                {
                    if (data.EquippedHelmetTrinkets[i] >= 0)
                    {
                        this.m_helmetTrinkets[i] = TrinketFactory.Create(this.m_trinkets[data.EquippedHelmetTrinkets[i]]) as ArmorTrinket;
                    }
                }
            }

            if (data.EquippedChestplateTrinkets is not null && data.EquippedChestplateTrinkets.Length == this.m_chestplateTrinkets.Length)
            {
                for (int i = 0; i < this.m_chestplateTrinkets.Length; ++i)
                {
                    if (data.EquippedChestplateTrinkets[i] >= 0)
                    {
                        this.m_chestplateTrinkets[i] = TrinketFactory.Create(this.m_trinkets[data.EquippedChestplateTrinkets[i]]) as ArmorTrinket;
                    }
                }
            }

            if (data.EquippedLeggingsTrinkets is not null && data.EquippedLeggingsTrinkets.Length == this.m_leggingsTrinkets.Length)
            {
                for (int i = 0; i < this.m_leggingsTrinkets.Length; ++i)
                {
                    if (data.EquippedLeggingsTrinkets[i] >= 0)
                    {
                        this.m_leggingsTrinkets[i] = TrinketFactory.Create(this.m_trinkets[data.EquippedLeggingsTrinkets[i]]) as ArmorTrinket;
                    }
                }
            }

            if (data.EquippedWeaponTrinkets is not null && data.EquippedWeaponTrinkets.Length == this.m_weaponTrinkets.Length)
            {
                for (int i = 0; i < this.m_weaponTrinkets.Length; ++i)
                {
                    if (data.EquippedWeaponTrinkets[i] >= 0)
                    {
                        this.m_weaponTrinkets[i] = TrinketFactory.Create(this.m_trinkets[data.EquippedWeaponTrinkets[i]]) as WeaponTrinket;
                    }
                }
            }

            this.RecalculateStats();
        }

        public Player(string race, string @class) : this(ResourceManager.Races.Find(_ => _.Name == race), ResourceManager.Classes.Find(_ => _.Name == @class))
        {
        }

        public Player(RaceInfo race, ClassInfo @class)
        {
            if (race is null)
            {
                throw new ArgumentException(nameof(race));
            }

            if (@class is null)
            {
                throw new ArgumentException(nameof(@class));
            }

            this.m_raceInfo = race;
            this.m_classInfo = @class;
            this.m_armors = new();
            this.m_weapons = new();
            this.m_potions = new();
            this.m_effects = new();
            this.m_trinkets = new();
            this.m_abilities = ResourceManager.Abilities.Where(_ => _.Class == @class.Name).Select(_ => new Ability(this, _)).ToArray();

            this.m_equippedPotions = new Potion[Player.MaxPotionSlots];
            this.m_helmetTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_chestplateTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_leggingsTrinkets = new ArmorTrinket[Armor.MaxTrinketSlots];
            this.m_weaponTrinkets = new WeaponTrinket[Weapon.MaxTrinketSlots];

            this.m_sprite = Player.GetSpriteForRaceClass(race.Name, @class.Name);

            this.m_money = Player.InitialPlayerBank;

            this.RecalculateStats();
        }



        private void RecalculateStats()
        {
            this.m_stats.MaxHealth = this.m_classInfo.Health;
            this.m_stats.MaxMana = this.m_classInfo.Mana;
            this.m_stats.Armor = this.m_classInfo.Armor;
            this.m_stats.Damage = this.m_classInfo.Damage;
            this.m_stats.Evasion = this.m_classInfo.Evasion;
            this.m_stats.Precision = this.m_classInfo.Precision;
            this.m_stats.CritChance = this.m_classInfo.CritChance;
            this.m_stats.CritMultiplier = this.m_classInfo.CritMultiplier + 1.0f;

            if (this.m_helmet is not null)
            {
                var stats = this.m_helmet.Stats;

                var trinkets = this.m_helmetTrinkets;

                if (trinkets is not null)
                {
                    for (int i = 0; i < trinkets.Length; ++i)
                    {
                        trinkets[i]?.ModifyStats(ref stats);
                    }
                }

                this.m_stats.Armor += stats.Armor;
                this.m_stats.Evasion += stats.Evasion;
                this.m_stats.Precision += stats.Precision;
            }

            if (this.m_chestplate is not null)
            {
                var stats = this.m_chestplate.Stats;

                var trinkets = this.m_chestplateTrinkets;

                if (trinkets is not null)
                {
                    for (int i = 0; i < trinkets.Length; ++i)
                    {
                        trinkets[i]?.ModifyStats(ref stats);
                    }
                }

                this.m_stats.Armor += stats.Armor;
                this.m_stats.Evasion += stats.Evasion;
                this.m_stats.Precision += stats.Precision;
            }

            if (this.m_leggings is not null)
            {
                var stats = this.m_leggings.Stats;

                var trinkets = this.m_leggingsTrinkets;

                if (trinkets is not null)
                {
                    for (int i = 0; i < trinkets.Length; ++i)
                    {
                        trinkets[i]?.ModifyStats(ref stats);
                    }
                }

                this.m_stats.Armor += stats.Armor;
                this.m_stats.Evasion += stats.Evasion;
                this.m_stats.Precision += stats.Precision;
            }

            if (this.m_weapon is not null)
            {
                var stats = this.m_weapon.Stats;

                var trinkets = this.m_weaponTrinkets;

                if (trinkets is not null)
                {
                    for (int i = 0; i < trinkets.Length; ++i)
                    {
                        trinkets[i]?.ModifyStats(ref stats);
                    }
                }

                this.m_stats.Damage += stats.Damage;
                this.m_stats.Precision += stats.Precision;
                this.m_stats.CritChance += stats.CritChance;
                this.m_stats.CritMultiplier += stats.CritMultiplier;
            }

            for (int i = 0; i < this.m_effects.Count; ++i)
            {
                this.m_effects[i].ModifyStats(ref this.m_stats, ref this.m_turn);
            }

            switch (this.m_raceInfo.Stat)
            {
                case Statistic.Health:
                    this.m_stats.MaxHealth += (int)(this.m_stats.MaxHealth * this.m_raceInfo.Modifier);
                   break;
            
               case Statistic.Mana:
                   this.m_stats.MaxMana += (int)(this.m_stats.MaxMana * this.m_raceInfo.Modifier);
                   break;
            
               case Statistic.Damage:
                   this.m_stats.Damage += (int)(this.m_stats.Damage * this.m_raceInfo.Modifier);
                   break;
            
               case Statistic.Armor:
                   this.m_stats.Armor += (int)(this.m_stats.Armor * this.m_raceInfo.Modifier);
                   break;
            
               case Statistic.Evasion:
                   this.m_stats.Evasion += this.m_stats.Evasion * this.m_raceInfo.Modifier;
                   break;
            
               case Statistic.Precision:
                   this.m_stats.Precision += this.m_stats.Precision * this.m_raceInfo.Modifier;
                   break;
            
               case Statistic.CritChance:
                   this.m_stats.CritChance += this.m_stats.CritChance * this.m_raceInfo.Modifier;
                   break;
            
               case Statistic.CritMultiplier:
                   this.m_stats.CritMultiplier += this.m_stats.CritMultiplier * this.m_raceInfo.Modifier;
                   break;
            }

            this.m_stats.CurHealth = Mathf.Clamp(this.m_stats.CurHealth, 0, this.m_stats.MaxHealth);
            this.m_stats.CurMana = Mathf.Clamp(this.m_stats.CurMana, 0, this.m_stats.MaxMana);
            this.m_stats.Evasion = Mathf.Clamp01(this.m_stats.Evasion);
            this.m_stats.Precision = Mathf.Clamp(this.m_stats.Precision, 0.0f, Single.PositiveInfinity);
            this.m_stats.CritChance = Mathf.Clamp01(this.m_stats.CritChance);
        }

        private void UnattachPotion(PotionData potion)
        {
            for (int i = 0; i < this.m_equippedPotions.Length; ++i)
            {
                if (this.m_equippedPotions[i]?.IsPotionDataSame(potion) ?? false)
                {
                    this.m_equippedPotions[i] = null;
                }
            }
        }

        private bool UnattachTrinket(TrinketData trinket)
        {
            for (int i = 0; i < this.m_helmetTrinkets.Length; ++i)
            {
                if (this.m_helmetTrinkets[i]?.IsTrinketDataSame(trinket) ?? false)
                {
                    this.m_helmetTrinkets[i] = null;

                    return true;
                }
            }

            for (int i = 0; i < this.m_chestplateTrinkets.Length; ++i)
            {
                if (this.m_chestplateTrinkets[i]?.IsTrinketDataSame(trinket) ?? false)
                {
                    this.m_chestplateTrinkets[i] = null;

                    return true;
                }
            }

            for (int i = 0; i < this.m_leggingsTrinkets.Length; ++i)
            {
                if (this.m_leggingsTrinkets[i]?.IsTrinketDataSame(trinket) ?? false)
                {
                    this.m_leggingsTrinkets[i] = null;

                    return true;
                }
            }

            for (int i = 0; i < this.m_weaponTrinkets.Length; ++i)
            {
                if (this.m_weaponTrinkets[i]?.IsTrinketDataSame(trinket) ?? false)
                {
                    this.m_weaponTrinkets[i] = null;

                    return true;
                }
            }

            return false;
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
            this.m_stats.CurHealth = Mathf.Min(this.m_stats.CurHealth + Player.HealthRegeneration, this.m_stats.MaxHealth);
            this.m_stats.CurMana = Mathf.Min(this.m_stats.CurMana + Player.ManaRegeneration, this.m_stats.MaxMana);
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
                    int curMana = this.m_stats.CurMana;

                    effect.Cooldown(ref this.m_stats, ref this.m_turn);

                    totalHeal += this.m_stats.CurHealth - curHealth;
                    totalMana += this.m_stats.CurMana - curMana;

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

        public void UsePotion(int potionIndex)
        {
            if (potionIndex >= 0 && potionIndex < this.m_equippedPotions.Length)
            {
                var potion = this.m_equippedPotions[potionIndex];

                var effect = potion.Use(in this.m_stats);

                this.m_equippedPotions[potionIndex] = null;

                this.m_potions.Remove(potion.Data);

                if (effect.Type == EffectType.IsImmediate)
                {
                    effect.ApplyImmediate(ref this.m_stats, ref this.m_turn);
                }
                else
                {
                    this.m_effects.Add(effect);
                }

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

                if (this.m_stats.CurMana < ability.ManaCost)
                {
                    return AbilityUsage.NotEnoughMana;
                }

                return AbilityUsage.CanUse;
            }

            return AbilityUsage.DoesNotExist;
        }

        public AbilityUsage CanUseAbility(Ability ability)
        {
            if (ability is not null && ability.Owner == this)
            {
#if DEBUG || DEVELOPMENT_BUILD
                Debug.Assert(Array.IndexOf(this.m_abilities, ability) >= 0);
#endif

                if (ability.IsOnCooldown)
                {
                    return AbilityUsage.OnCooldown;
                }

                if (this.m_stats.CurMana < ability.ManaCost)
                {
                    return AbilityUsage.NotEnoughMana;
                }

                return AbilityUsage.CanUse;
            }

            return AbilityUsage.DoesNotExist;
        }

        public void RemoveMana(int mana)
        {
            this.m_stats.CurMana -= mana;

            if (this.m_stats.CurMana < 0)
            {
                this.m_stats.CurMana = 0;
            }
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



#if DEBUG || DEVELOPMENT_BUILD
        public void SwapRaceClass(RaceInfo race, ClassInfo @class)
        {
            if (this.m_raceInfo != race || this.m_classInfo != @class)
            {
                this.m_raceInfo = race;
                this.m_classInfo = @class;
                this.m_sprite = Player.GetSpriteForRaceClass(race.Name, @class.Name);
                this.m_abilities = ResourceManager.Abilities.Where(_ => _.Class == @class.Name).Select(_ => new Ability(this, _)).ToArray();

                this.RecalculateStats();
            }
        }
#endif

        public void AwardReward(int money)
        {
            this.m_money += money;
        }

        public void EquipHelmet(int index)
        {
            if (index < 0 || index >= this.m_armors.Count)
            {
                this.m_helmet = null;
            }
            else
            {
                var armor = this.m_armors[index];

                if (armor.SlotType != ArmorSlotType.Helmet)
                {
                    throw new Exception($"Armor at index {index} is not a Helmet armor");
                }

                this.m_helmet = new Armor(armor);
            }

            this.m_helmetTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipHelmet(ArmorData armor)
        {
            if (armor is null)
            {
                this.m_helmet = null;
            }
            else
            {
                int index = this.m_armors.IndexOf(armor);

                if (index < 0)
                {
                    throw new Exception("Armor is not in the inventory");
                }

                if (armor.SlotType != ArmorSlotType.Helmet)
                {
                    throw new Exception($"Armor {armor.Name} is not a Helmet armor");
                }

                this.m_helmet = new Armor(armor);
            }

            this.m_helmetTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipChestplate(int index)
        {
            if (index < 0 || index >= this.m_armors.Count)
            {
                this.m_chestplate = null;
            }
            else
            {
                var armor = this.m_armors[index];

                if (armor.SlotType != ArmorSlotType.Chestplate)
                {
                    throw new Exception($"Armor at index {index} is not a Chestplate armor");
                }

                this.m_chestplate = new Armor(armor);
            }

            this.m_chestplateTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipChestplate(ArmorData armor)
        {
            if (armor is null)
            {
                this.m_chestplate = null;
            }
            else
            {
                int index = this.m_armors.IndexOf(armor);

                if (index < 0)
                {
                    throw new Exception("Armor is not in the inventory");
                }

                if (armor.SlotType != ArmorSlotType.Chestplate)
                {
                    throw new Exception($"Armor {armor.Name} is not a chestplate armor");
                }

                this.m_chestplate = new Armor(armor);
            }

            this.m_chestplateTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipLeggings(int index)
        {
            if (index < 0 || index >= this.m_armors.Count)
            {
                this.m_leggings = null;
            }
            else
            {
                var armor = this.m_armors[index];

                if (armor.SlotType != ArmorSlotType.Leggings)
                {
                    throw new Exception($"Armor at index {index} is not a leggings armor");
                }

                this.m_leggings = new Armor(armor);
            }

            this.m_leggingsTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipLeggings(ArmorData armor)
        {
            if (armor is null)
            {
                this.m_leggings = null;
            }
            else
            {
                int index = this.m_armors.IndexOf(armor);

                if (index < 0)
                {
                    throw new Exception("Armor is not in the inventory");
                }

                if (armor.SlotType != ArmorSlotType.Leggings)
                {
                    throw new Exception($"Armor {armor.Name} is not a leggings armor");
                }

                this.m_leggings = new Armor(armor);
            }

            this.m_leggingsTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipWeapon(int index)
        {
            if (index < 0 || index >= this.m_weapons.Count)
            {
                this.m_weapon = null;
            }
            else
            {
                this.m_weapon = new Weapon(this.m_weapons[index]);
            }

            this.m_weaponTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon is null)
            {
                this.m_weapon = null;
            }
            else
            {
                int index = this.m_weapons.IndexOf(weapon);

                if (index < 0)
                {
                    throw new Exception("Weapon is not in the inventory");
                }

                this.m_weapon = new Weapon(weapon);
            }

            this.m_weaponTrinkets.AsSpan().Clear();

            this.RecalculateStats();
        }

        public void EquipPotion(int slot, int index)
        {
            if (slot < 0)
            {
                throw new Exception("Potion slot number cannot be negative");
            }

            if (slot >= this.m_equippedPotions.Length)
            {
                throw new Exception($"Trying to set potion for slot {slot} when maximum slot count is {this.m_equippedPotions.Length}");
            }

            if (index < 0 || index >= this.m_potions.Count)
            {
                this.m_equippedPotions[slot] = null;
            }
            else
            {
                var potion = this.m_potions[slot];

                this.UnattachPotion(potion);

                this.m_equippedPotions[slot] = new Potion(potion);
            }
        }

        public void EquipPotion(int slot, PotionData potion)
        {
            if (slot < 0)
            {
                throw new Exception("Potion slot number cannot be negative");
            }

            if (slot >= this.m_equippedPotions.Length)
            {
                throw new Exception($"Trying to set potion for slot {slot} when maximum slot count is {this.m_equippedPotions.Length}");
            }

            if (potion is null)
            {
                this.m_equippedPotions[slot] = null;
            }
            else
            {
                int index = this.m_potions.IndexOf(potion);

                if (index < 0)
                {
                    throw new Exception("Potion is not in the inventory");
                }

                this.UnattachPotion(potion);

                this.m_equippedPotions[slot] = new Potion(potion);
            }
        }

        public void EquipHelmetTrinket(int slot, int index)
        {
            if (this.m_helmet is null)
            {
                throw new Exception("Unable to equip helmet trinket because helmet is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_helmet.MaxTrinketCount)
            {
                throw new Exception($"Trying to set helmet trinket for slot {slot} when maximum slot count is {this.m_helmet.MaxTrinketCount}");
            }

            if (index < 0 || index >= this.m_trinkets.Count)
            {
                this.m_helmetTrinkets[slot] = null;
            }
            else
            {
                var trinket = this.m_trinkets[index];

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to helmet because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_helmetTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipHelmetTrinket(int slot, TrinketData trinket)
        {
            if (this.m_helmet is null)
            {
                throw new Exception("Unable to equip helmet trinket because helmet is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_helmet.MaxTrinketCount)
            {
                throw new Exception($"Trying to set helmet trinket for slot {slot} when maximum slot count is {this.m_helmet.MaxTrinketCount}");
            }

            if (trinket is null)
            {
                this.m_helmetTrinkets[slot] = null;
            }
            else
            {
                int index = this.m_trinkets.IndexOf(trinket);

                if (index < 0)
                {
                    throw new Exception("Trinket is not in the inventory");
                }

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to helmet because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_helmetTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipChestplateTrinket(int slot, int index)
        {
            if (this.m_chestplate is null)
            {
                throw new Exception("Unable to equip chestplate trinket because chestplate is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_chestplate.MaxTrinketCount)
            {
                throw new Exception($"Trying to set chestplate trinket for slot {slot} when maximum slot count is {this.m_chestplate.MaxTrinketCount}");
            }

            if (index < 0 || index >= this.m_trinkets.Count)
            {
                this.m_chestplateTrinkets[slot] = null;
            }
            else
            {
                var trinket = this.m_trinkets[index];

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to chestplate because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_chestplateTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipChestplateTrinket(int slot, TrinketData trinket)
        {
            if (this.m_chestplate is null)
            {
                throw new Exception("Unable to equip chestplate trinket because chestplate is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_chestplate.MaxTrinketCount)
            {
                throw new Exception($"Trying to set chestplate trinket for slot {slot} when maximum slot count is {this.m_chestplate.MaxTrinketCount}");
            }

            if (trinket is null)
            {
                this.m_chestplateTrinkets[slot] = null;
            }
            else
            {
                int index = this.m_trinkets.IndexOf(trinket);

                if (index < 0)
                {
                    throw new Exception("Trinket is not in the inventory");
                }

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to chestplate because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_chestplateTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipLeggingsTrinket(int slot, int index)
        {
            if (this.m_leggings is null)
            {
                throw new Exception("Unable to equip leggings trinket because leggings is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_leggings.MaxTrinketCount)
            {
                throw new Exception($"Trying to set leggings trinket for slot {slot} when maximum slot count is {this.m_leggings.MaxTrinketCount}");
            }

            if (index < 0 || index >= this.m_trinkets.Count)
            {
                this.m_leggingsTrinkets[slot] = null;
            }
            else
            {
                var trinket = this.m_trinkets[index];

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to leggings because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_leggingsTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipLeggingsTrinket(int slot, TrinketData trinket)
        {
            if (this.m_leggings is null)
            {
                throw new Exception("Unable to equip leggings trinket because leggings is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_leggings.MaxTrinketCount)
            {
                throw new Exception($"Trying to set leggings trinket for slot {slot} when maximum slot count is {this.m_leggings.MaxTrinketCount}");
            }

            if (trinket is null)
            {
                this.m_leggingsTrinkets[slot] = null;
            }
            else
            {
                int index = this.m_trinkets.IndexOf(trinket);

                if (index < 0)
                {
                    throw new Exception("Trinket is not in the inventory");
                }

                if (trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to leggings because it is a weapon-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_leggingsTrinkets[slot] = TrinketFactory.Create(trinket) as ArmorTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipWeaponTrinket(int slot, int index)
        {
            if (this.m_weapon is null)
            {
                throw new Exception("Unable to equip weapon trinket because weapon is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_weapon.MaxTrinketCount)
            {
                throw new Exception($"Trying to set weapon trinket for slot {slot} when maximum slot count is {this.m_weapon.MaxTrinketCount}");
            }

            if (index < 0 || index >= this.m_trinkets.Count)
            {
                this.m_weaponTrinkets[slot] = null;
            }
            else
            {
                var trinket = this.m_trinkets[index];

                if (!trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to helmet because it is an armor-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_weaponTrinkets[slot] = TrinketFactory.Create(trinket) as WeaponTrinket;
            }

            this.RecalculateStats();
        }

        public void EquipWeaponTrinket(int slot, TrinketData trinket)
        {
            if (this.m_weapon is null)
            {
                throw new Exception("Unable to equip weapon trinket because weapon is not equipped");
            }

            if (slot < 0)
            {
                throw new Exception("Trinket slot number cannot be negative");
            }

            if (slot >= this.m_weapon.MaxTrinketCount)
            {
                throw new Exception($"Trying to set weapon trinket for slot {slot} when maximum slot count is {this.m_weapon.MaxTrinketCount}");
            }

            if (trinket is null)
            {
                this.m_weaponTrinkets[slot] = null;
            }
            else
            {
                int index = this.m_trinkets.IndexOf(trinket);

                if (index < 0)
                {
                    throw new Exception("Trinket is not in the inventory");
                }

                if (!trinket.IsWeaponTrinket)
                {
                    throw new Exception($"Trinket {trinket.Name} cannot be applied to weapon because it is an armor-only trinket");
                }

                this.UnattachTrinket(trinket);

                this.m_weaponTrinkets[slot] = TrinketFactory.Create(trinket) as WeaponTrinket;
            }

            this.RecalculateStats();
        }

        public bool HasEquippedItem(IItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (item is ArmorData armor)
            {
                if (this.m_helmet is not null && this.m_helmet.Data == armor)
                {
                    return true;
                }

                if (this.m_chestplate is not null && this.m_chestplate.Data == armor)
                {
                    return true;
                }

                if (this.m_leggings is not null && this.m_leggings.Data == armor)
                {
                    return true;
                }

                return false;
            }

            if (item is WeaponData weapon)
            {
                if (this.m_weapon is not null && this.m_weapon.Data == weapon)
                {
                    return true;
                }

                return false;
            }

            if (item is PotionData potion)
            {
                for (int i = 0; i < this.m_equippedPotions.Length; ++i)
                {
                    if (this.m_equippedPotions[i] is not null && this.m_equippedPotions[i].Data == potion)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (item is TrinketData trinket)
            {
                for (int i = 0; i < this.m_helmetTrinkets.Length; ++i)
                {
                    if (this.m_helmetTrinkets[i] is not null && this.m_helmetTrinkets[i].Data == trinket)
                    {
                        return true;
                    }
                }

                for (int i = 0; i < this.m_chestplateTrinkets.Length; ++i)
                {
                    if (this.m_chestplateTrinkets[i] is not null && this.m_chestplateTrinkets[i].Data == trinket)
                    {
                        return true;
                    }
                }

                for (int i = 0; i < this.m_leggingsTrinkets.Length; ++i)
                {
                    if (this.m_leggingsTrinkets[i] is not null && this.m_leggingsTrinkets[i].Data == trinket)
                    {
                        return true;
                    }
                }

                for (int i = 0; i < this.m_weaponTrinkets.Length; ++i)
                {
                    if (this.m_weaponTrinkets[i] is not null && this.m_weaponTrinkets[i].Data == trinket)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }



        public int PurchaseArmor(ArmorData armor)
        {
            if (armor is null)
            {
                throw new ArgumentNullException(nameof(armor));
            }

            if (armor.Price > this.m_money)
            {
                throw new ArgumentException("Unable to purchase because not enough money");
            }

            this.m_money -= armor.Price;

            int index = Player.BinarySearchInsertionPlace(armor, this.m_armors);

            this.m_armors.Insert(index, armor.Clone());

            return index;
        }

        public int PurchaseWeapon(WeaponData weapon)
        {
            if (weapon is null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            if (weapon.Price > this.m_money)
            {
                throw new ArgumentException("Unable to purchase because not enough money");
            }

            this.m_money -= weapon.Price;

            int index = Player.BinarySearchInsertionPlace(weapon, this.m_weapons);

            this.m_weapons.Insert(index, weapon.Clone());

            return index;
        }

        public int PurchasePotion(PotionData potion)
        {
            if (potion is null)
            {
                throw new ArgumentNullException(nameof(potion));
            }

            if (potion.Price > this.m_money)
            {
                throw new ArgumentException("Unable to purchase because not enough money");
            }

            this.m_money -= potion.Price;

            int index = Player.BinarySearchInsertionPlace(potion, this.m_potions);

            this.m_potions.Insert(index, potion.Clone());

            return index;
        }

        public int PurchaseTrinket(TrinketData trinket)
        {
            if (trinket is null)
            {
                throw new ArgumentNullException(nameof(trinket));
            }
            
            if (trinket.Price > this.m_money)
            {
                throw new ArgumentException("Unable to purchase because not enough money");
            }

            this.m_money -= trinket.Price;

            int index = Player.BinarySearchInsertionPlace(trinket, this.m_trinkets);

            this.m_trinkets.Insert(index, trinket.Clone());

            return index;
        }

        public void SellArmor(ArmorData armor)
        {
            if (armor is not null && this.m_armors.Remove(armor))
            {
                this.m_money += (int)(armor.Price * Player.SellMultiplier);

                if (this.m_helmet is not null && this.m_helmet.IsArmorDataSame(armor))
                {
                    this.EquipHelmet(null);
                }

                if (this.m_chestplate is not null && this.m_chestplate.IsArmorDataSame(armor))
                {
                    this.EquipChestplate(null);
                }

                if (this.m_leggings is not null && this.m_leggings.IsArmorDataSame(armor))
                {
                    this.EquipLeggings(null);
                }
            }
        }

        public void SellWeapon(WeaponData weapon)
        {
            if (weapon is not null && this.m_weapons.Remove(weapon))
            {
                this.m_money += (int)(weapon.Price * Player.SellMultiplier);

                if (this.m_weapon is not null && this.m_weapon.IsWeaponDataSame(weapon))
                {
                    this.EquipWeapon(null);
                }
            }
        }

        public void SellPotion(PotionData potion)
        {
            if (potion is not null && this.m_potions.Remove(potion))
            {
                this.m_money += (int)(potion.Price * Player.SellMultiplier);

                this.UnattachPotion(potion);
            }
        }

        public void SellTrinket(TrinketData trinket)
        {
            if (trinket is not null && this.m_trinkets.Remove(trinket))
            {
                this.m_money += (int)(trinket.Price * Player.SellMultiplier);

                if (this.UnattachTrinket(trinket))
                {
                    this.RecalculateStats();
                }
            }
        }

        public void SellArmor(int index)
        {
            if (index >= 0 && index < this.m_armors.Count)
            {
                var armor = this.m_armors[index];

                this.m_money += (int)(armor.Price * Player.SellMultiplier);

                if (this.m_helmet is not null && this.m_helmet.IsArmorDataSame(armor))
                {
                    this.EquipHelmet(null);
                }

                if (this.m_chestplate is not null && this.m_chestplate.IsArmorDataSame(armor))
                {
                    this.EquipChestplate(null);
                }

                if (this.m_leggings is not null && this.m_leggings.IsArmorDataSame(armor))
                {
                    this.EquipLeggings(null);
                }

                this.m_armors.RemoveAt(index);
            }
        }

        public void SellWeapon(int index)
        {
            if (index >= 0 && index < this.m_weapons.Count)
            {
                var weapon = this.m_weapons[index];

                this.m_money += (int)(weapon.Price * Player.SellMultiplier);

                if (this.m_weapon is not null && this.m_weapon.IsWeaponDataSame(weapon))
                {
                    this.EquipWeapon(null);
                }

                this.m_weapons.RemoveAt(index);
            }
        }

        public void SellPotion(int index)
        {
            if (index >= 0 && index < this.m_potions.Count)
            {
                var potion = this.m_potions[index];

                this.m_money += (int)(potion.Price * Player.SellMultiplier);

                this.m_potions.RemoveAt(index);

                this.UnattachPotion(potion);
            }
        }

        public void SellTrinket(int index)
        {
            if (index >= 0 && index < this.m_trinkets.Count)
            {
                var trinket = this.m_trinkets[index];

                this.m_money += (int)(trinket.Price * Player.SellMultiplier);

                if (this.UnattachTrinket(trinket))
                {
                    this.RecalculateStats();
                }

                this.m_trinkets.RemoveAt(index);
            }
        }

        public Data GetDataForSaving()
        {
            var data = new Data()
            {
                Money = this.m_money,
                Race = this.m_raceInfo.Name,
                Class = this.m_classInfo.Name,
                EquippedHelmet = this.m_armors.IndexOf(this.m_helmet?.Data),
                EquippedChestplate = this.m_armors.IndexOf(this.m_chestplate?.Data),
                EquippedLeggings = this.m_armors.IndexOf(this.m_leggings?.Data),
                EquippedWeapon = this.m_weapons.IndexOf(this.m_weapon?.Data),
                EquippedPotions = new int[this.m_equippedPotions.Length],
                EquippedHelmetTrinkets = new int[this.m_helmetTrinkets.Length],
                EquippedChestplateTrinkets = new int[this.m_chestplateTrinkets.Length],
                EquippedLeggingsTrinkets = new int[this.m_leggingsTrinkets.Length],
                EquippedWeaponTrinkets = new int[this.m_weaponTrinkets.Length],
                Armors = this.m_armors.Count == 0 ? null : new string[this.m_armors.Count],
                Weapons = this.m_weapons.Count == 0 ? null : new string[this.m_weapons.Count],
                Potions = this.m_potions.Count == 0 ? null : new string[this.m_potions.Count],
                Trinkets = this.m_trinkets.Count == 0 ? null : new string[this.m_trinkets.Count],
            };

            for (int i = 0; i < this.m_equippedPotions.Length; ++i)
            {
                data.EquippedPotions[i] = this.m_potions.IndexOf(this.m_equippedPotions[i]?.Data);
            }

            for (int i = 0; i < this.m_helmetTrinkets.Length; ++i)
            {
                data.EquippedHelmetTrinkets[i] = this.m_trinkets.IndexOf(this.m_helmetTrinkets[i]?.Data);
            }

            for (int i = 0; i < this.m_chestplateTrinkets.Length; ++i)
            {
                data.EquippedChestplateTrinkets[i] = this.m_trinkets.IndexOf(this.m_chestplateTrinkets[i]?.Data);
            }

            for (int i = 0; i < this.m_leggingsTrinkets.Length; ++i)
            {
                data.EquippedLeggingsTrinkets[i] = this.m_trinkets.IndexOf(this.m_leggingsTrinkets[i]?.Data);
            }

            for (int i = 0; i < this.m_weaponTrinkets.Length; ++i)
            {
                data.EquippedWeaponTrinkets[i] = this.m_trinkets.IndexOf(this.m_weaponTrinkets[i]?.Data);
            }

            for (int i = 0; i < this.m_armors.Count; ++i)
            {
                data.Armors[i] = this.m_armors[i].Name;
            }

            for (int i = 0; i < this.m_weapons.Count; ++i)
            {
                data.Weapons[i] = this.m_weapons[i].Name;
            }

            for (int i = 0; i < this.m_potions.Count; ++i)
            {
                data.Potions[i] = this.m_potions[i].Name;
            }

            for (int i = 0; i < this.m_trinkets.Count; ++i)
            {
                data.Trinkets[i] = this.m_trinkets[i].Name;
            }

            return data;
        }



        private static int BinarySearchInsertionPlace<T>(T item, IReadOnlyList<T> list) where T : IItem
        {
            int end = list.Count - 1;

            if (end < 0)
            {
                return 0;
            }

            int start = 0;

            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var evaled = list[middle];
                int result = (item.Tier == evaled.Tier) ? String.CompareOrdinal(item.Name, evaled.Name) : (item.Tier - evaled.Tier);

                if (result == 0)
                {
                    return middle + 1; // AFTER item with the same name (less elements to copy)
                }

                if (result < 0)
                {
                    end = middle - 1;
                }
                else
                {
                    start = middle + 1;
                }
            }

            return start;
        }

        public static void Initialize(string race, string @class)
        {
            ms_instance = new Player(race, @class);
        }

        public static void Initialize(RaceInfo race, ClassInfo @class)
        {
            ms_instance = new Player(race, @class);
        }

        public static void ReinitializeFromSaveData(Data data)
        {
            Player.ms_instance = new Player(data);
        }

        public static void Deinitialize()
        {
            ms_instance = null;
        }

        public static Sprite GetSpriteForRaceClass(string race, string @class)
        {
            return ResourceManager.LoadSprite(CharacterSpriteDB + race + @class);
        }
    }
}
