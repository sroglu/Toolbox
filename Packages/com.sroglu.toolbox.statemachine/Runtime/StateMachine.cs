using System;
using System.Collections.Generic;

namespace Sroglu.Toolbox.StateMachines
{
    /// <summary>
    /// A single state in a <see cref="StateMachine"/>. The machine calls <see cref="Enter"/>
    /// once when the state becomes current, <see cref="Update"/> on every tick while it stays
    /// current, and <see cref="Exit"/> once when the machine leaves it.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when this state becomes the current state.</summary>
        void Enter();

        /// <summary>Called each time the owning machine is updated while this state is current.</summary>
        void Update();

        /// <summary>Called once when the owning machine transitions away from this state.</summary>
        void Exit();
    }

    /// <summary>
    /// Minimal finite-state machine over <see cref="IState"/> instances. Transitions run
    /// <c>Exit</c> on the outgoing state before <c>Enter</c> on the incoming one and raise
    /// <see cref="StateChanged"/>. Not thread-safe; drive it from a single thread.
    /// </summary>
    public class StateMachine
    {
        /// <summary>The state that is currently active, or <c>null</c> before the first transition.</summary>
        public IState Current { get; private set; }

        /// <summary>Raised after a transition completes, with (previous, next). Previous may be <c>null</c>.</summary>
        public event Action<IState, IState> StateChanged;

        /// <summary>
        /// Transitions to <paramref name="next"/>. Does nothing if it is already the current
        /// state. Runs <c>Exit</c> on the outgoing state, then <c>Enter</c> on the incoming one.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="next"/> is <c>null</c>.</exception>
        public void ChangeState(IState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (ReferenceEquals(next, Current)) return;

            var previous = Current;
            previous?.Exit();
            Current = next;
            Current.Enter();
            StateChanged?.Invoke(previous, next);
        }

        /// <summary>Ticks the current state, if any.</summary>
        public void Update() => Current?.Update();
    }

    /// <summary>
    /// A <see cref="StateMachine"/> whose states are registered under keys of type
    /// <typeparamref name="TId"/>, so transitions can be requested by id.
    /// </summary>
    /// <typeparam name="TId">The state key type (e.g. an enum or string).</typeparam>
    public class StateMachine<TId>
    {
        private readonly Dictionary<TId, IState> _states = new Dictionary<TId, IState>();
        private readonly StateMachine _inner = new StateMachine();
        private TId _currentId;

        /// <summary>Creates a keyed state machine and re-raises the inner machine's transitions.</summary>
        public StateMachine()
        {
            _inner.StateChanged += (previous, next) => StateChanged?.Invoke(_previousId, _currentId);
        }

        private TId _previousId;

        /// <summary>Raised after a transition completes, with (previousId, nextId).</summary>
        public event Action<TId, TId> StateChanged;

        /// <summary>The id of the current state, or <c>default</c> before the first transition.</summary>
        public TId CurrentId => _currentId;

        /// <summary>The current state instance, or <c>null</c> before the first transition.</summary>
        public IState Current => _inner.Current;

        /// <summary>Registers <paramref name="state"/> under <paramref name="id"/>.</summary>
        /// <exception cref="ArgumentNullException">If <paramref name="state"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is already registered.</exception>
        public void AddState(TId id, IState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_states.ContainsKey(id))
                throw new ArgumentException($"A state is already registered for id '{id}'.", nameof(id));
            _states.Add(id, state);
        }

        /// <summary>Transitions to the state registered under <paramref name="id"/>.</summary>
        /// <exception cref="KeyNotFoundException">If no state is registered for <paramref name="id"/>.</exception>
        public void ChangeState(TId id)
        {
            if (!_states.TryGetValue(id, out var state))
                throw new KeyNotFoundException($"No state is registered for id '{id}'.");

            _previousId = _currentId;
            _currentId = id;
            _inner.ChangeState(state);
        }

        /// <summary>Ticks the current state, if any.</summary>
        public void Update() => _inner.Update();
    }
}
