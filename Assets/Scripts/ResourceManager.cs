using SupeRPG.Game;
using SupeRPG.Input;
using SupeRPG.Items;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SupeRPG
{
    public static class ResourceManager
    {
        public static readonly string DefaultTexturePath = "Sprites/Shared/DEFAULT_TEXTURE";

        public static readonly string DefaultSpritePath = "Sprites/Shared/DEFAULT_SPRITE";

        public static readonly string EnemyCursorPath = "Cursors/EnemyCursor";

        public static readonly string PlayerCursorPath = "Cursors/PlayerCursor";

        public static readonly string TargetCursorPath = "Cursors/TargetCursor";

        public static readonly string DefaultCursorPath = "Cursors/DefaultCursor";

        public static readonly string ArmorDataPath = "Data/Armor.csv";

        public static readonly string WeaponDataPath = "Data/Weapons.csv";

        public static readonly string PotionDataPath = "Data/Potions.csv";

        public static readonly string TrinketDataPath = "Data/Trinkets.csv";

        public static readonly string AbilityDataPath = "Data/Abilities.csv";

        public static readonly string RaceDataPath = "Data/Races.csv";

        public static readonly string ClassDataPath = "Data/Classes.csv";

        public static readonly string EnemyDataPath = "Data/Enemies.csv";

        public static readonly string EnemyAbilityPath = "Data/EnemyAbilities.csv";

        public static readonly string CampaignPath = "Data/Campaign.csv";

        public static readonly string DisallowPath = "UI/Shared/DisallowIcon";

        public static readonly string SelectedItemPath = "UI/Shared/SelectedItemBackground";

        private static readonly Dictionary<string, Texture2D> ms_loadedTextures = new();
        private static readonly Dictionary<string, Sprite> ms_loadedSprites = new();

        private static AbilityData[] ms_enemyAbilities;
        private static EnemyData[] ms_enemyDatas;

        private static TrinketData[] ms_trinkets;
        private static PotionData[] ms_potions;
        private static WeaponData[] ms_weapons;
        private static ArmorData[] ms_armors;

        private static Encounter[] ms_campaign;
        private static AbilityData[] ms_abilities;
        private static ClassInfo[] ms_classes;
        private static RaceInfo[] ms_races;

        private static Texture2D ms_defaultCursor;
        private static Texture2D ms_targetCursor;
        private static Texture2D ms_playerCursor;
        private static Texture2D ms_enemyCursor;

        private static Texture2D ms_defaultTexture;
        private static Sprite ms_defaultSprite;

        public static Texture2D DefaultTexture => ms_defaultTexture == null ? (ms_defaultTexture = Resources.Load<Texture2D>(DefaultTexturePath)) : ms_defaultTexture;

        public static Sprite DefaultSprite => ms_defaultSprite == null ? (ms_defaultSprite = Resources.Load<Sprite>(DefaultSpritePath)) : ms_defaultSprite;

        public static Texture2D EnemyCursor => ms_enemyCursor == null ? (ms_enemyCursor = Resources.Load<Texture2D>(EnemyCursorPath)) : ms_enemyCursor;

        public static Texture2D PlayerCursor => ms_playerCursor == null ? (ms_playerCursor = Resources.Load<Texture2D>(PlayerCursorPath)) : ms_playerCursor;

        public static Texture2D TargetCursor => ms_targetCursor == null ? (ms_targetCursor = Resources.Load<Texture2D>(TargetCursorPath)) : ms_targetCursor;

        public static Texture2D DefaultCursor => ms_defaultCursor == null ? (ms_defaultCursor = Resources.Load<Texture2D>(DefaultCursorPath)) : ms_defaultCursor;

        public static IReadOnlyList<RaceInfo> Races => ms_races ??= AssetParser.ParseFromCSV<RaceInfo>(RaceDataPath, true);

        public static IReadOnlyList<ClassInfo> Classes => ms_classes ??= AssetParser.ParseFromCSV<ClassInfo>(ClassDataPath, true);

        public static IReadOnlyList<AbilityData> Abilities => ms_abilities ??= AssetParser.ParseFromCSV<AbilityData>(AbilityDataPath, true);

        public static IReadOnlyList<ArmorData> Armors => ms_armors ??= AssetParser.ParseFromCSV<ArmorData>(ArmorDataPath, true);

        public static IReadOnlyList<WeaponData> Weapons => ms_weapons ??= AssetParser.ParseFromCSV<WeaponData>(WeaponDataPath, true);

        public static IReadOnlyList<PotionData> Potions => ms_potions ??= AssetParser.ParseFromCSV<PotionData>(PotionDataPath, true);

        public static IReadOnlyList<TrinketData> Trinkets => ms_trinkets ??= AssetParser.ParseFromCSV<TrinketData>(TrinketDataPath, true);

        public static IReadOnlyList<EnemyData> EnemyDatas => ms_enemyDatas ??= AssetParser.ParseFromCSV<EnemyData>(EnemyDataPath, true);

        public static IReadOnlyList<AbilityData> EnemyAbilityDatas => ms_enemyAbilities ??= AssetParser.ParseFromCSV<AbilityData>(EnemyAbilityPath, true);

        public static IReadOnlyList<Encounter> Campaign => ms_campaign ??= AssetParser.ParseFromCSV<Encounter>(CampaignPath, true);

        public static Texture2D LoadTexture2D(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (ms_loadedTextures.TryGetValue(path, out var texture))
            {
                if (texture != null)
                {
                    return texture;
                }
            }

            var resource = Resources.Load<Texture2D>(path);

            if (resource != null)
            {
                ms_loadedTextures[path] = resource;
            }
            else
            {
                resource = DefaultTexture;
            }

            return resource;
        }

        public static Sprite LoadSprite(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (ms_loadedSprites.TryGetValue(path, out var sprite))
            {
                if (sprite != null)
                {
                    return sprite;
                }
            }

            var resource = Resources.Load<Sprite>(path);

            if (resource != null)
            {
                ms_loadedSprites[path] = resource;
            }
            else
            {
                resource = DefaultSprite;
            }

            return resource;
        }

        public static T Find<T>(this IReadOnlyList<T> items, Func<T, bool> match)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            int count = items.Count;

            for (int i = 0; i < count; ++i)
            {
                var item = items[i];

                if (match(item))
                {
                    return item;
                }
            }

            return default;
        }
    }
}
