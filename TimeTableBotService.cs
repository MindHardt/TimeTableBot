using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;

namespace TimeTableBot
{
    public class TimeTableBotService : DiscordBotService
    {
        public readonly TimeTableContext _ctx;

        public TimeTableBotService(TimeTableContext ctx)
        {
            _ctx = ctx;
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            TimeTable table = new TimeTable()
            {
                GuildId = e.GuildId,
                FirstDay = DateOnly.FromDateTime(DateTime.Now)
            };

            _ctx.Add(table);
            await _ctx.SaveChangesAsync();

            Logger.LogInformation("Joined new guild ({id}), database record created OK.", e.GuildId.RawValue);
        }

        protected override async ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            TimeTable table = _ctx.TimeTables.Where(tt => tt.GuildId == e.GuildId).First();

            _ctx.TimeTables.Remove(table);
            await _ctx.SaveChangesAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Bot.WaitUntilReadyAsync(stoppingToken);

            var actualGuilds = Bot.GetGuilds().Select(g => g.Key.RawValue).ToArray();
            var savedGuilds = _ctx.TimeTables.Select(tt => tt.GuildId).ToArray();

            var missingGuilds = actualGuilds.Except(savedGuilds);

            var today = DateOnly.FromDateTime(DateTime.Now);
            foreach (var guild in missingGuilds)
            {
                TimeTable timeTable = new TimeTable()
                {
                    GuildId = guild,
                    FirstDay = today
                };
                _ctx.Add(timeTable);
            }

            await _ctx.SaveChangesAsync();
        }


    }
}
