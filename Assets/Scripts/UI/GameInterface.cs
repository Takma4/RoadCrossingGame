using Events;
using PrimeTween;
using TMPro;
using UnityEngine;
using Managers;

namespace UI
{
    /// <summary>
    /// Handles the UI for the main game loop.
    /// </summary>
    public class GameInterface : MonoBehaviour
    {
        [Header("Countdown")]
        [SerializeField] private TextMeshProUGUI countdownTimerText;

        [Header("Player Hit")]
        [SerializeField] private TextMeshProUGUI playerHitHeaderText;
        [SerializeField] private TextMeshProUGUI playerHitBodyText;

        private void Awake()
        {
            // Subscribe to events.
            EventBus.Subscribe<CountdownStartedEvent>(OnCountdownStarted);
            EventBus.Subscribe<GameStateEvent>(OnGameStateChanged);

            // Set the initial state of the text game objects.
            playerHitHeaderText.gameObject.SetActive(false);
            playerHitBodyText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to cleanup.
            EventBus.Unsubscribe<CountdownStartedEvent>(OnCountdownStarted);
            EventBus.Unsubscribe<GameStateEvent>(OnGameStateChanged);
        }

        /// <summary>
        /// Callback for when the game state changes.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnGameStateChanged(GameStateEvent e)
        {
            if (e.CurrentState == GameManager.GameState.PlayerHit) DisplayPlayerGotHitScreen();
        }

        /// <summary>
        /// Plays the UI animation when the player gets hit by a vehicle.
        /// </summary>
        private void DisplayPlayerGotHitScreen()
        {
            // Enable the text game objects.
            playerHitHeaderText.gameObject.SetActive(true);
            playerHitBodyText.gameObject.SetActive(true);

            // Set their properties - start large and transparent, we will tween back to a scale and alpha of 1.
            playerHitHeaderText.transform.localScale = new(1.5f, 1.5f, 1.5f);
            playerHitHeaderText.alpha = 0f;

            playerHitBodyText.transform.localScale = new(1.5f, 1.5f, 1.5f);
            playerHitBodyText.alpha = 0f;

            // Play the animation for the header text.
            // 0.3 duration for the scale.
            // 0.133 duration for the alpha.
            Tween.Scale(playerHitHeaderText.transform, 1f, 0.3f, Ease.InOutBounce);
            Tween.Alpha(playerHitHeaderText, 1f, 0.133f);

            // Wait 0.4 seconds before playing body animation.
            Tween.Delay(0.4f, () =>
            {
                // Play the animation for the body text.
                // 0.3 duration for the scale.
                // 0.133 duration for the alpha.
                Tween.Scale(playerHitBodyText.transform, 1f, 0.3f, Ease.InOutBounce);
                Tween.Alpha(playerHitBodyText, 1f, 0.133f);
            });
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