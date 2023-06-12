using SupeRPG.Game;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class BattleBuilder : UIBuilder
    {
        private enum SelectionType
        {
            None,
            Regular,
            Ability,
            Potion,
        }

        private enum AnimationType
        {
            None,
            Hide,
            Show,
        }

        private struct TooltipData
        {
            public Sprite Icon;
            public string Name;
            public string Type;
            public string Time;
            public string Cost;
            public string Desc;
        }

        private interface ITooltipProvider
        {
            ref readonly TooltipData Data { get; }
        }

        private interface ISelectableItem
        {
            SelectionType Type { get; }

            int Index { get; }

            void Select();

            void Deselect();
        }

        private class HealthStatistic : ITooltipProvider
        {
            private readonly TooltipData m_data;

            public readonly ProgressBar Bar;

            public readonly VisualElement Icon;

            public readonly VisualElement Background;

            public readonly VisualElement Progress;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public HealthStatistic(string name, string description, string icon, string bar, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(icon);

                this.Bar = builder.UI.rootVisualElement.Q<ProgressBar>(bar);

                if (this.Bar is not null)
                {
                    var progress = this.Bar.Q<VisualElement>("unity-progress-bar");

                    if (progress is not null)
                    {
                        for (int i = 0; i < progress.childCount; ++i)
                        {
                            var background = progress[i];

                            if (background.ClassListContains("unity-progress-bar__background"))
                            {
                                this.Background = background;

                                for (int k = 0; k < this.Background.childCount; ++k)
                                {
                                    var child = background[k];

                                    if (child.ClassListContains("unity-progress-bar__progress"))
                                    {
                                        this.Progress = child;

                                        goto LABEL_DATA;
                                    }
                                }
                            }
                        }
                    }
                }

            LABEL_DATA:
                this.m_data = new TooltipData()
                {
                    Icon = this.Icon?.resolvedStyle.backgroundImage.sprite,
                    Name = name,
                    Type = kStatistic,
                    Time = String.Empty,
                    Cost = String.Empty,
                    Desc = description,
                };

                this.SetupCallbacks();

                this.UpdateBar(0, 0);
            }

            private void SetupCallbacks()
            {
                if (this.Icon is not null)
                {
                    this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, false);
                        }
                    });

                    this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, true);
                        }
                    });
                }
            }

            public void UpdateBar(int value, int max)
            {
                if (this.Bar is not null)
                {
                    this.Bar.value = RemapToRange(value, 0.0f, max, this.Bar.lowValue, this.Bar.highValue);

                    this.Bar.title = String.Concat(value.ToString(), "/", max.ToString());

                    if (this.Background is not null && this.Progress is not null)
                    {
                        var percent = RemapToRange(value, 0.0f, max, 0.0f, 100.0f);

                        if (percent <= 15.0f) // red
                        {
                            this.Background.style.backgroundColor = (Color)new Color32(150, 40, 40, 255);
                            this.Progress.style.backgroundColor = (Color)new Color32(219, 52, 43, 255);
                        }
                        else if (percent <= 50.0f) // yellow
                        {
                            this.Background.style.backgroundColor = (Color)new Color32(127, 127, 40, 255);
                            this.Progress.style.backgroundColor = (Color)new Color32(223, 223, 35, 255);
                        }
                        else // green
                        {
                            this.Background.style.backgroundColor = (Color)new Color32(58, 115, 55, 255);
                            this.Progress.style.backgroundColor = (Color)new Color32(91, 207, 55, 255);
                        }
                    }
                }
            }
        }

        private class ManaStatistic : ITooltipProvider
        {
            private readonly TooltipData m_data;

            public readonly ProgressBar Bar;

            public readonly VisualElement Icon;

            public readonly VisualElement Parent;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public ManaStatistic(string name, string description, string icon, string bar, string parent, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(icon);

                this.Bar = builder.UI.rootVisualElement.Q<ProgressBar>(bar);

                this.Parent = builder.UI.rootVisualElement.Q<VisualElement>(parent);

                this.m_data = new TooltipData()
                {
                    Icon = this.Icon?.resolvedStyle.backgroundImage.sprite,
                    Name = name,
                    Type = kStatistic,
                    Time = String.Empty,
                    Cost = String.Empty,
                    Desc = description,
                };

                this.SetupCallbacks();

                this.UpdateBar(0, 0);
            }

            private void SetupCallbacks()
            {
                if (this.Icon is not null)
                {
                    this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, false);
                        }
                    });

                    this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, true);
                        }
                    });
                }
            }

            public void UpdateBar(int value, int max)
            {
                if (this.Bar is not null)
                {
                    this.Bar.value = RemapToRange(value, 0.0f, max, this.Bar.lowValue, this.Bar.highValue);

                    this.Bar.title = String.Concat(value.ToString(), "/", max.ToString());
                }
            }

            public void Display(bool enable)
            {
                if (this.Parent is not null)
                {
                    if (enable)
                    {
                        this.Parent.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        this.Parent.style.display = DisplayStyle.None;
                    }
                }
            }
        }

        private class TextStatistic : ITooltipProvider
        {
            private readonly TooltipData m_data;

            public readonly Label Text;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public TextStatistic(string name, string description, string icon, string text, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(icon);

                this.Text = builder.UI.rootVisualElement.Q<Label>(text);

                this.m_data = new TooltipData()
                {
                    Icon = this.Icon?.resolvedStyle.backgroundImage.sprite,
                    Name = name,
                    Type = kStatistic,
                    Time = String.Empty,
                    Cost = String.Empty,
                    Desc = description,
                };

                this.SetupCallbacks();
            }

            private void SetupCallbacks()
            {
                if (this.Icon is not null)
                {
                    this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, false);
                        }
                    });

                    this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.Builder.DisplayTooltip(this, true);
                        }
                    });
                }
            }

            public void UpdateText(int value, bool usePercentage)
            {
                if (this.Text is not null)
                {
                    this.Text.text = value.ToString() + (usePercentage ? "%" : String.Empty);
                }
            }
        }

        private class EffectStatistic : ITooltipProvider
        {
            private static int ms_uniqueId;

            private readonly TooltipData m_data;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public EffectStatistic(Effect effect, VisualElement parent, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = new VisualElement();

                this.m_data = new TooltipData()
                {
                    Icon = effect.Sprite,
                    Name = effect.Name,
                    Type = kEffect,
                    Time = $"Duration: {effect.RemainingDuration}",
                    Cost = String.Empty,
                    Desc = effect.Description,
                };

                this.Icon.name = "effect-icon-" + ms_uniqueId++.ToString();

                this.Icon.style.flexShrink = 0.0f;
                this.Icon.style.flexGrow = 0.0f;

                this.Icon.style.width = 16.0f;
                this.Icon.style.height = 16.0f;

                this.Icon.style.marginLeft = 2.0f;
                this.Icon.style.marginRight = 2.0f;
                this.Icon.style.marginTop = 2.0f;
                this.Icon.style.marginBottom = 2.0f;

                this.Icon.style.borderLeftWidth = 1.0f;
                this.Icon.style.borderRightWidth = 1.0f;
                this.Icon.style.borderTopWidth = 1.0f;
                this.Icon.style.borderBottomWidth = 1.0f;

                this.Icon.style.borderLeftColor = Color.black;
                this.Icon.style.borderRightColor = Color.black;
                this.Icon.style.borderTopColor = Color.black;
                this.Icon.style.borderBottomColor = Color.black;

                this.Icon.style.backgroundImage = new StyleBackground(effect.Sprite);

                parent.Add(this.Icon);

                this.SetupCallbacks();
            }

            private void SetupCallbacks()
            {
                this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.Builder.DisplayTooltip(this, false);
                    }
                });

                this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.Builder.DisplayTooltip(this, true);
                    }
                });
            }
        }

        private class EnemyAbility : ITooltipProvider
        {
            private TooltipData m_data;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public EnemyAbility(VisualElement element, BattleBuilder builder)
            {
                this.Icon = element;

                this.Builder = builder;

                element.pickingMode = PickingMode.Ignore;

                element.style.visibility = Visibility.Hidden;

                element.style.backgroundImage = new StyleBackground(StyleKeyword.None);

                this.SetupCallbacks();
            }

            private void SetupCallbacks()
            {
                this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.Builder.DisplayTooltip(this, false);
                    }
                });

                this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.Builder.DisplayTooltip(this, true);
                    }
                });
            }

            public void UpdateInfo(Ability ability)
            {
                this.Icon.style.backgroundImage = new StyleBackground(ability.Sprite);

                this.m_data = new TooltipData()
                {
                    Icon = ability.Sprite,
                    Name = ability.Name,
                    Type = kAbility,
                    Time = ability.IsOnCooldown ? $"Remaining Cooldown: {ability.RemainingCooldown}" : "Can Use",
                    Cost = String.Empty,
                    Desc = ability.Description,
                };
            }
        }

        private class AttackItem : ITooltipProvider, ISelectableItem, IDisposable
        {
            private readonly TooltipData m_data;
            private bool m_pressed;
            private bool m_locked;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public SelectionType Type => SelectionType.Regular;

            public int Index => 0;

            public AttackItem(BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(kRegularAttackIcon);

                this.m_data = new TooltipData()
                {
                    Icon = this.Icon?.resolvedStyle.backgroundImage.sprite,
                    Name = kAttackName,
                    Type = kAttackType,
                    Time = String.Empty,
                    Cost = String.Empty,
                    Desc = kAttackDesc,
                };

                this.SetupCallbacks(true);
            }

            ~AttackItem()
            {
                this.SetupCallbacks(false);
            }

            private void SetupCallbacks(bool attach)
            {
                if (this.Icon is not null)
                {
                    if (attach)
                    {
                        this.Icon.pickingMode = PickingMode.Position;

                        this.Icon.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                    else
                    {
                        this.Icon.pickingMode = PickingMode.Ignore;

                        this.Icon.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.UnregisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                }

                void OnPointerLeaveEvent(PointerLeaveEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, false);
                    }
                }

                void OnPointerEnterEvent(PointerEnterEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, true);
                    }
                }

                void OnPointerDownEvent(PointerDownEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0)
                    {
                        this.m_pressed = true;
                    }
                }

                void OnPointerUpEvent(PointerUpEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0 && this.m_pressed)
                    {
                        this.m_pressed = false;

                        this.Builder.SelectItem(this);
                    }
                }
            }

            public void Select()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderSelectColor;
                }
            }

            public void Deselect()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderDefaultColor;
                }
            }

            public void SetLocked(bool locked)
            {
                this.m_locked = locked;
                this.m_pressed = false;
            }

            public void Dispose()
            {
                this.SetupCallbacks(false);
                GC.SuppressFinalize(this);
            }
        }

        private class AbilityItem : ITooltipProvider, ISelectableItem, IDisposable
        {
            private readonly TooltipData m_data;
            private readonly Ability m_ability;
            private readonly int m_index;
            private bool m_selecatable;
            private bool m_pressed;
            private bool m_locked;

            public readonly Label Text;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public SelectionType Type => SelectionType.Ability;

            public int Index => this.m_index;

            public AbilityItem(string icon, string text, int index, Ability ability, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(icon);

                this.Text = builder.UI.rootVisualElement.Q<Label>(text);

                if (ability is null)
                {
                    this.m_data = new TooltipData()
                    {
                        Icon = null,
                        Name = String.Empty,
                        Type = kAbility,
                        Time = String.Empty,
                        Cost = String.Empty,
                        Desc = String.Empty,
                    };
                }
                else
                {
                    this.m_data = new TooltipData()
                    {
                        Icon = ability.Sprite,
                        Name = ability.Name,
                        Type = kAbility,
                        Time = $"Cooldown: {ability.CooldownTime}",
                        Cost = $"Mana Cost: {ability.ManaCost}",
                        Desc = ability.Description,
                    };
                }

                this.m_index = index;

                this.m_ability = ability;

                this.SetupCallbacks(true);

                this.UpdateStatus();
            }

            ~AbilityItem()
            {
                this.SetupCallbacks(false);
            }

            private void SetupCallbacks(bool attach)
            {
                if (this.Icon is not null)
                {
                    if (attach)
                    {
                        this.Icon.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                    else
                    {
                        this.Icon.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.UnregisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                }

                void OnPointerLeaveEvent(PointerLeaveEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, false);
                    }
                }

                void OnPointerEnterEvent(PointerEnterEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, true);
                    }
                }

                void OnPointerDownEvent(PointerDownEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0 && this.m_selecatable)
                    {
                        this.m_pressed = true;
                    }
                }

                void OnPointerUpEvent(PointerUpEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0 && this.m_selecatable && this.m_pressed)
                    {
                        this.m_pressed = false;

                        this.Builder.SelectItem(this);
                    }
                }
            }

            public void Select()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderSelectColor;
                }
            }

            public void Deselect()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderDefaultColor;
                }
            }

            public void SetLocked(bool locked)
            {
                this.m_locked = locked;
                this.m_pressed = false;
            }

            public void UpdateStatus()
            {
                if (this.Icon is not null)
                {
                    if (this.m_ability is not null)
                    {
                        var usage = this.m_ability.Owner.CanUseAbility(this.m_ability);

                        if (usage == AbilityUsage.OnCooldown)
                        {
                            this.Icon.style.backgroundColor = new StyleColor(new Color32(55, 55, 55, 255));
                            this.Icon.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                            this.m_selecatable = false;
                        }
                        else if (usage == AbilityUsage.NotEnoughMana)
                        {
                            this.Icon.style.backgroundColor = new StyleColor(new Color32(30, 60, 155, 255));
                            this.Icon.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                            this.m_selecatable = false;
                        }
                        else if (usage == AbilityUsage.CanUse)
                        {
                            this.Icon.style.backgroundColor = new StyleColor(ms_defaultInteractColor);
                            this.Icon.style.backgroundImage = new StyleBackground(this.m_ability.Sprite);
                            this.m_selecatable = true;
                        }
                        else
                        {
                            throw new Exception($"Ability {this.m_ability} does not belong to its owner?");
                        }

                        this.Icon.pickingMode = PickingMode.Position;

                        if (this.Text is not null)
                        {
                            this.Text.pickingMode = PickingMode.Position;

                            if (usage == AbilityUsage.OnCooldown)
                            {
                                this.Text.text = this.m_ability.RemainingCooldown.ToString();
                            }
                            else
                            {
                                this.Text.text = String.Empty;
                            }
                        }
                    }
                    else
                    {
                        this.Icon.style.backgroundColor = new StyleColor(ms_defaultInteractColor);
                        this.Icon.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                        this.Icon.pickingMode = PickingMode.Ignore;
                        this.m_selecatable = false;

                        if (this.Text is not null)
                        {
                            this.Text.pickingMode = PickingMode.Ignore;

                            this.Text.text = String.Empty;
                        }
                    }
                }
            }

            public void Dispose()
            {
                this.SetupCallbacks(false);
                GC.SuppressFinalize(this);
            }
        }

        private class PotionItem : ITooltipProvider, ISelectableItem, IDisposable
        {
            private readonly int m_index;
            private TooltipData m_data;
            private bool m_pressed;
            private bool m_locked;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ref readonly TooltipData Data => ref this.m_data;

            public SelectionType Type => SelectionType.Potion;

            public int Index => this.m_index;

            public PotionItem(string name, int index, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(name);

                this.m_index = index;

                this.SetupCallbacks(true);

                this.UpdatePotion(null);
            }

            ~PotionItem()
            {
                this.SetupCallbacks(false);
            }

            private void SetupCallbacks(bool attach)
            {
                if (this.Icon is not null)
                {
                    if (attach)
                    {
                        this.Icon.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                    else
                    {
                        this.Icon.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
                        this.Icon.UnregisterCallback<PointerEnterEvent>(OnPointerEnterEvent);
                        this.Icon.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
                        this.Icon.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
                    }
                }

                void OnPointerLeaveEvent(PointerLeaveEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, false);
                    }
                }

                void OnPointerEnterEvent(PointerEnterEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed = false;

                        this.Builder.DisplayTooltip(this, true);
                    }
                }

                void OnPointerDownEvent(PointerDownEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0)
                    {
                        this.m_pressed = true;
                    }
                }

                void OnPointerUpEvent(PointerUpEvent e)
                {
                    if (this.Icon.pickingMode == PickingMode.Position && !this.m_locked && e.button == 0 && this.m_pressed)
                    {
                        this.m_pressed = false;

                        this.Builder.SelectItem(this);
                    }
                }
            }

            public void Select()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderSelectColor;
                }
            }

            public void Deselect()
            {
                if (this.Icon is not null)
                {
                    var style = this.Icon.style;

                    style.borderBottomColor = style.borderLeftColor = style.borderRightColor = style.borderTopColor = ms_borderDefaultColor;
                }
            }

            public void SetLocked(bool locked)
            {
                this.m_locked = locked;
                this.m_pressed = false;
            }

            public void UpdatePotion(Potion potion)
            {
                if (this.Icon is not null)
                {
                    if (potion is null)
                    {
                        this.m_data = new TooltipData()
                        {
                            Icon = null,
                            Name = String.Empty,
                            Type = kPotion,
                            Time = String.Empty,
                            Cost = String.Empty,
                            Desc = String.Empty,
                        };

                        this.Icon.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                        this.Icon.pickingMode = PickingMode.Ignore;
                    }
                    else
                    {
                        this.m_data = new TooltipData()
                        {
                            Icon = potion.Sprite,
                            Name = potion.Name,
                            Type = kPotion,
                            Time = $"Duration: {potion.Duration}",
                            Cost = String.Empty,
                            Desc = potion.Description,
                        };

                        this.Icon.style.backgroundImage = new StyleBackground(potion.Sprite);
                        this.Icon.pickingMode = PickingMode.Position;
                    }
                }
            }

            public void Dispose()
            {
                this.SetupCallbacks(false);
                GC.SuppressFinalize(this);
            }
        }

        private class ActionButton
        {
            private bool m_pressed;
            private bool m_locked; // whether locked from interacting
            private bool m_using; // whether currently casting

            public readonly Label Text;

            public readonly VisualElement Icon;

            public readonly BattleBuilder Builder;

            public ActionButton(string icon, string text, BattleBuilder builder)
            {
                this.Builder = builder;

                this.Icon = builder.UI.rootVisualElement.Q<VisualElement>(icon);

                this.Text = builder.UI.rootVisualElement.Q<Label>(text);

                this.SetupCallbacks();

                this.UpdateStatus();
            }

            private void SetupCallbacks()
            {
                if (this.Icon is not null)
                {
                    this.Icon.RegisterCallback<PointerLeaveEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.m_pressed = false;

                            this.Icon.style.backgroundColor = ms_buttonIdledColor;
                        }
                    });

                    this.Icon.RegisterCallback<PointerEnterEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position)
                        {
                            this.m_pressed = false;

                            this.Icon.style.backgroundColor = ms_buttonHoverColor;
                        }
                    });

                    this.Icon.RegisterCallback<PointerDownEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position && e.button == 0)
                        {
                            this.m_pressed = true;

                            this.Icon.style.backgroundColor = ms_buttonPressColor;
                        }
                    });

                    this.Icon.RegisterCallback<PointerUpEvent>(e =>
                    {
                        if (this.Icon.pickingMode == PickingMode.Position && e.button == 0 && this.m_pressed)
                        {
                            this.m_pressed = false;

                            this.Icon.style.backgroundColor = ms_buttonHoverColor;

                            if (this.m_using)
                            {
                                this.m_using = false;

                                Debug.Assert(this.Builder.m_currentSelected is not null);

                                this.UpdateText();

                                this.Builder.OnCancelInteractRequest?.Invoke();
                            }
                            else
                            {
                                this.m_using = true;

                                var selected = this.Builder.m_currentSelected;

                                Debug.Assert(selected is not null);

                                this.UpdateText();

                                switch (this.Builder.m_currentSelected.Type)
                                {
                                    case SelectionType.Regular:
                                        this.Builder.OnUseAttackRequest?.Invoke();
                                        break;

                                    case SelectionType.Ability:
                                        this.Builder.OnUseAbilityRequest?.Invoke(selected.Index);
                                        break;

                                    case SelectionType.Potion:
                                        this.Builder.OnUsePotionRequest?.Invoke(selected.Index);
                                        break;
                                }
                            }
                        }
                    });
                }
            }

            private void UpdateText()
            {
                if (this.Text is not null)
                {
                    if (this.Builder.m_currentSelected is null || this.m_locked)
                    {
                        this.Text.text = "LOCKED";

                        this.Text.pickingMode = PickingMode.Ignore;
                    }
                    else
                    {
                        if (this.m_using)
                        {
                            this.Text.text = "CANCEL";
                        }
                        else
                        {
                            this.Text.text = this.Builder.m_currentSelected.Type switch
                            {
                                SelectionType.Regular => "ATTACK",
                                SelectionType.Ability => "CAST",
                                SelectionType.Potion => "USE",
                                _ => throw new Exception("Invalid selection type"),
                            };
                        }

                        this.Text.pickingMode = PickingMode.Position;
                    }
                }
            }

            public void SetLocked(bool locked)
            {
                this.m_locked = locked;
                this.m_pressed = false;
                this.UpdateStatus();
            }

            public void SetUsage(bool usage)
            {
                this.m_using = usage;
                this.UpdateStatus();
            }

            public void UpdateStatus()
            {
                if (this.Icon is not null)
                {
                    if (this.Builder.m_currentSelected is null || this.m_locked)
                    {
                        this.Icon.style.backgroundColor = ms_buttonLockedColor;
                        this.Icon.pickingMode = PickingMode.Ignore;
                    }
                    else
                    {
                        this.Icon.style.backgroundColor = ms_buttonIdledColor;
                        this.Icon.pickingMode = PickingMode.Position;
                    }
                }

                this.UpdateText();
            }

            public void SuperReset()
            {
                this.m_pressed = false;

                if (this.Icon is not null)
                {
                    this.Icon.style.backgroundColor = ms_buttonLockedColor;
                    this.Icon.pickingMode = PickingMode.Ignore;
                }

                if (this.Text is not null)
                {
                    this.Text.text = "LOCKED";
                    this.Text.pickingMode = PickingMode.Ignore;
                }
            }
        }

        private class HealthIndicator
        {
            private const float kLayoutWidth = 10.0f; // technically width and height should not matter since overflow
            private const float kLayoutHeight = 10.0f; // context solves all issues, but we need it for precise calcs

            private static int ms_uniqueId;

            private Vector2 m_currentPosition;

            public readonly IEntity Entity;

            public readonly ProgressBar Bar;

            public readonly VisualElement Layout;

            public readonly Func<Vector2> Fetcher;

            public HealthIndicator(Func<Vector2> fetcher, IEntity entity, BattleBuilder builder)
            {
                IStyle style;

                this.Entity = entity;
                this.Fetcher = fetcher;
                this.m_currentPosition = new Vector2(Single.NaN, Single.NaN);

                this.Layout = new VisualElement()
                {
                    name = "health-indicator-layout-" + ms_uniqueId.ToString(),
                    pickingMode = PickingMode.Ignore,
                };

                this.Bar = new ProgressBar()
                {
                    name = "health-indicator-bar-" + ms_uniqueId++.ToString(),
                    pickingMode = PickingMode.Ignore,
                    title = String.Empty,
                    lowValue = 0.0f,
                    highValue = 100.0f,
                    value = 50.0f,
                };

                style = this.Layout.style;

                style.position = Position.Absolute;
                style.width = new StyleLength(new Length(kLayoutWidth, LengthUnit.Percent));
                style.height = new StyleLength(new Length(kLayoutHeight, LengthUnit.Percent));

                style.flexGrow = 0.0f;
                style.flexShrink = 0.0f;
                style.alignItems = Align.Center;
                style.justifyContent = Justify.Center;

                this.Bar.AddToClassList("health-indicator__container");
                this.Bar.AddToClassList("health-indicator__background");
                this.Bar.AddToClassList("health-indicator__progress");
                this.Bar.AddToClassList("health-indicator__title");
                this.Bar.AddToClassList("health-indicator");

                this.Layout.Add(this.Bar);

                builder.m_battleOverlay.Add(this.Layout);

                this.Update();
            }

            public void Update()
            {
                var position = this.Fetcher();

                float x = (position.x + 1.0f) * 50.0f - (kLayoutWidth * 0.5f);
                float y = (position.y + 1.0f) * 50.0f - (kLayoutHeight * 0.5f);

                if (this.m_currentPosition.x != x || this.m_currentPosition.y != y)
                {
                    this.m_currentPosition = new Vector2(x, y); // perform update ONLY if changed

                    this.Layout.style.left = new StyleLength(new Length(x, LengthUnit.Percent));
                    this.Layout.style.bottom = new StyleLength(new Length(y, LengthUnit.Percent));
                }

                ref readonly var stats = ref this.Entity.EntityStats;

                var value = RemapToRange(stats.CurHealth, 0.0f, stats.MaxHealth, 0.0f, 100.0f);

                if (this.Bar.value != value)
                {
                    this.Bar.value = value; // perform update ONLY if changed
                }
            }
        }

        private class AnimatedText
        {
            private const float kTextWidth = 25.0f; // technically width and height should not matter since overflow
            private const float kTextHeight = 10.0f; // context solves all issues, but we need it for precise calcs
            private const float kDelayBeforeAlpha = 1.3f;

            private static int ms_uniqueId;

            private readonly Vector2 m_start;
            private readonly Vector2 m_end;
            private readonly float m_invDuration;
            private readonly float m_duration;
            private readonly float m_alphaMul;
            private float m_deltaTotal;

            public readonly Label Text;

            public AnimatedText(string text, float duration, float delay, int size, int width, Color color, Vector2 start, Vector2 end, BattleBuilder builder)
            {
                this.m_start = new Vector2((start.x + 1.0f) * 50.0f - (kTextWidth * 0.5f), (start.y + 1.0f) * 50.0f - (kTextHeight * 0.5f));
                this.m_end = new Vector2((end.x + 1.0f) * 50.0f - (kTextWidth * 0.5f), (end.y + 1.0f) * 50.0f - (kTextHeight * 0.5f));

                this.m_duration = duration;
                this.m_invDuration = 1.0f / duration;
                this.m_deltaTotal = -delay;
                this.m_alphaMul = 1.0f / (duration - (duration / kDelayBeforeAlpha));

                this.Text = new Label(text)
                {
                    name = "animated-label-" + ms_uniqueId++.ToString(),
                    pickingMode = PickingMode.Ignore,
                };

                var style = this.Text.style;

                style.position = Position.Absolute;
                style.visibility = Visibility.Hidden;

                style.width = new StyleLength(new Length(kTextWidth, LengthUnit.Percent));
                style.height = new StyleLength(new Length(kTextHeight, LengthUnit.Percent));

                style.marginLeft = 0;
                style.marginRight = 0;
                style.marginTop = 0;
                style.marginBottom = 0;

                style.paddingLeft = 0;
                style.paddingRight = 0;
                style.paddingTop = 0;
                style.paddingBottom = 0;

                style.unityFontDefinition = new StyleFontDefinition(builder.MainFont);
                style.fontSize = size;
                style.color = color;
                style.unityTextAlign = TextAnchor.MiddleCenter;
                style.unityTextOutlineColor = Color.black;
                style.unityTextOutlineWidth = width;
                style.unityFontStyleAndWeight = FontStyle.Bold;

                builder.m_battleOverlay.Add(this.Text);
            }

            public bool Update()
            {
                var style = this.Text.style;

                if (this.m_deltaTotal < 0.0f)
                {
                    this.m_deltaTotal += Time.deltaTime;

                    style.visibility = Visibility.Hidden;

                    return true;
                }

                if (this.m_deltaTotal < this.m_duration)
                {
                    style.visibility = Visibility.Visible;

                    this.m_deltaTotal += Time.deltaTime;

                    if (this.m_deltaTotal > this.m_duration)
                    {
                        this.m_deltaTotal = this.m_duration;
                    }

                    var position = this.m_start + (this.m_deltaTotal * this.m_invDuration * (this.m_end - this.m_start));

                    this.Text.style.left = new StyleLength(new Length(position.x, LengthUnit.Percent));
                    this.Text.style.bottom = new StyleLength(new Length(position.y, LengthUnit.Percent));

                    if (this.m_deltaTotal * kDelayBeforeAlpha > this.m_duration)
                    {
                        float alpha = this.m_alphaMul * (this.m_duration - this.m_deltaTotal);

                        var color = this.Text.style.color.value;

                        color.a = alpha;

                        this.Text.style.color = color;

                        this.Text.style.unityTextOutlineColor = new Color(0.0f, 0.0f, 0.0f, alpha);
                    }

                    return true;
                }

                style.visibility = Visibility.Hidden;

                return false;
            }
        }

        private const string kStatistic = "Statistic";
        private const string kAbility = "Ability";
        private const string kPotion = "Potion";
        private const string kEffect = "Effect";

        private const string kAttackName = "Base Attack";
        private const string kAttackType = "Attack";
        private const string kAttackDesc = "Performs a basic attack that deals base damage.";

        private const string kBattleOverlay = "battle-overlay";
        private const string kBackButton = "back-button";
        private const string kSoundButton = "sound-button";
        private const string kTurnLabel = "turn-label";

        private const string kGameOverOverlay = "gameover-overlay";
        private const string kOutcomeLabel = "outcome-label";
        private const string kRewardText = "reward-text";

        private const string kMainLayout = "main-layout";
        private const string kTooltipLayout = "tooltip-layout";

        private const string kEffectLayout = "effect-layout";
        private const string kPotionLayout = "potion-layout"; // only for player
        private const string kInteractLayout = "interact-layout"; // only for player
        private const string kManaLayout = "mana-layout"; // only for player

        private const string kPotionSlot1Icon = "potion-slot1-icon";
        private const string kPotionSlot2Icon = "potion-slot2-icon";
        private const string kPotionSlot3Icon = "potion-slot3-icon";

        private const string kRegularAttackIcon = "regular-attack-icon";

        private const string kAbilitySlot1Icon = "ability-slot1-icon";
        private const string kAbilitySlot1Text = "ability-slot1-text";

        private const string kAbilitySlot2Icon = "ability-slot2-icon";
        private const string kAbilitySlot2Text = "ability-slot2-text";

        private const string kAbilitySlot3Icon = "ability-slot3-icon";
        private const string kAbilitySlot3Text = "ability-slot3-text";

        private const string kEnemyAbility = "enemy-ability";

        private const string kActionButton = "action-button";
        private const string kActionLabel = "action-label";

        private const string kHealthIcon = "health-icon";
        private const string kHealthStat = "health-stat";
        private const string kHealthName = "Health";
        private const string kHealthDesc = "A statistic that determines the number of hit-points that a character has.";

        private const string kManaIcon = "mana-icon";
        private const string kManaStat = "mana-stat";
        private const string kManaName = "Mana";
        private const string kManaDesc = "A statistic that corresponds to the exact amount of ability casting resource a character has.";

        private const string kDamageIcon = "damage-icon";
        private const string kDamageStat = "damage-stat";
        private const string kDamageName = "Damage";
        private const string kDamageDesc = "A statistic that indicates how many health points is deducted from a struck target using a basic attack.";

        private const string kArmorIcon = "armor-icon";
        private const string kArmorStat = "armor-stat";
        private const string kArmorName = "Armor";
        private const string kArmorDesc = "Reduces the amount of damage taken, thus increasing character's effective health against damage.";

        private const string kEvasionIcon = "evasion-icon";
        private const string kEvasionStat = "evasion-stat";
        private const string kEvasionName = "Evasion";
        private const string kEvasionDesc = "Denotes the chance that incoming damage is completely mitigated/evaded, thus deducting 0 health points.";

        private const string kPrecisionIcon = "precision-icon";
        private const string kPrecisionStat = "precision-stat";
        private const string kPrecisionName = "Precision";
        private const string kPrecisionDesc = "A statistic that indicates how likely it is that an attack is going to hit the target, ignoring its evasion chance.";

        private const string kCritChanceIcon = "critchance-icon";
        private const string kCritChanceStat = "critchance-stat";
        private const string kCritChanceName = "Critical Strike Chance";
        private const string kCritChanceDesc = "Denotes the chance that a basic attack or ability will critically strike.";

        private const string kCritMultIcon = "critmult-icon";
        private const string kCritMultStat = "critmult-stat";
        private const string kCritMultName = "Critical Strike Damage";
        private const string kCritMultDesc = "Denotes the damage dealt when a basic attack or ability critically strikes.";

        private const string kTooltipIcon = "tooltip-icon";
        private const string kTooltipName = "tooltip-name";
        private const string kTooltipType = "tooltip-type";
        private const string kTooltipTime = "tooltip-time";
        private const string kTooltipCost = "tooltip-cost";
        private const string kTooltipDesc = "tooltip-desc";

        private static readonly Color ms_buttonIdledColor = new Color32(209, 156, 76, 255);
        private static readonly Color ms_buttonHoverColor = new Color32(170, 130, 66, 255);
        private static readonly Color ms_buttonPressColor = new Color32(147, 113, 60, 255);
        private static readonly Color ms_buttonLockedColor = new Color32(103, 63, 39, 255);

        private static readonly Color ms_defaultInteractColor = new Color32(103, 63, 39, 255);
        private static readonly Color ms_borderSelectColor = new Color32(0, 225, 255, 255);
        private static readonly Color ms_borderDefaultColor = Color.black;

        private static readonly float ms_startMainLayoutOffset = 0.0f;   // for top relative
        private static readonly float ms_finalMainLayoutOffset = 170.0f; // for top relative
        private static readonly float ms_mainLayoutAnimSpeed = 300.0f;

        [SerializeField]
        private FontAsset MainFont;

        private ITooltipProvider m_currentProvider;
        private ISelectableItem m_currentSelected;
        private IEntity m_currentEntity;

        private List<HealthIndicator> m_indicators;
        private List<AnimatedText> m_animations;
        private VisualElement m_battleOverlay;

        private List<EffectStatistic> m_effectList;
        private VisualElement m_effectContainer;

        private VisualElement m_tooltipIcon;
        private Label m_tooltipName;
        private Label m_tooltipType;
        private Label m_tooltipTime;
        private Label m_tooltipCost;
        private Label m_tooltipDesc;

        private PotionItem m_potionSlot3;
        private PotionItem m_potionSlot2;
        private PotionItem m_potionSlot1;

        private AbilityItem m_abilityItem3;
        private AbilityItem m_abilityItem2;
        private AbilityItem m_abilityItem1;

        private ActionButton m_actionButton;
        private EnemyAbility m_enemyAbility;
        private AttackItem m_attackItem;

        private TextStatistic m_critMultiplierStatistic;
        private TextStatistic m_critChanceStatistic;
        private TextStatistic m_precisionStatistic;
        private TextStatistic m_evasionStatistic;
        private TextStatistic m_damageStatistic;
        private TextStatistic m_armorStatistic;

        private HealthStatistic m_healthStatistic;
        private ManaStatistic m_manaStatistic;

        private VisualElement m_interactLayout;
        private VisualElement m_tooltipLayout;
        private VisualElement m_potionLayout;
        private VisualElement m_mainLayout;

        private Label m_turnLabel;

        private VisualElement m_soundButton;
        private bool m_soundPressed;
        
        private VisualElement m_backButton;
        private bool m_backPressed;

        private AnimationType m_animation;
        private bool m_lockedActions;

        public IEntity CurrentEntity
        {
            get
            {
                return this.m_currentEntity;
            }
            set
            {
                if (this.m_currentEntity != value)
                {
                    this.m_currentEntity = value;

                    this.UpdateEntity();
                }
            }
        }

        public bool HasAnyAnimationsPlaying => this.m_animations?.Count > 0;

        public event Action OnExitRequest;

        public event Action OnUseAttackRequest;

        public event Action<int> OnUseAbilityRequest;

        public event Action<int> OnUsePotionRequest;

        public event Action OnCancelInteractRequest;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
            this.OnUIUpdate += this.AnimateAll;

            this.OnUseAttackRequest += LockAll;
            this.OnUseAbilityRequest += LockAllIndexed;
            this.OnUsePotionRequest += LockAllIndexed;
            this.OnCancelInteractRequest += UnlockAll;

            void LockAll()
            {
                this.m_attackItem?.SetLocked(true);

                this.m_abilityItem1?.SetLocked(true);
                this.m_abilityItem2?.SetLocked(true);
                this.m_abilityItem3?.SetLocked(true);

                this.m_potionSlot1?.SetLocked(true);
                this.m_potionSlot2?.SetLocked(true);
                this.m_potionSlot3?.SetLocked(true);
            }

            void LockAllIndexed(int index)
            {
                this.m_attackItem?.SetLocked(true);

                this.m_abilityItem1?.SetLocked(true);
                this.m_abilityItem2?.SetLocked(true);
                this.m_abilityItem3?.SetLocked(true);

                this.m_potionSlot1?.SetLocked(true);
                this.m_potionSlot2?.SetLocked(true);
                this.m_potionSlot3?.SetLocked(true);
            }

            void UnlockAll()
            {
                this.m_attackItem?.SetLocked(false);

                this.m_abilityItem1?.SetLocked(false);
                this.m_abilityItem2?.SetLocked(false);
                this.m_abilityItem3?.SetLocked(false);

                this.m_potionSlot1?.SetLocked(false);
                this.m_potionSlot2?.SetLocked(false);
                this.m_potionSlot3?.SetLocked(false);
            }
        }

        private void OnEnableEvent()
        {
            this.SetupBackButton();
            this.SetupSoundButton();
            this.SetupTurnLabel();
            this.SetupLayouts();
            this.SetupStatistics();
            this.SetupEffectList();
            this.SetupEnemyAbility();
            this.SetupTooltip();

            this.UpdateEntity();
        }

        private void OnDisableEvent()
        {
            this.m_animation = AnimationType.None;
            this.m_lockedActions = false;

            this.m_currentEntity = null;
            this.m_currentSelected = null;
            this.m_currentProvider = null;

            this.m_animations = null;
            this.m_indicators = null;
            this.m_battleOverlay = null;

            this.m_backButton = null;
            this.m_backPressed = false;

            this.m_soundButton = null;
            this.m_soundPressed = false;

            this.m_turnLabel = null;

            this.m_mainLayout = null;
            this.m_potionLayout = null;
            this.m_tooltipLayout = null;
            this.m_interactLayout = null;

            this.m_healthStatistic = null;
            this.m_manaStatistic = null;

            this.m_damageStatistic = null;
            this.m_armorStatistic = null;
            this.m_evasionStatistic = null;
            this.m_precisionStatistic = null;
            this.m_critChanceStatistic = null;
            this.m_critMultiplierStatistic = null;

            this.m_effectList = null;
            this.m_effectContainer = null;

            this.m_actionButton = null;
            this.m_attackItem = null;

            this.m_abilityItem1 = null;
            this.m_abilityItem2 = null;
            this.m_abilityItem3 = null;

            this.m_potionSlot1 = null;
            this.m_potionSlot2 = null;
            this.m_potionSlot3 = null;

            this.m_tooltipIcon = null;
            this.m_tooltipName = null;
            this.m_tooltipType = null;
            this.m_tooltipTime = null;
            this.m_tooltipCost = null;
            this.m_tooltipDesc = null;
        }

        private void SetupBackButton()
        {
            this.m_backButton = this.UI.rootVisualElement.Q<VisualElement>(kBackButton);

            if (this.m_backButton is not null)
            {
                this.m_backButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    this.m_backPressed = false;

                    this.m_backButton.style.backgroundColor = ms_buttonIdledColor;
                });

                this.m_backButton.RegisterCallback<PointerEnterEvent>(e =>
                {
                    this.m_backPressed = false;

                    this.m_backButton.style.backgroundColor = ms_buttonHoverColor;
                });

                this.m_backButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        this.m_backPressed = true;

                        this.m_backButton.style.backgroundColor = ms_buttonPressColor;
                    }
                });

                this.m_backButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        if (this.m_backPressed)
                        {
                            this.m_backPressed = false;

                            this.m_backButton.style.backgroundColor = ms_buttonHoverColor;

                            this.OnExitRequest?.Invoke();
                        }
                    }
                });
            }
        }

        private void SetupSoundButton()
        {
            this.m_soundButton = this.UI.rootVisualElement.Q<VisualElement>(kSoundButton);

            if (this.m_soundButton is not null)
            {
                this.m_soundButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    this.m_soundPressed = false;

                    this.m_soundButton.style.backgroundColor = ms_buttonIdledColor;
                });

                this.m_soundButton.RegisterCallback<PointerEnterEvent>(e =>
                {
                    this.m_soundPressed = false;

                    this.m_soundButton.style.backgroundColor = ms_buttonHoverColor;
                });

                this.m_soundButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        this.m_soundPressed = true;

                        this.m_soundButton.style.backgroundColor = ms_buttonPressColor;
                    }
                });

                this.m_soundButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        if (this.m_soundPressed)
                        {
                            this.m_soundPressed = false;

                            this.m_soundButton.style.backgroundColor = ms_buttonHoverColor;

                            Main.Instance.EnableMusic = !Main.Instance.EnableMusic;
                        }
                    }
                });
            }
        }

        private void SetupTurnLabel()
        {
            this.m_turnLabel = this.UI.rootVisualElement.Q<Label>(kTurnLabel);
        }

        private void SetupLayouts()
        {
            var root = this.UI.rootVisualElement;

            this.m_mainLayout = root.Q<VisualElement>(kMainLayout);
            this.m_potionLayout = root.Q<VisualElement>(kPotionLayout);
            this.m_tooltipLayout = root.Q<VisualElement>(kTooltipLayout);
            this.m_interactLayout = root.Q<VisualElement>(kInteractLayout);

            this.m_battleOverlay = root.Q<VisualElement>(kBattleOverlay);

            if (this.m_battleOverlay is not null)
            {
                this.m_animations = new List<AnimatedText>();
                this.m_indicators = new List<HealthIndicator>();
            }
        }

        private void SetupStatistics()
        {
            this.m_healthStatistic = new HealthStatistic(kHealthName, kHealthDesc, kHealthIcon, kHealthStat, this);
            this.m_manaStatistic = new ManaStatistic(kManaName, kManaDesc, kManaIcon, kManaStat, kManaLayout, this);
            this.m_damageStatistic = new TextStatistic(kDamageName, kDamageDesc, kDamageIcon, kDamageStat, this);
            this.m_armorStatistic = new TextStatistic(kArmorName, kArmorDesc, kArmorIcon, kArmorStat, this);
            this.m_evasionStatistic = new TextStatistic(kEvasionName, kEvasionDesc, kEvasionIcon, kEvasionStat, this);
            this.m_precisionStatistic = new TextStatistic(kPrecisionName, kPrecisionDesc, kPrecisionIcon, kPrecisionStat, this);
            this.m_critChanceStatistic = new TextStatistic(kCritChanceName, kCritChanceDesc, kCritChanceIcon, kCritChanceStat, this);
            this.m_critMultiplierStatistic = new TextStatistic(kCritMultName, kCritMultDesc, kCritMultIcon, kCritMultStat, this);
        }

        private void SetupEffectList()
        {
            this.m_effectContainer = this.UI.rootVisualElement.Q<VisualElement>(kEffectLayout);

            if (this.m_effectContainer is not null)
            {
                this.m_effectList = new List<EffectStatistic>();
            }
        }

        private void SetupEnemyAbility()
        {
            var element = this.UI.rootVisualElement.Q<VisualElement>(kEnemyAbility);

            if (element is not null)
            {
                this.m_enemyAbility = new EnemyAbility(element, this);
            }
        }

        private void SetupTooltip()
        {
            var root = this.UI.rootVisualElement;

            this.m_tooltipIcon = root.Q<VisualElement>(kTooltipIcon);
            this.m_tooltipName = root.Q<Label>(kTooltipName);
            this.m_tooltipType = root.Q<Label>(kTooltipType);
            this.m_tooltipTime = root.Q<Label>(kTooltipTime);
            this.m_tooltipCost = root.Q<Label>(kTooltipCost);
            this.m_tooltipDesc = root.Q<Label>(kTooltipDesc);

            this.DisplayTooltip(null, false);
        }

        private void UpdateEntity()
        {
            if (this.m_attackItem is not null)
            {
                this.m_attackItem.Dispose();

                this.m_attackItem = null;
            }

            if (this.m_abilityItem1 is not null)
            {
                this.m_abilityItem1.Dispose();

                this.m_abilityItem1 = null;
            }

            if (this.m_abilityItem2 is not null)
            {
                this.m_abilityItem2.Dispose();

                this.m_abilityItem2 = null;
            }

            if (this.m_abilityItem3 is not null)
            {
                this.m_abilityItem3.Dispose();

                this.m_abilityItem3 = null;
            }

            if (this.m_potionSlot1 is not null)
            {
                this.m_potionSlot1.Dispose();

                this.m_potionSlot1 = null;
            }

            if (this.m_potionSlot2 is not null)
            {
                this.m_potionSlot2.Dispose();

                this.m_potionSlot2 = null;
            }

            if (this.m_potionSlot3 is not null)
            {
                this.m_potionSlot3.Dispose();

                this.m_potionSlot3 = null;
            }

            var entity = this.m_currentEntity;

            if (entity is null)
            {
                this.SelectItem(null);
                this.DisplayTooltip(null, false);

                if (this.m_actionButton is not null)
                {
                    this.m_actionButton.SuperReset();

                    this.m_actionButton = null;
                }

                if (this.m_tooltipLayout is not null)
                {
                    this.m_tooltipLayout.style.display = DisplayStyle.None;
                }

                if (this.m_mainLayout is not null)
                {
                    this.m_animation = AnimationType.Hide;

                    this.AnimateAll();
                }
            }
            else
            {
                if (entity.IsPlayer)
                {
                    if (this.m_potionLayout is not null)
                    {
                        this.m_potionLayout.style.display = DisplayStyle.Flex;
                    }

                    if (this.m_interactLayout is not null)
                    {
                        this.m_interactLayout.style.display = DisplayStyle.Flex;
                    }

                    if (this.m_enemyAbility is not null)
                    {
                        this.m_enemyAbility.Icon.style.visibility = Visibility.Hidden;

                        this.m_enemyAbility.Icon.pickingMode = PickingMode.Ignore;
                    }

                    this.m_attackItem = new AttackItem(this);

                    this.m_abilityItem1 = new AbilityItem(kAbilitySlot1Icon, kAbilitySlot1Text, 0, entity.Abilities.Count > 0 ? entity.Abilities[0] : null, this);
                    this.m_abilityItem2 = new AbilityItem(kAbilitySlot2Icon, kAbilitySlot2Text, 1, entity.Abilities.Count > 1 ? entity.Abilities[1] : null, this);
                    this.m_abilityItem3 = new AbilityItem(kAbilitySlot3Icon, kAbilitySlot3Text, 2, entity.Abilities.Count > 2 ? entity.Abilities[2] : null, this);

                    this.m_potionSlot1 = new PotionItem(kPotionSlot1Icon, 0, this);
                    this.m_potionSlot2 = new PotionItem(kPotionSlot2Icon, 1, this);
                    this.m_potionSlot3 = new PotionItem(kPotionSlot3Icon, 2, this);

                    this.m_actionButton = new ActionButton(kActionButton, kActionLabel, this);

                    this.m_actionButton.SetLocked(this.m_lockedActions);
                }
                else
                {
                    this.m_actionButton = null;

                    if (this.m_potionLayout is not null)
                    {
                        this.m_potionLayout.style.display = DisplayStyle.None;
                    }

                    if (this.m_interactLayout is not null)
                    {
                        this.m_interactLayout.style.display = DisplayStyle.None;
                    }

                    if (this.m_enemyAbility is not null)
                    {
                        this.m_enemyAbility.Icon.style.visibility = Visibility.Visible;

                        this.m_enemyAbility.Icon.pickingMode = PickingMode.Position;
                    }
                }

                this.SelectItem(null);
                this.DisplayTooltip(null, false);

                this.UpdateInterface();

                if (this.m_mainLayout is not null)
                {
                    this.m_animation = AnimationType.Show;

                    this.AnimateAll();
                }
            }
        }

        private void SelectItem(ISelectableItem item)
        {
            this.m_currentSelected?.Deselect();

            this.m_currentSelected = item;

            this.m_currentSelected?.Select();

            this.m_actionButton?.UpdateStatus();
        }

        private void DisplayTooltip(ITooltipProvider provider, bool display, bool forceUpdate = false)
        {
            // use two things: provider is null when no interactive UI is shown
            // otherwise, provider not null means element that we just entered/left hover-wise
            // display = true means entered an element to tooltip display, display = false otherwise
            // if we want to stop displaying item that already was overwritten, don't do anything

            if (this.m_animation != AnimationType.None)
            {
                return; // no tooltip is allowed when we are hiding/showing layout
            }

            if (provider is null || forceUpdate)
            {
                // doesn't matter what value 'display' is

                UpdateInternal(provider);
            }
            else
            {
                if (this.m_currentProvider is null)
                {
                    // null is currently displayed, update only if 'display' is true

                    if (display)
                    {
                        UpdateInternal(provider);
                    }
                }
                else
                {
                    // something else is displayed, check if it is this item

                    if (this.m_currentProvider == provider)
                    {
                        // if it is the same item, check if want to disable its display
                        
                        if (!display)
                        {
                            UpdateInternal(null);
                        }
                    }
                    else
                    {
                        // if it is different item, and we want to disable this one, do nothing
                        // since we already overwrote it at some point before (happens if mouse
                        // event triggers Enter of other element before Leave of this one)

                        if (display)
                        {
                            UpdateInternal(provider);
                        }
                    }
                }
            }

            void UpdateInternal(ITooltipProvider provider)
            {
                this.m_currentProvider = provider;

                if (provider is null)
                {
                    if (this.m_tooltipIcon is not null)
                    {
                        this.m_tooltipIcon.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                    }

                    if (this.m_tooltipName is not null)
                    {
                        this.m_tooltipName.text = String.Empty;
                    }

                    if (this.m_tooltipType is not null)
                    {
                        this.m_tooltipType.text = String.Empty;
                    }

                    if (this.m_tooltipTime is not null)
                    {
                        this.m_tooltipTime.text = String.Empty;
                    }

                    if (this.m_tooltipCost is not null)
                    {
                        this.m_tooltipCost.text = String.Empty;
                    }

                    if (this.m_tooltipDesc is not null)
                    {
                        this.m_tooltipDesc.text = String.Empty;
                    }

                    if (this.m_tooltipLayout is not null)
                    {
                        this.m_tooltipLayout.style.display = DisplayStyle.None;
                    }
                }
                else
                {
                    ref readonly var data = ref provider.Data;

                    if (this.m_tooltipIcon is not null)
                    {
                        this.m_tooltipIcon.style.backgroundImage = new StyleBackground(data.Icon);
                    }

                    if (this.m_tooltipName is not null)
                    {
                        this.m_tooltipName.text = data.Name;
                    }

                    if (this.m_tooltipType is not null)
                    {
                        this.m_tooltipType.text = data.Type;
                    }

                    if (this.m_tooltipTime is not null)
                    {
                        this.m_tooltipTime.text = data.Time;
                    }

                    if (this.m_tooltipCost is not null)
                    {
                        this.m_tooltipCost.text = data.Cost;
                    }

                    if (this.m_tooltipDesc is not null)
                    {
                        this.m_tooltipDesc.text = data.Desc;
                    }

                    if (this.m_tooltipLayout is not null)
                    {
                        this.m_tooltipLayout.style.display = DisplayStyle.Flex;
                    }
                }
            }
        }
        
        private void AnimateAll()
        {
            if (this.m_animation != AnimationType.None && this.m_mainLayout is not null)
            {
                this.m_mainLayout.pickingMode = PickingMode.Ignore;

                var style = this.m_mainLayout.style;

                if (this.m_animation == AnimationType.Hide)
                {
                    float next = style.top.value.value + ms_mainLayoutAnimSpeed * Time.deltaTime;

                    if (next >= ms_finalMainLayoutOffset)
                    {
                        style.top = ms_finalMainLayoutOffset;

                        style.display = DisplayStyle.None;

                        this.m_animation = AnimationType.None;
                    }
                    else
                    {
                        style.top = next;
                    }
                }
                
                if (this.m_animation == AnimationType.Show)
                {
                    style.display = DisplayStyle.Flex;

                    float next = style.top.value.value - ms_mainLayoutAnimSpeed * Time.deltaTime;

                    if (next <= ms_startMainLayoutOffset)
                    {
                        style.top = ms_startMainLayoutOffset;

                        this.m_mainLayout.pickingMode = PickingMode.Position;

                        this.m_animation = AnimationType.None;
                    }
                    else
                    {
                        style.top = next;
                    }
                }
            }

            var animations = this.m_animations;

            if (animations is not null)
            {
                for (int i = animations.Count - 1; i >= 0; --i)
                {
                    var animation = animations[i];

                    if (!animation.Update())
                    {
                        animation.Text.RemoveFromHierarchy();

                        animations.RemoveAt(i);
                    }
                }
            }

            var indicators = this.m_indicators;

            if (indicators is not null)
            {
                for (int i = indicators.Count - 1; i >= 0; --i)
                {
                    indicators[i].Update();
                }
            }
        }

        public void UpdateInterface()
        {
            if (this.m_effectContainer is not null)
            {
                for (int i = 0; i < this.m_effectList.Count; ++i)
                {
                    if (this.m_effectList[i] == this.m_currentProvider)
                    {
                        this.DisplayTooltip(null, false);
                    }
                }

                this.m_effectList.Clear();
                this.m_effectContainer.Clear();
            }

            var entity = this.m_currentEntity;

            if (entity is not null)
            {
                ref readonly var stats = ref entity.EntityStats;

                this.m_manaStatistic.Display(entity.IsPlayer);
                this.m_damageStatistic.UpdateText(stats.Damage, false);
                this.m_armorStatistic.UpdateText(stats.Armor, false);
                this.m_evasionStatistic.UpdateText(Mathf.CeilToInt(stats.Evasion * 100.0f), true);
                this.m_precisionStatistic.UpdateText(Mathf.CeilToInt(stats.Precision), false);
                this.m_critChanceStatistic.UpdateText(Mathf.CeilToInt(stats.CritChance * 100.0f), true);
                this.m_critMultiplierStatistic.UpdateText(Mathf.CeilToInt(stats.CritMultiplier * 100.0f), true);
                this.m_manaStatistic.UpdateBar(stats.CurMana, stats.MaxMana);
                this.m_healthStatistic.UpdateBar(stats.CurHealth, stats.MaxHealth);

                if (this.m_effectContainer is not null)
                {
                    var effects = entity.Effects;
                    int counter = effects.Count;

                    for (int i = 0; i < counter; ++i)
                    {
                        this.m_effectList.Add(new EffectStatistic(effects[i], this.m_effectContainer, this));
                    }
                }

                if (entity.IsPlayer)
                {
                    Debug.Assert(this.m_attackItem is not null);

                    this.m_abilityItem1.UpdateStatus();
                    this.m_abilityItem2.UpdateStatus();
                    this.m_abilityItem3.UpdateStatus();

                    this.m_potionSlot1.UpdatePotion(entity.EquippedPotions.Count > 0 ? entity.EquippedPotions[0] : null);
                    this.m_potionSlot2.UpdatePotion(entity.EquippedPotions.Count > 1 ? entity.EquippedPotions[1] : null);
                    this.m_potionSlot3.UpdatePotion(entity.EquippedPotions.Count > 2 ? entity.EquippedPotions[2] : null);
                }
                else
                {
                    Debug.Assert(entity.Abilities.Count > 0);

                    this.m_enemyAbility.UpdateInfo(entity.Abilities[0]);
                }

                this.DisplayTooltip(this.m_currentProvider, this.m_currentProvider is not null, true);
            }
        }

        public void LockActions()
        {
            this.m_lockedActions = true;
            this.m_actionButton?.SetLocked(true);
        }

        public void UnlockActions()
        {
            this.m_lockedActions = false;
            this.m_actionButton?.SetLocked(false);
        }

        public void ConfirmActionFinished()
        {
            this.m_actionButton?.SetUsage(false);

            this.m_attackItem?.SetLocked(false);

            this.m_abilityItem1?.SetLocked(false);
            this.m_abilityItem2?.SetLocked(false);
            this.m_abilityItem3?.SetLocked(false);

            this.m_potionSlot1?.SetLocked(false);
            this.m_potionSlot2?.SetLocked(false);
            this.m_potionSlot3?.SetLocked(false);

            this.SelectItem(null);
        }

        public void AddAnimatedText(string text, float duration, float delay, int size, int width, Color color, Vector2 start, Vector2 end)
        {
            this.m_animations?.Add(new AnimatedText(text, duration, delay, size, width, color, start, end, this));
        }

        public void AddHealthIndicator(Func<Vector2> positionFetcher, IEntity entity)
        {
            this.m_indicators?.Add(new HealthIndicator(positionFetcher, entity, this));
        }

        public void RemoveHealthIndicator(IEntity entity)
        {
            var indicators = this.m_indicators;

            if (indicators is not null)
            {
                for (int i = 0; i < indicators.Count; ++i)
                {
                    var indicator = indicators[i];

                    if (indicator.Entity == entity)
                    {
                        indicator.Layout.RemoveFromHierarchy();

                        indicators.RemoveAt(i);

                        break;
                    }
                }
            }
        }

        public void UpdateTurnLabel(string text, Color color)
        {
            if (this.m_turnLabel is not null)
            {
                this.m_turnLabel.text = text;
                this.m_turnLabel.style.color = color;
            }
        }

        public void ShowGameOverOverlay(string outcomeText, string rewardText, Color outcomeColor, string backgroundSpritePath)
        {
            var overlay = this.UI.rootVisualElement.Q<VisualElement>(kGameOverOverlay);

            if (overlay is not null)
            {
                overlay.style.display = DisplayStyle.Flex;

                overlay.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(backgroundSpritePath));

                var outcomeLabel = overlay.Q<Label>(kOutcomeLabel);

                if (outcomeLabel is not null)
                {
                    outcomeLabel.text = outcomeText;

                    outcomeLabel.style.color = outcomeColor;
                }

                var rewardLabel = overlay.Q<Label>(kRewardText);

                if (rewardLabel is not null)
                {
                    rewardLabel.text = rewardText;

                    rewardLabel.style.color = outcomeColor;
                }
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
