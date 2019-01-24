﻿using System.Threading.Tasks;
using Discord.Commands;
using Volte.Core.Files.Readers;
using Volte.Core.Modules;
using Volte.Helpers;

namespace Volte.Core.Services {
    public class EconomyService {
        public async Task Give(VolteContext ctx) {
            var config = ServerConfig.Get(ctx.Guild);
            var userData = Users.Get(ctx.User.Id);
            if (config.Leveling) {
                var oldLevel = userData.Level;
                userData.Money += 1;
                userData.Xp += 5;
                Users.Save();
                var newLevel = userData.Level;

                if (oldLevel != newLevel) {
                    var levelUp = await ctx.Channel.SendMessageAsync("", false,
                        Utils.CreateEmbed(ctx,
                            $"Good job {ctx.User.Mention}! You leveled up to level **{newLevel}**!"));
                    await Task.Delay(5000);
                    await levelUp.DeleteAsync();
                }
            }
        }
    }
}