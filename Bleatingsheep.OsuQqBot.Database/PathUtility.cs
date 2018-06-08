using System;
using System.Collections.Generic;
using System.Text;

namespace Bleatingsheep.OsuQqBot.Database
{
    internal class PathUtility
    {
        public static string BasePath
        {
            get
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (string.IsNullOrEmpty(desktop)) desktop = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return desktop;
            }
        }
    }
}
