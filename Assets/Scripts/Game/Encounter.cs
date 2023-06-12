using SupeRPG.Input;

using System;

namespace SupeRPG.Game
{
    public class Encounter : IDisposable
    {
        private string[] m_hardEnemyList;
        private int[] m_hardEnemyHealth;
        private int[] m_hardEnemyDamage;
        
        private string[] m_normalEnemyList;
        private int[] m_normalEnemyHealth;
        private int[] m_normalEnemyDamage;

        private string[] m_easyEnemyList;
        private int[] m_easyEnemyHealth;
        private int[] m_easyEnemyDamage;

        private int m_encounterReward;
        private bool m_isBossBattle;

        [Order(0)]
        public string[] EasyEnemyList
        {
            get => this.m_easyEnemyList;
            set => this.m_easyEnemyList = value ?? Array.Empty<string>();
        }

        [Order(1)]
        public string[] NormalEnemyList
        {
            get => this.m_normalEnemyList;
            set => this.m_normalEnemyList = value ?? Array.Empty<string>();
        }

        [Order(2)]
        public string[] HardEnemyList
        {
            get => this.m_hardEnemyList;
            set => this.m_hardEnemyList = value ?? Array.Empty<string>();
        }

        [Order(3)]
        public int[] EasyEnemyHealth
        {
            get => this.m_easyEnemyHealth;
            set => this.m_easyEnemyHealth = value ?? Array.Empty<int>();
        }

        [Order(4)]
        public int[] NormalEnemyHealth
        {
            get => this.m_normalEnemyHealth;
            set => this.m_normalEnemyHealth = value ?? Array.Empty<int>();
        }

        [Order(5)]
        public int[] HardEnemyHealth
        {
            get => this.m_hardEnemyHealth;
            set => this.m_hardEnemyHealth = value ?? Array.Empty<int>();
        }

        [Order(6)]
        public int[] EasyEnemyDamage
        {
            get => this.m_easyEnemyDamage;
            set => this.m_easyEnemyDamage = value ?? Array.Empty<int>();
        }

        [Order(7)]
        public int[] NormalEnemyDamage
        {
            get => this.m_normalEnemyDamage;
            set => this.m_normalEnemyDamage = value ?? Array.Empty<int>();
        }

        [Order(8)]
        public int[] HardEnemyDamage
        {
            get => this.m_hardEnemyDamage;
            set => this.m_hardEnemyDamage = value ?? Array.Empty<int>();
        }

        [Order(9)]
        public int EncounterReward
        {
            get => this.m_encounterReward;
            set => this.m_encounterReward = value;
        }

        [Order(10)]
        public bool IsBossBattle
        {
            get => this.m_isBossBattle;
            set => this.m_isBossBattle = value;
        }

        public Encounter()
        {
            this.m_easyEnemyList = Array.Empty<string>();
            this.m_easyEnemyHealth = Array.Empty<int>();
            this.m_easyEnemyDamage = Array.Empty<int>();

            this.m_normalEnemyList = Array.Empty<string>();
            this.m_normalEnemyHealth = Array.Empty<int>();
            this.m_normalEnemyDamage = Array.Empty<int>();

            this.m_hardEnemyList = Array.Empty<string>();
            this.m_hardEnemyHealth = Array.Empty<int>();
            this.m_hardEnemyDamage = Array.Empty<int>();

            this.m_encounterReward = 0;
            this.m_isBossBattle = false;
        }

        public void Dispose()
        {
        }
    }
}
