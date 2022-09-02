using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TimeTableBot
{
    public class Commands : DiscordApplicationGuildModuleBase
    {
        private const string Today = "Сегодня";
        private const string Tomorrow = "Завтра";
        private readonly TimeTableContext _ctx;

        public Commands(TimeTableContext ctx)
        {
            _ctx = ctx;
        }

        [SlashCommand("расписание")]
        [Description("Выводит расписание")]
        public async ValueTask<IResult> GetTimeTable(
            [Name("День")]
            [Description($"День в виде \"25.09.2022\", либо \"{Today}\" или \"{Tomorrow}\".")]
            string day)
        {
            await Deferral(false);

            //Parsing date
            DateOnly? date = day switch
            {
                Today => DateOnly.FromDateTime(DateTime.Now),
                Tomorrow => DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                _ => DateOnly.TryParse(day, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? result : null //If date is invalid it will be null, not throw
            };
            if (date is null) return Results.Failure("Неверная дата!");

            //Loading timetable
            TimeTable? table = _ctx.TimeTables
                .Where(tt => tt.GuildId == Context.GuildId.RawValue)
                .FirstOrDefault();
            if (table is null) return Results.Failure("У этого сервера нет сохраненного расписания!");


            string schedule = table.GetTimeTable(date.Value) ?? "Выходной, отдыхаем ; )";

            string timeStamp = new DateTimeOffset(date.Value.ToDateTime(new TimeOnly())).ToDiscordDate();

            LocalEmbed response = new LocalEmbed()
                .AddField($"Расписание на {timeStamp}", schedule);

            return Response(response);
        }


        [AutoComplete("расписание")]
        public void GetTimeTableAutocomplere(
            [Name("День")] AutoComplete<string> day)
        {
            if (day.IsFocused)
            {
                day.Choices.AddRange(Today, Tomorrow);
            }
        }


        [SlashCommand("файл-расписания")]
        [Description("Отправляет файл с расписанием вашего сервера.")]
        public IResult GetTemplate()
        {
            var timetable = _ctx.TimeTables.Where(tt => tt.GuildId == Context.GuildId.RawValue).First();

            var options = Context.Services.GetRequiredService<JsonSerializerOptions>();
            string json = JsonSerializer.Serialize(timetable, options);

            Stream asFile = new MemoryStream(Encoding.UTF8.GetBytes(json));

            LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
                .WithContent($"```json\n{json}\n```")
                .AddAttachment(new LocalAttachment(asFile, "timetable.json"));

            return Response(response);
        }


        [RequireAuthorPermissions(Permissions.Administrator)]
        [MessageCommand("Установить расписание")]
        [Description("Используйте чтобы установить расписание.")]
        public async ValueTask<IResult> SetTimeTable(IMessage msg)
        {
            await Deferral(false);

            var buttonId = Guid.NewGuid().ToString();
            var timeout = TimeSpan.FromMinutes(1);

            var response = new LocalMessage()
                .WithReply(msg.Id)
                .AddComponent(LocalComponent.Row(LocalComponent.Button(buttonId, "Продолжить")))
                .WithContent($"{Context.Author.Mention}\n Вы уверены что хотите установить расписание из этого сообщения? Игнорируйте это сообщение для отмены. \n\n" +
                $"Кнопка станет неактивна {(DateTimeOffset.Now + timeout).ToRelativeDiscordTime()}");

            var confirmation = await Bot.SendMessageAsync(Context.ChannelId, response);


            IComponentInteraction? e = await Context.WaitForInteractionAsync<IComponentInteraction>(i => i.CustomId == buttonId, timeout);
            await confirmation.DeleteAsync();
            if (e is null) return Results.Success;

            string json = string.Empty;

            if (string.IsNullOrWhiteSpace(msg.Content))
            {
                if (msg is not IUserMessage userMsg) return Results.Failure("Некорректное сообщение.");

                var jsons = userMsg.Attachments.Where(a => a.FileName.EndsWith(".json"));
                if (jsons.Count() > 1) return Results.Failure("В сообщении может быть только один json файл.");
                if (jsons.Any() == false) return Results.Failure("Не найден json файл или текст.");

                using var client = new HttpClient();

                json = await client.GetStringAsync(jsons.First().Url);
            }
            else
            {
                var match = Regex.Match(msg.Content, "```json\n(?<Json>[\\s\\S]*)\n```");
                if (match.Success == false) return Results.Failure("Не найден json расписания!");

                json = match.Groups["Json"].Value;
            }

            TimeTable? table = JsonSerializer.Deserialize<TimeTable>(json);
            if (table is null) return Results.Failure("Некорректный json расписания!");

            _ctx.TimeTables.Update(table);
            await _ctx.SaveChangesAsync();

            return Response("Успешно!");
        }


        [SlashCommand("чаво")]
        [Description("Частые вопросы")]
        public IResult FAQ()
        {
            var response = new LocalInteractionMessageResponse()
                .WithContent(
                "> Как сохранить расписание?\n" +
                "```Вам нужно записать данные в виде json. Для упрощения можете вызвать команду '/файл-расписания', " +
                "она вернет ваш стандартный файл с расписанием. После этого вам можно поставить расписания на дни, " +
                "где odd это первая неделя, а even - вторая. Также рекомендую поменять значение FirstDay, там желательно " +
                "поставить последний неучебный понедельник по первой неделе. После того как доделаете, администратор сервера " +
                "должен использовать контекстную команду 'Установить расписание' и вуаля!```\n" +
                "> Как отправить файл расписания?\n" +
                "```Либо в виде дискорд-кодблока с форматированием json (картинка ниже), " +
                "либо в сообщении без текста с единственным .json файлом.```\n" +
                "> Как использовать расписание?\n" +
                "```Командой '/расписание'.```\n" +
                "https://media.discordapp.net/attachments/956224287990243329/1015302549089292389/unknown.png");


            return Response(response);
        }

    }
}
