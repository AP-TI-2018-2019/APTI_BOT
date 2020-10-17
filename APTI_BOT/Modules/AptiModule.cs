using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("APTI commando's")]
    public class AptiModule : ModuleBase<SocketCommandContext>
    {
        private const string APTI_BASE_URL = @"https://apti.be/";
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public AptiModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("start")]
        [Summary("Start de setup procedure voor nieuwe (of bestaande) leden. Zo kan je uw naam en rol aanpassen en eenmalig uw identiteit.")]
        public async Task WelcomeDMMessageAsync()
        {
            StringBuilder text = new StringBuilder();
            text.Append("Hey, welkom in onze server!");
            text.Append(" Ik ben de APTI-bot en mijn doel is om het toetreden tot de server eenvoudiger te maken.");
            text.Append(" We zullen beginnen met je naam op de server in te stellen.");
            text.AppendLine(" Om dit te doen type je je naam en klas in het volgende formaat: `{Naam} - {Jaar}TI{Groep}` voorafgegeaan door `!naam`.");
            text.Append("Bijvoorbeeld: `!naam Maxim - 1TIC`.");
            await Context.User.SendMessageAsync(text.ToString());

            SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (!_guild.GetUser(Context.User.Id).Roles.Contains(_studentRole))
            {
                await _guild.GetUser(Context.User.Id).AddRoleAsync(_notVerifiedRole);
            }
        }

        [Command("apti")]
        [Summary("Vraag de URL van onze site portaal op.")]
        public async Task AptiAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}", false, null);
        }

        [Command("yt")]
        [Alias("youtube")]
        [Summary("Vraag de URL van onze YouTube channel op.")]
        public async Task YouTubeAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}youtube", false, null);
        }

        [Command("gt")]
        [Alias("github", "gh", "git")]
        [Summary("Vraag de URL van onze GitHub Repository op.")]
        public async Task GitHubAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}github", false, null);
        }

        [Command("dc")]
        [Alias("discord")]
        [Summary("Vraag de uitnodigingspagina van onze server op.")]
        public async Task DiscordAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}discord", false, null);
        }
    }
}
