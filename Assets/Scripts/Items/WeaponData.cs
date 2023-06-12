using SupeRPG.Input;

using System;

using UnityEngine;

namespace SupeRPG.Items
{
    public class WeaponData : IItem, IDisposable
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
        public int MaxTrinketCount { get; set; }

        [Order(2)]
        public int Damage { get; set; } // base weapon dmg
        
        [Order(3)]
        public float CritChance { get; set; } // crit chance
        
        [Order(4)]
        public float CritMultiplier { get; set; } // crit modifier
        
        [Order(5)]
        public float Precision { get; set; } // precision

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

        public string Description => this.CreateDescription();

        public WeaponData()
        {
            this.m_name = String.Empty;
            this.Damage = 7;
            this.CritChance = 0.5f;
            this.CritMultiplier = 0.5f;
            this.Precision = 0.0f;
            this.Price = 100;
            this.Tier = 1;
            this.m_sprite = ResourceManager.DefaultSprite;
        }

        public void Dispose()
        {
        }

        public WeaponData Clone()
        {
            return (WeaponData)this.MemberwiseClone();
        }

        private string CreateDescription()
        {
            return "Increases " +
                $"damage by {this.Damage} points, " +
                $"crit. chance by {this.CritChance * 100.0f}%, " +
                $"crit. mult. by {this.CritMultiplier * 100.0f}%, " +
                $"precision by {this.Precision} points and has " +
                $"{this.MaxTrinketCount} trinket slots.";
        }
    }
}
