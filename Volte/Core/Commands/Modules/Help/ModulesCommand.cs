using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Volte.Core.Extensions;

namespace Volte.Core.Commands.Modules.Help {
    public partial class HelpModule : VolteModule {
        [Command("Modules"), Alias("Mdls")]
        [Summary("Lists available modules.")]
        [Remarks("Usage: |prefix|modules")]
        public async Task Modules() {
            var modules = Cs.Modules.Aggregate(string.Empty,
                (current, module) => current + $"**{module.SanitizeName()}**\n");
            await Context.CreateEmbed(modules).ToEmbedBuilder().WithTitle("Available Modules").SendTo(Context.Channel);

        }
    }
}