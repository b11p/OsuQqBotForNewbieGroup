using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant
{
    public class ReplicaConfig
    {
        public bool DisablePrivate { get; set; }

        public static ReplicaConfig Parse(string accessToken)
        {
            var result = new ReplicaConfig();
            var index = accessToken.IndexOf(':');
            if (index == -1)
                return result;
            string[] kvps = accessToken[(index + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var item in kvps)
            {
                var iEqual = item.IndexOf('=');
                var (name, value) = iEqual == -1 ? (item, string.Empty) : (item[..iEqual], item[(iEqual + 1)..]);
                var property = typeof(ReplicaConfig).GetProperty(name);
                if (property is null)
                    continue;
                if (property.PropertyType == typeof(bool))
                {
                    if (string.IsNullOrWhiteSpace(value))
                        property.SetValue(result, true);
                    try
                    {
                        property.SetValue(result, Convert.ToBoolean(value));
                    }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                    catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                    {// ignored
                    }
                }
                try
                {
                    property.SetValue(result, Convert.ChangeType(value, property.PropertyType));
                }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                {// ignored
                }
            }
            return result;
        }
    }
}
