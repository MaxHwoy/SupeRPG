using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UIElements;

namespace SupeRPG.UI
{
    public class TransitionBuilder : UIBuilder
    {
        private const string kOverlay = "overlay";
        private const string kLeftPart = "left-part";
        private const string kRightPart = "right-part";

        private const float kTransitionDelay = 0.5f;
        private const float kMaximumOffset = -55.0f;

        private VisualElement m_rightPart;
        private VisualElement m_leftPart;
        private VisualElement m_overlay;

        protected override void BindEvents()
        {
            this.OnUIEnabled += this.OnEnableEvent;
            this.OnUIDisabled += this.OnDisableEvent;
        }

        private void OnEnableEvent()
        {
            this.m_overlay = this.UI.rootVisualElement.Q<VisualElement>(kOverlay);
            this.m_leftPart = this.UI.rootVisualElement.Q<VisualElement>(kLeftPart);
            this.m_rightPart = this.UI.rootVisualElement.Q<VisualElement>(kRightPart);
        }

        private void OnDisableEvent()
        {
            this.m_overlay = null;
            this.m_leftPart = null;
            this.m_rightPart = null;
        }

        public void BeginTransition(Action callback)
        {
            this.enabled = true;

            Debug.Assert(this.m_overlay is not null);
            Debug.Assert(this.m_leftPart is not null);
            Debug.Assert(this.m_rightPart is not null);

            this.StartCoroutine(this.BeginTransitionInternal(callback));
        }

        public void EndTransition(Action callback)
        {
            this.StartCoroutine(this.EndTransitionInternal(callback));
        }
        
        public void TransitionWithDelay(Action onBeginTransitionFinished, Action onEndTransitionFinished, float delay)
        {
            this.enabled = true;

            Debug.Assert(this.m_overlay is not null);
            Debug.Assert(this.m_leftPart is not null);
            Debug.Assert(this.m_rightPart is not null);

            this.StartCoroutine(this.TransitionWithDelayInternal(onBeginTransitionFinished, onEndTransitionFinished, delay));
        }

        private IEnumerator BeginTransitionInternal(Action callback)
        {
            float deltaTotal = 0.0f;

            while (deltaTotal < kTransitionDelay)
            {
                deltaTotal += Time.deltaTime;

                if (deltaTotal > kTransitionDelay)
                {
                    deltaTotal = kTransitionDelay;
                }

                var length = new StyleLength(new Length(Mathf.Lerp(kMaximumOffset, 0.0f, deltaTotal / kTransitionDelay), LengthUnit.Percent));

                this.m_leftPart.style.left = length;
                this.m_rightPart.style.right = length;

                yield return null;
            }

            callback?.Invoke();
        }

        private IEnumerator EndTransitionInternal(Action callback)
        {
            float deltaTotal = 0.0f;

            while (deltaTotal < kTransitionDelay)
            {
                deltaTotal += Time.deltaTime;

                if (deltaTotal > kTransitionDelay)
                {
                    deltaTotal = kTransitionDelay;
                }

                var length = new StyleLength(new Length(Mathf.Lerp(0.0f, kMaximumOffset, deltaTotal / kTransitionDelay), LengthUnit.Percent));

                this.m_leftPart.style.left = length;
                this.m_rightPart.style.right = length;

                yield return null;
            }

            this.enabled = false;

            Debug.Assert(this.m_overlay is null);
            Debug.Assert(this.m_leftPart is null);
            Debug.Assert(this.m_rightPart is null);

            callback?.Invoke();
        }

        private IEnumerator TransitionWithDelayInternal(Action onBeginTransitionFinished, Action onEndTransitionFinished, float delay)
        {
            yield return this.StartCoroutine(this.BeginTransitionInternal(onBeginTransitionFinished));

            yield return new WaitForSeconds(delay);

            yield return this.StartCoroutine(this.EndTransitionInternal(onEndTransitionFinished));
        }
    }
}
