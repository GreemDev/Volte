﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Volte.Core.Services;
using Volte.Core.Files.Readers;
using Volte.Core.Modules;
using Volte.Core.Runtime;

namespace Volte.Core.Discord {
    public class VolteHandler {
        private readonly DiscordSocketClient _client = VolteBot.Client;
        private readonly CommandService _service = VolteBot.CommandService;
        private readonly Log _logger = VolteBot.GetLogger();

        private readonly IServiceProvider _services = VolteBot.ServiceProvider;

        public async Task Init() {
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _service.CommandExecuted += OnCommand;
            _client.MessageReceived += HandleMessageOrCommand;
            _client.JoinedGuild += Guilds;
            _client.UserJoined += _services.GetRequiredService<WelcomeService>().Join;
            _client.UserJoined += _services.GetRequiredService<AutoroleService>().Apply;
            _client.UserLeft += _services.GetRequiredService<WelcomeService>().Leave;
            _client.Ready += OnReady;
        }

        private async Task OnReady() {
            var dbl = VolteBot.Client.GetGuild(264445053596991498);
            if (dbl == null || Config.GetOwner() == 168548441939509248) return;
            await dbl.GetTextChannel(265156286406983680).SendMessageAsync(
                $"<@168548441939509248>: I am a SIVA not owned by you. Please do not post SIVA to a bot list again, <@{Config.GetOwner()}>.");
            await dbl.LeaveAsync();
        }

        public async Task Guilds(SocketGuild guild) {
            if (Config.GetBlacklistedOwners().Contains(guild.OwnerId)) {
                await guild.LeaveAsync();
            }
        }

        public async Task HandleMessageOrCommand(SocketMessage s) {
            var msg = (SocketUserMessage) s;
            var ctx = new VolteContext(_client, msg);
            await _services.GetRequiredService<BlacklistService>().CheckMessage(s);
            await _services.GetRequiredService<AntilinkService>().CheckMessage(s);
            await _services.GetRequiredService<EconomyService>().Give(ctx);
            //await SupportMessageListener.Check(s);
            if (ctx.User.IsBot) return;
            var config = ServerConfig.Get(ctx.Guild);
            Users.Get(s.Author.Id);
            var prefix = config.CommandPrefix == string.Empty ? Config.GetCommandPrefix() : config.CommandPrefix;

            if (config.EmbedColourR == 0 && config.EmbedColourG == 0 && config.EmbedColourB == 0) {
                config.EmbedColourR = 112;
                config.EmbedColourG = 0;
                config.EmbedColourB = 251;
                ServerConfig.Save();
            }

            var argPos = 0; //i'd get rid of this but because of Discord.Net being finnicky i can't.

            var msgStrip = msg.Content.Replace(prefix, string.Empty);


            if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) {
                var result = await _service.ExecuteAsync(ctx, argPos, _services);

                if (!result.IsSuccess && result.ErrorReason != "Unknown command.") {
                    string reason;
                    switch (result.ErrorReason) {
                        case "The server responded with error 403: Forbidden":
                            reason =
                                "I'm not allowed to do that. Either I don't have permission or the requested user is higher than me in the role heirarchy.";
                            break;
                        case "Failed to parse Boolean":
                            reason = "You can only input `true` or `false` for this command.";
                            break;
                        default:
                            reason = result.ErrorReason;
                            break;
                    }

                    var embed = new EmbedBuilder();

                    if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) {
                        var nm = msg.Content.Replace($"<@{_client.CurrentUser.Id}> ", config.CommandPrefix);
                        embed.AddField("Error in command:", nm);
                        embed.AddField("Error reason:", reason);
                        embed.AddField("Weird error?",
                            "[Report it in the SIVA-dev server](https://discord.gg/prR9Yjq)");
                        embed.WithAuthor(ctx.User);
                        embed.WithColor(Config.GetErrorColour());
                        await ctx.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    else {
                        var nm = msg.Content;
                        embed.AddField("Error in command:", nm);
                        embed.AddField("Error reason:", reason);
                        embed.AddField("Weird error?",
                            "[Report it in the SIVA-dev server](https://discord.gg/prR9Yjq)");
                        embed.WithAuthor(ctx.User);
                        embed.WithColor(Config.GetErrorColour());
                        await ctx.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }

                if (result.ErrorReason.Equals("Unknown command.")) return;

                if (config.DeleteMessageOnCommand) {
                    await ctx.Message.DeleteAsync();
                }

                if (config.CustomCommands.ContainsKey(msgStrip)) {
                    await ctx.Channel.SendMessageAsync(
                        config.CustomCommands.FirstOrDefault(c => c.Key.ToLower() == msgStrip.ToLower()).Value
                    );
                }
            }
            else {
                if (msg.Content.Contains($"<@{_client.CurrentUser.Id}>")) {
                    await ctx.Channel.SendMessageAsync("<:whO_PENG:437088256291504130>");
                }
            }
        }

        private async Task OnCommand(Optional<CommandInfo> cinfo, ICommandContext ctx, IResult res) {
            if (Config.GetLogAllCommands()) {
                if (res.IsSuccess) {
                    _logger.Info($"--|  -Command from user: {ctx.User.Username}#{ctx.User.Discriminator}");
                    _logger.Info($"--|     -Command Issued: {ctx.Message.Content}");
                    _logger.Info($"--|           -In Guild: {ctx.Guild.Name}");
                    _logger.Info($"--|         -In Channel: #{ctx.Channel.Name}");
                    _logger.Info($"--|        -Time Issued: {DateTime.Now}");
                    _logger.Info($"--|           -Executed: {res.IsSuccess} ");
                    _logger.Info("-------------------------------------------------");
                }
                else {
                    _logger.Error($"--|  -Command from user: {ctx.User.Username}#{ctx.User.Discriminator}");
                    _logger.Error($"--|     -Command Issued: {ctx.Message.Content}");
                    _logger.Error($"--|           -In Guild: {ctx.Guild.Name}");
                    _logger.Error($"--|         -In Channel: #{ctx.Channel.Name}");
                    _logger.Error($"--|        -Time Issued: {DateTime.Now}");
                    _logger.Error($"--|           -Executed: {res.IsSuccess} | Reason: {res.ErrorReason}");
                    _logger.Error("-------------------------------------------------");
                }
            }

            try {
                File.AppendAllText("Commands.log",
                    $"--|  -Command from user: {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})\n");
                File.AppendAllText("Commands.log",
                    $"--|     -Command Issued: {ctx.Message.Content} ({ctx.Message.Id})\n");
                File.AppendAllText("Commands.log",
                    $"--|           -In Guild: {ctx.Guild.Name} ({ctx.Guild.Id})\n");
                File.AppendAllText("Commands.log",
                    $"--|         -In Channel: #{ctx.Channel.Name} ({ctx.Channel.Id})\n");
                File.AppendAllText("Commands.log", $"--|        -Time Issued: {DateTime.Now}\n");
                File.AppendAllText("Commands.log", res.IsSuccess
                    ? $"--|           -Executed: {res.IsSuccess}\n"
                    : $"--|           -Executed: {res.IsSuccess} | Reason: {res.ErrorReason}\n");
                File.AppendAllText("Commands.log", "-------------------------------------------------\n");
            }
            catch (FileNotFoundException) {
                _logger.Error("The Commands.log file doesn't exist. Creating it now.");
                File.Create("Commands.log");
            }
        }
    }
}