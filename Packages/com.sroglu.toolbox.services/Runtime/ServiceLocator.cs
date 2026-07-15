using System;
using System.Collections.Generic;

namespace Sroglu.Toolbox.Services
{
    /// <summary>
    /// A simple type-keyed registry of shared service instances. Each service is
    /// stored and looked up by its registration type <c>T</c>, backed by a
    /// <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public class ServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers <paramref name="instance"/> under the type <typeparamref name="T"/>.
        /// If a service is already registered for that type it is replaced.
        /// </summary>
        /// <typeparam name="T">Key type the service is looked up by.</typeparam>
        /// <param name="instance">The service instance. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public void Register<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _services[typeof(T)] = instance;
        }

        /// <summary>
        /// Removes the service registered under <typeparamref name="T"/>, if any.
        /// Unregistering an unknown type is a no-op.
        /// </summary>
        /// <typeparam name="T">Key type to remove.</typeparam>
        public void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Returns the service registered under <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Key type to resolve.</typeparam>
        /// <exception cref="KeyNotFoundException">Thrown when no service is registered for the type.</exception>
        public T Resolve<T>()
        {
            if (_services.TryGetValue(typeof(T), out object service))
                return (T)service;

            throw new KeyNotFoundException($"No service registered for type {typeof(T)}.");
        }

        /// <summary>
        /// Attempts to fetch the service registered under <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Key type to resolve.</typeparam>
        /// <param name="service">The resolved service, or the type default when absent.</param>
        /// <returns>True when a service was found; otherwise false.</returns>
        public bool TryResolve<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out object found))
            {
                service = (T)found;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Returns true when a service is registered under <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Key type to check.</typeparam>
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>Removes every registered service.</summary>
        public void Clear()
        {
            _services.Clear();
        }
    }
}
