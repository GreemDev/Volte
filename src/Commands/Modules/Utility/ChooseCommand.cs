﻿using System;
using System.Threading.Tasks;
using Gommon;
using Qmmands;
using Volte.Commands;

namespace Volte.Commands.Modules
{
    public sealed partial class UtilityModule
    {
        [Command("Choose")]
        [Description("Choose an item from a list separated by |.")]
        public Task<ActionResult> ChooseAsync([Remainder, Description("The options you want to choose from; separated by `|`.")]string options) 
            => Ok($"I choose `{options.Split('|', StringSplitOptions.RemoveEmptyEntries).Random()}`.");
    }
}