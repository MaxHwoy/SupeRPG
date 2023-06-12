using SupeRPG.Game;
using SupeRPG.Input;

using UnityEngine;

namespace SupeRPG.Battle
{
    public class BattleBehavior : MonoBehaviour
    {
        public enum AnimationType
        {
            None,
            Idle,
            Damaged,
            Attack,
            BackOff,
            Death,
        }

        private const float kOrthoDefault = 0.2f;

        private static Sprite ms_glowSprite;

        private BattleBehavior m_animDest;
        private AnimationType m_animation;
        private GameObject m_attackObj;
        private Vector2 m_animStart;
        private Vector2 m_animEnd;
        private float m_deltaTotal;
        private bool m_upsizing;

        private SpriteRenderer m_renderer;
        private BoxCollider2D m_collider;
        private float m_animationSpeed;

        private SpriteRenderer m_glowEffect;
        private Color m_currentGlow;
        private Color m_requestGlow;

        private Vector2 m_unitPosition;
        private Vector2 m_unitOrigin;
        private Vector2 m_worldPosition;
        private Vector2 m_worldScale;
        
        private float m_maximumScale;
        private float m_minimumScale;
        private float m_defaultScale;
        private bool m_isOnLeftSide;
        
        private IEntity m_entity;
        private int m_index;

        public static Sprite GlowSprite => ms_glowSprite == null ? (ms_glowSprite = ResourceManager.LoadSprite("Sprites/Battle/GlowUnderlay")) : ms_glowSprite;

        public IEntity Entity => this.m_entity;

        public AnimationType Current => this.m_animation;

        public int Index
        {
            get => this.m_index;
            set => this.m_index = value;
        }

