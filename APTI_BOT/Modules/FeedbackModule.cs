using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Feedback commando's")]
    public class FeedbackModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public FeedbackModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("feedback")]
        [Alias("feedb")]
        [Summary("Geef feedback over onze discord kanaal. Wat je graag zou zien, wat er ontbreekt of wat er van functionaliteiten toegevoegd mag worden!")]
        public async Task FeedbackAsync([Remainder] string feedback)
        {
            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (guild.GetUser(Context.User.Id).Roles.Contains(notVerifiedRole))
            {
                await Context.User.SendMessageAsync("Niet geverifieerde studenten kunnen geen feedback geven! Verifieer je om dit te kunnen doen.");
                return;
            }
            System.Console.WriteLine("FeedbackAsync");
            SocketTextChannel feedbackChannel = guild.GetTextChannel(ulong.Parse(_config["ids:feedbacklog"]));

            Discord.Rest.RestUserMessage message = await feedbackChannel.SendMessageAsync($"{feedback}");
            await message.AddReactionsAsync(Emojis.emojiVerificatie);
        }

        [Command("studvert")]
        [Alias("sv", "studentenvertegenwoordiger", "studentenvertegenwoordigers", "studvertegenwoordiger")]
        [Summary("Geef feedback over de richting, examenrooster")]
        public async Task StudentRepresentativeAsync([Remainder] string feedback)
        {
            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (guild.GetUser(Context.User.Id).Roles.Contains(notVerifiedRole))
            {
                await Context.User.SendMessageAsync("Niet geverifieerde studenten kunnen geen feedback geven! Verifieer je om dit te kunnen doen.");
                return;
            }
            System.Console.WriteLine("StudentRepresentativeAsync");
            SocketTextChannel studVertChannel = guild.GetTextChannel(ulong.Parse(_config["ids:studvertlog"]));

            Discord.Rest.RestUserMessage message = await studVertChannel.SendMessageAsync($"{feedback}");
            await message.AddReactionsAsync(Emojis.emojiVerificatie);
        }
    }
}
