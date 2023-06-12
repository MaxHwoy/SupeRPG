using Newtonsoft.Json;

using SupeRPG.Battle;
using SupeRPG.Game;
using SupeRPG.Map;

using System;
using System.IO;

using UnityEngine;

namespace SupeRPG.Input
{
    public static class SaveSystem
    {
        private class Data
        {
            public int CampaignLevel;
            public int CampaignDifficulty;
            public Player.Data PlayerData;
            public OverworldManager.Data MapData;
        }

        private const string kSavePath = "SaveData.json";

        private static string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, kSavePath);
        }

        public static bool HasData()
        {
            return File.Exists(GetPath());
        }

        public static void LoadData()
        {
            var path = GetPath();

            if (!File.Exists(path))
            {
                throw new Exception("Cannot load because no save data");
            }

            var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(path));

            Player.ReinitializeFromSaveData(data.PlayerData);
            MapManager.Instance.LevelIndex = data.CampaignLevel;
            MapManager.Instance.Difficulty = (MapManager.DifficultyLevel)data.CampaignDifficulty;
            OverworldManager.Instance.ReinitializeFromSaveData(data.MapData);
        }

        public static void SaveData()
        {
            var data = new Data()
            {
                CampaignLevel = MapManager.Instance.LevelIndex,
                CampaignDifficulty = (int)MapManager.Instance.Difficulty,
                PlayerData = Player.Instance.GetDataForSaving(),
                MapData = OverworldManager.Instance.GetDataForSaving(),
            };

            File.WriteAllText(GetPath(), JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
