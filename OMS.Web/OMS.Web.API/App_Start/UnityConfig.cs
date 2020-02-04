using MediatR;
using OMS.Web.Adapter.Contracts;
using OMS.Web.Adapter.Topology;
using OMS.Web.API.Hubs;
using OMS.Web.Common;
using OMS.Web.Common.Exceptions;
using OMS.Web.Common.Loggers;
using OMS.Web.Common.Mappers;
using OMS.Web.Services.Commands;
using OMS.Web.Services.Queries;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace OMS.Web.API
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // We register our types here
            container.RegisterType<GraphHub>();
            container.RegisterType<ScadaHub>();
            container.RegisterType<ICustomExceptionHandler, TopologyException>();
            container.RegisterType<IGraphMapper, GraphMapper>();
            container.RegisterType<ILogger, FileLogger>(new ContainerControlledLifetimeManager());

            // We register our mediatr commands here (concrete, not abstract)
            container.RegisterMediator();
            container.RegisterMediatorHandlers(Assembly.GetAssembly(typeof(TurnOffSwitchCommand)));
            container.RegisterMediatorHandlers(Assembly.GetAssembly(typeof(TurnOnSwitchCommand)));
            container.RegisterMediatorHandlers(Assembly.GetAssembly(typeof(GetTopologyQuery)));
        }

    #region MediatR Extensions
    public static IUnityContainer RegisterMediator(this IUnityContainer container)
    {
        return container.RegisterType<IMediator, Mediator>(new HierarchicalLifetimeManager())
            .RegisterInstance<ServiceFactory>(type =>
            {
                var enumerableType = type
                    .GetInterfaces()
                    .Concat(new[] { type })
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return enumerableType != null
                    ? container.ResolveAll(enumerableType.GetGenericArguments()[0])
                    : container.IsRegistered(type)
                        ? container.Resolve(type)
                        : null;
            });
    }

    public static IUnityContainer RegisterMediatorHandlers(this IUnityContainer container, Assembly assembly)
    {
        return container.RegisterTypesImplementingType(assembly, typeof(IRequestHandler<,>))
                        .RegisterNamedTypesImplementingType(assembly, typeof(INotificationHandler<>));
    }

    internal static bool IsGenericTypeOf(this Type type, Type genericType)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == genericType;
    }

    internal static void AddGenericTypes(this List<object> list, IUnityContainer container, Type genericType)
    {
        var genericHandlerRegistrations =
            container.Registrations.Where(reg => reg.RegisteredType == genericType);

        foreach (var handlerRegistration in genericHandlerRegistrations)
        {
            if (list.All(item => item.GetType() != handlerRegistration.MappedToType))
            {
                list.Add(container.Resolve(handlerRegistration.MappedToType));
            }
        }
    }

    /// <summary>
    ///     Register all implementations of a given type for provided assembly.
    /// </summary>
    public static IUnityContainer RegisterTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
    {
        foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
        {
            var interfaces = implementation.GetInterfaces();
            foreach (var @interface in interfaces)
                container.RegisterType(@interface, implementation);
        }

        return container;
    }

    /// <summary>
    ///     Register all implementations of a given type for provided assembly.
    /// </summary>
    public static IUnityContainer RegisterNamedTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
    {
        foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
        {
            var interfaces = implementation.GetInterfaces();
            foreach (var @interface in interfaces)
                container.RegisterType(@interface, implementation, implementation.FullName);
        }

        return container;
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == currentType)
                return true;

            toCheck = toCheck.BaseType;
        }

        return false;
    }
    #endregion

}
}