using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("APTI commando's")]
    public class AptiModule : ModuleBase<SocketCommandContext>
    {
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

        [Command("naam")]
        [Summary("Stel je bijnaam van de server in.")]
        public async Task ChangeNameAsync([Remainder] string message)
        {
            if (Context.User.IsBot)
            {
                return;
            }

            if (Context.IsPrivate)
            {
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));

                if (!Regex.Match(message, "[a-zA-Z][a-z]+ - [1-3]TI[A-Z]*").Success)
                {
                    await ReplyAsync("Je hebt je naam in een niet-geldig formaat ingevoerd. Gelieve het formaat te volgen.", false, null);
                    return;
                }
                message = message.Substring(0, 1).ToUpper() + message.Substring(1);
                SocketGuildUser user = _guild.GetUser(Context.User.Id);
                try
                {
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = message;
                    });
                    System.Collections.Generic.IEnumerator<SocketRole> roles = _guild.GetUser(Context.User.Id).Roles.GetEnumerator();
                    bool student = false;
                    while (roles.MoveNext())
                    {
                        if (roles.Current.Id == _studentRole.Id)
                        {
                            student = true;
                        }
                    }
                    StringBuilder text = new StringBuilder();
                    text.Append($"Je nickname is ingesteld op {message}.");
                    if (!student)
                    {
                        text.Append(" De volgende stap is verifiëren dat je een échte AP student bent.");
                        text.Append(" Om dit te doen stuur je een selfie met jouw AP studentenkaart.");
                        text.Append(" Zodra de verificatie is geslaagd, krijg je hier een bevestiging.");
                        await ReplyAsync(text.ToString());
                        await Context.User.SendFileAsync(@"../../../Assets/studentenkaart.png", "Zorg ervoor dat jouw gezicht goed zichtbaar is en de tekst van je studentenkaart leesbaar is.");
                    }
                    else
                    {
                        text.Append(" De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(" Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        IUserMessage sent = await ReplyAsync(text.ToString());
                        await sent.AddReactionsAsync(Emojis.emojiJaren);
                    }

                }
                catch (Discord.Net.HttpException e)
                {
                    if (e.HttpCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        StringBuilder text = new StringBuilder();
                        text.Append("Ik heb niet de machtigingen om jouw naam te veranderen, dit zal je zelf moeten doen.");
                        text.Append(" Als schrale troost mag je wel kiezen in welk jaar je zit :)");
                        IUserMessage sent_error = await ReplyAsync(text.ToString());
                        await sent_error.AddReactionsAsync(Emojis.emojiJaren);
                    }
                    else
                    {
                        StringBuilder text = new StringBuilder();
                        text.Append("Het instellen van je nickname is niet gelukt.");
                        text.Append(" Ik weet zelf niet wat er is fout gegaan.");
                        text.AppendLine($" Stuur een berichtje naar {Contributors.BOT_CONTRIBUTORS} met een screenshot van dit bericht.");
                        text.AppendLine($"Foutcode: {e.HttpCode}");
                        text.AppendLine();
                        text.Append("Je kan voorlopig al wel je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        IUserMessage sent_error_unknown = await ReplyAsync(text.ToString());
                        await sent_error_unknown.AddReactionsAsync(Emojis.emojiJaren);
                    }
                }
            }
        }

        [Command("apti")]
        [Summary("Vraag de URL van onze site portaal op.")]
        public async Task AptiAsync()
        {
            await ReplyAsync($"{Urls.APTI_BASE_URL}", false, null);
        }

        [Command("yt")]
        [Alias("youtube")]
        [Summary("Vraag de URL van onze YouTube channel op.")]
        public async Task YouTubeAsync()
        {
            await ReplyAsync($"{Urls.APTI_YOUTUBE_URL}", false, null);
        }

        [Command("gt")]
        [Alias("github", "gh", "git")]
        [Summary("Vraag de URL van onze GitHub Repository op.")]
        public async Task GitHubAsync()
        {
            await ReplyAsync($"{Urls.APTI_GITHUB_URL}", false, null);
        }

        [Command("dc")]
        [Alias("discord")]
        [Summary("Vraag de uitnodigingspagina van onze server op.")]
        public async Task DiscordAsync()
        {
            await ReplyAsync($"{Urls.APTI_DISCORD_URL}", false, null);
        }
    }
}
