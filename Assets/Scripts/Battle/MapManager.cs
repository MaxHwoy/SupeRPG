using SupeRPG.Game;
using SupeRPG.Input;
using SupeRPG.Map;
using SupeRPG.UI;

using System.Collections;

using UnityEngine;

namespace SupeRPG.Battle
{
    public class MapManager : MonoBehaviour
    {
        public enum DifficultyLevel
        {
            None,
            Easy,
            Medium,
            Hard
        }

        private static MapManager ms_instance;

        public static MapManager Instance => ms_instance == null ? (ms_instance = FindFirstObjectByType<MapManager>()) : ms_instance;

        [SerializeField]
        private GameObject OverworldPrefab;

        public InGameBuilder InGameUI;

        public DifficultyLevel Difficulty;

        public int LevelIndex = -1;

#if DEBUG || DEVELOPMENT_BUILD
        public string OverrideRace;
        public string OverrideClass;

        public bool OverrideEnemies = true;

        public int OverrideEnemyCount = 0;
        public string[] OverrideEnemyNames = new string[4];
        public int[] OverrideEnemyHealth = new int[4];
        public int[] OverrideEnemyDamage = new int[4];
#endif

        private void Awake()
        {
            if (this.InGameUI == null)
            {
                Debug.Log("Warning: InGameUI attached is null");
            }

            if (this.OverworldPrefab == null)
            {
                Debug.Log("Warning: Overworld prefab is null");
            }
        }

#if DEBUG || DEVELOPMENT_BUILD
        private void Update()
        {
            if (Player.IsPlayerLoaded && !OverworldManager.IsOverworldNull)
            {
                var raceInfo = ResourceManager.Races.Find(_ => _.Name == this.OverrideRace);
                var classInfo = ResourceManager.Classes.Find(_ => _.Name == this.OverrideClass);

                bool updateSprite = false;

                if (raceInfo is not null && classInfo is not null)
                {
                    updateSprite = true;

                    Player.Instance.SwapRaceClass(raceInfo, classInfo);
                }
                else
                {
                    if (raceInfo is not null)
                    {
                        updateSprite = true;

                        Player.Instance.SwapRaceClass(raceInfo, Player.Instance.Class);
                    }

                    if (classInfo is not null)
                    {
                        updateSprite = true;

                        Player.Instance.SwapRaceClass(Player.Instance.Race, classInfo);
                    }
                }

                if (updateSprite)
                {
                    OverworldManager.Instance.UpdatePlayerSprite();
                }
            }
        }
#endif

        public bool CanContinue()
        {
            return Player.IsPlayerLoaded || SaveSystem.HasData();
        }

        public OverworldManager CreateOverworld()
        {
            var obj = GameObject.Instantiate(this.OverworldPrefab);

            obj.name = "Overworld";

            return obj.GetComponent<OverworldManager>();
        }

        public void LoadInGame()
        {
            this.StartCoroutine(this.LoadWithNewGameInternal());
        }

        public void LoadInGameWithContinue()
        {
            this.StartCoroutine(this.LoadWithContinueInternal());
        }

        public void ReturnToMain()
        {
            UIManager.Instance.TransitionWithDelay(() =>
            {
                OverworldManager.Instance.gameObject.SetActive(false);
                
                UIManager.Instance.PerformScreenChange(UIManager.ScreenType.Main);
            }, null, 2.0f);
        }

        public void StartBattle()
        {
            this.StartCoroutine(this.StartBattleInternal());
        }

        public void FinishBattle()
        {
            this.StartCoroutine(this.FinishBattleInternal());
        }

        public void UpdateAction(InGameBuilder.ActionType action)
        {
            this.InGameUI.UpdateAction(action);
        }

        private IEnumerator LoadWithNewGameInternal()
        {
            Debug.Assert(Player.IsPlayerLoaded);

            bool done = false;

            UIManager.Instance.BeginTransitioning(() => done = true);

            while (!done)
            {
                yield return null;
            }

            this.LevelIndex = -1;
            this.Difficulty = DifficultyLevel.None;

            bool isNull = OverworldManager.IsOverworldNull;

            var overworld = OverworldManager.Instance;

            overworld.gameObject.SetActive(true);

            yield return null;

            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);

            yield return null;

            if (!isNull)
            {
                overworld.Reinitialize();
            }

            yield return null;

            this.InGameUI.UpdateMoneyInfo();

            yield return new WaitForSeconds(2.0f);

            UIManager.Instance.EndTransitioning(null);
        }

        private IEnumerator LoadWithContinueInternal()
        {
            bool done = false;

            UIManager.Instance.BeginTransitioning(() => done = true);

            while (!done)
            {
                yield return null;
            }

            var overworld = OverworldManager.Instance;

            overworld.gameObject.SetActive(true);

            yield return null;

            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);

            yield return null;

            if (!Player.IsPlayerLoaded)
            {
                SaveSystem.LoadData();
            }

            yield return null;

            this.InGameUI.UpdateMoneyInfo();

