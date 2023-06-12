using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace SupeRPG.Game
{
    public enum EffectType
    {
        IsImmediate,
        ModifyStats,
        AffectStats,
    }

    public enum EffectSide
    {
        Neutral,
        Positive,
        Negative,
    }

    public class Effect
    {
        private readonly EffectType m_type;
        private readonly EffectSide m_side;
        private readonly Sprite m_sprite;
        private readonly string m_description;
        private readonly string m_status;
        private readonly string m_name;
        private readonly float m_value;
        private int m_remainingDuration;

        public EffectType Type => this.m_type;

        public EffectSide Side => this.m_side;

        public bool IsLasting => this.m_remainingDuration > 0;

        public int RemainingDuration => this.m_remainingDuration;

        public float Value => this.m_value;

        public string Name => this.m_name;

        public string Status => this.m_status;

        public string Description => this.m_description;

        public Sprite Sprite => this.m_sprite;

        public Effect(EffectType type, EffectSide side, float value, int duration, string name, string status, string description, string spritePath)
        {
            if (type == EffectType.IsImmediate && duration != 0)
            {
                throw new Exception("Immediate and Super effects cannot have non-zero duration");
            }

            this.m_type = type;
            this.m_side = side;
            this.m_value = value;
            this.m_remainingDuration = duration;
            this.m_name = name ?? String.Empty;
            this.m_status = status ?? String.Empty;
            this.m_description = description ?? String.Empty;
            this.m_sprite = ResourceManager.LoadSprite(spritePath);
        }

        protected virtual void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
        }

        protected virtual void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
        }

        protected virtual void ApplyAllNowInternal(ref EntityStats entity, ref TurnStats turn)
        {
        }

        public void ModifyStats(ref EntityStats entity, ref TurnStats turn)
        {
            if (this.m_type == EffectType.ModifyStats)
            {
                this.ModifyStatsInternal(ref entity, ref turn);
            }
        }

        public void ApplyImmediate(ref EntityStats entity, ref TurnStats turn)
        {
            if (this.m_type == EffectType.IsImmediate)
            {
                this.ApplyAllNowInternal(ref entity, ref turn);
            }
        }

        public void Cooldown(ref EntityStats entity, ref TurnStats turn)
        {
            if (this.IsLasting)
            {
                this.m_remainingDuration--;

                if (this.m_type == EffectType.AffectStats)
                {
                    this.AffectStatsInternal(ref entity, ref turn);
                }
            }
        }

        public Effect Clone()
        {
            return (Effect)this.MemberwiseClone();
        }
    }

    public class PoisonEffect : Effect
    {
        public const string EffectName = "Poison";

        public const string SpritePath = "Sprites/Effects/PoisonEffect";

        public PoisonEffect(float value, int duration, in EntityStats initial) : base(EffectType.AffectStats, EffectSide.Negative, value * initial.Damage, duration, EffectName, String.Empty, GetDescription(value * initial.Damage, duration), SpritePath)
        {
        }

        protected override void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            if (entity.CurHealth > 0)
            {
                entity.CurHealth -= (int)this.Value;

                if (entity.CurHealth < 1)
                {
                    entity.CurHealth = 1;
                }
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Reduces health by {(int)value} points per turn over {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new PoisonEffect(value, duration, in initial);
            }
        }
    }

    public class BurnEffect : Effect
    {
        public const string EffectName = "Burn";

        public const string SpritePath = "Sprites/Effects/BurnEffect";

        public BurnEffect(float value, int duration, in EntityStats initial) : base(EffectType.AffectStats, EffectSide.Negative, value * initial.Damage, duration, EffectName, String.Empty, GetDescription(value * initial.Damage, duration), SpritePath)
        {
        }

        protected override void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CurHealth -= (int)this.Value;

            if (entity.CurHealth < 0)
            {
                entity.CurHealth = 0;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Reduces health by {(int)value} points per turn over {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new BurnEffect(value, duration, in initial);
            }
        }
    }

    public class ShredEffect : Effect
    {
        public const string EffectName = "Shred";

        public const string SpritePath = "Sprites/Effects/ShredEffect";

        public ShredEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Negative, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Armor -= (int)(entity.Armor * this.Value);

            if (entity.Armor < 0)
            {
                entity.Armor = 0;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Reduces armor by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new ShredEffect(value, duration);
            }
        }
    }

    public class EmpowerEffect : Effect
    {
        public const string EffectName = "Empower";

        public const string SpritePath = "Sprites/Effects/EmpowerEffect";

        public EmpowerEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Damage += (int)this.Value;
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases damage by {value} points for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new EmpowerEffect(value, duration);
            }
        }
    }

    public class AmplifyEffect : Effect
    {
        public const string EffectName = "Amplify";

        public const string SpritePath = "Sprites/Effects/AmplifyEffect";

        public AmplifyEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Damage += (int)(entity.Damage * this.Value);
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases damage by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new AmplifyEffect(value, duration);
            }
        }
    }

    public class HealingEffect : Effect
    {
        public const string EffectName = "Healing";

        public const string SpritePath = "Sprites/Effects/HealingEffect";

        public HealingEffect(float value) : base(EffectType.IsImmediate, EffectSide.Neutral, value, 0, EffectName, String.Empty, GetDescription(value), SpritePath)
        {
        }

        protected override void ApplyAllNowInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CurHealth += (int)(entity.MaxHealth * this.Value);

            if (entity.CurHealth > entity.MaxHealth)
            {
                entity.CurHealth = entity.MaxHealth;
            }
        }

        private static string GetDescription(float value)
        {
            return $"Restores {value * 100.0f}% health points immediately.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new HealingEffect(value);
            }
        }
    }

    public class SurgeEffect : Effect
    {
        public const string EffectName = "Surge";

        public const string SpritePath = "Sprites/Effects/SurgeEffect";

        public SurgeEffect(float value) : base(EffectType.IsImmediate, EffectSide.Neutral, value, 0, EffectName, String.Empty, GetDescription(value), SpritePath)
        {
        }

        protected override void ApplyAllNowInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CurMana += (int)(entity.MaxMana * this.Value);

            if (entity.CurMana > entity.MaxMana)
            {
                entity.CurMana = entity.MaxMana;
            }
        }

        private static string GetDescription(float value)
        {
            return $"Restores {value * 100.0f}% mana points immediately.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new SurgeEffect(value);
            }
        }
    }

    public class RegenerationEffect : Effect
    {
        public const string EffectName = "Regeneration";

        public const string SpritePath = "Sprites/Effects/RegenerationEffect";

        public RegenerationEffect(float value, int duration) : base(EffectType.AffectStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CurHealth += (int)(entity.MaxHealth * this.Value);

            if (entity.CurHealth > entity.MaxHealth)
            {
                entity.CurHealth = entity.MaxHealth;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Regenerates {value * 100.0f}% health points per turn over {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new RegenerationEffect(value, duration);
            }
        }
    }

    public class RenewalEffect : Effect
    {
        public const string EffectName = "Renewal";

        public const string SpritePath = "Sprites/Effects/RenewalEffect";

        public RenewalEffect(float value, int duration) : base(EffectType.AffectStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CurMana += (int)(entity.MaxMana * this.Value);

            if (entity.CurMana > entity.MaxMana)
            {
                entity.CurMana = entity.MaxMana;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Regenerates {value * 100.0f}% mana points per turn over {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new RenewalEffect(value, duration);
            }
        }
    }

    public class WeakenEffect : Effect
    {
        public const string EffectName = "Weaken";

        public const string SpritePath = "Sprites/Effects/WeakenEffect";

        public WeakenEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Negative, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Damage -= (int)(this.Value * entity.Damage);

            if (entity.Damage < 0)
            {
                entity.Damage = 0;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Decreases damage by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new WeakenEffect(value, duration);
            }
        }
    }

    public class LethalityEffect : Effect
    {
        public const string EffectName = "Lethality";

        public const string SpritePath = "Sprites/Effects/LethalityEffect";

        public LethalityEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CritChance += this.Value;

            if (entity.CritChance > 1.0f)
            {
                entity.CritChance = 1.0f;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases critical chance by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new LethalityEffect(value, duration);
            }
        }
    }

    public class ShatterEffect : Effect
    {
        public const string EffectName = "Shatter";

        public const string SpritePath = "Sprites/Effects/ShatterEffect";

        public ShatterEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.CritMultiplier += this.Value;
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases critical multiplier by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new ShatterEffect(value, duration);
            }
        }
    }

    public class SlowEffect : Effect
    {
        public const string EffectName = "Slow";

        public const string SpritePath = "Sprites/Effects/SlowEffect";

        public SlowEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Negative, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Evasion -= this.Value;

            if (entity.Evasion < 0.0f)
            {
                entity.Evasion = 0.0f;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Decreases evasion by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new SlowEffect(value, duration);
            }
        }
    }

    public class DizzynessEffect : Effect
    {
        public const string EffectName = "Dizzyness";

        public const string SpritePath = "Sprites/Effects/DizzynessEffect";

        public DizzynessEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Negative, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Precision -= this.Value;

            if (entity.Precision < 0.0f)
            {
                entity.Precision = 0.0f;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Decreases precision by {value} points for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new DizzynessEffect(value, duration);
            }
        }
    }

    public class FlowEffect : Effect
    {
        public const string EffectName = "Flow";

        public const string SpritePath = "Sprites/Effects/FlowEffect";

        public FlowEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Evasion += this.Value;

            if (entity.Evasion > 1.0f)
            {
                entity.Evasion = 1.0f;
            }
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases evasion by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new FlowEffect(value, duration);
            }
        }
    }

    public class SharpnessEffect : Effect
    {
        public const string EffectName = "Sharpness";

        public const string SpritePath = "Sprites/Effects/SharpnessEffect";

        public SharpnessEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Precision += this.Value;
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases precision by {value} points for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new SharpnessEffect(value, duration);
            }
        }
    }

    public class BarricadeEffect : Effect
    {
        public const string EffectName = "Barricade";

        public const string SpritePath = "Sprites/Effects/BarricadeEffect";

        public BarricadeEffect(float value, int duration) : base(EffectType.ModifyStats, EffectSide.Positive, value, duration, EffectName, String.Empty, GetDescription(value, duration), SpritePath)
        {
        }

        protected override void ModifyStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            entity.Armor += (int)(entity.Armor * this.Value);
        }

        private static string GetDescription(float value, int duration)
        {
            return $"Increases armor by {value * 100.0f}% for {duration} turns.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new BarricadeEffect(value, duration);
            }
        }
    }

    public class CleanseEffect : Effect
    {
        public const string EffectName = "Cleanse";

        public const string SpritePath = "Sprites/Effects/CleanseEffect";

        public CleanseEffect() : base(EffectType.IsImmediate, EffectSide.Neutral, 0.0f, 0, EffectName, "CLEANSED", GetDescription(), SpritePath)
        {
        }

        protected override void ApplyAllNowInternal(ref EntityStats entity, ref TurnStats turn)
        {
            turn.RemoveNegativeEffects = true;
        }

        private static string GetDescription()
        {
            return "Removes all negative effects immediately.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new CleanseEffect();
            }
        }
    }

    public class DispelEffect : Effect
    {
        public const string EffectName = "Dispel";

        public const string SpritePath = "Sprites/Effects/DispelEffect";

        public DispelEffect() : base(EffectType.IsImmediate, EffectSide.Neutral, 0.0f, 0, EffectName, "DISPELLED", GetDescription(), SpritePath)
        {
        }

        protected override void ApplyAllNowInternal(ref EntityStats entity, ref TurnStats turn)
        {
            turn.RemovePositiveEffects = true;
        }

        private static string GetDescription()
        {
            return "Removes all positive effects immediately.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new DispelEffect();
            }
        }
    }

    public class StunEffect : Effect
    {
        public const string EffectName = "Stun";

        public const string SpritePath = "Sprites/Effects/StunEffect";

        public StunEffect() : base(EffectType.AffectStats, EffectSide.Neutral, 0.0f, 1, EffectName, "STUNNED", GetDescription(), SpritePath)
        {
        }

        protected override void AffectStatsInternal(ref EntityStats entity, ref TurnStats turn)
        {
            turn.BlockCurrentMove = true;
        }

        private static string GetDescription()
        {
            return "Disables current move and abilities.";
        }

        public static void RegisterForFactory()
        {
            EffectFactory.Register(EffectName, Create);

            static Effect Create(float value, int duration, in EntityStats initial)
            {
                return new StunEffect();
            }
        }
    }

    public static class EffectFactory
    {
        public delegate Effect Activator(float value, int duration, in EntityStats initial);

        private static readonly Dictionary<string, Activator> ms_activatorMap = new();

        public static void Register(string name, Activator activator)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (activator is null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            ms_activatorMap[name] = activator;
        }

        public static Effect CreateEffect(string name, float value, int duration, in EntityStats initial)
        {
            if (ms_activatorMap.TryGetValue(name, out var activator))
            {
                return activator(value, duration, in initial);
            }

            throw new Exception($"Unable to create effect named {name}");
        }

        public static void Initialize()
        {
            PoisonEffect.RegisterForFactory();
            BurnEffect.RegisterForFactory();
            ShredEffect.RegisterForFactory();
            EmpowerEffect.RegisterForFactory();
            AmplifyEffect.RegisterForFactory();
            HealingEffect.RegisterForFactory();
            SurgeEffect.RegisterForFactory();
            RegenerationEffect.RegisterForFactory();
            RenewalEffect.RegisterForFactory();
            WeakenEffect.RegisterForFactory();
            LethalityEffect.RegisterForFactory();
            ShatterEffect.RegisterForFactory();
            SlowEffect.RegisterForFactory();
            DizzynessEffect.RegisterForFactory();
            FlowEffect.RegisterForFactory();
            SharpnessEffect.RegisterForFactory();
            BarricadeEffect.RegisterForFactory();
            CleanseEffect.RegisterForFactory();
            DispelEffect.RegisterForFactory();
            StunEffect.RegisterForFactory();
        }
    }
}
