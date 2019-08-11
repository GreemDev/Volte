using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gommon;
using Qmmands;

namespace Volte.Commands.TypeParsers
{
    [VolteTypeParser]
    public sealed class RoleParser : TypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(
            Parameter param,
            string value,
            CommandContext context,
            IServiceProvider provider)
        {
            var ctx = (VolteContext) context;
            SocketRole role = default;
            if (ulong.TryParse(value, out var id) || MentionUtils.TryParseRole(value, out id))
                role = ctx.Guild.GetRole(id).Cast<SocketRole>();

            if (role is null)
            {
                var match = ctx.Guild.Roles.Where(x => x.Name.EqualsIgnoreCase(value)).ToList();
                if (match.Count > 1)
                    return new ValueTask<TypeParserResult<SocketRole>>(TypeParserResult<SocketRole>.Unsuccessful(
                        "Multiple roles found. Try mentioning the role or using its ID."));

                role = match.FirstOrDefault().Cast<SocketRole>();
            }

            return new ValueTask<TypeParserResult<SocketRole>>(role is null
                ? Task.FromResult(TypeParserResult<SocketRole>.Unsuccessful($"Role `{value}` not found."))
                : Task.FromResult(TypeParserResult<SocketRole>.Successful(role)));
        }
    }
}