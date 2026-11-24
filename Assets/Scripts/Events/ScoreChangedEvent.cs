namespace Events
{
    /// <summary>
    /// Event to be invoked when the game score changes.
    /// </summary>
    public record ScoreChangedEvent : GameEvent
    {
        /// <summary>
        /// The current score.
        /// </summary>
        public int Score { get; }

        public ScoreChangedEvent(int score)
        {
            Score = score;
        }
    }
}