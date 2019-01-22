using System;

namespace Bleatingsheep.NewHydrant.Attributions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public FunctionAttribute(string name) => Name = name;

        public string Name { get; }
    }
}
