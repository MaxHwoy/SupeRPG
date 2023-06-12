using SupeRPG.Items;

using System;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SupeRPG.Game
{
    public abstract class Trinket
    {
        private readonly TrinketData m_data;

        public string Name => this.m_data.Name;

        public bool IsForWeapon => this.m_data.IsWeaponTrinket;

        public int Price => this.m_data.Price;
        
        public int Tier => this.m_data.Tier;

        public Sprite Sprite => this.m_data.Sprite;

        public string Description => this.m_data.Description;

        public TrinketData Data => this.m_data;

        public Trinket(TrinketData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.m_data = data;
        }

        public bool IsTrinketDataSame(TrinketData data)
        {
            return Object.ReferenceEquals(this.m_data, data);
        }
    }

    public abstract class ArmorTrinket : Trinket
    {
        public const bool IsWeapon = false;

        public ArmorTrinket(TrinketData data) : base(data)
        {
        }

        public abstract void ModifyStats(ref ArmorStats stats);
    }

    public abstract class WeaponTrinket : Trinket
    {
        public const bool IsWeapon = true;

        public WeaponTrinket(TrinketData data) : base(data)
        {
        }

        public abstract void ModifyStats(ref WeaponStats stats);
    }

    public class ExtraArmorValueTrinket : ArmorTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "armor";

        public readonly int ExtraArmorValue;

        public ExtraArmorValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraArmorValue = (int)data.StatModify;
        }

        public override void ModifyStats(ref ArmorStats stats)
        {
            stats.Armor += this.ExtraArmorValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraArmorValueTrinket(data));
        }
    }

    public class ExtraEvasionValueTrinket : ArmorTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "evasion";

        public readonly float ExtraEvasionValue;

        public ExtraEvasionValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraEvasionValue = data.StatModify;
        }

        public override void ModifyStats(ref ArmorStats stats)
        {
            stats.Evasion += this.ExtraEvasionValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraEvasionValueTrinket(data));
        }
    }

    public class ExtraArmorPercentageTrinket : ArmorTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "armor";

        public readonly float ExtraArmorPercentage;

        public ExtraArmorPercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraArmorPercentage = data.StatModify;
        }

        public override void ModifyStats(ref ArmorStats stats)
        {
            stats.Armor += (int)(stats.Armor * this.ExtraArmorPercentage);
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraArmorPercentageTrinket(data));
        }
    }

    public class ExtraEvasionPercentageTrinket : ArmorTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "evasion";

        public readonly float ExtraEvasionPercentage;

        public ExtraEvasionPercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraEvasionPercentage = data.StatModify;
        }

        public override void ModifyStats(ref ArmorStats stats)
        {
            stats.Evasion += stats.Evasion * this.ExtraEvasionPercentage;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraEvasionPercentageTrinket(data));
        }
    }

    public class ExtraDamageValueTrinket : WeaponTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "damage";

        public readonly int ExtraDamageValue;

        public ExtraDamageValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraDamageValue = (int)data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.Damage += this.ExtraDamageValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraDamageValueTrinket(data));
        }
    }

    public class ExtraPrecisionValueTrinket : WeaponTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "precision";

        public readonly float ExtraPrecisionValue;

        public ExtraPrecisionValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraPrecisionValue = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.Precision += this.ExtraPrecisionValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraPrecisionValueTrinket(data));
        }
    }

    public class ExtraCritChanceValueTrinket : WeaponTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "critical chance";

        public readonly float ExtraCritChanceValue;

        public ExtraCritChanceValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraCritChanceValue = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.CritChance += this.ExtraCritChanceValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraCritChanceValueTrinket(data));
        }
    }

    public class ExtraCritMultiplierValueTrinket : WeaponTrinket
    {
        public const bool IsPercentage = false;

        public const string StatName = "critical multiplier";

        public readonly float ExtraCritMultiplierValue;

        public ExtraCritMultiplierValueTrinket(TrinketData data) : base(data)
        {
            this.ExtraCritMultiplierValue = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.CritMultiplier += this.ExtraCritMultiplierValue;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraCritMultiplierValueTrinket(data));
        }
    }

    public class ExtraDamagePercentageTrinket : WeaponTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "damage";

        public readonly float ExtraDamagePercentage;

        public ExtraDamagePercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraDamagePercentage = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.Damage += (int)(stats.Damage * this.ExtraDamagePercentage);
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraDamagePercentageTrinket(data));
        }
    }

    public class ExtraPrecisionPercentageTrinket : WeaponTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "precision";

        public readonly float ExtraPrecisionPercentage;

        public ExtraPrecisionPercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraPrecisionPercentage = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.Precision += stats.Precision * this.ExtraPrecisionPercentage;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraPrecisionPercentageTrinket(data));
        }
    }

    public class ExtraCritChancePercentageTrinket : WeaponTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "critical chance";

        public readonly float ExtraCritChancePercentage;

        public ExtraCritChancePercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraCritChancePercentage = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.CritChance += stats.CritChance * this.ExtraCritChancePercentage;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraCritChancePercentageTrinket(data));
        }
    }

    public class ExtraCritMultiplierPercentageTrinket : WeaponTrinket
    {
        public const bool IsPercentage = true;

        public const string StatName = "critical multiplier";

        public readonly float ExtraCritMultiplierPercentage;

        public ExtraCritMultiplierPercentageTrinket(TrinketData data) : base(data)
        {
            this.ExtraCritMultiplierPercentage = data.StatModify;
        }

        public override void ModifyStats(ref WeaponStats stats)
        {
            stats.CritMultiplier += stats.CritMultiplier * this.ExtraCritMultiplierPercentage;
        }

        public static void RegisterForFactory()
        {
            TrinketFactory.Register(IsWeapon, IsPercentage, StatName, data => new ExtraCritMultiplierPercentageTrinket(data));
        }
    }

    public static class TrinketFactory
    {
        private struct TrinketType
        {
            public bool IsWeapon;
            public bool IsPercentage;
            public string StatName;
        }

        private class TypeEqualityComparer : IEqualityComparer<TrinketType>
        {
            public bool Equals(TrinketType x, TrinketType y)
            {
                return x.IsWeapon == y.IsWeapon && x.IsPercentage == y.IsPercentage && String.CompareOrdinal(x.StatName, y.StatName) == 0;
            }

            public int GetHashCode(TrinketType obj)
            {
                return HashCode.Combine(obj.IsWeapon, obj.IsPercentage, obj.StatName);
            }
        }

        private static readonly Dictionary<TrinketType, Func<TrinketData, Trinket>> ms_activatorMap = new(new TypeEqualityComparer());

        public static void Register(bool isWeapon, bool isPercentage, string statName, Func<TrinketData, Trinket> activator)
        {
            if (activator is null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            if (String.IsNullOrEmpty(statName))
            {
                throw new ArgumentNullException(nameof(statName));
            }

            ms_activatorMap[new TrinketType()
            {
                IsWeapon = isWeapon,
                IsPercentage = isPercentage,
                StatName = statName,
            }] = activator;
        }

        public static Trinket Create(TrinketData data)
        {
            if (ms_activatorMap.TryGetValue(new TrinketType()
            {
                IsWeapon = data.IsWeaponTrinket,
                IsPercentage = data.IsPercentage,
                StatName = data.StatName,
            }, out var activator))
            {
                return activator(data);
            }

            throw new Exception($"Unable to create trinket with the following parameters: IsWeapon = {data.IsWeaponTrinket}, IsPercentage = {data.IsPercentage}, StatName = {data.StatName}");
        }

        public static void Initialize()
        {
            ExtraArmorValueTrinket.RegisterForFactory();
            ExtraArmorPercentageTrinket.RegisterForFactory();

            ExtraEvasionValueTrinket.RegisterForFactory();
            ExtraEvasionPercentageTrinket.RegisterForFactory();

            ExtraDamageValueTrinket.RegisterForFactory();
            ExtraDamagePercentageTrinket.RegisterForFactory();

            ExtraPrecisionValueTrinket.RegisterForFactory();
            ExtraPrecisionPercentageTrinket.RegisterForFactory();

            ExtraCritChanceValueTrinket.RegisterForFactory();
            ExtraCritChancePercentageTrinket.RegisterForFactory();

            ExtraCritMultiplierValueTrinket.RegisterForFactory();
            ExtraCritMultiplierPercentageTrinket.RegisterForFactory();
        }
    }
}
