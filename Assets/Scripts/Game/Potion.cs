using SupeRPG.Items;

using System;

using UnityEngine;

using Object = System.Object;

namespace SupeRPG.Game
{
    public class Potion
    {
        private readonly PotionData m_data;

        public string Name => this.m_data.Name;

        public int Price => this.m_data.Price;
        
        public int Tier => this.m_data.Tier;

        public Sprite Sprite => this.m_data.Sprite;

        public string Description => this.m_data.Description;

        public string Effect => this.m_data.Effect;

        public float Modifier => this.m_data.Modifier;

        public int Duration => this.m_data.Duration;

        public PotionData Data => this.m_data;

        public Potion(PotionData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.m_data = data;
        }

        public bool IsPotionDataSame(PotionData data)
        {
            return Object.ReferenceEquals(this.m_data, data);
        }

        public Effect Use(in EntityStats initial)
        {
            return EffectFactory.CreateEffect(this.Effect, this.Modifier, this.Duration, in initial);
        }
    }
}
