﻿using System;

namespace Bleatingsheep.NewHydrant.Attributions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute
    {
        public ComponentAttribute(string name) => Name = name;

        public string Name { get; }
    }
}
