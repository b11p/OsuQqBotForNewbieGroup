using System;

namespace Bleatingsheep.NewHydrant.Attributions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ParameterAttribute : Attribute
    {

        public ParameterAttribute(string groupName)
            => GroupName = groupName;

        public string GroupName { get; }
    }
}
