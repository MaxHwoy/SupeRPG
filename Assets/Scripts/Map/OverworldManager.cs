using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SupeRPG.Battle;
using SupeRPG.Input;
using SupeRPG.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SupeRPG.Map
{
    public class OverworldManager : MonoBehaviour
    {
        public class Data
        {
            public enum CellType
            {
                None,
                Background,
                Shop,
                Battle,
            }

            public class Cell
            {
                [JsonConverter(typeof(StringEnumConverter))]
                public CellType CellType;

                [JsonConverter(typeof(StringEnumConverter))]
                public CellInfo.TileType TileType;

                public int Tier;
                public int YValue;
                public string SpritePath;
                public string[] Armors;
                public string[] Weapons;
                public string[] Potions;
                public string[] Trinkets;
            }

            public class Column
            {
                public Cell[] Cells;
                public int[] NonBlank;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public InGameBuilder.ActionType Action;
            public int CurrentX;
            public int CurrentY;
            public readonly List<Column> Columns;

            public Data()
            {
                this.Columns = new();
            }
        }

        private static OverworldManager ms_instance;

        public static bool IsOverworldNull => ms_instance == null;

        public static OverworldManager Instance => ms_instance == null ? (ms_instance = MapManager.Instance.CreateOverworld()) : ms_instance;

        public const int HorizontalTileCount = 13;
        public const int VerticalTileCount = 5;

        private const int kCenterColumn = 6;
        private const int kCenterRow = 2;
        private const int kUnitSize = 16;

        private readonly GameObject[,] m_gridTiles = new GameObject[VerticalTileCount, HorizontalTileCount];
        private readonly List<ColumnInfo> m_columns = new();
        private readonly Queue<Vector3> m_moveQueue = new();
        private readonly float m_moveSpeed = 48f;
        private InGameBuilder.ActionType m_action;
        private bool m_destination = false;
        private int m_currentX;

        public float MovePointX;
        public float MovePointY;
        public Transform Player;
        public GameObject BaseTile;
        public Transform TilesParent;
        public GameObject PlayerObject;

        private void Awake()
        {
            MapManager.Instance.InGameUI.OnUIEnabled += this.InGameUICallback;
        }

        private void OnDestroy()
        {
            if (MapManager.Instance != null && MapManager.Instance.InGameUI != null)
            {
                MapManager.Instance.InGameUI.OnUIEnabled -= this.InGameUICallback;
            }
        }

        private void Start()
        {
            this.Reinitialize();
        }

        private void Update()
        {
            var collider = InputProcessor.Instance.RaycastLeftSingular();

            if (collider)
            {
                var clickObj = collider.transform.gameObject.GetComponent<Click>();

                if (clickObj)
                {
                    clickObj.TriggerClick();
                }
            }

            this.Player.position = Vector3.MoveTowards(this.Player.position, new Vector3(0f, this.MovePointY, 0f), this.m_moveSpeed * Time.deltaTime);
            
            this.TilesParent.position = Vector3.MoveTowards(this.TilesParent.position, new Vector3(-this.MovePointX, 0f, 0f), this.m_moveSpeed * Time.deltaTime);

            UIManager.Instance.AllowSaving(!this.m_destination);

            if (!this.Moving() && this.m_moveQueue.Count != 0)
            {
                this.SetMovePoint(this.m_moveQueue.Dequeue());
            }
            else if (!this.Moving() && this.m_moveQueue.Count == 0 && this.m_destination)
            {
                this.m_destination = false;
                
                int playerY = (int)this.Player.position.y / kUnitSize + kCenterRow;
                
                Debug.Log("Arrived at position " + this.m_gridTiles[playerY, kCenterColumn].transform.position);
                
                this.SetSprites();

                var cell = this.m_columns[this.m_currentX].GetCell(playerY);

                switch (cell.Type)
                {
                    case CellInfo.TileType.Shop:
                        MapManager.Instance.Difficulty = MapManager.DifficultyLevel.None;
                        MapManager.Instance.UpdateAction(InGameBuilder.ActionType.Enter);
                        this.m_action = InGameBuilder.ActionType.Enter;
                        Unsafe.As<ShopInfo>(cell).SetupForUI();
                        this.GenerateBattle();
                        break;

                    case CellInfo.TileType.BattleEasy:
                        MapManager.Instance.Difficulty = MapManager.DifficultyLevel.Easy;
                        MapManager.Instance.UpdateAction(InGameBuilder.ActionType.Battle);
                        this.m_action = InGameBuilder.ActionType.Battle;
                        break;

                    case CellInfo.TileType.BattleMedium:
                        MapManager.Instance.Difficulty = MapManager.DifficultyLevel.Medium;
                        MapManager.Instance.UpdateAction(InGameBuilder.ActionType.Battle);
                        this.m_action = InGameBuilder.ActionType.Battle;
                        break;

                    case CellInfo.TileType.BattleHard:
                    case CellInfo.TileType.BattleMidBoss:
                    case CellInfo.TileType.BattleFinalBoss:
                        MapManager.Instance.Difficulty = MapManager.DifficultyLevel.Hard;
                        MapManager.Instance.UpdateAction(InGameBuilder.ActionType.Battle);
                        this.m_action = InGameBuilder.ActionType.Battle;
                        break;

                    default:
                        MapManager.Instance.Difficulty = MapManager.DifficultyLevel.None;
                        MapManager.Instance.UpdateAction(InGameBuilder.ActionType.None);
                        this.m_action = InGameBuilder.ActionType.None;
                        break;
                }
            }
        }

        public void SetDestination(Vector3 newPos)
        {
            int tilesX = -(int)this.TilesParent.position.x / kUnitSize;
            int playerY = (int)this.Player.position.y / kUnitSize;
            int newX = (int)newPos.x / kUnitSize;

            if (newX <= tilesX)
            {
                return;
            }
            
            MapManager.Instance.UpdateAction(this.m_action = InGameBuilder.ActionType.None);

            int newY = (int)newPos.y / kUnitSize;

            int deltaX = newX - tilesX;
            int deltaY = newY - playerY;

            int i = 0;
            int j = playerY;

            for (/* empty */; i <= deltaX / 2; ++i)
            {
                this.m_moveQueue.Enqueue(new Vector3((tilesX + i) * kUnitSize, playerY * kUnitSize, 0.0f));
            }

            if (deltaY > 0)
            {
                for (/* empty */; j <= newY; ++j)
                {
                    this.m_moveQueue.Enqueue(new Vector3((tilesX + i - 1) * kUnitSize, j * kUnitSize, 0.0f));
                }

                j--;
            }
            else if (deltaY < 0)
            {
                for (/* empty */; j >= newY; --j)
                {
                    this.m_moveQueue.Enqueue(new Vector3((tilesX + i - 1) * kUnitSize, j * kUnitSize, 0.0f));
                }

                j++;
            }

            for (/* empty */; i <= deltaX; ++i)
            {
                this.m_moveQueue.Enqueue(new Vector3((tilesX + i) * kUnitSize, j * kUnitSize, 0.0f));
            }

            this.m_destination = true;
        }

        private void ShiftGameObjects(float delta)
        {
            this.m_currentX += (int)delta;

            for (int i = 0; i < VerticalTileCount; ++i)
            {
                for (int j = 0; j < HorizontalTileCount; ++j)
                {
                    this.m_gridTiles[i, j].transform.position += new Vector3(kUnitSize * delta, 0.0f, 0.0f);

                    this.SetSprites();
                }
            }
        }

        private void GenerateLevel(List<CellInfo> newCells)
        {
            // heights range from 0 to 4
            // newCells in ascending y_value order (4 3 2 1 0)

            var tileColumn = new ColumnInfo();
            var playerSideTurns = new ColumnInfo();

            int playerHeight = (int)(this.Player.position.y + (kUnitSize * 2)) / kUnitSize;
            
            if (newCells.Count == 1)
            {
                if (newCells[0].YValue > playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.BottomRight));

                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.TopLeft));
                }
                else if (newCells[0].YValue == playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.Horizontal));

                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.Horizontal));
                }
                else if (newCells[0].YValue < playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.TopRight));

                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.BottomLeft));
                }
            }
            else if (newCells.Count > 1)
            {
                bool above = false;
                bool below = false;
                bool same = false;

                // bottom
                int bottomHeight = newCells[0].YValue;
                
                if (bottomHeight > playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.ForkRight));

                    above = true;
                }
                else if (bottomHeight == playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.ForkUp));

                    same = true;
                }
                else if (bottomHeight < playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[0].YValue, CellInfo.TileType.BottomLeft));

                    below = true;
                }

                int topIndex = newCells.Count - 1;
                int topHeight = newCells[topIndex].YValue;
                
                /*
                 * top and above -> top_left
                 * top and same -> fork_down
                 * top and below -> fork_right
                */
                
                if (topHeight > playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[topIndex].YValue, CellInfo.TileType.TopLeft));

                    above = true;
                }
                else if (topHeight == playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[topIndex].YValue, CellInfo.TileType.ForkDown));

                    same = true;
                }
                else if (topHeight < playerHeight)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(newCells[topIndex].YValue, CellInfo.TileType.ForkRight));

                    below = true;
                }

                /*
                 * middle and above -> fork_right
                 * middle and same -> crossroads
                 * middle and below -> fork_right
                */
                
                for (int i = 1; i < topIndex; ++i)
                {
                    var currentCell = newCells[i];
                    int height = currentCell.YValue;

                    if (height > playerHeight)
                    {
                        playerSideTurns.SetCell(new BackgroundInfo(newCells[i].YValue, CellInfo.TileType.ForkRight));

                        above = true;
                    }
                    else if (height == playerHeight)
                    {
                        playerSideTurns.SetCell(new BackgroundInfo(newCells[i].YValue, CellInfo.TileType.Crossroads));

                        same = true;
                    }
                    else if (height < playerHeight)
                    {
                        playerSideTurns.SetCell(new BackgroundInfo(newCells[i].YValue, CellInfo.TileType.ForkRight));

                        below = true;
                    }
                }

                if (above && !same && !below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.BottomRight));
                }
                else if (!above && same && !below)
                {
                    Debug.Log("There should not be 2 cells that match player height");
                }
                else if (!above && !same && below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.TopRight));
                }
                else if (above && same && !below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.ForkUp));
                }
                else if (!above && same && below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.ForkDown));
                }
                else if (above && !same && below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.ForkLeft));
                }
                else if (above && same && below)
                {
                    playerSideTurns.SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.Crossroads));
                }
            }
            else
            {
                Debug.Log("This should never happen");
            }

            int numDist = Random.Range(2, 4);

            int columnOffset = this.m_currentX + 1; // column index after player's current column

            for (int i = 0; i < numDist / 2; ++i)
            {
                if (columnOffset >= this.m_columns.Count)
                {
                    this.m_columns.Add(new ColumnInfo());
                }

                this.m_columns[columnOffset++].SetCell(new BackgroundInfo(playerHeight, CellInfo.TileType.Horizontal));
            }

            for (int i = playerSideTurns.NonBlank[0] + 1; i < playerSideTurns.NonBlank[^1]; ++i)
            {
                if (!playerSideTurns.NonBlank.Contains(i))
                {
                    playerSideTurns.SetCell(new BackgroundInfo(i, CellInfo.TileType.Vertical));
                }
            }

            if (columnOffset >= this.m_columns.Count)
            {
                this.m_columns.Add(playerSideTurns);

                columnOffset++;
            }
            else
            {
                var realPlayerSide = this.m_columns[columnOffset++];

                for (int i = 0; i < VerticalTileCount; ++i)
                {
                    var cell = playerSideTurns.GetCell(i);

                    if (cell.Type != CellInfo.TileType.Blank)
                    {
                        realPlayerSide.SetCell(cell);
                    }
                }
            }

            for (int i = numDist / 2; i < numDist; ++i)
            {
                if (columnOffset >= this.m_columns.Count)
                {
                    this.m_columns.Add(new ColumnInfo());
                }

                var spacer = this.m_columns[columnOffset++];

                for (int j = 0; j < newCells.Count; ++j)
                {
                    spacer.SetCell(new BackgroundInfo(newCells[j].YValue, CellInfo.TileType.Horizontal));
                }
            }

            for (int i = 0; i < newCells.Count; ++i)
            {
                tileColumn.SetCell(newCells[i]);
            }
            
            if (columnOffset >= this.m_columns.Count)
            {
                this.m_columns.Add(tileColumn);

                columnOffset++;
            }
            else
            {
                var realTile = this.m_columns[columnOffset++];

                for (int i = 0; i < VerticalTileCount; ++i)
                {
                    var cell = tileColumn.GetCell(i);

                    if (cell.Type != CellInfo.TileType.Blank)
                    {
                        realTile.SetCell(cell);
                    }
                }
            }
            
            /*
             * player-side tile logic
             * - all above -> bottom_right
             * - all below -> top_right
             * - exactly 1 and same -> horizontal
             * - all above and same -> fork_up
             * - all below and same -> fork_down
             * - above and below but not same -> fork_left
             * - above, below, and same -> crossroads
             * 
             * tile-side tile logic
             * - exactly 1 and above -> top_left
             * - exactly 1 and same -> horizontal
             * - exactly 1 and below -> bottom_left
             * 
             * - top and above -> top_left
             * - top and same -> fork_down
             * - top and below -> fork_right
             * 
             * - middle and above -> fork_right
             * - middle and same -> crossroads
             * - middle and below -> fork_right
             * 
             * - bottom and above -> fork_right
             * - bottom and same -> fork_up
             * - bottom and below -> bottom_left
            */
            
            this.SetSprites();
        }

        private bool Moving()
        {
            return this.Player.position.y != this.MovePointY || this.TilesParent.position.x != -this.MovePointX;
        }

        private void GenerateBattle()
        {
            MapManager.Instance.LevelIndex++;

            var currentEncounter = ResourceManager.Campaign[MapManager.Instance.LevelIndex];

            var level = new List<CellInfo>();
            var yValues = new List<int>();
            var heights = new HashSet<int>();
            var battleTypes = new List<CellInfo.TileType>();

            int random = Random.Range(0, 5);

            if (currentEncounter.IsBossBattle)
            {
                if (MapManager.Instance.LevelIndex + 1 == ResourceManager.Campaign.Count)
                {
                    battleTypes.Add(CellInfo.TileType.BattleFinalBoss);
                }
                else
                {
                    battleTypes.Add(CellInfo.TileType.BattleMidBoss);
                }
            }
            else
            {
                if (currentEncounter.EasyEnemyList.Length > 0)
                {
                    battleTypes.Add(CellInfo.TileType.BattleEasy);
                }

                if (currentEncounter.NormalEnemyList.Length > 0)
                {
                    battleTypes.Add(CellInfo.TileType.BattleMedium);
                }

                if (currentEncounter.HardEnemyList.Length > 0)
                {
                    battleTypes.Add(CellInfo.TileType.BattleHard);
                }
            }

            for (int i = 0; i < battleTypes.Count; ++i)
            {
                while (heights.Contains(random))
                {
                    random = Random.Range(0, 5);
                }

                heights.Add(random);
                
                level.Add(new BattleInfo(random, battleTypes[i]));
            }

            yValues.Sort();

            level.Sort((x, y) => x.YValue.CompareTo(y.YValue));

            this.GenerateLevel(level);
        }

        private void SetSprites()
        {
            for (int j = 0; j < HorizontalTileCount; j++)
            {
                int offset = this.m_currentX + j - kCenterColumn;

                Debug.Assert(offset >= 0);

                if (offset >= this.m_columns.Count)
                {
                    this.m_columns.Add(new ColumnInfo());
                }

                var column = this.m_columns[offset];

                for (int i = 0; i < VerticalTileCount; i++)
                {
                    //if (offset < 0 || offset >= this.m_columns.Count)
                    //{
                    //    this.m_gridTiles[i, j].GetComponent<SpriteRenderer>().sprite = column.GetSprite(j); //ResourceManager.LoadSprite("Map/PathTiles/Blank");
                    //    this.m_gridTiles[i, j].GetComponent<Click>().Clickable = false;
                    //}
                    //else
                    {
                        this.m_gridTiles[i, j].GetComponent<SpriteRenderer>().sprite = column.GetSprite(i);

                        if (this.m_columns[offset].GetCell(i).IsClickable())
                        {
                            this.m_gridTiles[i, j].GetComponent<Click>().Clickable = true;
                        }
                        else
                        {
                            this.m_gridTiles[i, j].GetComponent<Click>().Clickable = false;
                        }
                    }
                }
            }
        }

        private void InGameUICallback()
        {
            MapManager.Instance.UpdateAction(this.m_action);
        }

        private void SetMovePoint(Vector3 newPos)
        {
            if (this.Moving())
            {
                return;
            }

            if (newPos.x > this.MovePointX) // move left -> shift everything right by 1 tile
            {
                this.ShiftGameObjects((newPos.x - this.MovePointX) / kUnitSize);
            }
            else if (newPos.x < this.MovePointX) // move right -> shift everything left by 1 tile
            {
                this.ShiftGameObjects((newPos.x - this.MovePointX) / kUnitSize);
            }

            this.MovePointX = newPos.x;
            this.MovePointY = newPos.y;
        }

        public void GenerateShop()
        {
            var level = new List<CellInfo>();

            int height = Random.Range(0, 5);
            
            var shop = new ShopInfo(height);
            
            level.Add(shop);

            this.GenerateLevel(level);
        }

        public Data GetDataForSaving()
        {
            const int MaxColumnsSaved = HorizontalTileCount + 2;

            Debug.Assert(!this.m_destination);
            Debug.Assert(this.m_moveQueue.Count == 0);

            int columnCount = Mathf.Min(this.m_columns.Count, MaxColumnsSaved);
            int columnStart = this.m_columns.Count - columnCount;

            var data = new Data()
            {
                CurrentX = this.m_currentX - columnStart,
                CurrentY = (int)(this.MovePointY / kUnitSize) + kCenterRow,
                Action = this.m_action,
            };

            data.Columns.Capacity = Mathf.Max(data.Columns.Capacity, columnCount);

            for (int i = 0; i < columnCount; ++i)
            {
                var columnInfo = this.m_columns[columnStart + i];

                var column = new Data.Column()
                {
                    Cells = new Data.Cell[VerticalTileCount],
                    NonBlank = columnInfo.NonBlank.Count == 0 ? null : columnInfo.NonBlank.ToArray(),
                };

                for (int k = 0; k < VerticalTileCount; ++k)
                {
                    var cellInfo = columnInfo.GetCell(k);

                    var cell = new Data.Cell()
                    {
                        YValue = cellInfo.YValue,
                        TileType = cellInfo.Type,
                        SpritePath = cellInfo.SpritePath,
                    };

                    if (cellInfo is ShopInfo shop)
                    {
                        cell.CellType = Data.CellType.Shop;
                        cell.Tier = shop.Tier;
                        cell.Armors = shop.Armors.Select(_ => _.Name).ToArray();
                        cell.Weapons = shop.Weapons.Select(_ => _.Name).ToArray();
                        cell.Potions = shop.Potions.Select(_ => _.Name).ToArray();
                        cell.Trinkets = shop.Trinkets.Select(_ => _.Name).ToArray();
                    }
                    else if (cellInfo is BattleInfo)
                    {
                        cell.CellType = Data.CellType.Battle;
                    }
                    else if (cellInfo is BackgroundInfo)
                    {
                        cell.CellType = Data.CellType.Background;
                    }
                    else
                    {
                        cell.CellType = Data.CellType.None;
                    }

                    column.Cells[k] = cell;
                }

                data.Columns.Add(column);
            }

            return data;
        }

        public void Reinitialize()
        {
            this.MovePointX = 0.0f;
            this.MovePointY = 0.0f;
            this.m_currentX = 0;
            this.m_action = InGameBuilder.ActionType.None;
            this.m_destination = false;

            this.m_moveQueue.Clear();
            this.m_columns.Clear();

            this.Player.transform.position = Vector3.zero;
            this.TilesParent.transform.position = Vector3.zero;

            // Instantiate GameObjects to act as tiles
            for (int i = 0; i < VerticalTileCount; ++i)
            {
                for (int j = 0; j < HorizontalTileCount; ++j)
                {
                    var position = new Vector3((j - kCenterColumn) * kUnitSize, (i - kCenterRow) * kUnitSize, 0);

                    ref var tile = ref this.m_gridTiles[i, j];

                    if (tile == null)
                    {
                        tile = Object.Instantiate(this.BaseTile, position, this.transform.rotation, this.TilesParent);

                        tile.AddComponent<Click>();
                    }
                    else
                    {
                        tile.transform.SetPositionAndRotation(position, this.transform.rotation);
                    }
                }
            }

            // Add blanks to the left side of character starting point
            for (int i = 0; i < kCenterColumn; ++i)
            {
                this.m_columns.Add(new ColumnInfo());

                this.m_currentX++;
            }

            this.UpdatePlayerSprite();

            var starterColumn = new ColumnInfo();

            starterColumn.SetCell(new BackgroundInfo(kCenterRow, CellInfo.TileType.Horizontal));

            this.m_columns.Add(starterColumn);

            this.GenerateBattle();

            this.SetSprites();
        }

        public void ReinitializeFromSaveData(Data data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (this.m_destination)
            {
                throw new Exception("Cannot reinitialize while moving");
            }

            this.TilesParent.transform.position = Vector3.zero;

            for (int i = 0; i < VerticalTileCount; ++i)
            {
                for (int j = 0; j < HorizontalTileCount; ++j)
                {
                    this.m_gridTiles[i, j].transform.position = new Vector3((j - kCenterColumn) * kUnitSize, (i - kCenterRow) * kUnitSize, 0);
                }
            }

            this.m_moveQueue.Clear();
            this.m_columns.Clear();

            if (data.Columns is not null)
            {
                for (int i = 0; i < data.Columns.Count; ++i)
                {
                    var column = data.Columns[i];

                    var columnInfo = new ColumnInfo();

                    if (column.Cells is not null)
                    {
                        for (int k = 0; k < column.Cells.Length; ++k)
                        {
                            var cell = column.Cells[k];

                            switch (cell.CellType)
                            {
                                case Data.CellType.Shop:
                                    columnInfo.SetCellIgnoreBlankness(k, ShopInfo.CreateFromData(cell));
                                    break;

                                case Data.CellType.Battle:
                                    columnInfo.SetCellIgnoreBlankness(k, BattleInfo.CreateFromData(cell));
                                    break;

                                case Data.CellType.Background:
                                    columnInfo.SetCellIgnoreBlankness(k, BackgroundInfo.CreateFromData(cell));
                                    break;

                                default:
                                    columnInfo.SetCellIgnoreBlankness(k, CellInfo.CreateFromData(cell));
                                    break;
                            }
                        }
                    }

                    if (column.NonBlank is not null)
                    {
                        columnInfo.NonBlank.AddRange(column.NonBlank);
                    }

                    this.m_columns.Add(columnInfo);
                }
            }

            this.m_action = data.Action;
            this.m_currentX = data.CurrentX;
            this.MovePointX = 0.0f;
            this.MovePointY = (data.CurrentY - kCenterRow) * kUnitSize;

            this.Player.position = new Vector3(this.MovePointX, this.MovePointY, 0.0f);

            this.UpdatePlayerSprite();

            var current = this.m_columns[data.CurrentX].GetCell(data.CurrentY);

            if (current.Type == CellInfo.TileType.Shop)
            {
                Unsafe.As<ShopInfo>(current).SetupForUI();
            }

            this.InGameUICallback();
            this.SetSprites();
        }

        public void UpdatePlayerSprite()
        {
            if (Game.Player.IsPlayerLoaded)
            {
                this.PlayerObject.GetComponent<SpriteRenderer>().sprite = Game.Player.Instance.Sprite;
            }
        }
    }
}