            yield return new WaitForSeconds(2.0f);

            UIManager.Instance.EndTransitioning(null);
        }

        private IEnumerator StartBattleInternal()
        {
            bool done = false;

            UIManager.Instance.BeginTransitioning(() => done = true);

            while (!done)
            {
                yield return null;
            }

            Enemy[] enemies;

            int reward;

#if DEBUG || DEVELOPMENT_BUILD
            if (this.OverrideEnemies)
            {
                reward = 100;

                enemies = new Enemy[this.OverrideEnemyCount];

                for (int i = 0; i < enemies.Length; ++i)
                {
                    enemies[i] = new Enemy(this.OverrideEnemyNames[i], this.OverrideEnemyHealth[i], this.OverrideEnemyDamage[i]);
                }
            }
            else
#endif
            {
                Debug.Assert(this.LevelIndex >= 0 && this.LevelIndex < ResourceManager.Campaign.Count);
                Debug.Assert(this.Difficulty != DifficultyLevel.None);

                var encounter = ResourceManager.Campaign[this.LevelIndex];

                reward = encounter.EncounterReward;
                
                if (!encounter.IsBossBattle)
                {
                    reward = (int)(reward * this.Difficulty switch
                    {
                        DifficultyLevel.Easy => 1.0f,
                        DifficultyLevel.Medium => 1.1f,
                        DifficultyLevel.Hard => 1.2f,
                    });
                }

                (string[] names, int[] health, int[] damage) enemyData = this.Difficulty switch
                {
                    DifficultyLevel.Easy => (encounter.EasyEnemyList, encounter.EasyEnemyHealth, encounter.EasyEnemyDamage),
                    DifficultyLevel.Medium => (encounter.NormalEnemyList, encounter.NormalEnemyHealth, encounter.NormalEnemyDamage),
                    DifficultyLevel.Hard => (encounter.HardEnemyList, encounter.HardEnemyHealth, encounter.HardEnemyDamage),
                    _ => default, // never should be here b/c of assert and yes
                };

                Debug.Assert(enemyData.names.Length != 0);
                Debug.Assert(enemyData.names.Length == enemyData.health.Length);
                Debug.Assert(enemyData.names.Length == enemyData.damage.Length);

                enemies = new Enemy[enemyData.names.Length];

                for (int i = 0; i < enemies.Length; ++i)
                {
                    enemies[i] = new Enemy(enemyData.names[i], enemyData.health[i], enemyData.damage[i]);
                }
            }

            yield return null;

            OverworldManager.Instance.gameObject.SetActive(false);

            yield return null;

            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.Battle);

            yield return null;

            BattleManager.Instance.StartBattle(Player.Instance, enemies, reward, Player.RewardForDefeat, () =>
            {
                this.StartCoroutine(this.EndTransitionsAfterDelay(2.0f));
            }, this.FinishBattle);
        }

        private IEnumerator FinishBattleInternal()
        {
            bool done = false;

            UIManager.Instance.BeginTransitioning(() => done = true);

            while (!done)
            {
                yield return null;
            }

            var outcome = BattleManager.Instance.Outcome;

            BattleManager.Instance.FinishBattle();

            yield return null;

            var encounter = ResourceManager.Campaign[this.LevelIndex];
            var overworld = OverworldManager.Instance;

            Player.Instance.AwardReward(GetMoneyReward(encounter.EncounterReward, outcome, this.Difficulty));

            overworld.gameObject.SetActive(true);

            yield return null;

            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);

            yield return null;

            switch (outcome)
            {
                case BattleManager.BattleOutcome.Exit:
                    this.UpdateAction(InGameBuilder.ActionType.Battle);
                    break;

                case BattleManager.BattleOutcome.Defeat:
                    Player.Instance.AwardReward(5);
                    break;

                case BattleManager.BattleOutcome.Victory:
                    this.UpdateAction(InGameBuilder.ActionType.None);
                    this.Difficulty = DifficultyLevel.None;
                    OverworldManager.Instance.GenerateShop();
                    break;

                default:
                    break;
            }

            yield return this.StartCoroutine(this.EndTransitionsAfterDelay(2.0f));

            static int GetMoneyReward(int money, BattleManager.BattleOutcome outcome, DifficultyLevel difficulty)
            {
                if (outcome == BattleManager.BattleOutcome.Exit)
                {
                    return 0;
                }

                if (outcome == BattleManager.BattleOutcome.Defeat)
                {
                    return Player.RewardForDefeat;
                }

                if (outcome == BattleManager.BattleOutcome.Victory)
                {
                    return difficulty switch
                    {
                        DifficultyLevel.Easy => money,
                        DifficultyLevel.Medium => (int)(money * 1.10f),
                        DifficultyLevel.Hard => (int)(money * 1.20f),
                        _ => 0,
                    };
                }

                return 0;
            }
        }

        private IEnumerator EndTransitionsAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            UIManager.Instance.EndTransitioning(null);
        }
    }
}
