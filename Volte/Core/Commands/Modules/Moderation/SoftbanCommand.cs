using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Volte.Core.Commands.Preconditions;
using Volte.Core.Extensions;

namespace Volte.Core.Commands.Modules.Moderation {
    public partial class ModerationModule : VolteModule {
        [Command("Softban")]
        [Summary("Softbans the mentioned user, kicking them and deleting the last 7 days of messages.")]
        [Remarks("Usage: $softban {@user} [reason]")]
        [RequireBotPermission(GuildPermission.KickMembers | GuildPermission.BanMembers)]
        [RequireGuildModerator]
        public async Task SoftBan(SocketGuildUser user, [Remainder]string reason = "Softbanned by a Moderator.") {
            try {
                await Context.CreateEmbed($"You've been softbanned from **{Context.Guild.Name}** for **{reason}**.")
                    .SendTo(user);
            } catch (HttpException ignored) when (ignored.DiscordCode == 50007) {}
            await Context.Guild.AddBanAsync(
                user, 7, reason);
            await Context.Guild.RemoveBanAsync(user);
            await Context.CreateEmbed($"Successfully softbanned **{user.Username}#{user.Discriminator}**.")
                .SendTo(Context.Channel);
        }
    }
}