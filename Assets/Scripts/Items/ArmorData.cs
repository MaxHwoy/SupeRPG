using SupeRPG.Input;

using System;

using UnityEngine;

namespace SupeRPG.Items
{
    public enum ArmorSlotType
    {
        Helmet,
        Chestplate,
        Leggings,
    }

    public class ArmorData : IItem, IDisposable
    {
        private Sprite m_sprite;
        private string m_name;

        [Order(0)]
        public string Name
        {
            get => this.m_name;
            set => this.m_name = value ?? String.Empty;
        }

        [Order(1)]
        public ArmorSlotType SlotType { get; set; }

        [Order(2)]
        public int MaxTrinketCount { get; set; }

        [Order(3)]
        public int Value { get; set; }

        [Order(4)]
        public float PrecisionReduction { get; set; }

        [Order(5)]
        public float EvasionAddition { get; set; }

        [Order(6)]
        public int Price { get; set; }
        
        [Order(7)]
        public int Tier { get; set; }

        [Order(8)]
        public Sprite Sprite
        {
            get => this.m_sprite;
            set => this.m_sprite = value == null ? ResourceManager.DefaultSprite : value;
        }

        public string Description => this.GetDescription();

        public ArmorData()
        {
            this.m_name = String.Empty;
            this.Value = 0;
            this.Price = 0;
            this.Tier = 1;
            this.PrecisionReduction = 0.0f;
            this.EvasionAddition = 0.0f;
            this.m_sprite = ResourceManager.DefaultSprite;
        }

        public void Dispose()
        {
        }

        public ArmorData Clone()
        {
            return (ArmorData)this.MemberwiseClone();
        }

        private string GetDescription()
        {
            return $"Increases " +
                $"armor value by {this.Value} points, " +
                $"evasion by {this.EvasionAddition * 100.0f}%, reduces " +
                $"precision by {this.PrecisionReduction} points and has " +
                $"{this.MaxTrinketCount} trinket slots.";
        }
    }
}
