using SupeRPG.Items;

using System;

using UnityEngine;

using Object = System.Object;

namespace SupeRPG.Game
{
    public struct ArmorStats
    {
        public int Armor;
        public float Evasion;
        public float Precision;
    }

    public class Armor
    {
        public const int MaxTrinketSlots = 2;

        private readonly ArmorData m_data;

        public readonly ArmorStats Stats;

        public string Name => this.m_data.Name;

        public ArmorSlotType Type => this.m_data.SlotType;

        public int MaxTrinketCount => this.m_data.MaxTrinketCount;

        public int ArmorValue => this.m_data.Value;

        public float PrecisionReduction => this.m_data.PrecisionReduction;

        public float EvasionAddition => this.m_data.EvasionAddition;

        public int Price => this.m_data.Price;
        
        public int Tier => this.m_data.Tier;

        public Sprite Sprite => this.m_data.Sprite;

        public string Description => this.m_data.Description;

        public ArmorData Data => this.m_data;

        public Armor(ArmorData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.m_data = data;

            this.Stats = new ArmorStats()
            {
                Armor = data.Value,
                Evasion = data.EvasionAddition,
                Precision = -data.PrecisionReduction,
            };
        }

        public bool IsArmorDataSame(ArmorData data)
        {
            return Object.ReferenceEquals(this.m_data, data);
        }
    }
}
