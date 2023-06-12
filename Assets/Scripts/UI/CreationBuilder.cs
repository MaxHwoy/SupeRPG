using SupeRPG.Battle;
using SupeRPG.Game;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class CreationBuilder : UIBuilder
    {
        private class RaceElement : VisualElement
        {
            public readonly RaceInfo Race;

            public readonly VisualElement Image;

            public RaceElement(RaceInfo race)
            {
                this.Race = race;
                this.name = $"race-{race.Name}-back";
                this.pickingMode = PickingMode.Ignore;

                this.style.width = 80;
                this.style.height = 80;

                this.style.marginTop = 4;
                this.style.marginBottom = 4;
                this.style.marginLeft = 4;
                this.style.marginRight = 4;

                this.style.paddingTop = 4;
                this.style.paddingBottom = 4;
                this.style.paddingLeft = 4;
                this.style.paddingRight = 4;

                this.style.unityBackgroundImageTintColor = Color.clear;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.SelectedItemPath));

                this.Image = new VisualElement()
                {
                    name = $"race-{race.Name}-image",
                    pickingMode = PickingMode.Position,
                };

                this.Image.style.flexShrink = 1.0f;
                this.Image.style.flexGrow = 1.0f;

                this.Image.style.unityBackgroundImageTintColor = Color.white;
                this.Image.style.backgroundImage = new StyleBackground(race.Sprite);
                this.Image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                this.Add(this.Image);
            }

            public void Select()
            {
                this.style.unityBackgroundImageTintColor = ms_selectTint;
            }

            public void Deselect()
            {
                this.style.unityBackgroundImageTintColor = Color.clear;
            }
        }

        private class ClassElement : VisualElement
        {
            public readonly ClassInfo Class;

            public readonly VisualElement Image;

            public ClassElement(ClassInfo @class)
            {
                this.Class = @class;
                this.name = $"class-{@class.Name}-back";
                this.pickingMode = PickingMode.Ignore;

                this.style.width = 80;
                this.style.height = 80;

                this.style.marginTop = 4;
                this.style.marginBottom = 4;
                this.style.marginLeft = 4;
                this.style.marginRight = 4;

                this.style.paddingTop = 4;
                this.style.paddingBottom = 4;
                this.style.paddingLeft = 4;
                this.style.paddingRight = 4;

                this.style.unityBackgroundImageTintColor = Color.clear;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.SelectedItemPath));

                this.Image = new VisualElement()
                {
                    name = $"class-{@class.Name}-image",
                    pickingMode = PickingMode.Position,
                };

                this.Image.style.flexShrink = 1.0f;
                this.Image.style.flexGrow = 1.0f;

                this.Image.style.unityBackgroundImageTintColor = Color.white;
                this.Image.style.backgroundImage = new StyleBackground(@class.Sprite);
                this.Image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                this.Add(this.Image);
            }

            public void Select()
            {
                this.style.unityBackgroundImageTintColor = ms_selectTint;
            }

            public void Deselect()
            {
                this.style.unityBackgroundImageTintColor = Color.clear;
            }
        }

        private const string kCharacterImage = "character-image";
        private const string kFinishButton = "finish-button";
        private const string kClassView = "class-scrollview";
        private const string kRaceView = "race-scrollview";
        private const string kHealthBar = "health-bar";
        private const string kManaBar = "mana-bar";
        private const string kDamageBar = "damage-bar";
        private const string kArmorBar = "armor-bar";
        private const string kEvasionBar = "evasion-bar";
        private const string kBonusImage = "bonus-image";
        private const string kBonusLabel = "bonus-label";

        private static readonly Color ms_hoverTint = new Color32(200, 200, 200, 255);
        private static readonly Color ms_pressTint = new Color32(170, 170, 170, 255);

        private static readonly Color ms_selectTint = Color.black;

        private readonly List<ClassElement> m_classIcons = new();
        private readonly List<RaceElement> m_raceIcons = new();

        private float m_minimumHealth;
        private float m_maximumHealth;

        private float m_minimumMana;
        private float m_maximumMana;

        private float m_minimumDamage;
        private float m_maximumDamage;

        private float m_minimumArmor;
        private float m_maximumArmor;

        private float m_minimumEvasion;
        private float m_maximumEvasion;

        private int m_selectedClass = -1;
        private int m_selectedRace = -1;

        [SerializeField]
        private Sprite HealthSprite;

        [SerializeField]
        private Sprite ManaSprite;

        [SerializeField]
        private Sprite DamageSprite;

        [SerializeField]
        private Sprite ArmorSprite;

        [SerializeField]
        private Sprite EvasionSprite;

        [SerializeField]
        private Sprite PrecisionSprite;

        [SerializeField]
        private Sprite CritChanceSprite;

        [SerializeField]
        private Sprite CritMultSprite;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
        }

        private void OnEnableEvent()
        {
            this.SetupFinishButton();
            this.SetupClassIcons();
            this.SetupRaceIcons();
            this.SetupStatistics();
        }

        private void OnDisableEvent()
        {
            this.m_selectedClass = -1;
            this.m_selectedRace = -1;
            this.m_classIcons.Clear();
            this.m_raceIcons.Clear();
        }

        private void SetupFinishButton()
        {
            var element = this.UI.rootVisualElement.Q<VisualElement>(kFinishButton);

            if (element is not null)
            {
                element.pickingMode = PickingMode.Ignore;

                element.style.unityBackgroundImageTintColor = Color.clear;

                element.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    element.style.unityBackgroundImageTintColor = Color.white;
                });

                element.RegisterCallback<MouseEnterEvent>(e =>
                {
                    element.style.unityBackgroundImageTintColor = ms_hoverTint;
                });

                element.RegisterCallback<MouseDownEvent>(e =>
                {
                    element.style.unityBackgroundImageTintColor = ms_pressTint;
                });
                
                element.RegisterCallback<MouseUpEvent>(e =>
                {
                    element.style.unityBackgroundImageTintColor = ms_hoverTint;

                    this.OnFinishEvent();
                });

                this.BindKeyAction(Key.F, this.OnFinishEvent);
            }
        }

        private void SetupClassIcons()
        {
            var container = this.UI.rootVisualElement.Q<ScrollView>(kClassView);

            if (container is not null)
            {
                var classes = ResourceManager.Classes;

                for (int i = 0; i < classes.Count; ++i)
                {
                    var icon = new ClassElement(classes[i]);

                    icon.RegisterCallback<MouseLeaveEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = Color.white;
                    });

                    icon.RegisterCallback<MouseEnterEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_hoverTint;
                    });

                    icon.RegisterCallback<MouseDownEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_pressTint;
                    });

                    icon.RegisterCallback<MouseUpEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_hoverTint;

                        int index = this.m_classIcons.IndexOf(icon);

                        if (index != this.m_selectedClass)
                        {
                            if (this.m_selectedClass >= 0)
                            {
                                var selected = this.m_classIcons[this.m_selectedClass];

                                selected.Deselect();
                            }

                            icon.Select();

                            this.m_selectedClass = index;

                            this.UpdateCharacterAndFinish();
                        }

                        Debug.Log($"Currently selected class is \"{icon.Class.Name}\"!");
                    });

                    container.Add(icon);

                    this.m_classIcons.Add(icon);
                }
            }
        }

        private void SetupRaceIcons()
        {
            var container = this.UI.rootVisualElement.Q<ScrollView>(kRaceView);

            if (container is not null)
            {
                var races = ResourceManager.Races;

                for (int i = 0; i < races.Count; ++i)
                {
                    var icon = new RaceElement(races[i]);

                    icon.RegisterCallback<MouseLeaveEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = Color.white;
                    });

                    icon.RegisterCallback<MouseEnterEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_hoverTint;
                    });

                    icon.RegisterCallback<MouseDownEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_pressTint;
                    });

                    icon.RegisterCallback<MouseUpEvent>(e =>
                    {
                        icon.Image.style.unityBackgroundImageTintColor = ms_hoverTint;

                        int index = this.m_raceIcons.IndexOf(icon);

                        if (index != this.m_selectedRace)
                        {
                            if (this.m_selectedRace >= 0)
                            {
                                var selected = this.m_raceIcons[this.m_selectedRace];

                                selected.Deselect();
                            }

                            icon.Select();

                            this.m_selectedRace = index;

                            this.UpdateCharacterAndFinish();
                        }

                        Debug.Log($"Currently selected race is \"{icon.Race.Name}\"!");
                    });

                    container.Add(icon);

                    this.m_raceIcons.Add(icon);
                }
            }
        }

        private void SetupStatistics()
        {
            var stats = ResourceManager.Classes;
            int count = stats.Count;

            if (count != 0)
            {
                float min;
                float max;

                min = Single.MaxValue;
                max = Single.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    var value = stats[i].Health;

                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }

                this.m_maximumHealth = max;
                this.m_minimumHealth = min;

                min = Single.MaxValue;
                max = Single.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    var value = stats[i].Mana;

                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }

                this.m_maximumMana = max;
                this.m_minimumMana = min;

                min = Single.MaxValue;
                max = Single.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    var value = stats[i].Damage;

                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }

                this.m_maximumDamage = max;
                this.m_minimumDamage = min;

                min = Single.MaxValue;
                max = Single.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    var value = stats[i].Armor;

                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }

                this.m_maximumArmor = max;
                this.m_minimumArmor = min;

                min = Single.MaxValue;
                max = Single.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    var value = stats[i].Evasion;

                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }

                this.m_maximumEvasion = max;
                this.m_minimumEvasion = min;
            }

            this.UpdateStatistics(null, null);
        }

        private void UpdateStatistics(RaceInfo raceInfo, ClassInfo classInfo)
        {
            var root = this.UI.rootVisualElement;

            var healthBar = root.Q<ProgressBar>(kHealthBar);
            var manaBar = root.Q<ProgressBar>(kManaBar);
            var damageBar = root.Q<ProgressBar>(kDamageBar);
            var armorBar = root.Q<ProgressBar>(kArmorBar);
            var evasionBar = root.Q<ProgressBar>(kEvasionBar);
            var bonusImage = root.Q<VisualElement>(kBonusImage);
            var bonusLabel = root.Q<Label>(kBonusLabel);

            if (classInfo is null)
            {
                if (healthBar is not null)
                {
                    healthBar.value = 0.0f;
                }

                if (manaBar is not null)
                {
                    manaBar.value = 0.0f;
                }

                if (damageBar is not null)
                {
                    damageBar.value = 0.0f;
                }

                if (armorBar is not null)
                {
                    this.m_minimumArmor = armorBar.lowValue;
                }

                if (evasionBar is not null)
                {
                    evasionBar.value = 0.0f;
                }

                if (bonusImage is not null)
                {
                    bonusImage.style.backgroundImage = new StyleBackground(StyleKeyword.None);

                    bonusImage.style.visibility = Visibility.Hidden;
                }

                if (bonusLabel is not null)
                {
                    bonusLabel.text = String.Empty;

                    bonusLabel.style.visibility = Visibility.Hidden;
                }
            }
            else
            {
                if (healthBar is not null)
                {
                    healthBar.value = RemapToRange(classInfo.Health, this.m_minimumHealth, this.m_maximumHealth, 0.0f, 100.0f);
                }

                if (manaBar is not null)
                {
                    manaBar.value = RemapToRange(classInfo.Mana, this.m_minimumMana, this.m_maximumMana, 0.0f, 100.0f);
                }

                if (damageBar is not null)
                {
                    damageBar.value = RemapToRange(classInfo.Damage, this.m_minimumDamage, this.m_maximumDamage, 0.0f, 100.0f);
                }

                if (armorBar is not null)
                {
                    armorBar.value = RemapToRange(classInfo.Armor, this.m_minimumArmor, this.m_maximumArmor, 0.0f, 100.0f);
                }

                if (evasionBar is not null)
                {
                    evasionBar.value = RemapToRange(classInfo.Evasion, this.m_minimumEvasion, this.m_maximumEvasion, 0.0f, 100.0f);
                }

                if (bonusImage is not null)
                {
                    bonusImage.style.backgroundImage = new StyleBackground(GetSpriteForBonus(raceInfo));

                    bonusImage.style.visibility = Visibility.Visible;
                }

                if (bonusLabel is not null)
                {
                    bonusLabel.text = GetStringForBonus(raceInfo);

                    bonusLabel.style.visibility = Visibility.Visible;
                }
            }

            Sprite GetSpriteForBonus(RaceInfo info)
            {
                return info.Stat switch
                {
                    Statistic.Health => this.HealthSprite,
                    Statistic.Mana => this.ManaSprite,
                    Statistic.Damage => this.DamageSprite,
                    Statistic.Armor => this.ArmorSprite,
                    Statistic.Evasion => this.EvasionSprite,
                    Statistic.Precision => this.PrecisionSprite,
                    Statistic.CritChance => this.CritChanceSprite,
                    Statistic.CritMultiplier => this.CritMultSprite,
                    _ => null,
                };
            }

            string GetStringForBonus(RaceInfo info)
            {
                return (info.Modifier * 100.0f).ToString() + "% " + info.Stat switch
                {
                    Statistic.Health => "HEALTH",
                    Statistic.Mana => "MANA",
                    Statistic.Damage => "DAMAGE",
                    Statistic.Armor => "ARMOR",
                    Statistic.Evasion => "EVASION",
                    Statistic.Precision => "PRECISION",
                    Statistic.CritChance => "CRIT. CHANCE",
                    Statistic.CritMultiplier => "CRIT. DAMAGE",
                    _ => null,
                } + " BONUS";
            }
        }

        private void UpdateCharacterAndFinish()
        {
            var raceInfo = default(RaceInfo);
            var classInfo = default(ClassInfo);
            
            if (this.IsFinishButtonInteractable())
            {
                raceInfo = this.m_raceIcons[this.m_selectedRace].Race;
                classInfo = this.m_classIcons[this.m_selectedClass].Class;
            }

            var finish = this.UI.rootVisualElement.Q<VisualElement>(kFinishButton);

            if (finish is not null)
            {
                if (raceInfo is null || classInfo is null)
                {
                    finish.style.unityBackgroundImageTintColor = Color.clear;

                    finish.pickingMode = PickingMode.Ignore;
                }
                else
                {
                    finish.style.unityBackgroundImageTintColor = Color.white;

                    finish.pickingMode = PickingMode.Position;
                }
            }

            var character = this.UI.rootVisualElement.Q<VisualElement>(kCharacterImage);

            if (character is not null)
            {
                if (raceInfo is null || classInfo is null)
                {
                    character.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                }
                else
                {
                    character.style.backgroundImage = new StyleBackground(Player.GetSpriteForRaceClass(raceInfo.Name, classInfo.Name));
                }
            }

            this.UpdateStatistics(raceInfo, classInfo);
        }

        private bool IsFinishButtonInteractable()
        {
            return this.m_selectedClass >= 0 && this.m_selectedRace >= 0;
        }

        private void OnFinishEvent()
        {
            if (this.IsFinishButtonInteractable())
            {
                var race = this.m_raceIcons[this.m_selectedRace].Race;
                var @class = this.m_classIcons[this.m_selectedClass].Class;

                Player.Initialize(race, @class);

                MapManager.Instance.LoadInGame();
            }
        }

        private static float RemapToRange(float value, float inMin, float inMax, float outMin, float outMax)
        {
            if (Mathf.Approximately(inMin, inMax))
            {
                return outMin;
            }

            return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}
