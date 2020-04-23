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
            newbieContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            using var newbieContext2 = new NewbieContext();
            newbieContext2.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var strategy1 = newbieContext.Database.CreateExecutionStrategy();
            strategy1.Execute(() =>
            {
                using var transaction1 = newbieContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                using var transaction2 = newbieContext2.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                newbieContext.PlayRecordQueryTemps.Add(new PlayRecordQueryTemp
                {
                    UserId = 10,
                    Mode = Bleatingsheep.Osu.Mode.Standard,
                    StartNumber = 30,
                });
                newbieContext2.PlayRecordQueryTemps.Add(new PlayRecordQueryTemp
                {
                    UserId = 10,
                    Mode = Bleatingsheep.Osu.Mode.Taiko,
                    StartNumber = 30,
                });
                newbieContext2.SaveChanges();
                newbieContext.PlayRecordQueryTemps.Add(new PlayRecordQueryTemp
                {
                    UserId = 10,
                    Mode = Bleatingsheep.Osu.Mode.Taiko,
                    StartNumber = 30,
                });
                newbieContext.SaveChanges(); // Deadlock. Throws after 5 mins.
                Console.WriteLine(newbieContext.PlayRecordQueryTemps.Where(r => r.UserId == 10).Count());
                Console.WriteLine(newbieContext2.PlayRecordQueryTemps.Where(r => r.UserId == 10).Count());
                Task.Run(() =>
                {
                    Task.Delay(10_000).Wait();
                    transaction1.Dispose();
                });
                newbieContext2.PlayRecordQueryTemps.Add(new PlayRecordQueryTemp
                {
                    UserId = 10,
                    Mode = Bleatingsheep.Osu.Mode.Standard,
                    StartNumber = 30,
                });
                newbieContext2.SaveChanges();
                Console.WriteLine(newbieContext2.PlayRecordQueryTemps.Where(r => r.UserId == 10).Count());
            });
        }
    }
}
