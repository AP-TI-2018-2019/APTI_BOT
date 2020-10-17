using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        public VerificatieModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionRemoved += RemoveYearAsync;
            _client.ReactionAdded += VerifyIdAsync;
            _client.ReactionAdded += AddYearAsync;
            _client.MessageReceived += CreateEmbedInVerificationChannelAsync;
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

        private async Task CreateEmbedInVerificationChannelAsync(SocketMessage message)
        {
            if (message.Channel is IPrivateChannel && message.Source == MessageSource.User && message.Attachments.Count > 0 && !message.Author.IsBot)
            {
                Console.WriteLine("I'm making another embed!");
                EmbedBuilder embedBuilder = new EmbedBuilder().WithTitle("Verificatie student");

                embedBuilder = embedBuilder.WithImageUrl(message.Attachments.FirstOrDefault().Url);

                Embed embed = embedBuilder
                    .AddField("Id", message.Author.Id.ToString(), false)
                    .WithAuthor(message.Author.ToString(), message.Author.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .WithFooter(footer => footer.WithText($"Account gecreëerd op: {message.Author.CreatedAt}"))
                    .WithTimestamp(DateTime.Now.ToLocalTime())
                    .Build();

                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                ISocketMessageChannel verificationLogChannel = (ISocketMessageChannel)_guild.GetChannel(ulong.Parse(_config["ids:verificatielog"]));

                IEnumerable<IMessage> messages = await verificationLogChannel.GetMessagesAsync(30).FlattenAsync();

                await verificationLogChannel.SendMessageAsync("", false, embed).Result.AddReactionsAsync(emojiVerificatie);
            }
        }

        [Command("naam")]
        [Summary("Stel je bijnaam van de server in.")]
        public async Task ChangeNameAsync([Remainder] string message)
        {
            if (Context.IsPrivate && !Context.User.IsBot)
            {
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                bool isInvalidName = false;
                if (!Regex.Match(message, "[a-z]+ - [1-3]TI[A-Z]*").Success)
                {
                    await ReplyAsync("Je hebt je naam in een niet-geldig formaat ingevoerd. Gelieve het formaat te volgen.", false, null);
                    isInvalidName = true;
                }

                if (!isInvalidName)
                {
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
        }

        [Command("jaar")]
        [Summary("Stel je jaar van de server in.")]
        public async Task ChangeYearAsync()
        {
            if (Context.IsPrivate && !Context.User.IsBot)
            {
                System.Console.WriteLine("ChangeYearAsync");
                IUserMessage sent = await ReplyAsync("Kies je jaar door op één of meer van de emoji onder dit bericht te klikken.");
                await sent.AddReactionsAsync(emojiJaren);
            }
        }

        private async Task RemoveYearAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                System.Console.WriteLine("RemoveYearAsync");

                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole role;

                if (reaction.Emote.Equals(JAAR_1_EMOJI))
                {
                    role = _guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                }
                else if (reaction.Emote.Equals(JAAR_2_EMOJI))
                {
                    role = _guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                }

                else if (reaction.Emote.Equals(JAAR_3_EMOJI))
                {
                    role = _guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                }
                else
                {
                    role = null;
                }

                if (role != null)
                {
                    await _guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
                }
            }
        }

        private async Task VerifyIdAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Channel.Id == ulong.Parse(_config["ids:verificatielog"]) && !reaction.User.Value.IsBot)
            {
                System.Console.WriteLine("VerifyIdAsync");
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                IEnumerator<IEmbed> embeds = message.DownloadAsync().Result.Embeds.GetEnumerator();
                embeds.MoveNext();
                bool isStudent = _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.Contains(_studentRole);
                bool isNietVerificeerd = _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.Contains(_notVerifiedRole);
                if (!isStudent && isNietVerificeerd)
                {
                    if (reaction.Emote.ToString().Equals(ACCEPTEER_EMOJI.ToString()) && !reaction.User.Value.IsBot && channel is IPrivateChannel)
                    {
                        SocketGuildUser user = _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value));
                        await user.AddRoleAsync(_studentRole);

                        StringBuilder text = new StringBuilder();
                        text.Append("Jouw inzending werd zojuist goedgekeurd.");
                        text.Append(" De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(" Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        await _guild.GetUser(reaction.UserId).RemoveRoleAsync(_notVerifiedRole);
                        await _guild.GetUser(reaction.UserId).AddRoleAsync(_studentRole);
                        IUserMessage sent = await user.SendMessageAsync(text.ToString());
                        await sent.AddReactionsAsync(emojiJaren);
                    }
                    else if (reaction.Emote.ToString().Equals(WEIGER_EMOJI.ToString()) && !reaction.User.Value.IsBot && channel is IPrivateChannel)
                    {
                        await _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).SendMessageAsync("Jouw inzending werd afgekeurd. Dien een nieuwe foto in.");
                    }
                }
            }
        }

        private async Task AddYearAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                System.Console.WriteLine("AddYearAsync");

                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                if (reaction.Emote.ToString() == JAAR_1_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == JAAR_2_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == JAAR_3_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
            }
        }
    }
}
