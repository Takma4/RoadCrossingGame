using UnityEngine;

namespace Events
{
    public record TrafficCollisionEvent : GameEvent
    {
        /// <summary>
        /// The point of the collision.
        /// </summary>
        public Vector3 Position { get; }

        public TrafficCollisionEvent(Vector3 position)
        {
            Position = position;
        }
    }
}