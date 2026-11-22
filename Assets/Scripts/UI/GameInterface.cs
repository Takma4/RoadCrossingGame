using Events;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Handles the UI for the main game loop.
    /// </summary>
    public class GameInterface : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI countdownTimerText;

        private void Awake()
        {
            EventBus.Subscribe<CountdownStartedEvent>(OnCountdownStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CountdownStartedEvent>(OnCountdownStarted);
        }

        /// <summary>
        /// Callback for when the "start game countdown" starts.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnCountdownStarted(CountdownStartedEvent e)
        {
            // Retrieve the number of seconds before the game starts.
            float seconds = e.Seconds;

            // Set the UI text to the number of seconds before the game starts.
            countdownTimerText.text = $"{seconds}";

            // Play a bounce animation on the text.
            PlayCountdownBounceAnimation();

            // Countdown for the next second.
            PlayCountdownAnimation(seconds - 1f);
        }

        /// <summary>
        /// Waits one second and:
        /// 
        /// <list type="number">
        /// <item>Updates the <see cref="countdownTimerText"/></item>
        /// <item>Plays a bounce animation (see <seealso cref="PlayCountdownBounceAnimation"/>)</item>
        /// <item>Calls <see cref="PlayCountdownAnimation(float)"/> again but passes <paramref name="seconds"/> - 1 as an argument.</item>
        /// </list>
        /// 
        /// When <paramref name="seconds"/> goes below 0, it breaks out of this "loop".
        /// 
        /// </summary>
        /// <param name="seconds">The amount of seconds remaining on the countdown timer.</param>
        private void PlayCountdownAnimation(float seconds)
        {
            // If the amount of seconds drops below 0 then that means that the countdown is over.
            // So, we break out of the "recursive" call.
            if (seconds < 0f)
            {
                // Disable the text.
                countdownTimerText.gameObject.SetActive(false);
                return;
            }
            
            // Wait one second then update the text, play a bounce animation and wait for the next second.
            Tween.Delay(1f, () =>
            {
                countdownTimerText.text = $"{seconds}";
                PlayCountdownBounceAnimation();
                PlayCountdownAnimation(seconds - 1f);
            });
        }

        /// <summary>
        /// Plays a bounce animation on the <see cref="countdownTimerText"/>. The total duration of the animation is 0.7 seconds.
        /// </summary>
        private void PlayCountdownBounceAnimation()
        {
            // Set the inital scale of the text to 0.
            countdownTimerText.transform.localScale = Vector3.zero;

            // Scale at 1.2 in 0.5 seconds and immediately scale down to 1 in 0.2 seconds.
            Tween.Scale(countdownTimerText.transform, 1.2f, 0.5f, Ease.InOutBounce).Chain(
                Tween.Scale(countdownTimerText.transform, 1f, 0.2f, Ease.Linear));
        }
    }
}