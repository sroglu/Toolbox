using System;
using System.Collections.Generic;

namespace Sroglu.Toolbox.Pooling
{
    /// <summary>
    /// A generic, allocation-free pool for reusable reference-type instances.
    /// Items are recycled through an internal stack; taking an item pops (or
    /// creates) one, and releasing it pushes it back for later reuse.
    /// </summary>
    /// <typeparam name="T">Reference type held by the pool.</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _inactive;
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        private int _activeCount;

        /// <summary>
        /// Creates a pool.
        /// </summary>
        /// <param name="factory">Creates a fresh instance when the pool is empty. Required.</param>
        /// <param name="onGet">Optional hook invoked on each instance as it is taken.</param>
        /// <param name="onRelease">Optional hook invoked on each instance as it is returned.</param>
        /// <param name="prewarm">Number of instances to create up front into the inactive stack.</param>
        /// <param name="maxSize">Maximum inactive instances to retain; 0 means unbounded.</param>
        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null, int prewarm = 0, int maxSize = 0)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factory = factory;
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
            _inactive = new Stack<T>(prewarm > 0 ? prewarm : 0);

            for (int i = 0; i < prewarm; i++)
                _inactive.Push(_factory());
        }

        /// <summary>Number of instances currently sitting idle in the pool.</summary>
        public int CountInactive => _inactive.Count;

        /// <summary>Number of instances currently taken and not yet released.</summary>
        public int CountActive => _activeCount;

        /// <summary>Total number of instances the pool is aware of (active plus inactive).</summary>
        public int CountAll => _activeCount + _inactive.Count;

        /// <summary>
        /// Takes an instance from the pool, reusing an idle one when available or
        /// creating a new one via the factory. Runs the get hook before returning.
        /// </summary>
        public T Get()
        {
            T item = _inactive.Count > 0 ? _inactive.Pop() : _factory();
            _onGet?.Invoke(item);
            _activeCount++;
            return item;
        }

        /// <summary>
        /// Returns an instance to the pool. Runs the release hook, then retains the
        /// instance for reuse unless the inactive stack is already at the maximum
        /// size, in which case the instance is dropped for the garbage collector.
        /// </summary>
        public void Release(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _onRelease?.Invoke(item);

            if (_maxSize <= 0 || _inactive.Count < _maxSize)
                _inactive.Push(item);

            _activeCount--;
        }

        /// <summary>
        /// Drops every idle instance and resets tracked counts. Instances that are
        /// currently taken are unaffected and can still be released later.
        /// </summary>
        public void Clear()
        {
            _inactive.Clear();
            _activeCount = 0;
        }
    }
}
