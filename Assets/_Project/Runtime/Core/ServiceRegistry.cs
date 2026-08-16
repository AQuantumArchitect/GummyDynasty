using System;
using System.Collections.Generic;

namespace GummyDynasty.Core
{
    /// <summary>Tiny composition root. Register at boot, resolve in systems. No hidden singletons in domain types.</summary>
    public sealed class ServiceRegistry
    {
        public static ServiceRegistry Current { get; private set; }

        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Install(ServiceRegistry registry) => Current = registry;

        public void Register<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(T)] = instance;
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var value))
                return (T)value;
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var value))
            {
                service = (T)value;
                return true;
            }

            service = null;
            return false;
        }

        public void Clear() => _services.Clear();
    }
}
