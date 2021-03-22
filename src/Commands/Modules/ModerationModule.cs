using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Volte.Core.Entities;
using Volte.Services;

namespace Volte.Commands.Modules
{
    [RequireGuildModerator]
    public sealed partial class ModerationModule : VolteModule
    {
        public static async Task WarnAsync(SocketGuildUser issuer, GuildData data, SocketGuildUser member, DatabaseService db, LoggingService logger, string reason)
        {
            data.Extras.AddWarn(w =>
            {
                w.User = member.Id;
                w.Reason = reason;
                w.Issuer = issuer.Id;
                w.Date = DateTimeOffset.Now;
            });
            db.Save(data);

            var e = new EmbedBuilder().WithSuccessColor().WithAuthor(issuer)
                .WithDescription($"You've been warned in **{issuer.Guild.Name}** for `{reason}`.");
            if (!data.Configuration.Moderation.ShowResponsibleModerator)
                e.WithAuthor(author: null);

            if (!await member.TrySendMessageAsync(
                embed: e.Build()))
            {
                logger.Warn(LogSource.Volte,
                    $"encountered a 403 when trying to message {member}!");
            }
        }
    }
}