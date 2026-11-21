using System;
using System.Collections.Generic;

namespace Events
{
    /// <summary>
    /// A global event bus for communication between game systems.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> subscribers = new();

        /// <summary>
        /// Adds a listener to an event type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="listener">The callback to add.</param>
        public static void Subscribe<T>(Action<T> listener) where T : GameEvent
        {
            // Get the event type.
            Type eventType = typeof(T);

            // If there is already a subscriber to this event type,
            // combine the existing one with the one we're trying to add and
            // update the dictionary entry.
            if (subscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                subscribers[eventType] = Delegate.Combine(existingDelegate, listener);
            }
            // Otherwise, just assosciate the event type with this listener.
            else
            {
                subscribers[eventType] = listener;
            }
        }

        /// <summary>
        /// Removes a listener from an event type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        public static void Unsubscribe<T>(Action<T> listener) where T : GameEvent
        {
            // Get the event type.
            Type eventType = typeof(T);

            // If there is no listener assosciated with this event type, do nothing.
            if (!subscribers.TryGetValue(eventType, out Delegate existingDelegate))
                return;

            // Remove the listener from the existing delegate.
            Delegate newDelegate = Delegate.Remove(existingDelegate, listener);

            // If after the removal, there are no other listeners, remove the
            // event type from the subscribers dictionary all together.
            if (newDelegate == null)
                subscribers.Remove(eventType);
            // Otherwise, update the dictionary for this event type.
            else
                subscribers[eventType] = newDelegate;
        }

        /// <summary>
        /// Fire an event for all listeners.
        /// </summary>
        /// <typeparam name="T">The event type to fire.</typeparam>
        /// <param name="gameEvent">The event argument.</param>
        public static void Publish<T>(T gameEvent) where T : GameEvent
        {
            // Get the event type.
            Type eventType = typeof(T);

            // Find the listeners for this event type.
            if (subscribers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                // Invoke the callback if it matches the event type.
                if (existingDelegate is Action<T> callback)
                {
                    callback.Invoke(gameEvent);
                }
            }
        }

        /// <summary>
        /// Clear all subscribers from all event types.
        /// </summary>
        public static void Clear()
        {
            subscribers.Clear();
        }
    }
}