using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SupeRPG.Input
{
    public class InputProcessor : MonoBehaviour
    {
        private static InputProcessor ms_instance;

        public static InputProcessor Instance => InputProcessor.ms_instance == null ? (InputProcessor.ms_instance = Object.FindFirstObjectByType<InputProcessor>()) : ms_instance;

        private Vector2 m_mouseWorldPos;
        private bool m_wasRightPressed;
        private bool m_wasLeftPressed;
        private bool m_mouseRightDown;
        private bool m_mouseLeftDown;
        private Camera m_mainCamera;

        private void Awake()
        {
            this.m_mainCamera = Camera.main;
        }

        private void Start()
        {
            this.m_mainCamera = Camera.main;
            this.m_mouseWorldPos = Vector2.zero;
            this.m_mouseRightDown = false;
            this.m_mouseLeftDown = false;
        }

        private void Update()
        {
            this.m_wasLeftPressed = false;
            this.m_wasRightPressed = false;

            if (this.m_mouseLeftDown)
            {
                if (!Mouse.current.leftButton.isPressed)
                {
                    this.m_mouseLeftDown = false;
                }
            }
            else
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    this.m_mouseLeftDown = true;
                    this.m_wasLeftPressed = true;
                }
            }

            if (this.m_mouseRightDown)
            {
                if (!Mouse.current.rightButton.isPressed)
                {
                    this.m_mouseRightDown = false;
                }
            }
            else
            {
                if (Mouse.current.rightButton.isPressed)
                {
                    this.m_mouseRightDown = true;
                    this.m_wasRightPressed = true;
                }
            }

            this.m_mouseWorldPos = this.m_mainCamera.ScreenToWorldPoint(Mouse.current.position.value);
        }

        public bool IsButtonPressed(Key key)
        {
            return Keyboard.current[key].isPressed;
        }

        public bool IsPointerOverUIObject()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        public bool IsPointerOverCollider(out Collider2D collider)
        {
            collider = null;

            if (!this.IsPointerOverUIObject())
            {
                collider = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero).collider;
            }

            return collider != null;
        }

        public Vector2 MouseWorldPosition()
        {
            return this.m_mouseWorldPos;
        }

        public bool IsLeftMousePressed()
        {
            return this.m_mouseLeftDown;
        }

        public bool IsRightMousePressed()
        {
            return this.m_mouseRightDown;
        }

        public Collider2D RaycastViaMouse()
        {
            if (!this.IsPointerOverUIObject())
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                return raycast.collider;
            }

            return null;
        }

        public Collider2D RaycastLeftContinuous()
        {
            if (!this.IsPointerOverUIObject() && this.m_mouseLeftDown)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                return raycast.collider;
            }

            return null;
        }

        public Collider2D RaycastRightContinuous()
        {
            if (!this.IsPointerOverUIObject() && this.m_mouseRightDown)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                return raycast.collider;
            }

            return null;
        }

        public Collider2D RaycastLeftSingular()
        {
            if (!this.IsPointerOverUIObject() && this.m_wasLeftPressed)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                return raycast.collider;
            }

            return null;
        }

        public Collider2D RaycastRightSingular()
        {
            if (!this.IsPointerOverUIObject() && this.m_wasRightPressed)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                return raycast.collider;
            }

            return null;
        }

        public bool RaycastLeftClickIfHappened(out Collider2D collider)
        {
            collider = null;

            if (!this.IsPointerOverUIObject() && this.m_wasLeftPressed)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                collider = raycast.collider;

                return true;
            }

            return false;
        }

        public bool RaycastRightClickIfHappened(out Collider2D collider)
        {
            collider = null;

            if (!this.IsPointerOverUIObject() && this.m_wasRightPressed)
            {
                var raycast = Physics2D.Raycast(this.m_mouseWorldPos, Vector2.zero);
#if DEBUG
                if (raycast)
                {
                    Debug.Log($"Raycasted {raycast.collider.name}");
                }
#endif
                collider = raycast.collider;

                return true;
            }

            return false;
        }
    }
}
