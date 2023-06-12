using SupeRPG.Input;

using System;

using UnityEngine;

namespace SupeRPG.Game
{
    public class ClassInfo : IDisposable
    {
        private static readonly string ms_defaultPath = "Sprites/Classes/DefaultClass";

        private Sprite m_sprite;
        private string m_name;

        [Order(0)]
        public string Name
        {
            get => this.m_name;
            set => this.m_name = value ?? String.Empty;
        }

        [Order(1)]
        public bool IsMelee { get; set; }

        [Order(2)]
        public int Health { get; set; }

        [Order(3)]
        public int Mana { get; set; }

        [Order(4)]
        public int Damage { get; set; }

        [Order(5)]
        public int Armor { get; set; }

        [Order(6)]
        public float Evasion { get; set; }

        [Order(7)]
        public float Precision { get; set; }

        [Order(8)]
        public float CritChance { get; set; }

        [Order(9)]
        public float CritMultiplier { get; set; }

        [Order(10)]
        public Sprite Sprite
        {
            get
            {
                return this.m_sprite;
            }
            set
            {
                if (value == null || value == ResourceManager.DefaultSprite)
                {
                    this.m_sprite = ResourceManager.LoadSprite(ms_defaultPath);
                }
                else
                {
                    this.m_sprite = value;
                }
            }
        }

        public ClassInfo()
        {
            this.m_name = String.Empty;
            this.m_sprite = ResourceManager.LoadSprite(ms_defaultPath);
        }

        public void Dispose()
        {
        }
    }
}
