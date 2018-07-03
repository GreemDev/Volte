﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SIVA.Core.Files.Readers;
using SIVA.Helpers;

namespace SIVA.Core.Discord.Modules.General
{
    public class RandomRoles : SIVACommand
    {
        [Command("RandomRoleMe")]
        public async Task PickARole()
        {
            var config = ServerConfig.Get(Context.Guild);
            var r = new Random().Next(0, config.RandomRoles.Count);
            var role = config.RandomRoles.ElementAt(r);
            var targetRole = Context.Guild.Roles.FirstOrDefault(ro => ro.Id == role);

            await ((SocketGuildUser) Context.User).AddRoleAsync(targetRole);

            await Context.Channel.SendMessageAsync("", false,
                Utils.CreateEmbed(Context, $"Chose a random role for you....your role is **{targetRole.Name}**."));
        }
    }
}