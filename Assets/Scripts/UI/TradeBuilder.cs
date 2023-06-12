using SupeRPG.Game;
using SupeRPG.Items;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class TradeBuilder : UIBuilder
    {
        private class IconElement : VisualElement
        {
            private static int ms_uniqueId;

            private bool m_hovered;
            private bool m_pressed;
            private bool m_locked;

            public readonly IItem Item;

            public readonly bool IsPlayers;

            public bool Locked => this.m_locked;

            public bool Hovered => this.m_hovered;

            public bool Pressed => this.m_pressed;

            public readonly VisualElement Image;

            public IconElement(IItem item, bool isPlayers)
            {
                this.Item = item;
                this.IsPlayers = isPlayers;

                this.name = $"item-slot-{ms_uniqueId}-back";
                this.pickingMode = PickingMode.Ignore;

                this.m_locked = false;
                this.m_hovered = false;
                this.m_pressed = false;

                this.style.width = 48;
                this.style.height = 48;

                this.style.marginTop = 5;
                this.style.marginBottom = 5;
                this.style.marginLeft = 5;
                this.style.marginRight = 5;

                this.style.paddingTop = 5;
                this.style.paddingBottom = 5;
                this.style.paddingLeft = 5;
                this.style.paddingRight = 5;

                this.style.backgroundColor = ms_unlockedBackIdledColor;
                this.style.unityBackgroundImageTintColor = Color.clear;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.SelectedItemPath));

                this.Image = new VisualElement()
                {
                    name = $"item-slot-{ms_uniqueId++}-image",
                    pickingMode = PickingMode.Position,
                };

                this.Image.style.flexShrink = 1;
                this.Image.style.flexGrow = 1;

                this.Image.style.unityBackgroundImageTintColor = Color.white;
                this.Image.style.backgroundImage = new StyleBackground(item.Sprite);
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

            public void Lock()
            {
                this.m_locked = true;

                if (this.m_hovered)
                {
                    this.Hover();
                }
                else if (this.m_pressed)
                {
                    this.Press();
                }
                else
                {
                    this.Idle();
                }
            }

            public void Unlock()
            {
                this.m_locked = false;

                if (this.m_hovered)
                {
                    this.Hover();
                }
                else if (this.m_pressed)
                {
                    this.Press();
                }
                else
                {
                    this.Idle();
                }
            }

            public void Idle()
            {
                this.m_hovered = false;
                this.m_pressed = false;

                this.style.backgroundColor = this.m_locked ? ms_lockedBackIdledColor : ms_unlockedBackIdledColor;
                this.Image.style.unityBackgroundImageTintColor = this.m_locked ? ms_lockedIdledTint : ms_unlockedIdledTint;
            }

            public void Hover()
            {
                this.m_hovered = true;
                this.m_pressed = false;

                this.style.backgroundColor = this.m_locked ? ms_lockedBackHoverColor : ms_unlockedBackHoverColor;
                this.Image.style.unityBackgroundImageTintColor = this.m_locked ? ms_lockedHoverTint : ms_unlockedHoverTint;
            }

            public void Press()
            {
                this.m_hovered = false;
                this.m_pressed = true;

                this.style.backgroundColor = this.m_locked ? ms_lockedBackPressColor : ms_unlockedBackPressColor;
                this.Image.style.unityBackgroundImageTintColor = this.m_locked ? ms_lockedPressTint : ms_unlockedPressTint;
            }
        }

        private enum Tab
        {
            Armor,
            Weapon,
            Potion,
            Trinket,
            Action, // technically not a tab, but used for press info
            Count,
        }

        private const string kBackButton = "back-button";

        private const string kArmorButton = "armor-button";
        private const string kWeaponButton = "weapon-button";
        private const string kPotionButton = "potion-button";
        private const string kTrinketButton = "trinket-button";
        private const string kActionButton = "action-button";

        private const string kPlayerInventory = "player-scrollview";
        private const string kSellerInventory = "seller-scrollview";

        private const string kPlayerMoney = "player-money-label";

        private const string kSelectedName = "selected-item-name";
        private const string kSelectedDesc = "selected-item-desc";
        private const string kSelectedPrice = "selected-item-price";
        private const string kSelectedImage = "selected-item-image";

        private static readonly Color ms_unlockedIdledTint = new Color32(255, 255, 255, 255);
        private static readonly Color ms_unlockedHoverTint = new Color32(200, 200, 200, 255);
        private static readonly Color ms_unlockedPressTint = new Color32(170, 170, 170, 255);

        private static readonly Color ms_unlockedBackIdledColor = new Color32(101, 59, 28, 255);
        private static readonly Color ms_unlockedBackHoverColor = new Color32(86, 47, 20, 255);
        private static readonly Color ms_unlockedBackPressColor = new Color32(75, 40, 15, 255);

        private static readonly Color ms_lockedIdledTint = new Color32(100, 100, 100, 255);
        private static readonly Color ms_lockedHoverTint = new Color32(75, 75, 75, 255);
        private static readonly Color ms_lockedPressTint = new Color32(50, 50, 50, 255);

        private static readonly Color ms_lockedBackIdledColor = new Color32(61, 29, 14, 255);
        private static readonly Color ms_lockedBackHoverColor = new Color32(48, 20, 9, 255);
        private static readonly Color ms_lockedBackPressColor = new Color32(40, 15, 5, 255);

        private static readonly Color ms_activeTint = new Color32(150, 150, 150, 255);
        private static readonly Color ms_selectTint = new Color32(255, 255, 255, 255);

        private readonly List<IconElement> m_playerInventory = new();
        private readonly List<IconElement> m_sellerInventory = new();
        private readonly bool[] m_pressed = new bool[(int)Tab.Count];

        private IReadOnlyList<TrinketData> m_tradeTrinkets;
        private IReadOnlyList<PotionData> m_tradePotions;
        private IReadOnlyList<WeaponData> m_tradeWeapons;
        private IReadOnlyList<ArmorData> m_tradeArmors;

        private ScrollView m_playerView;
        private ScrollView m_sellerView;

        private bool m_isFocusedOnSeller = false;
        private int m_selectedIndex = -1;
        private Tab m_currentTab = Tab.Armor;
        private bool m_backPressed = false;
        private bool m_showAllTradeItems = false;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
        }

        private void OnEnableEvent()
        {
            this.m_showAllTradeItems = Main.Instance.ShowAllTradeElements;

            Main.Instance.ShowAllTradeElements = false;

            this.m_playerView = this.UI.rootVisualElement.Q<ScrollView>(kPlayerInventory);
            this.m_sellerView = this.UI.rootVisualElement.Q<ScrollView>(kSellerInventory);

            this.m_playerView.Clear();
            this.m_sellerView.Clear();

            this.SetupBackButton();
            this.SetupArmorButton();
            this.SetupWeaponButton();
            this.SetupPotionButton();
            this.SetupTrinketButton();
            this.SetupActionButton();
            this.UpdateMoneyLabel();
            this.UpdateDisplayedItem(null);

            this.BindKeyAction(Key.Escape, this.OnEscapeKeyHit);

            this.ReinitializeAll(this.m_currentTab);
        }

        private void OnDisableEvent()
        {
            this.m_selectedIndex = -1;
            this.m_isFocusedOnSeller = false;
            this.m_backPressed = false;

            this.m_playerInventory.Clear();
            this.m_sellerInventory.Clear();

            this.m_playerView = null;
            this.m_sellerView = null;

            this.m_pressed.AsSpan().Clear();

            this.m_currentTab = Tab.Armor;
        }

        private void OnEscapeKeyHit()
        {
            // #TODO perform save

            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);
        }

        private void SetupArmorButton()
        {
            this.SetupCallbacksInternal(kArmorButton, Tab.Armor, Key.A, () => this.ReinitializeAll(Tab.Armor));
        }

        private void SetupWeaponButton()
        {
            this.SetupCallbacksInternal(kWeaponButton, Tab.Weapon, Key.W, () => this.ReinitializeAll(Tab.Weapon));
        }

        private void SetupPotionButton()
        {
            this.SetupCallbacksInternal(kPotionButton, Tab.Potion, Key.P, () => this.ReinitializeAll(Tab.Potion));
        }

        private void SetupTrinketButton()
        {
            this.SetupCallbacksInternal(kTrinketButton, Tab.Trinket, Key.T, () => this.ReinitializeAll(Tab.Trinket));
        }

        private void SetupActionButton()
        {
            this.SetupCallbacksInternal(kActionButton, Tab.Action, Key.S, () => this.PerformInventoryAction());
        }

        private void SetupBackButton()
        {
            var element = this.UI.rootVisualElement.Q<VisualElement>(kBackButton);

            if (element is not null)
            {
                element.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    this.m_backPressed = false;

                    element.style.unityBackgroundImageTintColor = Color.black;
                });

                element.RegisterCallback<MouseEnterEvent>(e =>
                {
                    this.m_backPressed = false;

                    element.style.unityBackgroundImageTintColor = (Color)new Color32(235, 30, 30, 255);
                });

                element.RegisterCallback<MouseDownEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        this.m_backPressed = true;

                        element.style.unityBackgroundImageTintColor = (Color)new Color32(200, 0, 0, 255);
                    }
                });

                element.RegisterCallback<MouseUpEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        if (this.m_backPressed)
                        {
                            this.m_backPressed = false;

                            element.style.unityBackgroundImageTintColor = (Color)new Color32(235, 30, 30, 255);

                            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);
                        }
                    }
                });
            }
        }

        private void SetupCallbacksInternal(string name, Tab tab, Key key, Action onMouseUp)
        {
            var element = this.UI.rootVisualElement.Q<VisualElement>(name);

            if (element is not null)
            {
                element.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    if (element.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed[(int)tab] = false;

                        element.style.unityBackgroundImageTintColor = ms_unlockedIdledTint;
                    }
                });

                element.RegisterCallback<MouseEnterEvent>(e =>
                {
                    if (element.pickingMode == PickingMode.Position)
                    {
                        this.m_pressed[(int)tab] = false;

                        element.style.unityBackgroundImageTintColor = ms_unlockedHoverTint;
                    }
                });

                element.RegisterCallback<MouseDownEvent>(e =>
                {
                    if (element.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_pressed[(int)tab] = true;

                        element.style.unityBackgroundImageTintColor = ms_unlockedPressTint;
                    }
                });

                element.RegisterCallback<MouseUpEvent>(e =>
                {
                    if (element.pickingMode == PickingMode.Position && e.button == 0 && this.m_pressed[(int)tab])
                    {
                        this.m_pressed[(int)tab] = false;

                        onMouseUp?.Invoke();
                    }
                });

                this.BindKeyAction(key, onMouseUp);
            }
        }

        private void PerformInventoryAction()
        {
            if (this.IsActionInteractable())
            {
                if (this.m_isFocusedOnSeller) // buy from store
                {
                    var item = this.m_sellerInventory[this.m_selectedIndex].Item;

                    int index = this.m_currentTab switch
                    {
                        Tab.Armor => Player.Instance.PurchaseArmor(Unsafe.As<ArmorData>(item)),
                        Tab.Weapon => Player.Instance.PurchaseWeapon(Unsafe.As<WeaponData>(item)),
                        Tab.Potion => Player.Instance.PurchasePotion(Unsafe.As<PotionData>(item)),
                        Tab.Trinket => Player.Instance.PurchaseTrinket(Unsafe.As<TrinketData>(item)),
                        _ => throw new Exception("The current tab is invalid"),
                    };

                    IItem real = this.m_currentTab switch
                    {
                        Tab.Armor => Player.Instance.Armors[index],
                        Tab.Weapon => Player.Instance.Weapons[index],
                        Tab.Potion => Player.Instance.Potions[index],
                        Tab.Trinket => Player.Instance.Trinkets[index],
                        _ => throw new Exception("The current tab is invalid"),
                    };

                    this.CreateIconElement(this.m_playerView, real, this.m_playerInventory, true, index);

                    this.UpdateMoneyLabel();

                    this.RecalculatePurchasableItems();

                    this.TryMakeActionInteractable();
                }
                else // sell to store
                {
                    switch (this.m_currentTab)
                    {
                        case Tab.Armor:
                            Player.Instance.SellArmor(this.m_selectedIndex);
                            break;

                        case Tab.Weapon:
                            Player.Instance.SellWeapon(this.m_selectedIndex);
                            break;

                        case Tab.Potion:
                            Player.Instance.SellPotion(this.m_selectedIndex);
                            break;

                        case Tab.Trinket:
                            Player.Instance.SellTrinket(this.m_selectedIndex);
                            break;
                    }

                    this.m_playerView[this.m_selectedIndex].Blur();

                    this.m_playerView.RemoveAt(this.m_selectedIndex);

                    this.m_playerInventory.RemoveAt(this.m_selectedIndex);

                    this.UpdateMoneyLabel();

                    this.RecalculatePurchasableItems();

                    this.ResetFocusedElement();
                }
            }
        }

        private void ReinitializeAll(Tab newTab)
        {
            this.SetCurrentButtonStatus(false);

            this.ResetInventories();

            this.m_currentTab = newTab;

            this.SetCurrentButtonStatus(true);

            this.SetupInventories();
        }

        private void SetCurrentButtonStatus(bool active)
        {
            var root = this.UI.rootVisualElement;

            var button = this.m_currentTab switch
            {
                Tab.Armor => root.Q<VisualElement>(kArmorButton),
                Tab.Weapon => root.Q<VisualElement>(kWeaponButton),
                Tab.Potion => root.Q<VisualElement>(kPotionButton),
                Tab.Trinket => root.Q<VisualElement>(kTrinketButton),
                _ => throw new Exception("The current tab is invalid"),
            };

            if (button is not null)
            {
                if (active)
                {
                    button.style.unityBackgroundImageTintColor = ms_activeTint;

                    button.pickingMode = PickingMode.Ignore;
                }
                else
                {
                    button.style.unityBackgroundImageTintColor = Color.white;

                    button.pickingMode = PickingMode.Position;
                }
            }
        }

        private void ResetInventories()
        {
            this.ResetFocusedElement();

            if (this.m_playerView is not null && this.m_playerInventory.Count > 0)
            {
                this.m_playerView.ScrollTo(this.m_playerInventory[0]);

                this.m_playerView.Clear();
            }

            if (this.m_sellerView is not null && this.m_sellerInventory.Count > 0)
            {
                this.m_sellerView.ScrollTo(this.m_sellerInventory[0]);

                this.m_sellerView.Clear();
            }

            this.m_playerInventory.Clear();
            this.m_sellerInventory.Clear();
        }

        private void ResetFocusedElement()
        {
            this.m_selectedIndex = -1;
            this.m_isFocusedOnSeller = false;

            this.UpdateDisplayedItem(null);
        }

        private void SetupInventories()
        {
            this.SetupPlayerInventory();
            this.SetupSellerInventory();
        }

        private void SetupPlayerInventory()
        {
            var view = this.m_playerView;

            if (view is not null)
            {
                switch (this.m_currentTab)
                {
                    case Tab.Armor:
                        this.SetupViewFromItemList(view, Player.Instance.Armors, this.m_playerInventory, true);
                        break;

                    case Tab.Weapon:
                        this.SetupViewFromItemList(view, Player.Instance.Weapons, this.m_playerInventory, true);
                        break;

                    case Tab.Potion:
                        this.SetupViewFromItemList(view, Player.Instance.Potions, this.m_playerInventory, true);
                        break;

                    case Tab.Trinket:
                        this.SetupViewFromItemList(view, Player.Instance.Trinkets, this.m_playerInventory, true);
                        break;
                }
            }
        }

        private void SetupSellerInventory()
        {
            var view = this.m_sellerView;

            if (view is not null)
            {
                if (this.m_showAllTradeItems)
                {
                    switch (this.m_currentTab)
                    {
                        case Tab.Armor:
                            this.SetupViewFromItemList(view, ResourceManager.Armors, this.m_sellerInventory, false);
                            break;

                        case Tab.Weapon:
                            this.SetupViewFromItemList(view, ResourceManager.Weapons, this.m_sellerInventory, false);
                            break;

                        case Tab.Potion:
                            this.SetupViewFromItemList(view, ResourceManager.Potions, this.m_sellerInventory, false);
                            break;

                        case Tab.Trinket:
                            this.SetupViewFromItemList(view, ResourceManager.Trinkets, this.m_sellerInventory, false);
                            break;
                    }
                }
                else
                {
                    switch (this.m_currentTab)
                    {
                        case Tab.Armor:
                            this.SetupViewFromItemList(view, this.m_tradeArmors, this.m_sellerInventory, false);
                            break;

                        case Tab.Weapon:
                            this.SetupViewFromItemList(view, this.m_tradeWeapons, this.m_sellerInventory, false);
                            break;

                        case Tab.Potion:
                            this.SetupViewFromItemList(view, this.m_tradePotions, this.m_sellerInventory, false);
                            break;

                        case Tab.Trinket:
                            this.SetupViewFromItemList(view, this.m_tradeTrinkets, this.m_sellerInventory, false);
                            break;
                    }
                }

                view.MarkDirtyRepaint();

                this.RecalculatePurchasableItems();
            }
        }

        private void SetupViewFromItemList(ScrollView view, IReadOnlyList<IItem> items, List<IconElement> elements, bool isPlayers)
        {
            if (items is not null)
            {
#if DEBUG
                Debug.Assert(view is not null);
                Debug.Assert(elements is not null);
#endif
                for (int i = 0; i < items.Count; ++i)
                {
                    this.CreateIconElement(view, items[i], elements, isPlayers);
                }
            }
        }

        private void CreateIconElement(ScrollView view, IItem item, List<IconElement> elements, bool isPlayers, int index = -1)
        {
            var icon = new IconElement(item, isPlayers);

            icon.RegisterCallback<PointerLeaveEvent>(e =>
            {
                icon.Idle();
            });

            icon.RegisterCallback<PointerEnterEvent>(e =>
            {
                icon.Hover();
            });

            icon.RegisterCallback<PointerDownEvent>(e =>
            {
                icon.Press();
            });

            icon.RegisterCallback<PointerUpEvent>(e =>
            {
                if (icon.Pressed && e.button == 0)
                {
                    int index = elements.IndexOf(icon);

                    if (index != this.m_selectedIndex || icon.IsPlayers == this.m_isFocusedOnSeller)
                    {
                        if (this.m_selectedIndex >= 0)
                        {
                            var selected = this.m_isFocusedOnSeller
                                ? this.m_sellerInventory[this.m_selectedIndex]
                                : this.m_playerInventory[this.m_selectedIndex];

                            selected.Deselect();
                        }

                        icon.Select();

                        this.m_selectedIndex = index;

                        this.m_isFocusedOnSeller = !icon.IsPlayers;

                        this.UpdateDisplayedItem(icon.Item);
                    }

                    Debug.Log($"Currently selected item is \"{icon.Item.Name}\"!");
                }

                icon.Hover();
            });

            if (index < 0 || index >= elements.Count)
            {
                view.Add(icon);

                elements.Add(icon);
            }
            else
            {
                view.Insert(index, icon);

                elements.Insert(index, icon);
            }
        }

        private void RecalculatePurchasableItems()
        {
            int money = Player.Instance.Money;

            for (int i = 0; i < this.m_sellerInventory.Count; ++i)
            {
                var icon = this.m_sellerInventory[i];

                if (icon.Item.Price > money)
                {
                    icon.Lock();
                }
                else
                {
                    icon.Unlock();
                }
            }
        }

        private void UpdateMoneyLabel()
        {
            var money = this.UI.rootVisualElement.Q<Label>(kPlayerMoney);

            if (money is not null)
            {
                money.text = "$" + Player.Instance.Money.ToString();
            }
        }

        private void UpdateDisplayedItem(IItem item)
        {
#if DEBUG
            Debug.Assert((item is null) == (this.m_selectedIndex < 0));
#endif
            var root = this.UI.rootVisualElement;
            var name = root.Q<Label>(kSelectedName);
            var desc = root.Q<Label>(kSelectedDesc);
            var price = root.Q<Label>(kSelectedPrice);
            var image = root.Q<VisualElement>(kSelectedImage);

            if (name is not null)
            {
                name.text = item is null ? String.Empty : item.Name;

                name.style.fontSize = GetFontSizeForString(name.text);
            }

            if (desc is not null)
            {
                desc.text = item is null ? String.Empty : item.Description;
            }

            if (price is not null)
            {
                if (item is null)
                {
                    price.text = String.Empty;
                }
                else
                {
                    price.text = "$" + (this.m_isFocusedOnSeller ? item.Price.ToString() : ((int)(item.Price * Player.SellMultiplier)).ToString());
                }
            }

            if (image is not null)
            {
                image.style.backgroundImage = item is null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(item.Sprite);
            }

            this.TryMakeActionInteractable();
        }

        private bool IsActionInteractable()
        {
            return this.m_selectedIndex >= 0 && (!this.m_isFocusedOnSeller || !this.m_sellerInventory[this.m_selectedIndex].Locked);
        }

        private void TryMakeActionInteractable()
        {
            var action = this.UI.rootVisualElement.Q<VisualElement>(kActionButton);

            if (this.IsActionInteractable())
            {
                action.style.unityBackgroundImageTintColor = ms_unlockedIdledTint;

                action.pickingMode = PickingMode.Position;
            }
            else
            {
                action.style.unityBackgroundImageTintColor = ms_lockedIdledTint;

                action.pickingMode = PickingMode.Ignore;
            }
        }

        public void SetTradeArmors(IReadOnlyList<ArmorData> armors)
        {
            this.m_tradeArmors = armors;
        }

        public void SetTradeWeapons(IReadOnlyList<WeaponData> weapons)
        {
            this.m_tradeWeapons = weapons;
        }

        public void SetTradePotions(IReadOnlyList<PotionData> potions)
        {
            this.m_tradePotions = potions;
        }

        public void SetTradeTrinkets(IReadOnlyList<TrinketData> trinkets)
        {
            this.m_tradeTrinkets = trinkets;
        }

        private static int GetFontSizeForString(string value)
        {
            if (value is null || value.Length < 18)
            {
                return 20;
            }

            if (value.Length < 20)
            {
                return 18;
            }

            if (value.Length < 22)
            {
                return 16;
            }

            if (value.Length < 25)
            {
                return 14;
            }

            if (value.Length < 30)
            {
                return 12;
            }

            return 10;
        }
    }
}
