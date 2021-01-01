using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    internal static class TypeExtensions
    {
        public static object CreateInstance(this Type type, IServiceScope scope)
        {
            if (type is null || scope is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return scope.ServiceProvider.GetService(type);
        }

        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentNullException"/>
        public static T CreateInstance<T>(this Type type, IServiceScope scope) where T : notnull
            => (T)type.CreateInstance(scope);
    }
#nullable restore
}
