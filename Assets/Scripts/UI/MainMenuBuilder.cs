using SupeRPG.Battle;
using SupeRPG.Input;

using UnityEngine;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class MainMenuBuilder : UIBuilder
    {
        private const string kNewGameButton = "newgame-button";
        private const string kContinueButton = "continue-button";
        private const string kEndlessButton = "endless-button";
        private const string kSoundButton = "sound-button";
        private const string kCreditsButton = "credits-button";

        private static readonly Color ms_idledColor = new Color32(175, 175, 175, 255);
        private static readonly Color ms_hoverColor = new Color32(140, 140, 140, 255);
        private static readonly Color ms_pressColor = new Color32(115, 115, 115, 255);
        private static readonly Color ms_locksColor = new Color32(70, 70, 70, 255);

        private VisualElement m_newgameButton;
        private bool m_newgamePressed;

        private VisualElement m_continueButton;
        private bool m_continuePressed;

        private VisualElement m_endlessButton;
        private bool m_endlessPressed;

        private VisualElement m_soundButton;
        private bool m_soundPressed;

        private VisualElement m_creditsButton;
        private bool m_creditsPressed;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
        }

        private void OnEnableEvent()
        {
            this.SetupNewGameButton();
            this.SetupContinueButton();
            this.SetupEndlessButton();
            this.SetupSoundButton();
            this.SetupCreditsButton();
        }

        private void OnDisableEvent()
        {
            this.m_newgameButton = null;
            this.m_continueButton = null;
            this.m_endlessButton = null;
            this.m_soundButton = null;
            this.m_creditsButton = null;

            this.m_newgamePressed = false;
            this.m_continuePressed = false;
            this.m_endlessPressed = false;
            this.m_soundPressed = false;
            this.m_creditsPressed = false;
        }

        private void SetupNewGameButton()
        {
            this.m_newgameButton = this.UI.rootVisualElement.Q<VisualElement>(kNewGameButton);

            if (this.m_newgameButton is not null)
            {
                this.m_newgameButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.m_newgameButton.pickingMode == PickingMode.Position)
                    {
                        this.m_newgameButton.style.backgroundColor = ms_idledColor;

                        this.m_newgamePressed = false;
                    }
                });

                this.m_newgameButton.RegisterCallback<PointerOverEvent>(e =>
                {
                    if (this.m_newgameButton.pickingMode == PickingMode.Position)
                    {
                        this.m_newgameButton.style.backgroundColor = ms_hoverColor;

                        this.m_newgamePressed = false;
                    }
                });

                this.m_newgameButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.m_newgameButton.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_newgameButton.style.backgroundColor = ms_pressColor;

                        this.m_newgamePressed = true;
                    }
                });

                this.m_newgameButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.m_newgameButton.pickingMode == PickingMode.Position && e.button == 0 && this.m_newgamePressed)
                    {
                        this.m_newgameButton.style.backgroundColor = ms_hoverColor;

                        this.m_newgamePressed = false;

                        UIManager.Instance.TransitionWithDelay(() => UIManager.Instance.PerformScreenChange(UIManager.ScreenType.Creation), null, 2.0f);
                    }
                });
            }
        }

        private void SetupContinueButton()
        {
            this.m_continueButton = this.UI.rootVisualElement.Q<VisualElement>(kContinueButton);

            if (this.m_continueButton is not null)
            {
                if (MapManager.Instance.CanContinue())
                {
                    this.m_continueButton.pickingMode = PickingMode.Position;

                    this.m_continueButton.style.backgroundColor = ms_idledColor;
                }
                else
                {
                    this.m_continueButton.pickingMode = PickingMode.Ignore;

                    this.m_continueButton.style.backgroundColor = ms_locksColor;
                }

                this.m_continueButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.m_continueButton.pickingMode == PickingMode.Position)
                    {
                        this.m_continueButton.style.backgroundColor = ms_idledColor;

                        this.m_continuePressed = false;
                    }
                });

                this.m_continueButton.RegisterCallback<PointerOverEvent>(e =>
                {
                    if (this.m_continueButton.pickingMode == PickingMode.Position)
                    {
                        this.m_continueButton.style.backgroundColor = ms_hoverColor;

                        this.m_continuePressed = false;
                    }
                });

                this.m_continueButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.m_continueButton.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_continueButton.style.backgroundColor = ms_pressColor;

                        this.m_continuePressed = true;
                    }
                });

                this.m_continueButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.m_continueButton.pickingMode == PickingMode.Position && e.button == 0 && this.m_continuePressed)
                    {
                        this.m_continueButton.style.backgroundColor = ms_hoverColor;

                        this.m_continuePressed = false;

                        MapManager.Instance.LoadInGameWithContinue();
                    }
                });
            }
        }

        private void SetupEndlessButton()
        {
            this.m_endlessButton = this.UI.rootVisualElement.Q<VisualElement>(kEndlessButton);

            if (this.m_endlessButton is not null)
            {
                // #TODO for now endless mode is disabled

                this.m_endlessButton.pickingMode = PickingMode.Ignore;

                this.m_endlessButton.style.backgroundColor = ms_locksColor;

                this.m_endlessButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.m_endlessButton.pickingMode == PickingMode.Position)
                    {
                        this.m_endlessButton.style.backgroundColor = ms_idledColor;

                        this.m_endlessPressed = false;
                    }
                });

                this.m_endlessButton.RegisterCallback<PointerOverEvent>(e =>
                {
                    if (this.m_endlessButton.pickingMode == PickingMode.Position)
                    {
                        this.m_endlessButton.style.backgroundColor = ms_hoverColor;

                        this.m_endlessPressed = false;
                    }
                });

                this.m_endlessButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.m_endlessButton.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_endlessButton.style.backgroundColor = ms_pressColor;

                        this.m_endlessPressed = true;
                    }
                });

                this.m_endlessButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.m_endlessButton.pickingMode == PickingMode.Position && e.button == 0 && this.m_endlessPressed)
                    {
                        this.m_endlessButton.style.backgroundColor = ms_hoverColor;

                        this.m_endlessPressed = false;

                        // #TODO SETUP ENDLESS
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
                    if (this.m_soundButton.pickingMode == PickingMode.Position)
                    {
                        this.m_soundButton.style.backgroundColor = ms_idledColor;

                        this.m_soundPressed = false;
                    }
                });

                this.m_soundButton.RegisterCallback<PointerOverEvent>(e =>
                {
                    if (this.m_soundButton.pickingMode == PickingMode.Position)
                    {
                        this.m_soundButton.style.backgroundColor = ms_hoverColor;

                        this.m_soundPressed = false;
                    }
                });

                this.m_soundButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.m_soundButton.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_soundButton.style.backgroundColor = ms_pressColor;

                        this.m_soundPressed = true;
                    }
                });

                this.m_soundButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.m_soundButton.pickingMode == PickingMode.Position && e.button == 0 && this.m_soundPressed)
                    {
                        this.m_soundButton.style.backgroundColor = ms_hoverColor;

                        this.m_soundPressed = false;

                        Main.Instance.EnableMusic = !Main.Instance.EnableMusic;
                    }
                });
            }
        }

        private void SetupCreditsButton()
        {
            this.m_creditsButton = this.UI.rootVisualElement.Q<VisualElement>(kCreditsButton);

            if (this.m_creditsButton is not null)
            {
                // #TODO for now credits are disabled

                this.m_creditsButton.pickingMode = PickingMode.Ignore;

                this.m_creditsButton.style.backgroundColor = ms_locksColor;

                this.m_creditsButton.RegisterCallback<PointerLeaveEvent>(e =>
                {
                    if (this.m_creditsButton.pickingMode == PickingMode.Position)
                    {
                        this.m_creditsButton.style.backgroundColor = ms_idledColor;

                        this.m_creditsPressed = false;
                    }
                });

                this.m_creditsButton.RegisterCallback<PointerOverEvent>(e =>
                {
                    if (this.m_creditsButton.pickingMode == PickingMode.Position)
                    {
                        this.m_creditsButton.style.backgroundColor = ms_hoverColor;

                        this.m_creditsPressed = false;
                    }
                });

                this.m_creditsButton.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (this.m_creditsButton.pickingMode == PickingMode.Position && e.button == 0)
                    {
                        this.m_creditsButton.style.backgroundColor = ms_pressColor;

                        this.m_creditsPressed = true;
                    }
                });

                this.m_creditsButton.RegisterCallback<PointerUpEvent>(e =>
                {
                    if (this.m_creditsButton.pickingMode == PickingMode.Position && e.button == 0 && this.m_creditsPressed)
                    {
                        this.m_creditsButton.style.backgroundColor = ms_hoverColor;

                        this.m_creditsPressed = false;

                        // #TODO SETUP CREDITS
                    }
                });
            }
        }
    }
}
