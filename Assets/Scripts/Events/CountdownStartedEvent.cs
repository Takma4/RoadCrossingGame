namespace Events
{
    /// <summary>
    /// This event is invoked when the countdown starts before the beginning of the game.
    /// </summary>
    public record CountdownStartedEvent : GameEvent
    {
        public float Seconds { get; }

        public CountdownStartedEvent(float seconds)
        {
            Seconds = seconds;
        }
    }
}
