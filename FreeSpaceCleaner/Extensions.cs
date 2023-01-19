namespace FreeSpaceCleaner
{
    public static class Extensions
    {
        public static string ToPercentage(this long value)
        {
            return (value / (1024 * 1024 * 1024)).ToString("0.00");
        }
    }
}