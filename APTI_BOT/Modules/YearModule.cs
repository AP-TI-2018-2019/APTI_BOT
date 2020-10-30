using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Jaar instellen commando's")]
    public class YearModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

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
            if (Context.IsPrivate)
            {
                System.Console.WriteLine("ChangeYearAsync");
                IUserMessage sent = await ReplyAsync("Kies je jaar door op één of meer van de emoji onder dit bericht te klikken.");
                await sent.AddReactionsAsync(Emojis.emojiJaren);
            }
        }

        public async Task RemoveYearAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser())
            {
                return;
            }

            if (channel is IPrivateChannel)
            {
                System.Console.WriteLine("RemoveYearAsync");

                SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole role;

                if (reaction.Emote.Equals(Emojis.JAAR_1_EMOJI))
                {
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                }
                else if (reaction.Emote.Equals(Emojis.JAAR_2_EMOJI))
                {
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                }

                else if (reaction.Emote.Equals(Emojis.JAAR_3_EMOJI))
                {
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                }
                else
                {
                    role = null;
                }

                if (role != null)
                {
                    await guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
                }
            }
        }
    }
}
