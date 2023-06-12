using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SupeRPG.Input
{
    public class ScreenManager : MonoBehaviour
    {
        private static ScreenManager ms_instance;

        private Texture2D m_cursorTexture;
        private CursorMode m_cursorMode;
        private Vector2 m_cursorOrigin;

        private float m_screenHeight;
        private float m_screenWidth;
        private Camera m_camera;

        public static ScreenManager Instance => ScreenManager.ms_instance == null ? (ScreenManager.ms_instance = Object.FindFirstObjectByType<ScreenManager>()) : ScreenManager.ms_instance;

        public float Width => this.m_screenWidth;

        public float Height => this.m_screenHeight;

        public float AspectRatio => this.m_screenWidth / this.m_screenHeight;

        public float OrthographicSize => this.m_camera.orthographicSize;

        public Texture2D CursorTexture => this.m_cursorTexture;

        public event Action OnScreenResolutionChanged;

        private void Awake()
        {
            this.m_camera = Camera.main;
            this.m_screenWidth = Screen.width;
            this.m_screenHeight = Screen.height;
        }

        private void Update()
        {
            float width = Screen.width;
            float height = Screen.height;

            if (this.m_screenWidth != width || this.m_screenHeight != height)
            {
                this.m_screenWidth = width;
                this.m_screenHeight = height;

                this.OnScreenResolutionChanged?.Invoke();
            }
        }

        public void SetCursorTexture(Texture2D cursorTexture, Vector2 origin, CursorMode mode = CursorMode.Auto)
        {
            if (this.m_cursorTexture != cursorTexture || this.m_cursorOrigin != origin || this.m_cursorMode != mode)
            {
                this.m_cursorTexture = cursorTexture;

                this.m_cursorOrigin = origin;

                this.m_cursorMode = mode;

#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR_WIN
                Cursor.SetCursor(cursorTexture, origin, mode); // this only works properly on windows
#endif
            }
        }

        public Vector2 ScreenToWorldPoint(Vector2 point)
        {
            return this.m_camera.ScreenToWorldPoint(point);
        }

        public Vector2 WorldPositionToUnitScreenPoint(Vector2 point)
        {
            var ratio = this.m_screenHeight / this.m_screenWidth;

            var size = this.m_camera.orthographicSize;

            return new Vector2(point.x * ratio / size, point.y / size);
        }

        public Vector2 UnitScreenPointToWorldPosition(Vector2 point)
        {
            return this.m_camera.orthographicSize * new Vector2(point.x * this.m_screenWidth / this.m_screenHeight, point.y);
        }

        public static Vector2 WorldPositionToUnitScreenPoint(Vector2 point, Vector2 resolution, float orthographicSize)
        {
            var ratio = resolution.y / resolution.x;

            return new Vector2(point.x * ratio / orthographicSize, point.y / orthographicSize);
        }

        public static Vector2 UnitScreenPointToWorldPosition(Vector2 point, Vector2 resolution, float orthographicSize)
        {
            return orthographicSize * new Vector2(point.x * resolution.x / resolution.y, point.y);
        }

        public static Vector2 UnitScreenPointToScreenSpace(Vector2 point, Vector2 resolution)
        {
            return new Vector2(resolution.x * (0.5f * (point.x + 1.0f)), resolution.y * (0.5f * (point.y + 1.0f)));
        }

        public static Vector2 ScreenSpaceToUnitScreenPoint(Vector2 point, Vector2 resolution)
        {
            return new Vector2((2.0f * point.x / resolution.x) - 1.0f, (2.0f * point.y / resolution.y) - 1.0f);
        }
    }
}
