using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using Gommon;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public partial class HelpModule : VolteModule
    {
        [Command("Help", "H")]
        [Description("Shows the commands used for module listing, command listing, and command info.")]
        [Remarks("Usage: |prefix|help")]
        public Task<ActionResult> HelpAsync(string moduleOrCommand = null)
        {
            if (moduleOrCommand is null)
            {
                return Ok("Hey, I'm Volte! Here's a list of my modules and commands designed to help you out. \n" +
                          $"Use `{Context.GuildData.Configuration.CommandPrefix}help {{moduleName}}` to list all commands in a module, " +
                          $"and `{Context.GuildData.Configuration.CommandPrefix}help {{commandName}}` to show information about a command." +
                          "\n\n" +
                          $"Available Modules: `{CommandService.GetAllModules().Select(x => x.SanitizeName()).Join("`, `")}`" +
                          "\n\n" +
                          $"Available Commands: `{CommandService.GetAllCommands().Select(x => x.Name).Join("`, `")}`");
            }

            var module = GetTargetModule(moduleOrCommand);
            var command = GetTargetCommand(moduleOrCommand);

            if (module is null && command is null)
            {
                return BadRequest($"{EmojiService.X} No matching module/command was found.");
            }

            if (module != null && command is null)
            {
                var commands = $"`{module.Commands.Select(x => x.FullAliases.First()).Join("`, `")}`";
                return Ok(Context.CreateEmbedBuilder().WithDescription(commands)
                    .WithTitle($"Commands for {module.SanitizeName()}"));
            }

            if (module is null && command != null)
            {
                return Ok($"**Command**: {command.Name}\n" +
                          $"**Module**: {command.Module.SanitizeName()}\n" +
                          $"**Description**: {command.Description ?? "No summary provided."}\n" +
                          $"**Usage**: {command.SanitizeRemarks(Context)}");
            }

            if (module != null && command != null)
            {
                return BadRequest($"{EmojiService.X} Found more than one Module or Command. Results:\n" +
                                  $"**{module.SanitizeName()}**\n" +
                                  $"**{command.Name}**");
            }

            return None();
        }

        private Module GetTargetModule(string input)
            => CommandService.GetAllModules().FirstOrDefault(x => x.SanitizeName().EqualsIgnoreCase(input));

        private Command GetTargetCommand(string input)
            => CommandService.GetAllCommands().FirstOrDefault(x => x.FullAliases.ContainsIgnoreCase(input));
    }
}