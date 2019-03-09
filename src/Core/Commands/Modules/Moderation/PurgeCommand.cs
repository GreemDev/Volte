using System.Threading.Tasks;
using Discord;
using Qmmands;
using Volte.Core.Commands.Preconditions;
using Volte.Core.Extensions;

namespace Volte.Core.Commands.Modules.Moderation
{
    public partial class ModerationModule : VolteModule
    {
        [Command("Purge", "clear", "clean")]
        [Description("Purges the last x messages.")]
        [Remarks("Usage: $purge {count}")]
        [RequireBotChannelPermission(ChannelPermission.ManageMessages)]
        [RequireGuildModerator]
        public async Task PurgeAsync(int count)
        {
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(
                await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync());
            //+1 to include the command invocation message, and actually delete the last x messages instead of x - 1.
            //lets you theoretically use 0 to delete only the invocation message, for testing or something.

            var msg = await Context
                .CreateEmbed($"Successfully deleted **{count}** {(count != 1 ? "messages" : "message")}")
                .SendToAsync(Context.Channel);
            await Task.Delay(5000).ContinueWith(_ => msg.DeleteAsync());
        }
    }
}