using SupeRPG.Input;

using System;

using UnityEngine;

namespace SupeRPG.Items
{
    public class TrinketData : IItem, IDisposable
    {
        private Sprite m_sprite;
        private string m_stat;
        private string m_name;

        [Order(0)]
        public string Name
        {
            get => this.m_name;
            set => this.m_name = value ?? String.Empty;
        }

        [Order(1)]
        public bool IsWeaponTrinket { get; set; }

        [Order(2)]
        public bool IsPercentage { get; set; }

        [Order(3)]
        public bool IsValueBased { get; set; }

        [Order(4)]
        public string StatName
        {
            get => this.m_stat;
            set => this.m_stat = value ?? String.Empty;
        }

        [Order(5)]
        public float StatModify { get; set; }

        [Order(6)]
        public int Price { get; set; }
        
        [Order(7)]
        public int Tier {get; set; }

        [Order(8)]
        public Sprite Sprite
        {
            get => this.m_sprite;
            set => this.m_sprite = value == null ? ResourceManager.DefaultSprite : value;
        }

        public string Description => this.CreateDescription();

        public TrinketData()
        {
            this.m_name = String.Empty;
            this.m_stat = String.Empty;
            this.StatModify = 0.0f;
            this.Price = 0;
            this.Tier = 1;
            this.m_sprite = ResourceManager.DefaultSprite;
        }

        public void Dispose()
        {
        }

        public TrinketData Clone()
        {
            return (TrinketData)this.MemberwiseClone();
        }

        private string CreateDescription()
        {
            if (this.IsWeaponTrinket)
            {
                if (this.IsPercentage)
                {
                    return $"Weapon only: increases {this.m_stat} of the weapon it is attached to by {this.StatModify * 100.0f}% of its current value";
                }
                else
                {
                    var value = this.IsValueBased ? (this.StatModify.ToString() + " points") : ((this.StatModify * 100.0f).ToString() + "%");

                    return $"Weapon only: increases total {this.m_stat} value of the weapon it is attached to by {value}";
                }
            }
            else
            {
                if (this.IsPercentage)
                {
                    return $"Armor only: increases {this.m_stat} of the armor it is attached to by {this.StatModify * 100.0f}% of its current value";
                }
                else
                {
                    var value = this.IsValueBased ? (this.StatModify.ToString() + " points") : ((this.StatModify * 100.0f).ToString() + "%");

                    return $"Armor only: increases total {this.m_stat} value of the armor it is attached to by {value}";
                }
            }
        }
    }
}
