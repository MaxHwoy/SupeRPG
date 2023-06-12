using SupeRPG.Game;
using SupeRPG.Items;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class InventoryBuilder : UIBuilder
    {
        private class IconElement : VisualElement
        {
            private static readonly string ms_nullItemPath = "UI/Shared/NullItem";

            private static int ms_uniqueId;

            private bool m_hovered;
            private bool m_pressed;
            private bool m_allowed;
            private bool m_display;

            public static readonly Color BackIdledColor = new Color32(189, 146, 82, 255);
            public static readonly Color BackHoverColor = new Color32(150, 115, 60, 255);
            public static readonly Color BackPressColor = new Color32(130, 100, 50, 255);

            public static readonly Color ForeIdledColor = new Color32(255, 255, 255, 255);
            public static readonly Color ForeHoverColor = new Color32(215, 215, 215, 255);
            public static readonly Color ForePressColor = new Color32(175, 175, 175, 255);

            public static readonly Color BackDisallowColor = new Color32(96, 28, 28, 255);
            public static readonly Color ForeDisallowColor = new Color32(155, 155, 155, 255);

            public readonly IItem Item;

            public readonly VisualElement Image;

            public readonly InventoryBuilder Builder;

            public bool IsAllowed => this.m_allowed;

            public IconElement(IItem item, InventoryBuilder builder)
            {
                this.Item = item;
                this.Builder = builder;

                this.name = $"item-slot-{ms_uniqueId}-back";
                this.pickingMode = PickingMode.Position;

                this.m_allowed = true;
                this.m_display = true;
                this.m_hovered = false;
                this.m_pressed = false;

                this.style.width = 46;
                this.style.height = 46;

                this.style.marginTop = 5;
                this.style.marginBottom = 5;
                this.style.marginLeft = 5;
                this.style.marginRight = 5;

                this.style.paddingTop = 5;
                this.style.paddingBottom = 5;
                this.style.paddingLeft = 5;
                this.style.paddingRight = 5;

                this.style.borderTopColor = Color.black;
                this.style.borderBottomColor = Color.black;
                this.style.borderLeftColor = Color.black;
                this.style.borderRightColor = Color.black;

                this.style.borderTopWidth = 1.0f;
                this.style.borderBottomWidth = 1.0f;
                this.style.borderLeftWidth = 1.0f;
                this.style.borderRightWidth = 1.0f;

                this.style.display = DisplayStyle.Flex;
                this.style.backgroundColor = BackIdledColor;
                this.style.unityBackgroundImageTintColor = SelectionInfo.InactiveColor;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.SelectedItemPath));

                this.Image = new VisualElement()
                {
                    name = $"item-slot-{ms_uniqueId++}-image",
                    pickingMode = PickingMode.Position,
                };

                this.Image.style.flexShrink = 1;
                this.Image.style.flexGrow = 1;

                this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                this.Image.style.backgroundImage = new StyleBackground(item is null ? ResourceManager.LoadSprite(ms_nullItemPath) : item.Sprite);
                this.Image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                this.SetupCallbacks();

                this.Add(this.Image);
            }

            public void Allow()
            {
                this.m_allowed = true;
                this.pickingMode = PickingMode.Position;
                this.Image.style.backgroundImage = new StyleBackground(this.Item is null ? ResourceManager.LoadSprite(ms_nullItemPath) : this.Item.Sprite);

                if (this.m_hovered)
                {
                    this.style.backgroundColor = BackHoverColor;
                    this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                }
                else if (this.m_pressed)
                {
                    this.style.backgroundColor = BackPressColor;
                    this.Image.style.unityBackgroundImageTintColor = ForePressColor;
                }
                else
                {
                    this.style.backgroundColor = BackIdledColor;
                    this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                }
            }

            public void Disallow()
            {
                this.m_allowed = false;
                this.pickingMode = PickingMode.Ignore;
                this.Image.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.DisallowPath));

                this.style.backgroundColor = BackDisallowColor;
                this.Image.style.unityBackgroundImageTintColor = ForeDisallowColor;
            }

            public void SetDisplay(bool display)
            {
                if (this.m_display != display)
                {
                    this.m_display = display;

                    this.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            private void SetupCallbacks()
            {
                this.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = false;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                    }
                });

                this.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = true;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackHoverColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                    }
                });

                this.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        this.m_hovered = false;
                        this.m_pressed = true;

                        this.style.backgroundColor = BackPressColor;
                        this.Image.style.unityBackgroundImageTintColor = ForePressColor;
                    }
                });

                this.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        if (this.m_pressed)
                        {
                            this.m_hovered = true;
                            this.m_pressed = false;

                            this.Builder.m_selection.SelectItem(this);

                            this.style.backgroundColor = BackHoverColor;
                            this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                        }
                    }
                });

                this.RegisterCallback<PointerMoveEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && this.m_pressed)
                    {
                        this.m_hovered = false;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;

                        this.Builder.m_dragged.Drag(e.position, this);
                    }
                });
            }
        }

        private class EquipElement : VisualElement
        {
            private bool m_hovered;
            private bool m_pressed;
            private bool m_allowed;
            private bool m_enabled;
            private IItem m_item;

            public static readonly Color BackIdledColor = new Color32(103, 63, 39, 255);
            public static readonly Color BackHoverColor = new Color32(135, 88, 61, 255);
            public static readonly Color BackPressColor = new Color32(155, 125, 110, 255);

            public static readonly Color ForeIdledColor = new Color32(255, 255, 255, 255);
            public static readonly Color ForeHoverColor = new Color32(215, 215, 215, 255);
            public static readonly Color ForePressColor = new Color32(175, 175, 175, 255);

            public static readonly Color BackDisallowColor = new Color32(96, 28, 28, 255);
            public static readonly Color ForeDisallowColor = new Color32(155, 155, 155, 255);

            public static readonly Color BackDisabledColor = new Color32(60, 60, 60, 255);
            public static readonly Color ForeDisabledColor = new Color32(100, 100, 100, 255);

            public readonly int Id;

            public readonly IconType Type;

            public readonly VisualElement Image;

            public readonly InventoryBuilder Builder;

            public bool IsAllowed => this.m_allowed;

            public bool Enabled => this.m_enabled;

            public IItem Item
            {
                get
                {
                    return this.m_item;
                }
                set
                {
                    this.m_item = value;

                    this.Image.style.backgroundImage = value is null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(value.Sprite);
                }
            }

            public EquipElement(int id, IconType type, bool enabled, InventoryBuilder builder)
            {
                string name = type.ToString().ToLower() + (id < 0 ? String.Empty : id.ToString());

                this.Id = id;
                this.Type = type;
                this.Builder = builder;

                this.name = name + "-back";
                this.pickingMode = PickingMode.Position;

                this.m_allowed = true;
                this.m_hovered = false;
                this.m_pressed = false;

                this.style.width = 40;
                this.style.height = 40;

                this.style.paddingTop = 2;
                this.style.paddingBottom = 2;
                this.style.paddingLeft = 2;
                this.style.paddingRight = 2;

                this.style.borderTopColor = Color.black;
                this.style.borderBottomColor = Color.black;
                this.style.borderLeftColor = Color.black;
                this.style.borderRightColor = Color.black;

                this.style.borderTopWidth = 1.0f;
                this.style.borderBottomWidth = 1.0f;
                this.style.borderLeftWidth = 1.0f;
                this.style.borderRightWidth = 1.0f;

                this.style.backgroundColor = BackIdledColor;
                this.style.unityBackgroundImageTintColor = SelectionInfo.InactiveColor;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.SelectedItemPath));

                this.Image = new VisualElement()
                {
                    name = name + "-image",
                    pickingMode = PickingMode.Position,
                };

                this.Image.style.flexShrink = 1;
                this.Image.style.flexGrow = 1;

                this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                this.Image.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                this.Image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                this.Add(this.Image);

                this.SetupCallbacks();

                this.SetEnabled(enabled);
            }

            public void Allow()
            {
                if (this.m_enabled)
                {
                    this.m_allowed = true;
                    this.pickingMode = PickingMode.Position;
                    this.Image.style.backgroundImage = this.Item is null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(this.Item.Sprite);

                    if (this.m_hovered)
                    {
                        this.style.backgroundColor = BackHoverColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                    }
                    else if (this.m_pressed)
                    {
                        this.style.backgroundColor = BackPressColor;
                        this.Image.style.unityBackgroundImageTintColor = ForePressColor;
                    }
                    else
                    {
                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                    }
                }
            }

            public void Disallow()
            {
                if (this.m_enabled)
                {
                    this.m_allowed = false;
                    this.pickingMode = PickingMode.Ignore;
                    this.Image.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.DisallowPath));

                    this.style.backgroundColor = BackDisallowColor;
                    this.Image.style.unityBackgroundImageTintColor = ForeDisallowColor;
                }
            }

            public void ForceHover(bool hover)
            {
                if (hover != this.m_hovered)
                {
                    if (hover)
                    {
                        this.m_hovered = true;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackHoverColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                    }
                    else
                    {
                        this.m_hovered = false;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                    }
                }
            }

            public new void SetEnabled(bool enable)
            {
                this.m_enabled = enable;

                if (enable)
                {
                    this.pickingMode = PickingMode.Position;
                    this.Image.style.backgroundImage = this.Item is null ? new StyleBackground(StyleKeyword.None) : new StyleBackground(this.Item.Sprite);
                    this.style.backgroundColor = BackIdledColor;
                    this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                }
                else
                {
                    this.pickingMode = PickingMode.Ignore;
                    this.Image.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(ResourceManager.DisallowPath));
                    this.style.backgroundColor = BackDisabledColor;
                    this.Image.style.unityBackgroundImageTintColor = ForeDisabledColor;
                }
            }

            private void SetupCallbacks()
            {
                this.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = false;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;
                    }
                });

                this.RegisterCallback<MouseEnterEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = true;
                        this.m_pressed = false;

                        this.style.backgroundColor = BackHoverColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;
                    }
                });

                this.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        this.m_hovered = false;
                        this.m_pressed = true;

                        this.style.backgroundColor = BackPressColor;
                        this.Image.style.unityBackgroundImageTintColor = ForePressColor;
                    }
                });

                this.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        if (this.m_pressed)
                        {
                            this.m_hovered = true;
                            this.m_pressed = false;

                            this.style.backgroundColor = BackHoverColor;
                            this.Image.style.unityBackgroundImageTintColor = ForeHoverColor;

                            this.Builder.m_selection.SelectItem(this);
                        }
                    }
                });

                this.RegisterCallback<PointerMoveEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && this.m_pressed)
                    {
                        this.m_pressed = false;
                        this.m_hovered = false;

                        this.style.backgroundColor = BackIdledColor;
                        this.Image.style.unityBackgroundImageTintColor = ForeIdledColor;

                        this.Builder.m_dragged.Drag(e.position, this);
                    }
                });
            }
        }

        private class TabButton : VisualElement
        {
            private bool m_hovered;
            private bool m_pressed;

            public static readonly Color BackIdledTint = new Color32(255, 255, 255, 255);
            public static readonly Color BackHoverTint = new Color32(200, 200, 200, 255);
            public static readonly Color BackPressTint = new Color32(170, 170, 170, 255);
            public static readonly Color BackActiveTint = new Color32(150, 150, 150, 255);

            public readonly ItemTab Tab;

            public readonly InventoryBuilder Builder;

            public bool Hovered => this.m_hovered;

            public bool Pressed => this.m_pressed;

            public TabButton(ItemTab tab, string name, string spritePath, InventoryBuilder builder)
            {
                this.Tab = tab;
                this.Builder = builder;

                this.name = name;
                this.pickingMode = PickingMode.Position;

                this.m_hovered = false;
                this.m_pressed = false;

                this.style.flexShrink = 0.0f;
                this.style.flexGrow = 0.0f;

                this.style.width = 48;
                this.style.height = 48;

                this.style.borderTopColor = Color.black;
                this.style.borderBottomColor = Color.black;
                this.style.borderLeftColor = Color.black;
                this.style.borderRightColor = Color.black;

                this.style.borderTopWidth = 1.0f;
                this.style.borderBottomWidth = 1.0f;
                this.style.borderLeftWidth = 1.0f;
                this.style.borderRightWidth = 1.0f;

                this.style.unityBackgroundImageTintColor = BackIdledTint;
                this.style.backgroundImage = new StyleBackground(ResourceManager.LoadSprite(spritePath));
                this.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                this.SetupCallbacks();
            }

            private void SetupCallbacks()
            {
                this.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = false;
                        this.m_pressed = false;

                        this.style.unityBackgroundImageTintColor = BackIdledTint;
                    }
                });

                this.RegisterCallback<PointerEnterEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore)
                    {
                        this.m_hovered = true;
                        this.m_pressed = false;

                        this.style.unityBackgroundImageTintColor = BackHoverTint;
                    }
                });

                this.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        this.m_hovered = false;
                        this.m_pressed = true;

                        this.style.unityBackgroundImageTintColor = BackPressTint;
                    }
                });

                this.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.pickingMode != PickingMode.Ignore && e.button == 0)
                    {
                        if (this.m_pressed)
                        {
                            this.m_hovered = false;
                            this.m_pressed = false;

                            this.Builder.m_selection.SelectTab(this.Tab);
                        }
                    }
                });
            }
        }

        private class DraggedElement : VisualElement
        {
            private EquipElement m_equip;

            private IconElement m_icon;

            public readonly VisualElement Image;

            public readonly InventoryBuilder Builder;

            public DraggedElement(InventoryBuilder builder)
            {
                this.m_icon = null;
                this.m_equip = null;
                this.Builder = builder;

                this.style.position = Position.Absolute;
                this.style.left = 0;
                this.style.top = 0;
                this.visible = false;
                this.pickingMode = PickingMode.Ignore;

                this.Image = new VisualElement();
                this.Image.style.position = Position.Absolute;
                this.Image.style.width = 36;
                this.Image.style.height = 36;
                this.Image.style.left = 0;
                this.Image.style.top = 0;
                this.Image.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                this.Image.visible = false;
                this.Image.pickingMode = PickingMode.Ignore;

                this.SetupCallbacks();

                this.Add(this.Image);

                builder.UI.rootVisualElement.Add(this);

                this.StretchToParentSize();
            }

            private void SetupCallbacks()
            {
                this.RegisterCallback<PointerMoveEvent>(e =>
                {
                    if (this.m_icon is not null || this.m_equip is not null)
                    {
                        EquipElement[] array;

                        this.Image.style.left = e.position.x - (this.Image.layout.width / 2);
                        this.Image.style.top = e.position.y - (this.Image.layout.height / 2);

                        array = this.Builder.m_equipEquip;

                        for (int i = 0; i < array.Length; ++i)
                        {
                            MaybeHover(array[i], e.position);
                        }

                        array = this.Builder.m_potionEquip;

                        for (int i = 0; i < array.Length; ++i)
                        {
                            MaybeHover(array[i], e.position);
                        }

                        array = this.Builder.m_trinketEquip;

                        for (int i = 0; i < array.Length; ++i)
                        {
                            MaybeHover(array[i], e.position);
                        }
                    }
                });

                this.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        var element = this.FindEquipElementThatWeDragOnto(e.position);

                        if (element is not null && this.m_equip != element) // check if we don't drag onto ourselves
                        {
                            if (this.m_icon is not null)
                            {
                                this.m_icon.Image.style.backgroundImage = this.style.backgroundImage;
                            }

                            if (this.m_equip is not null)
                            {
                                this.m_equip.Image.style.backgroundImage = this.style.backgroundImage;
                            }

                            this.Complete();

                            if (this.m_icon is not null)
                            {
                                this.Builder.EquipFromInventory(this.m_icon, element);
                            }

                            if (this.m_equip is not null)
                            {
                                this.Builder.EquipFromEquipment(this.m_equip, element);
                            }
                        }
                        else
                        {
                            if (this.m_icon is not null)
                            {
                                this.m_icon.Image.style.backgroundImage = this.style.backgroundImage;
                            }

                            if (this.m_equip is not null)
                            {
                                this.m_equip.Image.style.backgroundImage = this.style.backgroundImage;
                            }

                            this.Complete();
                        }

                        this.m_icon = null;

                        this.m_equip = null;
                    }
                });

                static void MaybeHover(EquipElement element, Vector2 position)
                {
                    if (element.Enabled && element.IsAllowed)
                    {
                        element.ForceHover(element.worldBound.Contains(position));
                    }
                }
            }

            public void Drag(Vector2 position, IconElement icon)
            {
                this.m_icon = icon;
                this.m_equip = null;

                this.Image.style.left = position.x - (this.Image.layout.width / 2);
                this.Image.style.top = position.y - (this.Image.layout.height / 2);
                this.Image.style.backgroundImage = icon.Image.style.backgroundImage;

                icon.Image.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                
                Span<IconType> valid = stackalloc IconType[(int)IconType.Count];

                int size = this.GetIconTypeOnWhichItemGetBeDraggedOnto(IconType.Inventory, icon.Item, valid);

                this.DisableAllElementsThatCannotBeDraggedOnto(valid.Slice(0, size));

                this.pickingMode = PickingMode.Position;
                this.Image.pickingMode = PickingMode.Position;

                this.visible = true;
                this.Image.visible = true;
            }

            public void Drag(Vector2 position, EquipElement equip)
            {
                this.m_icon = null;
                this.m_equip = equip;

                this.Image.style.left = position.x - (this.Image.layout.width / 2);
                this.Image.style.top = position.y - (this.Image.layout.height / 2);
                this.Image.style.backgroundImage = equip.Image.style.backgroundImage;
                
                equip.Image.style.backgroundImage = new StyleBackground(StyleKeyword.None);

                Span<IconType> valid = stackalloc IconType[(int)IconType.Count];

                int size = this.GetIconTypeOnWhichItemGetBeDraggedOnto(equip.Type, null, valid);

                this.DisableAllElementsThatCannotBeDraggedOnto(valid.Slice(0, size));

                this.pickingMode = PickingMode.Position;
                this.Image.pickingMode = PickingMode.Position;

                this.visible = true;
                this.Image.visible = true;
            }

            private void Complete()
            {
                this.ReenableAllElementsInTheBuilder();

                this.Image.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                this.Image.style.left = 0;
                this.Image.style.top = 0;

                this.pickingMode = PickingMode.Ignore;
                this.Image.pickingMode = PickingMode.Ignore;

                this.visible = false;
                this.Image.visible = false;
            }

            private void DisableAllElementsThatCannotBeDraggedOnto(ReadOnlySpan<IconType> valid)
            {
                ValidationCheck(this.Builder.m_equipEquip, valid);
                ValidationCheck(this.Builder.m_potionEquip, valid);
                ValidationCheck(this.Builder.m_trinketEquip, valid);

                var list = this.Builder.m_inventory;

                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].Disallow(); // never allow dragging onto inventory itself
                }

                static void ValidationCheck(EquipElement[] equip, ReadOnlySpan<IconType> valid)
                {
                    if (equip is not null)
                    {
                        for (int i = 0; i < equip.Length; ++i)
                        {
                            var element = equip[i];

                            if (element.Enabled)
                            {
                                bool allow = false;

                                for (int k = 0; k < valid.Length; ++k)
                                {
                                    if (element.Type == valid[k])
                                    {
                                        allow = true;

                                        break;
                                    }
                                }

                                if (!allow)
                                {
                                    element.Disallow();
                                }
                            }
                        }
                    }
                }
            }

            private EquipElement FindEquipElementThatWeDragOnto(Vector2 position)
            {
                {
                    var array = this.Builder.m_equipEquip;

                    for (int i = 0; i < array.Length; ++i)
                    {
                        var element = array[i];

                        if (element.Enabled && element.IsAllowed && element.worldBound.Contains(position))
                        {
                            return element;
                        }
                    }
                }

                {
                    var array = this.Builder.m_trinketEquip;

                    for (int i = 0; i < array.Length; ++i)
                    {
                        var element = array[i];

                        if (element.Enabled && element.IsAllowed && element.worldBound.Contains(position))
                        {
                            return element;
                        }
                    }
                }

                {
                    var array = this.Builder.m_potionEquip;

                    for (int i = 0; i < array.Length; ++i)
                    {
                        var element = array[i];

                        if (element.Enabled && element.IsAllowed && element.worldBound.Contains(position))
                        {
                            return element;
                        }
                    }
                }

                return null;
            }

            private void ReenableAllElementsInTheBuilder()
            {
                AllowAllInArray(this.Builder.m_equipEquip);
                AllowAllInArray(this.Builder.m_potionEquip);
                AllowAllInArray(this.Builder.m_trinketEquip);

                var list = this.Builder.m_inventory;

                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].Allow();
                }

                static void AllowAllInArray(EquipElement[] equip)
                {
                    if (equip is not null)
                    {
                        for (int i = 0; i < equip.Length; ++i)
                        {
                            var element = equip[i];

                            if (element.Enabled)
                            {
                                element.Allow();
                            }
                        }
                    }
                }
            }

            private int GetIconTypeOnWhichItemGetBeDraggedOnto(IconType drag, IItem item, Span<IconType> valid)
            {
                if (drag == IconType.Inventory)
                {
                    var tab = this.Builder.m_selection.CurrentItemTab;

                    if (tab == ItemTab.Armor)
                    {
                        if (item is null)
                        {
                            valid[0] = IconType.Helmet;
                            valid[1] = IconType.Chestplate;
                            valid[2] = IconType.Leggings;

                            return 3;
                        }
                        else
                        {
                            valid[0] = Unsafe.As<ArmorData>(item).SlotType switch
                            {
                                ArmorSlotType.Helmet => IconType.Helmet,
                                ArmorSlotType.Chestplate => IconType.Chestplate,
                                ArmorSlotType.Leggings => IconType.Leggings,
                                _ => throw new Exception("Invalid armor slot type"),
                            };

                            return 1;
                        }
                    }

                    if (tab == ItemTab.Trinket)
                    {
                        if (item is null)
                        {
                            valid[0] = IconType.Trinket;

                            return 1;
                        }
                        else
                        {
                            var type = this.Builder.m_selection.CurrentEquipType;

                            if (type != EquipType.Invalid)
                            {
                                if (Unsafe.As<TrinketData>(item).IsWeaponTrinket == (type == EquipType.Weapon))
                                {
                                    valid[0] = IconType.Trinket;

                                    return 1;
                                }
                            }

                            return 0;
                        }
                    }

                    if (tab == ItemTab.Weapon)
                    {
                        valid[0] = IconType.Weapon;

                        return 1;
                    }

                    if (tab == ItemTab.Potion)
                    {
                        valid[0] = IconType.Potion;

                        return 1;
                    }

                    throw new Exception("Invalid current inventory tab");
                }

                valid[0] = drag; // return drag itself for any armor, weapon, potion, trinket...

                return 1;
            }
        }

        private class SelectionInfo
        {
            public static readonly Color SelectedColor = Color.white;
            public static readonly Color EquippedColor = Color.yellow;
            public static readonly Color InactiveColor = Color.clear;

            private EquipType m_equip;
            private IconType m_icon;
            private ItemTab m_tab;

            private VisualElement m_equipment;
            private VisualElement m_selection;
            private int m_selectedSlot;
            private IItem m_displayed;

            public readonly InventoryBuilder Builder;

            public ItemTab CurrentItemTab => this.m_tab;

            public EquipType CurrentEquipType => this.m_equip;

            public IconType CurrentDisplayedIcon => this.m_icon;

            public IItem CurrentDisplayedItem => this.m_displayed;

            public int CurrentDisplayedSlot => this.m_selectedSlot;

            public SelectionInfo(InventoryBuilder builder)
            {
                this.Builder = builder;

                this.m_equipment = null;
                this.m_selection = null;
                this.m_selectedSlot = -1;

                this.m_equip = EquipType.Invalid;
                this.m_icon = IconType.Invalid;
                this.m_tab = ItemTab.Armor;
            }

            public void ResetIfInventory()
            {
                if (this.m_icon == IconType.Inventory)
                {
                    this.m_selection = null;
                    this.m_selectedSlot = -1;
                    this.m_icon = IconType.Invalid;
                    this.Builder.UpdateDisplayedItem(null);
                }
            }

            public void SelectTab(ItemTab tab)
            {
                var button = this.m_tab switch
                {
                    ItemTab.Armor => this.Builder.m_armorButton,
                    ItemTab.Weapon => this.Builder.m_weaponButton,
                    ItemTab.Potion => this.Builder.m_potionButton,
                    ItemTab.Trinket => this.Builder.m_trinketButton,
                    _ => throw new Exception("Invalid current tab"),
                };

                if (button is not null)
                {
                    button.style.unityBackgroundImageTintColor = TabButton.BackIdledTint;

                    button.pickingMode = PickingMode.Position;
                }

                this.m_tab = tab;

                button = tab switch
                {
                    ItemTab.Armor => this.Builder.m_armorButton,
                    ItemTab.Weapon => this.Builder.m_weaponButton,
                    ItemTab.Potion => this.Builder.m_potionButton,
                    ItemTab.Trinket => this.Builder.m_trinketButton,
                    _ => throw new Exception("Invalid passed tab"),
                };

                if (button is not null)
                {
                    button.style.unityBackgroundImageTintColor = TabButton.BackActiveTint;

                    button.pickingMode = PickingMode.Ignore;
                }

                this.Builder.SetupInventory();
            }

            public void SelectItem(IconElement icon)
            {
                this.DeselectItem();

                this.m_icon = IconType.Inventory;

                this.m_selectedSlot = -1;

                this.m_selection = icon;

                this.m_selection.style.unityBackgroundImageTintColor = SelectedColor;

                this.Builder.UpdateDisplayedItem(this.m_displayed = icon.Item);
            }

            public void SelectItem(EquipElement equip)
            {
                this.DeselectItem();

                var type = IconToEquip(equip.Type);

                if (type != EquipType.Invalid)
                {
                    if (this.m_equipment is not null)
                    {
                        this.m_equipment.style.unityBackgroundImageTintColor = InactiveColor;
                    }

                    this.Builder.UpdateDisplayedTrinkets(type);

                    this.m_equipment = equip;

                    this.m_equip = type;
                }

                this.m_icon = equip.Type;

                this.m_selectedSlot = equip.Id;

                this.m_selection = equip;

                this.m_selection.style.unityBackgroundImageTintColor = SelectedColor;

                this.Builder.UpdateDisplayedItem(this.m_displayed = equip.Item);
            }

            public void DeselectItem()
            {
                if (this.m_selection is not null)
                {
                    if (IconToEquip(this.m_icon) == EquipType.Invalid)
                    {
                        this.m_selection.style.unityBackgroundImageTintColor = InactiveColor;
                    }
                    else
                    {
                        this.m_selection.style.unityBackgroundImageTintColor = EquippedColor;
                    }

                    this.m_selectedSlot = -1;

                    this.m_selection = null;
                }
            }

            public void UpdateDisplayed(IItem item)
            {
                this.Builder.UpdateDisplayedItem(this.m_displayed = item);
            }

            private static EquipType IconToEquip(IconType icon)
            {
                return icon switch
                {
                    IconType.Helmet => EquipType.Helmet,
                    IconType.Chestplate => EquipType.Chestplate,
                    IconType.Leggings => EquipType.Leggings,
                    IconType.Weapon => EquipType.Weapon,
                    _ => EquipType.Invalid,
                };
            }
        }

        /// <summary>
        /// The equipment for which trinkets are currently displayed.
        /// </summary>
        private enum EquipType
        {
            Invalid = -1,
            Helmet,
            Chestplate,
            Leggings,
            Weapon,
            Count,
        }

        /// <summary>
        /// The type of items that the inventory is currently set up for.
        /// </summary>
        private enum ItemTab
        {
            Armor,
            Weapon,
            Potion,
            Trinket,
        }

        /// <summary>
        /// The type of the item currently displayed in the menu.
        /// </summary>
        private enum IconType
        {
            Invalid = -1,
            Inventory,
            Helmet,
            Chestplate,
            Leggings,
            Weapon,
            Trinket,
            Potion,
            Count,
        }

        private const string kBackButton = "back-button";
        private const string kInventoryView = "inventory-view";

        private const string kButtonContainer = "button-container";
        private const string kEquipmentSet = "equipment-set";
        private const string kTrinketSet = "trinket-set";
        private const string kPotionSet = "potion-set";

        private const string kSelectedName = "item-name-label";
        private const string kSelectedCost = "item-cost-label";
        private const string kSelectedDesc = "item-desc-label";

        private const string kHealthLabel = "health-label";
        private const string kManaLabel = "mana-label";
        private const string kDamageLabel = "damage-label";
        private const string kArmorLabel = "armor-label";
        private const string kEvasionLabel = "evasion-label";
        private const string kPrecisionLabel = "precision-label";
        private const string kCritChanceLabel = "critchance-label";
        private const string kCritMultLabel = "critmult-label";

        private List<IconElement> m_inventory;
        private EquipElement[] m_trinketEquip;
        private EquipElement[] m_potionEquip;
        private EquipElement[] m_equipEquip;
        private DraggedElement m_dragged;
        private SelectionInfo m_selection;
        private ScrollView m_itemview;
        private VisualElement m_back;

        private TabButton m_armorButton;
        private TabButton m_weaponButton;
        private TabButton m_potionButton;
        private TabButton m_trinketButton;

        private Label m_healthLabel;
        private Label m_manaLabel;
        private Label m_damageLabel;
        private Label m_armorLabel;
        private Label m_evasionLabel;
        private Label m_precisionLabel;
        private Label m_critChanceLabel;
        private Label m_critMultLabel;
        private Label m_itemNameLabel;
        private Label m_itemCostLabel;
        private Label m_itemDescLabel;

        private bool m_closedPressed;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
        }

        private void OnEnableEvent()
        {
            this.m_inventory = new();

            this.m_dragged = new DraggedElement(this);

            this.InitializeBackButton();
            this.InitializeView();
            this.InitializeEquipment();
            this.InitializeTrinkets();
            this.InitializePotions();
            this.InitializeButtons();
            this.InitializeSelect();
            this.InitializeStats();

            this.m_selection = new SelectionInfo(this);

            this.m_selection.SelectTab(ItemTab.Armor);
            this.m_selection.UpdateDisplayed(null);

            this.UpdateDisplayedStatistics();
        }

        private void OnDisableEvent()
        {
            this.m_closedPressed = false;

            this.m_back = null;
            this.m_inventory = null;

            this.m_equipEquip = null;
            this.m_potionEquip = null;
            this.m_trinketEquip = null;

            this.m_dragged = null;
            this.m_itemview = null;
            this.m_selection = null;

            this.m_armorButton = null;
            this.m_weaponButton = null;
            this.m_potionButton = null;
            this.m_trinketButton = null;

            this.m_healthLabel = null;
            this.m_manaLabel = null;
            this.m_damageLabel = null;
            this.m_armorLabel = null;
            this.m_evasionLabel = null;
            this.m_precisionLabel = null;
            this.m_critChanceLabel = null;
            this.m_critMultLabel = null;
        }

        private void InitializeBackButton()
        {
            this.m_back = this.UI.rootVisualElement.Q<VisualElement>(kBackButton);

            if (this.m_back is not null)
            {
                this.m_back.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    this.m_closedPressed = false;

                    this.m_back.style.unityBackgroundImageTintColor = Color.black;
                });

                this.m_back.RegisterCallback<PointerEnterEvent>(e =>
                {
                    this.m_closedPressed = false;

                    this.m_back.style.unityBackgroundImageTintColor = (Color)new Color32(235, 30, 30, 255);
                });

                this.m_back.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        this.m_closedPressed = true;

                        this.m_back.style.unityBackgroundImageTintColor = (Color)new Color32(200, 0, 0, 255);
                    }
                });

                this.m_back.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (e.button == 0)
                    {
                        if (this.m_closedPressed)
                        {
                            this.m_closedPressed = false;

                            this.m_back.style.unityBackgroundImageTintColor = (Color)new Color32(235, 30, 30, 255);

                            UIManager.Instance.PerformScreenChange(UIManager.ScreenType.InGame);
                        }
                    }
                });
            }
        }

        private void InitializeView()
        {
            this.m_itemview = this.UI.rootVisualElement.Q<ScrollView>(kInventoryView);

            if (this.m_itemview is not null)
            {
                this.m_itemview.Clear();
            }
        }

        private void InitializeEquipment()
        {
            this.m_equipEquip = new EquipElement[(int)EquipType.Count];

            var set = this.UI.rootVisualElement.Q<VisualElement>(kEquipmentSet);

            if (set is not null)
            {
                set.Clear();

                for (int i = 0; i < this.m_equipEquip.Length; ++i)
                {
                    set.Add(this.m_equipEquip[i] = new EquipElement(-1, EquipToIconType((EquipType)i), true, this)
                    {
                        Item = this.GetItemForEquippedElement(EquipToIconType((EquipType)i), -1)
                    });
                }
            }
        }

        private void InitializeTrinkets()
        {
            this.m_trinketEquip = new EquipElement[Mathf.Max(Armor.MaxTrinketSlots, Weapon.MaxTrinketSlots)];

            var set = this.UI.rootVisualElement.Q<VisualElement>(kTrinketSet);

            if (set is not null)
            {
                set.Clear();

                for (int i = 0; i < this.m_trinketEquip.Length; ++i)
                {
                    set.Add(this.m_trinketEquip[i] = new EquipElement(i, IconType.Trinket, false, this));
                }
            }
        }

        private void InitializePotions()
        {
            this.m_potionEquip = new EquipElement[Player.MaxPotionSlots];

            var set = this.UI.rootVisualElement.Q<VisualElement>(kPotionSet);

            if (set is not null)
            {
                set.Clear();

                for (int i = 0; i < this.m_potionEquip.Length; ++i)
                {
                    set.Add(this.m_potionEquip[i] = new EquipElement(i, IconType.Potion, true, this)
                    {
                        Item = this.GetItemForEquippedElement(IconType.Potion, i),
                    });
                }
            }
        }

        private void InitializeButtons()
        {
            var container = this.UI.rootVisualElement.Q<VisualElement>(kButtonContainer);

            if (container is not null)
            {
                container.Clear();

                container.Add(this.m_armorButton = new TabButton(ItemTab.Armor, "armor-button", "UI/Shared/Armor", this));
                container.Add(this.m_weaponButton = new TabButton(ItemTab.Weapon, "weapon-button", "UI/Shared/Weapon", this));
                container.Add(this.m_potionButton = new TabButton(ItemTab.Potion, "potion-button", "UI/Shared/Potion", this));
                container.Add(this.m_trinketButton = new TabButton(ItemTab.Trinket, "trinket-button", "UI/Shared/Trinket", this));
            }
        }

        private void InitializeSelect()
        {
            var root = this.UI.rootVisualElement;

            this.m_itemNameLabel = root.Q<Label>(kSelectedName);
            this.m_itemCostLabel = root.Q<Label>(kSelectedCost);
            this.m_itemDescLabel = root.Q<Label>(kSelectedDesc);
        }

        private void InitializeStats()
        {
            var root = this.UI.rootVisualElement;

            this.m_healthLabel = root.Q<Label>(kHealthLabel);
            this.m_manaLabel = root.Q<Label>(kManaLabel);
            this.m_damageLabel = root.Q<Label>(kDamageLabel);
            this.m_armorLabel = root.Q<Label>(kArmorLabel);
            this.m_evasionLabel = root.Q<Label>(kEvasionLabel);
            this.m_precisionLabel = root.Q<Label>(kPrecisionLabel);
            this.m_critChanceLabel = root.Q<Label>(kCritChanceLabel);
            this.m_critMultLabel = root.Q<Label>(kCritMultLabel);
        }

        private void SetupInventory()
        {
            this.m_inventory.Clear();

            if (this.m_itemview is not null)
            {
                this.m_itemview.Clear();

                IReadOnlyList<IItem> items = this.m_selection.CurrentItemTab switch
                {
                    ItemTab.Armor => Player.Instance.Armors,
                    ItemTab.Weapon => Player.Instance.Weapons,
                    ItemTab.Potion => Player.Instance.Potions,
                    ItemTab.Trinket => Player.Instance.Trinkets,
                    _ => throw new Exception("Invalid tab type"),
                };

                this.AddItemToTheInventory(null); // empty item (for equipment reset)

                for (int i = 0; i < items.Count; ++i)
                {
                    this.AddItemToTheInventory(items[i]);
                }

                this.UpdateDisplayedInventory();
            }
        }

        private void AddItemToTheInventory(IItem item)
        {
            var icon = new IconElement(item, this);

            this.m_inventory.Add(icon);

            this.m_itemview.Add(icon);
        }

        private IItem GetItemForEquippedElement(IconType icon, int id)
        {
            return icon switch
            {
                IconType.Helmet => Player.Instance.Helmet?.Data,
                IconType.Chestplate => Player.Instance.Chestplate?.Data,
                IconType.Leggings => Player.Instance.Leggings?.Data,
                IconType.Weapon => Player.Instance.Weapon?.Data,
                IconType.Potion => Player.Instance.EquippedPotions[id]?.Data,
                IconType.Trinket => GetItemFromTrinketSlot(this.m_selection.CurrentEquipType, id),
                _ => throw new Exception("Invalid icon type"),
            };

            static IItem GetItemFromTrinketSlot(EquipType equip, int id)
            {
                return equip switch
                {
                    EquipType.Helmet => Player.Instance.HelmetTrinkets[id]?.Data,
                    EquipType.Chestplate => Player.Instance.ChestplateTrinkets[id]?.Data,
                    EquipType.Leggings => Player.Instance.LeggingsTrinkets[id]?.Data,
                    EquipType.Weapon => Player.Instance.WeaponTrinkets[id]?.Data,
                    _ => throw new Exception("Invalid current equip type"),
                };
            }
        }

        private void UpdateDisplayedEquipment()
        {
            if (this.m_equipEquip is not null)
            {
                for (int i = 0; i < this.m_equipEquip.Length; ++i)
                {
                    var element = this.m_equipEquip[i];

                    element.Item = this.GetItemForEquippedElement(element.Type, element.Id);
                }
            }

            if (this.m_potionEquip is not null)
            {
                for (int i = 0; i < this.m_potionEquip.Length; ++i)
                {
                    var element = this.m_potionEquip[i];

                    element.Item = this.GetItemForEquippedElement(element.Type, element.Id);
                }
            }

            this.UpdateDisplayedTrinkets(this.m_selection.CurrentEquipType);

            var current = this.m_selection.CurrentDisplayedIcon;

            if (current is not IconType.Invalid and not IconType.Inventory)
            {
                var displayed = this.GetItemForEquippedElement(current, this.m_selection.CurrentDisplayedSlot);

                if (displayed != this.m_selection.CurrentDisplayedItem)
                {
                    this.m_selection.UpdateDisplayed(displayed);
                }
            }
        }

        private void UpdateDisplayedTrinkets(EquipType type)
        {
            if (this.m_trinketEquip is not null)
            {
                IReadOnlyList<Trinket> trinkets = type switch
                {
                    EquipType.Helmet => Player.Instance.HelmetTrinkets,
                    EquipType.Chestplate => Player.Instance.ChestplateTrinkets,
                    EquipType.Leggings => Player.Instance.LeggingsTrinkets,
                    EquipType.Weapon => Player.Instance.WeaponTrinkets,
                    _ => null,
                };

                int maxCount = type switch
                {
                    EquipType.Helmet => Player.Instance.Helmet?.MaxTrinketCount ?? 0,
                    EquipType.Chestplate => Player.Instance.Chestplate?.MaxTrinketCount ?? 0,
                    EquipType.Leggings => Player.Instance.Leggings?.MaxTrinketCount ?? 0,
                    EquipType.Weapon => Player.Instance.Weapon?.MaxTrinketCount ?? 0,
                    _ => 0,
                };

                for (int i = 0; i < maxCount; ++i)
                {
                    var element = this.m_trinketEquip[i];

                    element.Item = trinkets[i]?.Data;

                    element.SetEnabled(true);
                }

                for (int i = maxCount; i < this.m_trinketEquip.Length; ++i)
                {
                    var element = this.m_trinketEquip[i];

                    element.Item = null;

                    element.SetEnabled(false);
                }
            }
        }

        private void UpdateDisplayedItem(IItem item)
        {
            if (this.m_itemNameLabel is not null)
            {
                this.m_itemNameLabel.text = item is null ? String.Empty : item.Name;
            }

            if (this.m_itemCostLabel is not null)
            {
                this.m_itemCostLabel.text = item is null ? String.Empty : ("$" + item.Price.ToString());
            }

            if (this.m_itemDescLabel is not null)
            {
                this.m_itemDescLabel.text = item is null ? String.Empty : item.Description;
            }
        }

        private void UpdateDisplayedStatistics()
        {
            ref readonly var stats = ref Player.Instance.EntityStats;

            if (this.m_healthLabel is not null)
            {
                this.m_healthLabel.text = stats.MaxHealth.ToString();
            }

            if (this.m_manaLabel is not null)
            {
                this.m_manaLabel.text = stats.MaxMana.ToString();
            }

            if (this.m_damageLabel is not null)
            {
                this.m_damageLabel.text = stats.Damage.ToString();
            }

            if (this.m_armorLabel is not null)
            {
                this.m_armorLabel.text = stats.Armor.ToString();
            }

            if (this.m_evasionLabel is not null)
            {
                this.m_evasionLabel.text = ((int)(stats.Evasion * 100.0f)).ToString() + "%";
            }

            if (this.m_precisionLabel is not null)
            {
                this.m_precisionLabel.text = ((int)stats.Precision).ToString();
            }

            if (this.m_critChanceLabel is not null)
            {
                this.m_critChanceLabel.text = ((int)(stats.CritChance * 100.0f)).ToString() + "%";
            }

            if (this.m_critMultLabel is not null)
            {
                this.m_critMultLabel.text = ((int)(stats.CritMultiplier * 100.0f)).ToString() + "%";
            }
        }

        private void UpdateDisplayedInventory()
        {
            var player = Player.Instance;
            var inventory = this.m_inventory;

            for (int i = 0; i < inventory.Count; ++i)
            {
                inventory[i].SetDisplay(!player.HasEquippedItem(inventory[i].Item));
            }
        }

        private void EquipFromInventory(IconElement src, EquipElement dst)
        {
#if DEBUG
            Debug.Assert(src is not null);
            Debug.Assert(dst is not null);
#endif
            this.EquipItemInternal(src.Item, dst.Id, dst.Type);
            this.UpdateDisplayedInventory();
            this.UpdateDisplayedEquipment();
            this.UpdateDisplayedStatistics();
        }

        private void EquipFromEquipment(EquipElement src, EquipElement dst)
        {
#if DEBUG
            Debug.Assert(src is not null);
            Debug.Assert(dst is not null);
            Debug.Assert(src.Type == dst.Type);
#endif
            this.EquipItemInternal(src.Item, dst.Id, dst.Type);
            this.UpdateDisplayedInventory();
            this.UpdateDisplayedEquipment();
            this.UpdateDisplayedStatistics();
        }

        private void EquipItemInternal(IItem src, int slot, IconType type)
        {
            switch (type)
            {
                case IconType.Helmet: // should never be here
                    Player.Instance.EquipHelmet(Unsafe.As<ArmorData>(src));
                    break;

                case IconType.Chestplate: // should never be here
                    Player.Instance.EquipChestplate(Unsafe.As<ArmorData>(src));
                    break;

                case IconType.Leggings: // should never be here
                    Player.Instance.EquipLeggings(Unsafe.As<ArmorData>(src));
                    break;

                case IconType.Weapon: // should never be here
                    Player.Instance.EquipWeapon(Unsafe.As<WeaponData>(src));
                    break;

                case IconType.Potion:
                    Player.Instance.EquipPotion(slot, Unsafe.As<PotionData>(src));
                    break;

                case IconType.Trinket:
                    {
                        switch (this.m_selection.CurrentEquipType)
                        {
                            case EquipType.Helmet:
                                Player.Instance.EquipHelmetTrinket(slot, Unsafe.As<TrinketData>(src));
                                break;

                            case EquipType.Chestplate:
                                Player.Instance.EquipChestplateTrinket(slot, Unsafe.As<TrinketData>(src));
                                break;

                            case EquipType.Leggings:
                                Player.Instance.EquipLeggingsTrinket(slot, Unsafe.As<TrinketData>(src));
                                break;

                            case EquipType.Weapon:
                                Player.Instance.EquipWeaponTrinket(slot, Unsafe.As<TrinketData>(src));
                                break;
                        }
                    }
                    break;
            }


        }

        private static IconType EquipToIconType(EquipType type)
        {
            return type switch
            {
                EquipType.Helmet => IconType.Helmet,
                EquipType.Chestplate => IconType.Chestplate,
                EquipType.Leggings => IconType.Leggings,
                EquipType.Weapon => IconType.Weapon,
                _ => throw new Exception("Invalid equip type"),
            };
        }
    }
}
