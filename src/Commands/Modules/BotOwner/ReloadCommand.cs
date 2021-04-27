﻿using System.Threading.Tasks;
using Qmmands;
using Volte.Core;

namespace Volte.Commands.Modules
{
    public sealed partial class BotOwnerModule
    {
        [Command("Reload", "Rl")]
        [Description(
            "Reloads the bot's configuration file if you've changed it. NOTE: This will throw an exception if the config file is invalid JSON!")]
        public Task<ActionResult> ReloadAsync()
            => Config.Reload()
                ? Ok("Config reloaded!")
                : BadRequest("Something bad happened. Check console for more detailed information.");
    }
}