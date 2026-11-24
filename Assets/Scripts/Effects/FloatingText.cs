using Pools;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Effects
{
    /// <summary>
    /// A text in world space that fades in, plays an animation and fades out.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro text;
        
        [Header("Settings")]
        [SerializeField] private Vector3 targetPositionOffset;
        [SerializeField] private float positionHorizontalVariance = 1f;
        [SerializeField] private float positionVerticalVariance = 0.6f;
        [SerializeField] private float animationDuration = 1.2f;
        [SerializeField] private Ease scaleEase = Ease.InOutBounce;
        [SerializeField] private Ease positionEase = Ease.InOutQuad;

        private FloatingTextPool pool;

        private void Reset()
        {
            // Auto-grab reference.
            text = GetComponent<TextMeshPro>();
        }

        private void Update()
        {
            // Face the main camera.
            transform.rotation = Camera.main.transform.rotation;
        }

        /// <summary>
        /// Initialize this text. Will play the animation.
        /// </summary>
        /// <param name="position">The spawn position.</param>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="pool">The pool this text belongs to.</param>
        public void Initialize(Vector3 position, string text, FloatingTextPool pool)
        {
            SetPosition(position);
            SetText(text);
            SetPool(pool);
            PlayAnimation();
        }

        /// <summary>
        /// Set the position of the text.
        /// </summary>
        /// <param name="position">The target position.</param>
        private void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Set the text of the <see cref="text"/> component.
        /// </summary>
        /// <param name="text">The target text.</param>
        private void SetText(string text)
        {
            this.text.SetText(text);
        }

        /// <summary>
        /// Plays the animation and returns to its <see cref="pool"/>.
        /// </summary>
        private void PlayAnimation()
        {
            // Set the initial state - 0 alpha and scale.
            text.alpha = 0f;
            text.transform.localScale = Vector3.zero;

            // Fade to full alpha in 10% of the animation duration.
            Tween.Alpha(text, 1f, animationDuration * 0.1f);

            // Scale to 1 (normal scale) in 10% of the animation duration.
            Tween.Scale(text.transform, 1f, animationDuration * 0.1f, scaleEase);

            // Generate offset.
            float xOffset = Random.Range(-positionHorizontalVariance, positionHorizontalVariance);
            float yOffset = Random.Range(-positionVerticalVariance, positionVerticalVariance);
            Vector3 offset = new(xOffset, yOffset, 0);

            // Calculate target position and offset.
            Vector3 targetPosition = transform.position + (targetPositionOffset + offset);
            
            // Animate to position in 100% of the animation duration.
            Tween.Position(transform, targetPosition, animationDuration, positionEase);

            // Wait until 80% of the animation duration.
            Tween.Delay(animationDuration * 0.8f, () =>
            {
                // Fade out the text for the remaining 20% of the animation duration
                // and return the text to its pool.
                Tween.Alpha(text, 0f, animationDuration * 0.2f).OnComplete(() =>
                {
                    ReturnToPool();
                });
            });
        }

        /// <summary>
        /// Set the pool reference. To be used when the animation finishes and the text returns to its pool.
        /// </summary>
        /// <param name="pool">The pool.</param>
        private void SetPool(FloatingTextPool pool)
        {
            this.pool = pool;
        }

        /// <summary>
        /// Returns this text to its pool if there is one.
        /// </summary>
        private void ReturnToPool()
        {
            if (pool == null)
            {
                Debug.LogError($"This floating text has no pool!");
                return;
            }

            pool.Release(this);
        }
    }
}