using System;

namespace OsuQqBot.Charts
{
    internal class ScoreStatisticsField : StringField
    {
        public override bool Update(string arg)
        {
            try
            {
                Statistics.ParseScorer(arg);
            }
            catch (Exception)
            {
                return false;
            }
            return base.Update(arg);
        }
    }
}
