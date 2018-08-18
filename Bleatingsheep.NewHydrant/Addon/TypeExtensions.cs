using System;

namespace Bleatingsheep.NewHydrant.Addon
{
    internal static class TypeExtensions
    {
        public static object CreateInstance(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.Assembly.CreateInstance(type.FullName);
        }

        /// <exception cref="InvalidCastException"></exception>
        public static T CreateInstance<T>(this Type type) => (T)type.CreateInstance();
    }
}
