using System;
using System.Collections.Generic;
using System.Text;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace OsuQqBot.Data
{
    class EFData : IBindings
    {
        public int? Bind(long qq, int osuId, string osuName, string source, long operatorId, string operatorName)
        {
            throw new NotImplementedException();
        }

        public BindingInfo GetBindingInfo(long qq) => throw new NotImplementedException();
        public int? UserIdOf(long qq) => throw new NotImplementedException();
    }
}
