using System;
using System.Collections.Generic;

namespace Sroglu.Toolbox.Events
{
    /// <summary>
    /// A lightweight, type-keyed publish/subscribe hub. Each event type keys its
    /// own set of handlers; publishing an instance invokes every handler registered
    /// for that exact type. Intended for single-threaded (main-thread) use.
    /// </summary>
    /// <remarks>
    /// Dispatch is re-entrancy safe: <see cref="Publish{T}"/> snapshots the current
    /// handler list before invoking, so a handler may subscribe or unsubscribe
    /// during dispatch without affecting the in-flight notification.
    /// </remarks>
    public class EventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Registers a handler for events of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Event type to listen for.</typeparam>
        /// <param name="handler">Callback invoked on each published event. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out Delegate existing))
                _handlers[key] = Delegate.Combine(existing, handler);
            else
                _handlers[key] = handler;
        }

        /// <summary>
        /// Removes a previously-registered handler for events of type
        /// <typeparamref name="T"/>. Unsubscribing an unknown handler is a no-op.
        /// </summary>
        /// <typeparam name="T">Event type the handler was registered for.</typeparam>
        /// <param name="handler">The handler to remove. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out Delegate existing))
                return;

            Delegate remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
                _handlers.Remove(key);
            else
                _handlers[key] = remaining;
        }

        /// <summary>
        /// Delivers <paramref name="evt"/> to every handler registered for type
        /// <typeparamref name="T"/>. Does nothing when no handler is registered.
        /// </summary>
        /// <typeparam name="T">Event type being published.</typeparam>
        /// <param name="evt">The event instance to deliver.</param>
        public void Publish<T>(T evt)
        {
            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out Delegate registered))
                return;

            // Snapshot so handlers may subscribe/unsubscribe mid-dispatch safely.
            Delegate[] invocationList = registered.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
                ((Action<T>)invocationList[i]).Invoke(evt);
        }

        /// <summary>Removes every handler for every event type.</summary>
        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
