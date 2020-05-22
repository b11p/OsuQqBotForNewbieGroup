using System;
using Autofac;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    internal static class TypeExtensions
    {
        public static object CreateInstance(this Type type, IComponentContext container)
        {
            if (type is null || container is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return container.Resolve(type);
        }

        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"/>
        public static T CreateInstance<T>(this Type type, IComponentContext container) where T : notnull
            => (T)type.CreateInstance(container);
    }
#nullable restore
}
