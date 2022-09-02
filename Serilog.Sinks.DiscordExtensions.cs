using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Discord;
using System.Text.RegularExpressions;

namespace TimeTableBot.Serilog.Sinks
{
    public static class DiscordExtensions
    {
        public static LoggerConfiguration Discord(
            this LoggerSinkConfiguration config,
            string webhook,
            IFormatProvider? formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            var match = Regex.Match(webhook, "https://discord.com/api/webhooks/(?<Id>\\d*)/(?<Token>.*)");

            if (match.Success == false || ulong.TryParse(match.Groups["Id"].Value, out ulong id) == false) throw new FormatException("Webhook url is incorrect.");
            string token = match.Groups["Token"].Value;

            return config.Discord(id, token, formatProvider, restrictedToMinimumLevel);
        }
    }
}
