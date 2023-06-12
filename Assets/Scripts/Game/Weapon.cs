using SupeRPG.Items;

using System;

using UnityEngine;

using Object = System.Object;

namespace SupeRPG.Game
{
    public struct WeaponStats
    {
        public int Damage;
        public float Precision;
        public float CritChance;
        public float CritMultiplier;
    }

    public class Weapon
    {
        public const int MaxTrinketSlots = 3;

        private readonly WeaponData m_data;

        public readonly WeaponStats Stats;

        public string Name => this.m_data.Name;

        public int MaxTrinketCount => this.m_data.MaxTrinketCount;

        public int DamageValue => this.m_data.Damage;

        public float PrecisionAddition => this.m_data.Precision;

        public float CritChanceAddition => this.m_data.CritChance;

        public float CritMultiplierAddition => this.m_data.CritMultiplier;

        public int Price => this.m_data.Price;
        
        public int Tier => this.m_data.Tier;

        public Sprite Sprite => this.m_data.Sprite;

        public string Description => this.m_data.Description;

        public WeaponData Data => this.m_data;

        public Weapon(WeaponData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.m_data = data;

            this.Stats = new WeaponStats()
            {
                Damage = data.Damage,
                Precision = data.Precision,
                CritChance = data.CritChance,
                CritMultiplier = data.CritMultiplier,
            };
        }

        public bool IsWeaponDataSame(WeaponData data)
        {
            return Object.ReferenceEquals(this.m_data, data);
        }
    }
}
