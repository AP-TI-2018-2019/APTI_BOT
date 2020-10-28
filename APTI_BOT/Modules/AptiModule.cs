using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
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

        [Command("naam")]
        [Summary("Stel je bijnaam van de server in.")]
        public async Task ChangeNameAsync([Remainder] string message)
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            if (Context.IsPrivate)
            {
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));

                if (!message.IsValidName())
                {
                    await ReplyAsync("Je hebt je naam in een niet-geldig formaat ingevoerd. Gelieve het volgende formaat te volgen: `!naam Jouwnaam - (1-3)(TI/EICT)(1-9/A-Z)` te volgen.\nVoorbeeld 1: `!naam Dana - 3TIA`\nVoorbeeld 2: `!naam Kevin - 1EICTD`", false, null);
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
                    bool hasStudentRole = false;
                    while (roles.MoveNext())
                    {
                        if (roles.Current.Id == _studentRole.Id)
                        {
                            hasStudentRole = true;
                        }
                    }
                    StringBuilder text = new StringBuilder();
                    text.Append($"Je nickname is ingesteld op {message}.");
                    if (!hasStudentRole)
                    {
                        text.Append(" De volgende stap is verifiëren dat je een échte AP student bent.");
                        text.Append(" Om dit te doen stuur je (**bij voorkeur**) een selfie van jou met jouw AP studentenkaart.");
                        text.Append(" Voor de studenten die nog geen studentenkaart hebben, is er de mogelijkheid om een screenshot van jouw iBaMaFlex dossier door te sturen. Let wel op dat de verificatie hiervan langer kan duren aangezien er op strengere wijze geverifieerd zal worden.");
                        text.Append(" Zodra de verificatie is geslaagd, krijg je hier een bevestiging.");
                        await ReplyAsync(text.ToString());
                        await Context.User.SendFileAsync(@"../../../Assets/studentenkaart.png", "In geval van een selfie: Zorg ervoor dat jouw gezicht goed zichtbaar is en de tekst van je studentenkaart leesbaar is.");
                        await Context.User.SendFileAsync(@"../../../Assets/ibamaflex.png", "In geval van een screenshot van iBaMaFlex: Zorg ervoor dat je een screenshot neemt van dezelfde gegevens die hier worden getoond.");
                    }
                    else
                    {
                        text.Append(" De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wil zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
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
                finally
                {
                    SocketRole tiRole = _guild.GetRole(ulong.Parse(_config["ids:toegepasteinformatierol"]));
                    SocketRole eictRole = _guild.GetRole(ulong.Parse(_config["ids:elektronicaictrol"]));

                    if (message.Contains("EICT"))
                    {
                        await user.AddRoleAsync(eictRole);
                        await user.RemoveRoleAsync(tiRole);
                    }
                    else if (message.Contains("TI"))
                    {
                        await user.AddRoleAsync(tiRole);
                        await user.RemoveRoleAsync(eictRole);
                    }
                }
            }
        }

        [Command("apti")]
        [Summary("Vraag de URL van onze site portaal op.")]
        public async Task AptiAsync()
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{Urls.APTI_BASE_URL}", false, null);
        }

        [Command("yt")]
        [Alias("youtube")]
        [Summary("Vraag de URL van onze YouTube channel op.")]
        public async Task YouTubeAsync()
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{Urls.APTI_YOUTUBE_URL}", false, null);
        }

        [Command("gt")]
        [Alias("github", "gh", "git")]
        [Summary("Vraag de URL van onze GitHub Repository op.")]
        public async Task GitHubAsync()
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{Urls.APTI_GITHUB_URL}", false, null);
        }

        [Command("dc")]
        [Alias("discord")]
        [Summary("Vraag de uitnodigingspagina van onze server op.")]
        public async Task DiscordAsync()
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{Urls.APTI_DISCORD_URL}", false, null);
        }
    }
}
