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
    [Name("Feedback commando's")]
    public class FeedbackModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public FeedbackModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("feedback")]
        [Alias("feedb")]
        [Summary(
            "Geef feedback over onze discord kanaal. Wat je graag zou zien, wat er ontbreekt of wat er van functionaliteiten toegevoegd mag worden!")]
        public async Task FeedbackAsync([Remainder] string feedback)
        {
            var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            var notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (guild.GetUser(Context.User.Id).Roles.Contains(notVerifiedRole))
            {
                await Context.User.SendMessageAsync(
                    "Niet geverifieerde studenten kunnen geen feedback geven! Verifieer je om dit te kunnen doen.");
                return;
            }

            Console.WriteLine("FeedbackAsync");
            var feedbackChannel = guild.GetTextChannel(ulong.Parse(_config["ids:feedbacklog"]));

            var message = await feedbackChannel.SendMessageAsync($"{feedback}");
            await message.AddReactionsAsync(Emojis.emojiVerificatie);
        }

        [Command("studvert")]
        [Alias("sv", "studentenvertegenwoordiger", "studentenvertegenwoordigers", "studvertegenwoordiger")]
        [Summary("Geef feedback over de richting, examenrooster")]
        public async Task StudentRepresentativeAsync([Remainder] string feedback)
        {
            var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            var notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (guild.GetUser(Context.User.Id).Roles.Contains(notVerifiedRole))
            {
                await Context.User.SendMessageAsync(
                    "Niet geverifieerde studenten kunnen geen feedback geven! Verifieer je om dit te kunnen doen.");
                return;
            }

            Console.WriteLine("StudentRepresentativeAsync");
            var studVertChannel = guild.GetTextChannel(ulong.Parse(_config["ids:studvertlog"]));

            var message = await studVertChannel.SendMessageAsync($"{feedback}");
            await message.AddReactionsAsync(Emojis.emojiVerificatie);
        }
    }
}