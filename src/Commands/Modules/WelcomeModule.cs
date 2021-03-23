using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Commands;
using Volte.Core.Entities;
using Volte.Services;

namespace Volte.Commands.Modules
{
    [Group("Welcome", "W")]
    [RequireGuildAdmin]
    public class WelcomeModule : VolteModule
    {
        public WelcomeService Service { get; set; }
        
        [Command, DummyCommand, Description("The command group for modifying the Welcome system.")]
        public Task<ActionResult> BaseAsync() => None();
        
        [Command("Channel", "C")]
        [Description("Sets the channel used for welcoming new users for this guild.")]
        public Task<ActionResult> WelcomeChannelAsync([Remainder, Description("The channel to use for welcoming messages.")] SocketTextChannel channel)
        {
            Context.GuildData.Configuration.Welcome.WelcomeChannel = channel.Id;
            Db.Save(Context.GuildData);
            return Ok($"Set this guild's welcome channel to {channel.Mention}.");
        }

        [Command("Join")]
        [Description("Sets or shows the welcome message used to welcome new users for this guild.")]
        public Task<ActionResult> WelcomeMessageAsync([Remainder] string message = null)
        {
            if (message is null)
            {
                return Ok(new StringBuilder()
                    .AppendLine($"The current welcome message for this guild is: {Format.Code(Context.GuildData.Configuration.Welcome.WelcomeMessage)}")
                    .ToString());
            }

            Context.GuildData.Configuration.Welcome.WelcomeMessage = message;
            Db.Save(Context.GuildData);
            var welcomeChannel = Context.Guild.GetTextChannel(Context.GuildData.Configuration.Welcome.WelcomeChannel);
            var sendingTest = welcomeChannel is null
                ? "Not sending a test message as you do not have a welcome channel set." +
                  "Set a welcome channel to fully complete the setup!"
                : $"Sending a test message to {welcomeChannel.Mention}.";

            return Ok(new StringBuilder()
                .AppendLine($"Set this guild's welcome message to {Format.Code(message)}")
                .AppendLine()
                .AppendLine($"{sendingTest}").ToString(),
                async _ => {
                    if (welcomeChannel != null)
                        await Service.JoinAsync(new UserJoinedEventArgs(Context.User));
                });
        }

        [Command("Color", "Colour", "Cl")]
        [Description("Sets the color used for welcome embeds for this guild.")]
        public Task<ActionResult> WelcomeColorAsync([Remainder] Color color)
        {
            Context.GuildData.Configuration.Welcome.WelcomeColor = color.RawValue;
            Db.Save(Context.GuildData);
            return Ok("Successfully set this guild's welcome message embed color!");
        }

        [Command("Left")]
        [Description("Sets or shows the leaving message used to say bye for this guild.")]
        public Task<ActionResult> LeavingMessageAsync([Remainder] string message = null)
        {
            if (message is null)
            {
                return Ok(new StringBuilder()
                    .AppendLine($"The current leaving message for this guild is: {Format.Code(Context.GuildData.Configuration.Welcome.LeavingMessage)}")
                    .ToString());
            }

            Context.GuildData.Configuration.Welcome.LeavingMessage = message;
            Db.Save(Context.GuildData);
            var welcomeChannel = Context.Guild.GetTextChannel(Context.GuildData.Configuration.Welcome.WelcomeChannel);
            var sendingTest = Context.GuildData.Configuration.Welcome.WelcomeChannel == 0 || welcomeChannel is null
                ? "Not sending a test message, as you do not have a welcome channel set. " +
                  "Set a welcome channel to fully complete the setup!"
                : $"Sending a test message to {welcomeChannel.Mention}.";

            return Ok(new StringBuilder()
                    .AppendLine($"Set this server's leaving message to {Format.Code(message)}")
                    .AppendLine()
                    .AppendLine($"{sendingTest}").ToString(),
                async _ =>
                {
                    if (welcomeChannel != null)
                        await Service.LeaveAsync(new UserLeftEventArgs(Context.User));
                });
        }

        [Command("Dm")]
        [Description("Sets or disables the message to be (attempted to) sent to members upon joining.")]
        public Task<ActionResult> WelcomeDmMessageAsync(string message = null)
        {
            if (message is null)
                return Ok($"Unset the WelcomeDmMessage that was previously set to: {Format.Code(Context.GuildData.Configuration.Welcome.WelcomeDmMessage)}");

            Context.GuildData.Configuration.Welcome.WelcomeDmMessage = message;
            Db.Save(Context.GuildData);
            return Ok($"Set the WelcomeDmMessage to: {Format.Code(message)}");
        }
    }
}