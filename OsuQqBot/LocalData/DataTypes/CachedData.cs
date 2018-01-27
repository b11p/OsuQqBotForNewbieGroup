using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData.DataTypes
{
    sealed class CachedData
    {
        public CachedData(string username)
        {
            LastUpdate = DateTime.UtcNow;
            Username = username;
        }
        //public CachedData(DateTime updateTime, string username)
        //{
        //    LastUpdate = updateTime;
        //    Username = username;
        //}
        public DateTime LastUpdate { get; set; }
        public string Username { get; set; }
    }
}
