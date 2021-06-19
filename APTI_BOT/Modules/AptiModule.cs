using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("APTI commando's")]
    public class AptiModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public AptiModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("naam")]
        [Summary("Stel je bijnaam van de server in.")]
        public async Task ChangeNameAsync([Remainder] string message)
        {
            if (Context.IsPrivate)
            {
                Console.WriteLine("ChangeNameAsync");
                var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                var studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));

                if (!message.IsValidName())
                {
                    await ReplyAsync(
                        "Je hebt je naam in een niet-geldig formaat ingevoerd. Gelieve het volgende formaat te volgen: `!naam Jouwnaam - (1-3)(TI/EICT/PRO)(Optioneel: Specialisatie)(Optioneel: klasgroep)`.\nVoorbeeld 1: `!naam Dana - 3TIA`\nVoorbeeld 2: `!naam Kevin - 1EICTD`\nVoorbeeld 3: `!naam Yorgi - 1ITIOT1`\nVoorbeeld 3: `!naam Kobe - 1PRO1`");
                    return;
                }

                message = message.Substring(0, 1).ToUpper() + message.Substring(1);
                var user = guild.GetUser(Context.User.Id);
                try
                {
                    await user.ModifyAsync(x => { x.Nickname = message; });

                    using var roles = guild.GetUser(Context.User.Id).Roles.GetEnumerator();
                    var hasStudentRole = false;
                    while (roles.MoveNext())
                        if (roles.Current != null && roles.Current.Id == studentRole.Id)
                            hasStudentRole = true;
                    var text = new StringBuilder();
                    text.Append($"Je nickname is ingesteld op {message}.");
                    if (!hasStudentRole)
                    {
                        text.Append(" De volgende stap is verifiëren dat je een échte AP student bent.");
                        text.Append(
                            " Om dit te doen stuur je (**bij voorkeur**) een selfie van jou met jouw AP studentenkaart.");
                        text.Append(
                            " Voor de studenten die nog geen studentenkaart hebben, is er de mogelijkheid om een screenshot van jouw iBaMaFlex dossier door te sturen. Let wel op dat de verificatie hiervan langer kan duren aangezien er op strengere wijze geverifieerd zal worden.");
                        text.Append(" Zodra de verificatie is geslaagd, krijg je hier een bevestiging.\n\n");
                        text.Append(
                            "Opgepast: het verwijderen van jouw verificatie-afbeelding nog vóór uw verficatie is afgerond, zorgt ervoor dat we de foto niet kunnen zien. Zo kan de verificatie niet worden voltooid. Dit mag je doen nadat de verificatie is voltooid.");
                        await ReplyAsync(text.ToString());
                        await Context.User.SendFileAsync(@"../../../Assets/studentenkaart.png",
                            "In geval van een selfie: Zorg ervoor dat jouw gezicht goed zichtbaar is en de tekst van je studentenkaart leesbaar is.");
                        await Context.User.SendFileAsync(@"../../../Assets/ibamaflex.png",
                            "In geval van een screenshot van iBaMaFlex: Zorg ervoor dat je een screenshot neemt van dezelfde gegevens die hier worden getoond.");
                    }
                    else
                    {
                        text.Append(
                            " De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(
                            " Als je geen kanalen meer wil zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(
                            " Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        var sent = await ReplyAsync(text.ToString());
                        await sent.AddReactionsAsync(Emojis.emojiJaren);
                    }
                }
                catch (HttpException e)
                {
                    if (e.HttpCode == HttpStatusCode.Forbidden)
                    {
                        var text = new StringBuilder();
                        text.Append(
                            "Ik heb niet de machtigingen om jouw naam te veranderen, dit zal je zelf moeten doen.");
                        text.Append(" Als schrale troost mag je wel kiezen in welk jaar je zit :)");
                        var sentError = await ReplyAsync(text.ToString());
                        await sentError.AddReactionsAsync(Emojis.emojiJaren);
                    }
                    else
                    {
                        var text = new StringBuilder();
                        text.Append("Het instellen van je nickname is niet gelukt.");
                        text.Append(" Ik weet zelf niet wat er is fout gegaan.");
                        text.AppendLine(
                            $" Stuur een berichtje naar {Contributors.BOT_CONTRIBUTORS} met een screenshot van dit bericht.");
                        text.AppendLine($"Foutcode: {e.HttpCode}");
                        await ReplyAsync(text.ToString());
                    }
                }
                finally
                {
                    var tiRole = guild.GetRole(ulong.Parse(_config["ids:toegepasteinformatierol"]));
                    var eictRole = guild.GetRole(ulong.Parse(_config["ids:elektronicaictrol"]));
                    var proRole = Context.Guild.GetRole(ulong.Parse(_config["ids:programmerenrol"]));

                    if (user.Nickname.Contains("EICT"))
                    {
                        await user.AddRoleAsync(eictRole);
                        await user.RemoveRolesAsync(new List<IRole> {tiRole, proRole});
                    }
                    else if (user.Nickname.Contains("TI") || user.Nickname.Contains("IT"))
                    {
                        await user.AddRoleAsync(tiRole);
                        await user.RemoveRolesAsync(new List<IRole> {eictRole, proRole});
                    }
                    else if (user.Nickname.Contains("PRO"))
                    {
                        await user.AddRoleAsync(proRole);
                        await user.RemoveRolesAsync(new List<IRole> {eictRole, tiRole});
                    }
                }
            }
        }

        [Command("apti")]
        [Summary("Vraag de URL van onze site portaal op.")]
        public async Task AptiAsync()
        {
            Console.WriteLine("AptiAsync");
            await ReplyAsync($"{Urls.APTI_BASE_URL}");
        }

        [Command("yt")]
        [Alias("youtube")]
        [Summary("Vraag de URL van onze YouTube channel op.")]
        public async Task YouTubeAsync()
        {
            Console.WriteLine("YouTubeAsync");
            await ReplyAsync($"{Urls.APTI_YOUTUBE_URL}");
        }

        [Command("gt")]
        [Alias("github", "gh", "git")]
        [Summary("Vraag de URL van onze GitHub Repository op.")]
        public async Task GitHubAsync()
        {
            Console.WriteLine("GitHubAsync");
            await ReplyAsync($"{Urls.APTI_GITHUB_URL}");
        }

        [Command("dc")]
        [Alias("discord")]
        [Summary("Vraag de uitnodigingspagina van onze server op.")]
        public async Task DiscordAsync()
        {
            Console.WriteLine("DiscordAsync");
            await ReplyAsync($"{Urls.APTI_DISCORD_URL}");
        }
    }
}