        public float AnimationSpeed
        {
            get => this.m_animationSpeed / (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
            set => this.m_animationSpeed = value * (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
        }

        public float MaximumScale
        {
            get => this.m_maximumScale / (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
            set => this.m_maximumScale = value * (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
        }

        public float MinimumScale
        {
            get => this.m_minimumScale / (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
            set => this.m_minimumScale = value * (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
        }

        public float DefaultScale
        {
            get => this.m_defaultScale / (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
            set => this.m_defaultScale = value * (ScreenManager.Instance.OrthographicSize * kOrthoDefault);
        }

        public Vector2 UnitOrigin
        {
            get => this.m_unitOrigin;
            set => this.m_unitOrigin = value;
        }

        public Vector2 UnitPosition => this.m_unitPosition;

        public Vector2 TopMostPoint => ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_worldPosition + new Vector2(0.0f, this.m_worldScale.y));

        public Vector2 LeftMostPoint => ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_worldPosition - new Vector2(this.m_worldScale.x, 0.0f));

        public Vector2 RightMostPoint => ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_worldPosition + new Vector2(this.m_worldScale.x, 0.0f));

        public Vector2 BottomMostPoint => ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_worldPosition - new Vector2(0.0f, this.m_worldScale.y));

        public bool NoneIsIdleAnimation;

        private void Update()
        {
            switch (this.m_animation)
            {
                case AnimationType.None:
                    break;

                case AnimationType.Idle:
                    this.AnimateIdle();
                    break;

                case AnimationType.Damaged:
                    this.AnimateDamaged();
                    break;

                case AnimationType.Attack:
                    this.AnimateAttack();
                    break;

                case AnimationType.BackOff:
                    this.AnimateBackOff();
                    break;

                case AnimationType.Death:
                    this.AnimateDeath();
                    break;
            }

            if (this.m_currentGlow != this.m_requestGlow)
            {
                this.m_currentGlow = this.m_requestGlow;

                this.m_glowEffect.color = this.m_currentGlow;
            }
        }

        private void AnimateIdle()
        {
            var transform = this.transform;

            var position = transform.position;
            var oldScale = transform.localScale;
            var oldBound = this.m_renderer.bounds.size.y;

            float newScale;

            if (this.m_upsizing)
            {
                newScale = oldScale.y + this.m_animationSpeed * Time.deltaTime;

                if (newScale >= this.m_maximumScale)
                {
                    newScale = this.m_maximumScale;

                    this.m_upsizing = false;
                }
            }
            else
            {
                newScale = oldScale.y - this.m_animationSpeed * Time.deltaTime;

                if (newScale <= this.m_minimumScale)
                {
                    newScale = this.m_minimumScale;

                    this.m_upsizing = true;
                }
            }

            transform.localScale = new Vector3(oldScale.x, newScale, oldScale.z);

            position.y += (this.m_renderer.bounds.size.y - oldBound) * 0.5f;

            transform.position = position;
        }

        private void AnimateDamaged()
        {
            float endLerp = this.m_animationSpeed * 0.16f;

            if (this.m_deltaTotal < endLerp)
            {
                this.m_deltaTotal += Time.deltaTime;

                if (this.m_deltaTotal > endLerp)
                {
                    this.m_deltaTotal = endLerp;
                }

                float halfLerp = endLerp * 0.5f;

                float scaleX;
                float scaleY;

                if (this.m_deltaTotal <= halfLerp)
                {
                    scaleX = this.m_defaultScale + (this.m_defaultScale * 0.5f) * (this.m_deltaTotal / halfLerp);
                    scaleY = this.m_defaultScale - (this.m_defaultScale / 3.0f) * (this.m_deltaTotal / halfLerp);
                }
                else
                {
                    scaleX = this.m_defaultScale + (this.m_defaultScale * 0.5f) * ((endLerp - this.m_deltaTotal) / halfLerp);
                    scaleY = this.m_defaultScale - (this.m_defaultScale / 3.0f) * ((endLerp - this.m_deltaTotal) / halfLerp);
                }

                var position = this.transform.position;
                var oldBound = this.m_renderer.bounds.size.y;

                this.transform.localScale = new Vector3(scaleX, scaleY, 1.0f);

                position.y += (this.m_renderer.bounds.size.y - oldBound) * 0.5f;

                this.transform.position = position;
            }
            else
            {
                this.PlayAnimation(this.NoneIsIdleAnimation ? AnimationType.Idle : AnimationType.None);
            }
        }

        private void AnimateAttack()
        {
            float endLerp = this.m_animationSpeed * 0.08f;

            if (this.m_deltaTotal < endLerp)
            {
                this.m_deltaTotal += Time.deltaTime;

                if (this.m_deltaTotal > endLerp)
                {
                    this.m_deltaTotal = endLerp;
                }

                if (this.m_entity.IsMelee)
                {
                    this.m_unitPosition = this.m_animStart + (this.m_animEnd - this.m_animStart) * (this.m_deltaTotal / endLerp);

                    this.m_worldPosition = ScreenManager.Instance.UnitScreenPointToWorldPosition(this.m_unitPosition);

                    this.transform.position = this.m_worldPosition;
                }
                else
                {
                    if (this.m_attackObj == null)
                    {
                        this.m_attackObj = new GameObject("Attack Element");

                        var renderer = this.m_attackObj.AddComponent<SpriteRenderer>();

                        renderer.sortingOrder = 100;
                        renderer.sprite = ResourceManager.LoadSprite("Sprites/Weapons/DeadFish"); // #TODO

                        this.m_attackObj.transform.localScale = new Vector3(2.0f * this.m_defaultScale, 2.0f * this.m_defaultScale, 1.0f);
                    }

                    var position = this.m_animStart + (this.m_animEnd - this.m_animStart) * (this.m_deltaTotal / endLerp);

                    this.m_attackObj.transform.position = ScreenManager.Instance.UnitScreenPointToWorldPosition(position);
                }
            }
            else
            {
                if (this.m_attackObj != null)
                {
                    Object.Destroy(this.m_attackObj);
                }

                this.PlayAnimation(this.NoneIsIdleAnimation ? AnimationType.Idle : AnimationType.None);
            }
        }

        private void AnimateBackOff()
        {
            if (this.m_entity.IsMelee)
            {
                float endLerp = this.m_animationSpeed * 0.08f;

                if (this.m_deltaTotal < endLerp)
                {
                    this.m_deltaTotal += Time.deltaTime;

                    if (this.m_deltaTotal > endLerp)
                    {
                        this.m_deltaTotal = endLerp;
                    }

                    this.m_unitPosition = this.m_animStart + (this.m_animEnd - this.m_animStart) * (this.m_deltaTotal / endLerp);

                    this.m_worldPosition = ScreenManager.Instance.UnitScreenPointToWorldPosition(this.m_unitPosition);

                    this.transform.position = this.m_worldPosition;

                    return;
                }
            }

            this.PlayAnimation(this.NoneIsIdleAnimation ? AnimationType.Idle : AnimationType.None);
        }

        private void AnimateDeath()
        {
            float endLerp = this.m_animationSpeed * 0.25f;

            if (this.m_deltaTotal < endLerp)
            {
                this.m_deltaTotal += Time.deltaTime;

                if (this.m_deltaTotal > endLerp)
                {
                    this.m_deltaTotal = endLerp;
                }

                this.m_unitPosition = this.m_animStart + (this.m_animEnd - this.m_animStart) * (this.m_deltaTotal / endLerp);

                this.m_worldPosition = ScreenManager.Instance.UnitScreenPointToWorldPosition(this.m_unitPosition);

                this.transform.position = this.m_worldPosition;
            }
            else
            {
                this.gameObject.SetActive(false);

                this.PlayAnimation(this.NoneIsIdleAnimation ? AnimationType.Idle : AnimationType.None);
            }
        }

        private void ComputeStartEndIdle()
        {
            this.m_animStart = default;
            this.m_animEnd = default;
        }

        private void ComputeStartEndDamaged()
        {
            this.m_animStart = default;
            this.m_animEnd = default;
        }

        private void ComputeStartEndAttack()
        {
            this.m_animStart = this.m_unitPosition;
            
            if (this.m_entity.IsMelee)
            {
                this.m_animEnd = this.m_animDest.m_isOnLeftSide
                    ? this.m_animDest.RightMostPoint
                    : this.m_animDest.LeftMostPoint;
            }
            else
            {
                this.m_animEnd = this.m_animDest.m_isOnLeftSide
                    ? ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_animDest.m_worldPosition + new Vector2(this.m_worldScale.x * 0.5f, 0.0f))
                    : ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_animDest.m_worldPosition - new Vector2(this.m_worldScale.x * 0.5f, 0.0f));
            }
        }

        private void ComputeStartEndBackOff()
        {
            this.m_animStart = this.m_unitPosition;
            this.m_animEnd = this.m_unitOrigin;
        }

        private void ComputeStartEndDeath()
        {
            this.m_animStart = this.m_unitPosition;

            this.m_animEnd = this.m_isOnLeftSide
                ? this.m_unitPosition + new Vector2(-1.0f, +0.5f)
                : this.m_unitPosition + new Vector2(+1.0f, +0.5f);
        }

        public void Initialize(IEntity entity, string name, int layer)
        {
            const float GlowScale = 1.5f;

            this.m_entity = entity;
            this.gameObject.name = name;

            this.m_renderer = this.gameObject.GetComponent<SpriteRenderer>();
            this.m_collider = this.gameObject.GetComponent<BoxCollider2D>();

            this.transform.localScale = new Vector3(this.m_maximumScale, this.m_maximumScale, 1.0f);

            this.m_renderer.sortingOrder = layer;
            this.m_renderer.sprite = entity.Sprite;

            this.m_unitPosition = this.m_unitOrigin;

            this.m_worldScale = this.m_renderer.bounds.size;

            this.m_glowEffect = new GameObject(this.name + "-glow").AddComponent<SpriteRenderer>();

            this.m_glowEffect.gameObject.transform.parent = this.gameObject.transform;
            this.m_glowEffect.sprite = BattleBehavior.GlowSprite;
            this.m_glowEffect.color = Color.clear;
            this.m_currentGlow = Color.clear;
            this.m_requestGlow = Color.clear;

            var slength = Mathf.Sqrt((this.m_worldScale.x * this.m_worldScale.x) + (this.m_worldScale.y * this.m_worldScale.y)) * GlowScale;
            var gbounds = this.m_glowEffect.bounds.size;

            this.m_glowEffect.gameObject.transform.localScale = new Vector3(slength / (gbounds.x * this.m_maximumScale), slength / (gbounds.y * this.m_maximumScale), 1.0f);

            this.RecalculateTransform();
        }

        public void RecalculateTransform()
        {
            this.m_worldPosition = ScreenManager.Instance.UnitScreenPointToWorldPosition(this.m_unitPosition);

            var localScale = this.transform.localScale;

            localScale.x = Mathf.Clamp(localScale.x, this.m_minimumScale, this.m_maximumScale);
            localScale.y = Mathf.Clamp(localScale.y, this.m_minimumScale, this.m_maximumScale);

            this.transform.position = this.m_worldPosition;
            this.transform.localScale = localScale;

            var boundsSize = this.m_renderer.bounds.size;
            
            this.m_collider.size = new Vector2(boundsSize.x / localScale.x, boundsSize.y / localScale.y);

            this.m_upsizing = false;
            this.m_isOnLeftSide = this.m_unitOrigin.x < 0.0f;
        }

        public void PlayAnimation(AnimationType type, BattleBehavior dst = null)
        {
            this.transform.localScale = new Vector3(this.m_defaultScale, this.m_defaultScale, 1.0f);

            this.m_deltaTotal = 0.0f;

            this.m_animation = type;

            this.m_animDest = dst;

            this.RecalculateTransform();

            switch (type)
            {
                case AnimationType.None:
                    break;

                case AnimationType.Idle:
                    this.ComputeStartEndIdle();
                    break;

                case AnimationType.Damaged:
                    this.ComputeStartEndDamaged();
                    break;

                case AnimationType.Attack:
                    this.ComputeStartEndAttack();
                    break;

                case AnimationType.BackOff:
                    this.ComputeStartEndBackOff();
                    break;

                case AnimationType.Death:
                    this.ComputeStartEndDeath();
                    break;
            }
        }

        public Vector2 GetPositionForHealthIndicator()
        {
            return ScreenManager.Instance.WorldPositionToUnitScreenPoint(this.m_worldPosition - new Vector2(0.0f, this.m_worldScale.y * 0.6f));
        }

        public void SetGlowHighlight(bool enable, Color color)
        {
            this.m_requestGlow = enable ? color : Color.clear;
        }
    }
}
