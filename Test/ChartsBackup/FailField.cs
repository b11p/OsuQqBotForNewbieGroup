namespace OsuQqBot.Charts
{
    internal class FailField : ChartInfoField<bool>
    {
        public override bool Update(string arg)
        {
            if (!string.IsNullOrWhiteSpace(arg)) return false;
            Value = true;
            return true;
        }
    }
}
