using Managers;

namespace Events
{
    /// <summary>
    /// An event for when the <see cref="GameManager.GameState"/> changes.
    /// Provides the previous and the current state of the game.
    /// </summary>
    public record GameStateEvent : GameEvent
    {
        public GameManager.GameState PreviousState { get; }
        public GameManager.GameState CurrentState { get; }

        public GameStateEvent(GameManager.GameState previous, GameManager.GameState current)
        {
            PreviousState = previous;
            CurrentState = current;
        }
    }
}
