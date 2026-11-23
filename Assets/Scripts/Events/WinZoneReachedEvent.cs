namespace Events
{
    /// <summary>
    /// Event to be invoked when the player reaches the end of the level.
    /// </summary>
    public record WinZoneReachedEvent : GameEvent
    {
        public WinZoneReachedEvent()
        {

        }
    }
}