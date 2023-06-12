using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

using InputProcessor = SupeRPG.Input.InputProcessor;

namespace SupeRPG.UI
{
    public class UIBuilder : MonoBehaviour
    {
        private readonly List<(Key key, Action action)> m_keyEvents = new();
        private UIDocument m_ui;

        public UIDocument UI => this.m_ui == null ? (this.m_ui = this.gameObject.GetComponent<UIDocument>()) : this.m_ui;

        public event Action OnUIEnabled;

        public event Action OnUIDisabled;

        public event Action OnUIUpdate;

        private void Awake()
        {
            this.BindEvents();
        }

        private void OnEnable()
        {
            this.UI.enabled = true; // ensure UI is enabled BEFORE we set it up
            this.OnUIEnabled?.Invoke();
        }

        private void OnDisable()
        {
            this.OnUIDisabled?.Invoke();
            this.UI.enabled = false; // ensure UI is disabled AFTER the cleanup
        }

        private void Update()
        {
            var processor = InputProcessor.Instance;

            if (processor != null)
            {
                for (int i = 0; i < this.m_keyEvents.Count; ++i)
                {
                    var keyEvent = this.m_keyEvents[i];

                    if (processor.IsButtonPressed(keyEvent.key))
                    {
                        keyEvent.action();
                    }
                }
            }

            this.OnUIUpdate?.Invoke();
        }

        public void BindKeyAction(Key key, Action action)
        {
            if (action is not null)
            {
                this.m_keyEvents.Add((key, action));
            }
        }

        protected virtual void BindEvents()
        {
        }
    }
}
