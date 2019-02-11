namespace NFabric.Core.Extensions
{
    using NFabric.Core.Attributes;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> to facilitate binding of named instances.
    /// A type which should be able to resolve named instance
    /// should put in its constructor a parameter of type <c>Func&lt;string, T&gt;</c>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> under the name and lifetime defined in tuple and factory method <c>Func&lt;TService, TName&gt;</c> (registered in lifecycle defined in factoryLifeTime argument).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="TService">Service interface or abstract class.</typeparam>
        /// <typeparam name="TName">Name type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="namedTypes">Tuples of types to be registered; it contains name, service implementation type and lifetime. All types must implement TService.</param>
        /// <param name="factoryLifeTime">Factory method lifecycle.</param>
        /// <param name="defaultName">When set it will try to fallback to this name instead of the originaly requested one.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection Add<TService, TName>(
            this IServiceCollection services,
            IEnumerable<(TName name, Type type, ServiceLifetime lifetime)> namedTypes,
            ServiceLifetime factoryLifeTime = ServiceLifetime.Singleton,
            TName? defaultName = null)
            where TService : class
            where TName : struct
        {
            CheckValidTypes<TService>(namedTypes.Select(t => t.type));

            foreach (var (name, type, lifetime) in namedTypes)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }

            Func<TName, TService> ImplementationFactory(IServiceProvider provider) => (TName name) =>
            {
                 var item = namedTypes.FirstOrDefault(t => t.name.Equals(name));
                 if (item.type != null)
                 {
                     var service = provider.GetService(item.type);
                     return service as TService;
                 }

                 if (defaultName.HasValue)
                 {
                     item = namedTypes.FirstOrDefault(t => t.name.Equals(defaultName.Value));
                     if (item.type != null)
                     {
                         var service = provider.GetService(item.type);
                         return service as TService;
                     }
                 }

                 return default(TService);
            };

            return factoryLifeTime == ServiceLifetime.Singleton
                ? services.AddSingleton(ImplementationFactory)
                : (factoryLifeTime == ServiceLifetime.Scoped
                    ? services.AddScoped(ImplementationFactory)
                    : services.AddTransient(ImplementationFactory));
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> under the name and lifetime defined in tuple and factory method <c>Func&lt;string, T&gt;</c> (registered in lifecycle defined in factoryLifeTime argument).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Service interface or abstract class.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="namedTypes">Tuples of types to be registered; it contains name, service implementation type and lifetime. All types must implement T.</param>
        /// <param name="factoryLifeTime">Factory method lifecycle.</param>
        /// <param name="defaultName">When set it will try to fallback to this name instead of the originaly requested one.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection Add<T>(
            this IServiceCollection services,
            IEnumerable<(string name, Type type, ServiceLifetime lifetime)> namedTypes,
            ServiceLifetime factoryLifeTime = ServiceLifetime.Singleton,
            string defaultName = null)
            where T : class
        {
            CheckValidTypes<T>(namedTypes.Select(t => t.type));

            foreach (var (name, type, lifetime) in namedTypes)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }

            Func<string, T> ImplementationFactory(IServiceProvider provider) => (string name) =>
            {
                var item = namedTypes.FirstOrDefault(t => t.name.Equals(name, StringComparison.InvariantCulture));
                if (item.type != null)
                {
                    var service = provider.GetService(item.type);
                    return service as T;
                }

                if (!string.IsNullOrWhiteSpace(defaultName))
                {
                    item = namedTypes.FirstOrDefault(t => t.name.Equals(defaultName, StringComparison.InvariantCulture));
                    if (item.type != null)
                    {
                        var service = provider.GetService(item.type);
                        return service as T;
                    }
                }

                return default(T);
            };

            return factoryLifeTime == ServiceLifetime.Singleton
                ? services.AddSingleton(ImplementationFactory)
                : (factoryLifeTime == ServiceLifetime.Scoped
                    ? services.AddScoped(ImplementationFactory)
                    : services.AddTransient(ImplementationFactory));
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> under the name and lifetime defined in tuple and factory method <c>Func&lt;string, T&gt;</c> (registered in lifecycle defined in factoryLifeTime argument).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Service interface or abstract class.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="factoryLifeTime">Factory method lifecycle.</param>
        /// <param name="namedTypes">Tuples of types to be registered; it contains name, service implementation type and lifetime. All types must implement T.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection Add<T>(this IServiceCollection services, ServiceLifetime factoryLifeTime, params (string name, Type type, ServiceLifetime lifetime)[] namedTypes)
            where T : class
        {
            return Add<T>(services, namedTypes, factoryLifeTime, null);
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> under the name and lifetime defined in tuple and factory method <c>Func&lt;TService, TName&gt;</c> (registered in lifecycle defined in factoryLifeTime argument).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="TService">Service interface or abstract class.</typeparam>
        /// <typeparam name="TName">Name type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="factoryLifeTime">Factory method lifecycle.</param>
        /// <param name="namedTypes">Tuples of types to be registered; it contains name, service implementation type and lifetime. All types must implement TService.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection Add<TService, TName>(this IServiceCollection services, ServiceLifetime factoryLifeTime, params (TName, Type, ServiceLifetime lifetime)[] namedTypes)
            where TService : class
            where TName : struct
        {
            return Add<TService, TName>(services, namedTypes, factoryLifeTime, default(TName?));
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> under the name and lifetime defined in tuple and factory method <c>Func&lt;string, T&gt;</c> (registered in lifecycle defined in factoryLifeTime argument).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Service interface or abstract class.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="factoryLifeTime">Factory method lifecycle.</param>
        /// <param name="types">Tuples of types to be registered; it contains name, service implementation type and lifetime. All types must implement T.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection Add<T>(this IServiceCollection services, ServiceLifetime factoryLifeTime, params (Type type, ServiceLifetime lifetime)[] types)
            where T : class
        {
            var tuples = types.Select(t => (TypeName(t.type), t.type, t.lifetime));
            return Add<T>(services, tuples, factoryLifeTime);
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as transient under the name of its key from the dictionary and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Dictionary of types to be registered; keys of the dictionary will be used as service names. All types must implement T.</param>
        /// <param name="defaultName">When set it will try to fallback to this name instead of the originaly requested one.</param>
        public static IServiceCollection AddTransients<T>(this IServiceCollection services, IDictionary<string, Type> namedTypes, string defaultName = null)
            where T : class
        {
            CheckValidTypes<T>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddTransient(type);
            }

            return services.AddSingleton<Func<string, T>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                if (!string.IsNullOrEmpty(defaultName) && namedTypes.TryGetValue(defaultName, out type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                return default(T);
            });
        }

        public static IServiceCollection AddTransients<TService, TName>(this IServiceCollection services, IDictionary<TName, Type> namedTypes, TName? defaultName = null)
            where TService : class
            where TName : struct
        {
            CheckValidTypes<TService>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddTransient(type);
            }

            return services.AddSingleton<Func<TName, TService>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                if (defaultName.HasValue && namedTypes.TryGetValue(defaultName.Value, out type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                return default(TService);
            });
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as transient and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Array of tuples with types to be registered; first item in tuple will be used as service name. All types must implement T.</param>
        public static IServiceCollection AddTransients<T>(this IServiceCollection services, params (string, Type)[] namedTypes)
            where T : class
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddTransients<T>(services, dict);
        }

        public static IServiceCollection AddTransients<TService, TName>(this IServiceCollection services, params (TName, Type)[] namedTypes)
            where TService : class
            where TName : struct
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddTransients<TService, TName>(services, dict);
        }

        /// <summary>
        /// Registers types from assembly which do implement T as transient and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="assembly">The assembly to search for types.  The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        /// <param name="namespaceStartsWith">Limits search for types to this namespace root.</param>
        public static IServiceCollection AddTransients<T>(this IServiceCollection services, Assembly assembly, string namespaceStartsWith = null)
            where T : class
        {
            var types = assembly.FindImplementationsOf(typeof(T), namespaceStartsWith);
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddTransients<T>(services, dict);
        }

        /// <summary>
        /// Registers types from parameter <c>types</c> as transient and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="types">Array of concrete types which must implement T. The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        public static IServiceCollection AddTransients<T>(this IServiceCollection services, params Type[] types)
            where T : class
        {
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddTransients<T>(services, dict);
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as singletons under the name of its key from the dictionary and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Dictionary of types to be registered; keys of the dictionary will be used as service names. All types must implement T.</param>
        /// <param name="defaultName">When set it will try to fallback to this name instead of the originaly requested one.</param>
        public static IServiceCollection AddSingletons<T>(this IServiceCollection services, IDictionary<string, Type> namedTypes, string defaultName = null)
            where T : class
        {
            CheckValidTypes<T>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddSingleton(type);
            }

            return services.AddSingleton<Func<string, T>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                if (!string.IsNullOrEmpty(defaultName) && namedTypes.TryGetValue(defaultName, out type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                return default(T);
            });
        }

        public static IServiceCollection AddSingletons<TService, TName>(this IServiceCollection services, IDictionary<TName, Type> namedTypes, TName? defaultName = null)
            where TService : class
            where TName : struct
        {
            CheckValidTypes<TService>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddSingleton(type);
            }

            return services.AddSingleton<Func<TName, TService>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                if (defaultName.HasValue && namedTypes.TryGetValue(defaultName.Value, out type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                return default(TService);
            });
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as singletons and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Array of tuples with types to be registered; first item in tuple will be used as service name. All types must implement T.</param>
        public static IServiceCollection AddSingletons<T>(this IServiceCollection services, params (string, Type)[] namedTypes)
            where T : class
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddSingletons<T>(services, dict);
        }

        public static IServiceCollection AddSingletons<TService, TName>(this IServiceCollection services, params (TName, Type)[] namedTypes)
            where TService : class
            where TName : struct
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddSingletons<TService, TName>(services, dict);
        }

        /// <summary>
        /// Registers types from assembly which do implement T as singletons and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="assembly">The assembly to search for types.  The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        /// <param name="namespaceStartsWith">Limits search for types to this namespace root.</param>
        public static IServiceCollection AddSingletons<T>(this IServiceCollection services, Assembly assembly, string namespaceStartsWith = null)
            where T : class
        {
            var types = assembly.FindImplementationsOf(typeof(T), namespaceStartsWith);
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddSingletons<T>(services, dict);
        }

        /// <summary>
        /// Registers types from parameter <c>types</c> as singletons and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="types">Array of concrete types which must implement T. The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        public static IServiceCollection AddSingletons<T>(this IServiceCollection services, params Type[] types)
            where T : class
        {
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddSingletons<T>(services, dict);
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as scoped under the name of its key from the dictionary and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Dictionary of types to be registered; keys of the dictionary will be used as service names.</param>
        /// <param name="defaultName">When set it will try to fallback to this name instead of the originaly requested one.</param>
        public static IServiceCollection AddScoped<T>(this IServiceCollection services, IDictionary<string, Type> namedTypes, string defaultName = null)
            where T : class
        {
            CheckValidTypes<T>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddScoped(type);
            }

            return services.AddSingleton<Func<string, T>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                if (!string.IsNullOrEmpty(defaultName) && namedTypes.TryGetValue(defaultName, out type))
                {
                    var service = provider.GetService(type);
                    return service as T;
                }

                return default(T);
            });
        }

        public static IServiceCollection AddScoped<TService, TName>(this IServiceCollection services, IDictionary<TName, Type> namedTypes, TName? defaultName = null)
            where TService : class
            where TName : struct
        {
            CheckValidTypes<TService>(namedTypes.Values);

            foreach (var type in namedTypes.Values)
            {
                services.AddScoped(type);
            }

            return services.AddSingleton<Func<TName, TService>>(provider => name =>
            {
                if (namedTypes.TryGetValue(name, out Type type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                if (defaultName.HasValue && namedTypes.TryGetValue(defaultName.Value, out type))
                {
                    var service = provider.GetService(type);
                    return service as TService;
                }

                return default(TService);
            });
        }

        /// <summary>
        /// Registers types from parameter <c>namedTypes</c> as scoped and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="namedTypes">Array of tuples with types to be registered; first item in tuple will be used as service name. All types must implement T.</param>
        public static IServiceCollection AddScoped<T>(this IServiceCollection services, params (string, Type)[] namedTypes)
            where T : class
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddScoped<T>(services, dict);
        }

        public static IServiceCollection AddScoped<TService, TName>(this IServiceCollection services, params (TName, Type)[] namedTypes)
            where TService : class
            where TName : struct
        {
            var dict = namedTypes.ToDictionary(t => t.Item1, t => t.Item2);
            return AddScoped<TService, TName>(services, dict);
        }

        /// <summary>
        /// Registers types from assembly which do implement T as scoped and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="assembly">The assembly to search for types.  The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        /// <param name="namespaceStartsWith">Limits search for types to this namespace root.</param>
        public static IServiceCollection AddScoped<T>(this IServiceCollection services, Assembly assembly, string namespaceStartsWith = null)
            where T : class
        {
            var types = assembly.FindImplementationsOf(typeof(T), namespaceStartsWith);
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddScoped<T>(services, dict);
        }

        /// <summary>
        /// Registers types from parameter <c>types</c> as scoped and factory method <c>Func&lt;string, T&gt;</c> (as singleton).
        /// This factory method should be a constructor parameter of types which wish to consume one of the named services.
        /// It will return <c>null</c> if the resolving of the instance fails; for example if you pass a name which has not been registered.
        /// </summary>
        /// <typeparam name="T">Interface or abstract class</typeparam>
        /// <param name="types">Array of concrete types which must implement T. The names will be taken from <see cref="ServiceNameAttribute"/> or Type.FullName.</param>
        public static IServiceCollection AddScoped<T>(this IServiceCollection services, params Type[] types)
            where T : class
        {
            var dict = types.ToDictionary(t => TypeName(t), t => t);
            return AddScoped<T>(services, dict);
        }

        private static string TypeName(Type type)
        {
            var attr = type.GetCustomAttribute<ServiceNameAttribute>();
            return attr != null
                    ? attr.Name
                    : type.FullName;
        }

        private static void CheckValidTypes<T>(IEnumerable<Type> types)
            where T : class
        {
            var notImplements = types.Where(t => !t.Implements(typeof(T))).Select(t => t.Name).ToArray();
            if (notImplements.Length > 0)
            {
                var names = string.Join(", ", notImplements);
                throw new ArgumentException($"Types {names} does not implement {typeof(T).Name} and therefore cannot be registered.");
            }
        }
    }
}
