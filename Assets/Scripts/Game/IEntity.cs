using System.Collections.Generic;

using UnityEngine;

namespace SupeRPG.Game
{
    public enum AbilityUsage
    {
        CanUse,
        OnCooldown,
        NotEnoughMana,
        DoesNotExist,
    }

    public interface IEntity
    {
        Sprite Sprite { get; }

        bool IsPlayer { get; }

        bool IsAlive { get; }

        bool IsMelee { get; }

        ref readonly TurnStats TurnStats { get; }

        ref readonly EntityStats EntityStats { get; }

        IReadOnlyList<Effect> Effects { get; }

        IReadOnlyList<Ability> Abilities { get; }

        IReadOnlyList<Potion> EquippedPotions { get; }

        void InitBattle();

        void InitTurn();

        void FinishBattle();

        void Regenerate();

        void Cooldown(out int totalHeal, out int totalMana, out int totalDmgs);

        void ApplyDamage(int damage);

        void AddEffect(Effect effect);

        AbilityUsage CanUseAbility(int abilityIndex);

        AbilityUsage CanUseAbility(Ability ability);

        void ApplyImmediateEffects();

        void RemoveEffectsOfSide(EffectSide side);
    }

    public struct EntityStats
    {
        public int MaxHealth;
        public int MaxMana;
        public int CurHealth;
        public int CurMana;
        public int Armor;
        public int Damage;
        public float Evasion;
        public float Precision;
        public float CritChance;
        public float CritMultiplier;
    }

    public struct TurnStats
    {
        public bool BlockCurrentMove;
        public bool RemovePositiveEffects;
        public bool RemoveNegativeEffects;
    }
}
