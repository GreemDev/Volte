﻿using System.Security.Cryptography;
using System.Threading.Tasks;
using Discord.WebSocket;
using Qmmands;
using Volte.Core.Commands.Preconditions;
using Volte.Core.Extensions;

namespace Volte.Core.Commands.Modules.Admin
{
    public partial class AdminModule : VolteModule
    {
        [Command("VerificationRole", "Vr")]
        [Description("Sets the role to be given to users when they verify in this server.")]
        [Remarks("Usage: |prefix|verificationrole {role}")]
        [RequireGuildAdmin]
        public async Task VerificationRoleAsync(SocketRole role)
        {
            var config = Db.GetConfig(Context.Guild);
            config.VerificationOptions.RoleId = role.Id;
            config.VerificationOptions.Enabled = true;
            Db.UpdateConfig(config);
            await Context.CreateEmbed($"Successfully set **{role.Name}** as the role to use for Verification.")
                .SendToAsync(Context.Channel);
        }

        [Command("VerificationMessage", "Vm")]
        [Description("Sets the message to be used for users when they verify.")]
        [Remarks("Usage: |prefix|verificationmessage {messageId}")]
        [RequireGuildAdmin]
        public async Task VerificationMessageAsync(ulong messageId)
        {
            var config = Db.GetConfig(Context.Guild);
            config.VerificationOptions.MessageId = messageId;
            config.VerificationOptions.Enabled = true;
            Db.UpdateConfig(config);
            await Context
                .CreateEmbed($"Successfully set message with ID **{messageId}** as the message used for Verification.")
                .SendToAsync(Context.Channel);
        }
    }
}
