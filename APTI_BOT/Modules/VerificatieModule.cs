using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    public class VerificatieModule : ModuleBase<SocketCommandContext>
    { /*
         * Contributors
         */
        private const string DISCORD_CHARACTER = "@";
        private const string DISCORD_MAXIM = DISCORD_CHARACTER + "mixxamm#7308";
        private const string DISCORD_DANA = DISCORD_CHARACTER + "Ding Dong Gaming#8988";
        private static readonly string BOT_CONTRIBUTORS = $"{DISCORD_MAXIM} of {DISCORD_DANA}";

        /*
        *  Jaar related
        */
        private static readonly Emoji JAAR_1_EMOJI = new Emoji("🥇");
        private static readonly Emoji JAAR_2_EMOJI = new Emoji("🥈");
        private static readonly Emoji JAAR_3_EMOJI = new Emoji("🥉");

        /*
         * Actie related
         */
        private static readonly Emoji ACCEPTEER_EMOJI = new Emoji("✅");
        private static readonly Emoji WEIGER_EMOJI = new Emoji("❌");

        /*
         * Emoji arrays
         */
        private readonly Emoji[] emojiJaren = new Emoji[] { JAAR_1_EMOJI, JAAR_2_EMOJI, JAAR_3_EMOJI };
        private readonly Emoji[] emojiVerificatie = new Emoji[] { ACCEPTEER_EMOJI, WEIGER_EMOJI };

        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        private readonly SocketGuild guild;
        private readonly SocketRole studentRole;
        private readonly SocketRole notVerifiedRole;

        public VerificatieModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionRemoved += RemoveYearAsync;
            _client.ReactionAdded += VerifyIdAsync;
            _client.ReactionAdded += AddYearAsync;
            _client.MessageReceived += CreateEmbedInVerificationChannelAsync;

            guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
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

            if (!guild.GetUser(Context.User.Id).Roles.Contains(studentRole))
            {
                await guild.GetUser(Context.User.Id).AddRoleAsync(notVerifiedRole);
            }
        }

        public async Task CreateEmbedInVerificationChannelAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (message.Channel is IPrivateChannel && message.Source == MessageSource.User && message.Attachments.Count > 0)
            {
                // Verificatie ding
                EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("Verificatie student");

                foreach (IAttachment attachment in message.Attachments)
                {
                    if (attachment.IsSpoiler())
                    {
                        embedBuilder = embedBuilder.AddField("Foto", $"||{attachment.Url}||", false);
                    }
                    else
                    {
                        embedBuilder = embedBuilder.WithImageUrl(attachment.Url);
                    }
                }
                Embed embed = embedBuilder
                    .AddField("Id", message.Author.Id.ToString(), false)
                    .WithAuthor(message.Author.ToString(), message.Author.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .WithFooter(footer => footer.WithText($"Account gecreëerd op: {message.Author.CreatedAt}"))
                    .WithTimestamp(DateTime.Now.ToLocalTime())
                     .Build();
                RestUserMessage verification = await ((ISocketMessageChannel)_client.GetChannel(ulong.Parse(_config["ids:verificatielog"]))).SendMessageAsync("", false, embed);
                await verification.AddReactionsAsync(emojiVerificatie);
            }
        }

        [Command("naam")]
        [Summary("Stel je bijnaam van de server in.")]
        public async Task ChangeNameAsync([Remainder] string message)
        {
            if (Context.IsPrivate)
            {
                if (!Regex.Match(message, "[a-z]+ - [1-3]TI[A-Z]*").Success)
                {
                    await ReplyAsync("Je hebt je naam in een niet-geldig formaat ingevoerd. Gelieve het formaat te volgen.", false, null);
                    return;
                }
                message = message.Substring(0, 1).ToUpper() + message.Substring(1);
                SocketGuildUser user = guild.GetUser(Context.User.Id);
                try
                {
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = message;
                    });
                    System.Collections.Generic.IEnumerator<SocketRole> roles = guild.GetUser(Context.User.Id).Roles.GetEnumerator();
                    bool student = false;
                    while (roles.MoveNext())
                    {
                        if (roles.Current.Id == studentRole.Id)
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
                        await sent.AddReactionsAsync(emojiJaren);
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
                        await sent_error.AddReactionsAsync(emojiJaren);
                    }
                    else
                    {
                        StringBuilder text = new StringBuilder();
                        text.Append("Het instellen van je nickname is niet gelukt.");
                        text.Append(" Ik weet zelf niet wat er is fout gegaan.");
                        text.AppendLine($" Stuur een berichtje naar {BOT_CONTRIBUTORS} met een screenshot van dit bericht.");
                        text.AppendLine($"Foutcode: {e.HttpCode}");
                        text.AppendLine();
                        text.Append("Je kan voorlopig al wel je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        IUserMessage sent_error_unknown = await ReplyAsync(text.ToString());
                        await sent_error_unknown.AddReactionsAsync(emojiJaren);
                    }
                }
            }
        }

        [Command("jaar")]
        [Summary("Stel je jaar van de server in.")]
        public async Task ChangeYearAsync()
        {
            System.Console.WriteLine("ChangeYearAsync");
            if (Context.IsPrivate)
            {
                IUserMessage sent = await ReplyAsync("Kies je jaar door op één of meer van de emoji onder dit bericht te klikken.");
                await sent.AddReactionsAsync(emojiJaren);
            }
        }

        public async Task RemoveYearAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
            {
                return;
            }

            System.Console.WriteLine("RemoveYearAsync");
            if (channel is IPrivateChannel)
            {
                SocketRole role;

                if (reaction.Emote.Equals(JAAR_1_EMOJI))
                {
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                }
                else if (reaction.Emote.Equals(JAAR_2_EMOJI))
                {
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                }

                else if (reaction.Emote.Equals(JAAR_3_EMOJI))
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

        public async Task VerifyIdAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
            {
                return;
            }

            System.Console.WriteLine("VerifyIdAsync");

            if (reaction.Channel.Id == ulong.Parse(_config["ids:verificatielog"]))
            {
                var embeds = message.DownloadAsync().Result.Embeds.GetEnumerator();
                embeds.MoveNext();
                bool isStudent = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.Contains(studentRole);
                bool isNietVerificeerd = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.Contains(notVerifiedRole);
                if (!isStudent && isNietVerificeerd)
                {
                    if (reaction.Emote.ToString().Equals(ACCEPTEER_EMOJI.ToString()) && !reaction.User.Value.IsBot && channel is IPrivateChannel)
                    {
                        SocketGuildUser user = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value));
                        await user.AddRoleAsync(studentRole);

                        StringBuilder text = new StringBuilder();
                        text.Append("Jouw inzending werd zojuist goedgekeurd.");
                        text.Append(" De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(" Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        await guild.GetUser(reaction.UserId).RemoveRoleAsync(notVerifiedRole);
                        await guild.GetUser(reaction.UserId).AddRoleAsync(studentRole);
                        IUserMessage sent = await user.SendMessageAsync(text.ToString());
                        await sent.AddReactionsAsync(emojiJaren);
                    }
                    else if (reaction.Emote.ToString().Equals(WEIGER_EMOJI.ToString()) && !reaction.User.Value.IsBot && channel is IPrivateChannel)
                    {
                        await guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).SendMessageAsync("Jouw inzending werd afgekeurd. Dien een nieuwe foto in.");
                    }
                }
            }

            return;
        }

        public async Task AddYearAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
            {
                return;
            }

            System.Console.WriteLine("AddYearAsync");

            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                if (reaction.Emote.ToString() == JAAR_1_EMOJI.ToString())
                {
                    SocketRole role = guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == JAAR_2_EMOJI.ToString())
                {
                    SocketRole role = guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == JAAR_3_EMOJI.ToString())
                {
                    SocketRole role = guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
            }
        }
    }
}
