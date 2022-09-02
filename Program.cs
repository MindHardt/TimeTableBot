using Disqord.Bot.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Discord;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text.Json;
using TimeTableBot;
using TimeTableBot.Serilog.Sinks;

new HostBuilder()
    .UseSerilog((ctx, logger) =>
    {
        string? webhook = ctx.Configuration.GetSection("Discord").GetSection("LogsWebhook").Get<string>();

        logger
        .Filter.ByExcluding(e => e.Exception is Disqord.WebSocket.WebSocketClosedException)
        .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
        .WriteTo.File("Logs/Log-.log", rollingInterval: RollingInterval.Day, shared: true);

        if (webhook is not null) logger.WriteTo.Discord(webhook, restrictedToMinimumLevel: LogEventLevel.Warning);
    })
    .ConfigureHostConfiguration(config =>
    {
        config
        .AddJsonFile("config.json")
        .AddCommandLine(args);
    })
    .ConfigureServices(services =>
    {
        services
        .AddDbContext<TimeTableContext>(options => options.UseSqlite("Data Source=TimeTables.db;"))

        .AddTransient(_ => new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    })
    .ConfigureDiscordBot((ctx, bot) =>
    {
        bot.Token = ctx.Configuration.GetSection("Discord").GetSection("Token").Get<string>() ?? throw new KeyNotFoundException("No token found!");
        bot.Intents = Disqord.Gateway.GatewayIntents.Unprivileged;
    })
    .Build()
    .Run();

