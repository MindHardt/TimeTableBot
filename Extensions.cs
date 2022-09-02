namespace TimeTableBot
{
    public static class Extensions
    {
        /// <summary>
        /// Formats <paramref name="time"/> as a discord timestamp, which dynamically changes according to current system time of a discord user.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToRelativeDiscordTime(this DateTimeOffset time) => $"<t:{time.ToUnixTimeSeconds()}:R>";

        /// <summary>
        /// Formats <paramref name="time"/> as a discord date.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToDiscordDate(this DateTimeOffset time) => $"<t:{time.ToUnixTimeSeconds()}:D>";
    }
}
