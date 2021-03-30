using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Volte.Core.Entities;
using Volte.Core.Helpers;
using Timer = System.Threading.Timer;

namespace Volte.Services
{
    public class ReminderService : VolteService
    {
        private const int PollRate = 30; //seconds
        private static Timer _checker;
        private readonly DatabaseService _db;
        private readonly DiscordShardedClient _client;

        public ReminderService(DatabaseService databaseService,
            DiscordShardedClient client)
        {
            _client = client;
            _db = databaseService;
        }

        /// <summary>
        ///     Sets the value of private static field <see cref="_checker"/>.
        ///     If its value is already set; this method returns immediately.
        /// </summary>
        public void Initialize()
        {
            if (_checker != null)
                return;

            _checker = new Timer(
                _ => Check(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(PollRate)
            );
        }

        private void Check()
        {
            Logger.Debug(LogSource.Service, "Checking all reminders.");
            foreach (var reminder in _db.GetAllReminders())
            {
                Logger.Debug(LogSource.Service, $"Reminder '{reminder.Value}', set for {reminder.TargetTime}");
                if (reminder.TargetTime.Ticks <= DateTime.Now.Ticks)
                {
                    Executor.Execute(async () => await SendReminderAsync(reminder));
                }
            }
        }

        private async Task SendReminderAsync(Reminder reminder)
        {
            var guild = _client.GetGuild(reminder.GuildId);
            var channel = guild?.GetTextChannel(reminder.ChannelId);
            if (channel is null)
            {
                if (_db.TryDeleteReminder(reminder))
                    Logger.Debug(LogSource.Service,
                        "Reminder deleted from the database as Volte no longer has access to the channel it was created in.");
                Logger.Debug(LogSource.Service,
                    "Reminder's target channel was no longer accessible in the guild; aborting.");
                return;
            }

            var author = guild.GetUser(reminder.CreatorId);
            if (author is null)
            {
                if (_db.TryDeleteReminder(reminder))
                    Logger.Debug(LogSource.Service,
                        "Reminder deleted from the database as its creator is no longer in the guild it was made.");
                Logger.Debug(LogSource.Service, "Reminder's creator was no longer present in the guild; aborting.");
                return;
            }

            var e = new EmbedBuilder()
                .WithTitle("Reminder")
                .WithRelevantColor(author)
                .WithDescription(
                    $"You asked me {reminder.CreationTime.Humanize(false)} to remind you about: {Format.Code(reminder.Value, "")}");
            await channel.SendMessageAsync(author.Mention, embed: e.Build());
            _db.TryDeleteReminder(reminder);
        }
    }
}