using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Tests.Database
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            using var newbieContext = new NewbieContext();
            // This works, w/o any Update() statements.
            //var first = newbieContext.UpdateSchedules.First();
            //first.ActiveIndex++;
            //newbieContext.SaveChanges();
            using var newbieContext2 = new NewbieContext();
            var strategy1 = newbieContext.Database.CreateExecutionStrategy();
            strategy1.Execute(() =>
            {
                using var transaction1 = newbieContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                using var transaction2 = newbieContext2.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                var first = newbieContext.UpdateSchedules.First();
                first.ActiveIndex += 10;
                var first2 = newbieContext2.UpdateSchedules.First();
                first2.ActiveIndex += 20;
                newbieContext2.SaveChanges();
                Task.Run(() =>
                {
                    Task.Delay(10_000).Wait();
                    transaction2.Commit();
                });
                newbieContext.SaveChanges(); // Throws?
            });
        }
    }
}
