using SupeRPG.Items;
using SupeRPG.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SupeRPG.Map
{
    public class CellInfo
    {
        public enum TileType
        {
            Blank,
            Horizontal,
            Vertical,
            BottomRight,
            TopRight,
            BottomLeft,
            TopLeft,
            ForkRight,
            ForkUp,
            ForkDown,
            ForkLeft,
            Crossroads,
            Shop,
            BattleEasy,
            BattleMedium,
            BattleHard,
            BattleMidBoss,
            BattleFinalBoss,
        }

        public readonly int YValue;
        public readonly TileType Type;
        public readonly string SpritePath;

        public bool IsClickable()
        {
            switch (this.Type)
            {
                case TileType.Shop:
                case TileType.BattleEasy:
                case TileType.BattleMedium:
                case TileType.BattleHard:
                case TileType.BattleMidBoss:
                case TileType.BattleFinalBoss:
                    return true;

                default:
                    return false;
            }
        }

        private CellInfo(OverworldManager.Data.Cell cell)
        {
            this.YValue = cell.YValue;
            this.Type = cell.TileType;
            this.SpritePath = cell.SpritePath;
        }

        public CellInfo(int yValue, TileType type, string spritePath)
        {
            this.YValue = yValue;
            this.Type = type;
            this.SpritePath = spritePath;
        }

        public Sprite GetSprite()
        {
            return ResourceManager.LoadSprite(SpritePath);
        }

        public static CellInfo CreateFromData(OverworldManager.Data.Cell cell)
        {
            return new CellInfo(cell);
        }
    }

    public class BackgroundInfo : CellInfo
    {
        private BackgroundInfo(OverworldManager.Data.Cell cell) : base(cell.YValue, cell.TileType, cell.SpritePath)
        {
        }

        public BackgroundInfo(int yValue, TileType type) : base(yValue, type, "Map/PathTiles/" + (type == TileType.Blank ? ("Grass" + Random.Range(1, 17).ToString()) : type.ToString()))
        //public BackgroundInfo(int yValue, TileType type) : base(yValue, type, "Map/PathTiles/" + type.ToString())
        {
        }

        public static new BackgroundInfo CreateFromData(OverworldManager.Data.Cell cell)
        {
            return new BackgroundInfo(cell);
        }
    }

    public class ShopInfo : CellInfo
    {
        public readonly int Tier;
        public readonly HashSet<ArmorData> Armors;
        public readonly HashSet<WeaponData> Weapons;
        public readonly HashSet<PotionData> Potions;
        public readonly HashSet<TrinketData> Trinkets;

        private ShopInfo(OverworldManager.Data.Cell cell) : base(cell.YValue, cell.TileType, cell.SpritePath)
        {
            this.Tier = cell.Tier;
            this.Armors = new();
            this.Weapons = new();
            this.Potions = new();
            this.Trinkets = new();

            if (cell.Armors is not null)
            {
                for (int i = 0; i < cell.Armors.Length; ++i)
                {
                    this.Armors.Add(ResourceManager.Armors.Find(_ => _.Name == cell.Armors[i]));
                }
            }

            if (cell.Weapons is not null)
            {
                for (int i = 0; i < cell.Weapons.Length; ++i)
                {
                    this.Weapons.Add(ResourceManager.Weapons.Find(_ => _.Name == cell.Weapons[i]));
                }
            }

            if (cell.Potions is not null)
            {
                for (int i = 0; i < cell.Potions.Length; ++i)
                {
                    this.Potions.Add(ResourceManager.Potions.Find(_ => _.Name == cell.Potions[i]));
                }
            }

            if (cell.Trinkets is not null)
            {
                for (int i = 0; i < cell.Trinkets.Length; ++i)
                {
                    this.Trinkets.Add(ResourceManager.Trinkets.Find(_ => _.Name == cell.Trinkets[i]));
                }
            }
        }

        public ShopInfo(int yValue) : base(yValue, TileType.Shop, "Map/Trade_Tier" + Random.Range(1, 4))
        {
            this.Tier = this.SpritePath[^1] - '0';

            this.Armors = new();
            this.Weapons = new();
            this.Potions = new();
            this.Trinkets = new();

            if (Battle.MapManager.Instance.LevelIndex + 2 >= ResourceManager.Campaign.Count)
            {
                this.Armors.UnionWith(ResourceManager.Armors);
                this.Weapons.UnionWith(ResourceManager.Weapons);
                this.Potions.UnionWith(ResourceManager.Potions);
                this.Trinkets.UnionWith(ResourceManager.Trinkets);
            }
            else
            {
                ShopInfo.GenerateItems(ResourceManager.Armors, this.Armors, this.Tier);
                ShopInfo.GenerateItems(ResourceManager.Weapons, this.Weapons, this.Tier);
                ShopInfo.GenerateItems(ResourceManager.Potions, this.Potions, this.Tier);
                ShopInfo.GenerateItems(ResourceManager.Trinkets, this.Trinkets, this.Tier);
            }
        }

        public void SetupForUI()
        {
            var armors = new ArmorData[this.Armors.Count];
            var weapons = new WeaponData[this.Weapons.Count];
            var potions = new PotionData[this.Potions.Count];
            var trinkets = new TrinketData[this.Trinkets.Count];

            this.Armors.CopyTo(armors);
            this.Weapons.CopyTo(weapons);
            this.Potions.CopyTo(potions);
            this.Trinkets.CopyTo(trinkets);

            Array.Sort(armors, (x, y) =>
            {
                if (x.Tier - y.Tier == 0)
                {
                    return x.Price - y.Price;
                }

                return y.Tier - x.Tier;
            });

            Array.Sort(weapons, (x, y) =>
            {
                if (x.Tier - y.Tier == 0)
                {
                    return x.Price - y.Price;
                }

                return y.Tier - x.Tier;
            });

            Array.Sort(potions, (x, y) =>
            {
                if (x.Tier - y.Tier == 0)
                {
                    return x.Price - y.Price;
                }

                return y.Tier - x.Tier;
            });

            Array.Sort(trinkets, (x, y) =>
            {
                if (x.Tier - y.Tier == 0)
                {
                    return x.Price - y.Price;
                }

                return y.Tier - x.Tier;
            });

            UIManager.Instance.SetupTradeItems(armors, weapons, potions, trinkets);
        }

        private static void GenerateItems<T>(IReadOnlyList<T> src, HashSet<T> dst, int tier) where T : IItem
        {
            var tier1Array = ShopInfo.GetItemsOfTier<T>(src, 1);
            var tier2Array = ShopInfo.GetItemsOfTier<T>(src, 2);
            var tier3Array = ShopInfo.GetItemsOfTier<T>(src, 3);

            int tier1Count = Mathf.Min(tier1Array.Length, tier1Array.Length >> (tier == 1 ? 1 : 2));
            int tier2Count = Mathf.Min(tier2Array.Length, tier2Array.Length >> (tier == 2 ? 1 : 2));
            int tier3Count = Mathf.Min(tier3Array.Length, tier3Array.Length >> (tier == 3 ? 1 : 2));

            ShopInfo.AddRandomItemsIntoSet(tier1Array, dst, tier1Count);
            ShopInfo.AddRandomItemsIntoSet(tier2Array, dst, tier2Count);
            ShopInfo.AddRandomItemsIntoSet(tier3Array, dst, tier3Count);
        }

        private static T[] GetItemsOfTier<T>(IReadOnlyList<T> list, int tier) where T : IItem
        {
            int count = 0;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].Tier == tier)
                {
                    count++;
                }
            }

            var array = new T[count];

            for (int i = list.Count - 1; i >= 0; --i)
            {
                var item = list[i];

                if (item.Tier == tier)
                {
                    array[--count] = item;
                }
            }

            return array;
        }

        private static void AddRandomItemsIntoSet<T>(T[] src, HashSet<T> dst, int count) where T : IItem
        {
            Span<bool> table = stackalloc bool[src.Length];

            table.Fill(false);

            while (count > 0)
            {
                int index = Random.Range(0, src.Length);

                if (!table[index])
                {
                    table[index] = true;

                    bool added = dst.Add(src[index]);

                    Debug.Assert(added);

                    count--;
                }
            }
        }

        public static new ShopInfo CreateFromData(OverworldManager.Data.Cell cell)
        {
            return new ShopInfo(cell);
        }
    }

    public class BattleInfo : CellInfo
    {
        private BattleInfo(OverworldManager.Data.Cell cell) : base(cell.YValue, cell.TileType, cell.SpritePath)
        {
        }

        public BattleInfo(int yValue, TileType type) : base(yValue, type, "Sprites/Battle/" + type.ToString())
        {
        }

        public static new BattleInfo CreateFromData(OverworldManager.Data.Cell cell)
        {
            return new BattleInfo(cell);
        }
    }

    public class ColumnInfo
    {
        private readonly CellInfo[] m_cells;

        public readonly List<int> NonBlank;

        public ColumnInfo()
        {
            this.NonBlank = new();
            this.m_cells = new CellInfo[OverworldManager.VerticalTileCount];

            for (int i = 0; i < OverworldManager.VerticalTileCount; ++i)
            {
                this.m_cells[i] = new BackgroundInfo(i, CellInfo.TileType.Blank);
            }
        }

        public Sprite GetSprite(int tileRow)
        {
            return this.m_cells[tileRow].GetSprite();
        }

        public CellInfo GetCell(int index)
        {
            return this.m_cells[index];
        }

        public static CellInfo RandomCell(int yValue)
        {
            if (Random.Range(0, 2) == 1)
            {
                return new ShopInfo(yValue);
            }
            else
            {
                return new BackgroundInfo(yValue, CellInfo.TileType.Blank);
            }
        }

        public void SetCell(CellInfo newCell)
        {
            this.m_cells[newCell.YValue] = newCell;
            this.NonBlank.Add(newCell.YValue);
            this.NonBlank.Sort();
        }

        public void SetCellIgnoreBlankness(int index, CellInfo cell)
        {
            this.m_cells[index] = cell;
        }
    }
}
