﻿using System;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Reflection;
using SIVA.Core.LevelingSystem;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Discord;
using SIVA.Core.Config;

namespace SIVA
{
    internal class MessageHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
            _client.UserJoined += Welcome;
            _client.MessageReceived += SupportChannelUtils;
            _client.UserJoined += Autorole;
            _client.JoinedGuild += GuildUtils;
            //_client.ChannelUpdated
            //_client.GuildMemberUpdated
            //_client.MessageDeleted
            //_client.RoleCreated
            //_client.UserLeft
            //_client.UserBanned += BannedUser;
            //_client.ChannelCreated
        }

        /*private async Task BannedUser(SocketGuildUser user, SocketGuild guild, Task task)
        {
            var embed = new EmbedBuilder();
        }*/

        private async Task Autorole(SocketGuildUser user)
        {
            var config = GuildConfig.GetGuildConfig(user.Guild.Id);
            if (config.RoleToApply != null || config.RoleToApply != "")
            {
                var targetRole = user.Guild.Roles.FirstOrDefault(r => r.Name == config.RoleToApply);
                await user.AddRoleAsync(targetRole);
            }
        }


        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            var config = GuildConfig.GetGuildConfig(context.Guild.Id);

            if (config.Leveling)
            {
                Leveling.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel);
            }

            string prefix = " ";

            int argPos = 0;
            if (msg.HasStringPrefix(prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos);
                Console.WriteLine($"\\|-Command from user: {context.User.Username}#{context.User.Discriminator} ({context.User.Id})");
                Console.WriteLine($"\\|   -Command Issued: {msg.Content} ({msg.Id})");
                Console.WriteLine($"\\|         -In Guild: {context.Guild.Name} ({context.Guild.Id})");
                Console.WriteLine($"\\|       -In Channel: #{context.Channel.Name} ({context.Channel.Id})");
                Console.WriteLine($"\\|      -Time Issued: {DateTime.Now}");
                Console.WriteLine(result.IsSuccess
                    ? $"\\|         -Executed: {result.IsSuccess}"
                    : $"\\|         -Executed: {result.IsSuccess} | Reason: {result.ErrorReason}");
            }
        }

        public async Task Welcome(SocketGuildUser s)
        {
            var config = GuildConfig.GetGuildConfig(s.Guild.Id);

            if (config.WelcomeChannel != 0)
            {
                var msg = config.WelcomeMessage.Replace("{UserMention}", s.Mention);
                var replaced = msg.Replace("{ServerName}", s.Guild.Name);

                var channel = s.Guild.GetTextChannel(config.WelcomeChannel);
                var embed = new EmbedBuilder();
                embed.WithDescription(replaced);
                embed.WithColor(Config.bot.DefaultEmbedColour);
                embed.WithFooter($"User ID: {s.Id} | Guild ID: {s.Guild.Id} | Guild Owner: {s.Guild.Owner.Username}#{s.Guild.Owner.Discriminator}");
                embed.WithThumbnailUrl(s.Guild.IconUrl);
                await channel.SendMessageAsync("", false, embed);
            }
        }

        public async Task GuildUtils(SocketGuild s)
        {

            var config = GuildConfig.GetGuildConfig(s.Id) ??
                         GuildConfig.CreateGuildConfig(s.Id);

            if (s.Owner.Id == 396003871434211339)
            {
                await s.LeaveAsync();
            }

            config.GuildOwnerId = s.Owner.Id;
            GuildConfig.SaveGuildConfig();

        }

        public async Task MassPengChecks(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            if (msg.Content.Contains("@everyone") || msg.Content.Contains("@here") && context.User != context.Guild.Owner)
            {
                await msg.DeleteAsync();
                await context.Channel.SendMessageAsync($"{msg.Author.Mention}, try not to mass ping.");
            }
        }

        public async Task SupportChannelUtils(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            var config = GuildConfig.GetGuildConfig(context.Guild.Id) ??
                         GuildConfig.CreateGuildConfig(context.Guild.Id);
            config.GuildOwnerId = context.Guild.OwnerId;

            if (msg.Content == "SetupSupport" && msg.Author.Id == config.GuildOwnerId)
            {
                var embed = new EmbedBuilder();
                embed.WithColor(Config.bot.DefaultEmbedColour);
                embed.WithDescription(Utilities.GetAlert("SupportEmbedText"));
                embed.WithAuthor(context.Guild.Owner);
                await context.Channel.SendMessageAsync("", false, embed);
                config.SupportChannelId = context.Channel.Id;
                config.SupportChannelName = context.Channel.Name;
                config.CanCloseOwnTicket = true;
                config.SupportRole = "Support";

            }
            
            if (msg.Content != "SetupSupport")
            {
                var supportConfig = GuildConfig.GetGuildConfig(context.Guild.Id);
                var supportStartChannel = context.Guild.Channels.FirstOrDefault(c => c.Name == supportConfig.SupportChannelName);

                if (msg.Channel == supportStartChannel)
                {
                    //var categoryId = supportConfig.SupportCategoryId;
                    var supportChannelExists = context.Guild.Channels.FirstOrDefault(c => c.Name == $"{supportConfig.SupportChannelName}-{context.User.Id}");
                    var role = context.Guild.Roles.FirstOrDefault(r => r.Name == supportConfig.SupportRole);

                    if (supportChannelExists == null)
                    {
                        await msg.DeleteAsync();
                        var channel = await context.Guild.CreateTextChannelAsync($"{supportConfig.SupportChannelName}-{context.User.Id}");
                        await channel.AddPermissionOverwriteAsync(context.User, OverwritePermissions.AllowAll(channel));
                        await channel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
                        await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                        var embed = new EmbedBuilder();
                        embed.WithAuthor(msg.Author);
                        embed.WithThumbnailUrl(context.User.GetAvatarUrl());
                        embed.AddInlineField("What do you need help with?", $"{msg.Content}");
                        embed.WithColor(Config.bot.DefaultEmbedColour);
                        embed.WithFooter($"Time Created: {DateTime.Now}");
                        await channel.SendMessageAsync($"You can close this ticket if you have the role set for moderating tickets: `{supportConfig.SupportRole}`");
                        await channel.SendMessageAsync("", false, embed);

                    }
                    else
                    {
                        var channel = context.Guild.GetTextChannel(supportChannelExists.Id);
                        await channel.SendMessageAsync($"{context.User.Mention}, please send your message here rather than the primary support channel. Text: ```{msg.Content}``` If you cannot type in here, please tell an admin.");
                        await msg.DeleteAsync();
                    }
                }
            }
        }
    }
}
