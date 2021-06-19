using System;
using System.Linq;
using System.Threading.Tasks;
using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Jaar instellen commando's")]
    public class YearModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public YearModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionRemoved += RemoveYearAsync;
        }

        [Command("jaar")]
        [Summary("Stel je jaar van de server in.")]
        public async Task ChangeYearAsync()
        {
            var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            var studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            if (!guild.GetUser(Context.User.Id).Roles.Contains(studentRole))
                return;

            if (Context.IsPrivate)
            {
                Console.WriteLine("ChangeYearAsync");
                var sent = await ReplyAsync(
                    "Kies je jaar door op één of meer van de emoji onder dit bericht te klikken.");
                await sent.AddReactionsAsync(Emojis.emojiJaren);
            }
        }

        public async Task RemoveYearAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser()) return;

            if (channel is IPrivateChannel)
            {
                Console.WriteLine("RemoveYearAsync");

                var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole role;

                if (reaction.Emote.Equals(Emojis.JAAR_1_EMOJI))
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                else if (reaction.Emote.Equals(Emojis.JAAR_2_EMOJI))
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));

                else if (reaction.Emote.Equals(Emojis.JAAR_3_EMOJI))
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                else
                    role = null;

                if (role != null) await guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
            }
        }
    }
}