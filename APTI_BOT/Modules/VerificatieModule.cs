using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Verificatie commando's")]
    public class VerificatieModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public VerificatieModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionAdded += VerifyIdAsync;
            _client.ReactionAdded += AddYearAsync;
            _client.MessageReceived += CreateEmbedInVerificationChannelAsync;
        }

        public async Task CreateEmbedInVerificationChannelAsync(SocketMessage message)
        {
            if (!message.Author.IsAUser()) return;

            if (message.Channel is IPrivateChannel && message.Source == MessageSource.User &&
                message.Attachments.Count > 0)
            {
                Console.WriteLine("I'm making another embed!");
                var embedBuilder = new EmbedBuilder().WithTitle("Verificatie student");
                foreach (IAttachment attachment in message.Attachments)
                    if (attachment.IsSpoiler())
                        embedBuilder = embedBuilder.AddField("Foto", $"||{attachment.Url}||");
                    else
                        embedBuilder = embedBuilder.WithImageUrl(attachment.Url);

                var embed = embedBuilder
                    .AddField("Id", message.Author.Id.ToString())
                    .WithAuthor(message.Author.ToString(), message.Author.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .WithFooter(footer => footer.WithText($"Account gecreëerd op: {message.Author.CreatedAt}"))
                    .WithTimestamp(DateTime.Now.ToLocalTime())
                    .Build();
                var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                var verificationLogChannel =
                    (ISocketMessageChannel)guild.GetChannel(ulong.Parse(_config["ids:verificatielog"]));
                var verificationEmbed = await verificationLogChannel.SendMessageAsync("", false, embed);
                await verificationEmbed.AddReactionsAsync(Emojis.emojiVerificatie);
            }
        }

        public async Task VerifyIdAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser()) return;

            if (reaction.Channel.Id == ulong.Parse(_config["ids:verificatielog"]))
            {
                Console.WriteLine("VerifyIdAsync");

                var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                var studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                var notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                var embeds = message.DownloadAsync().Result.Embeds.GetEnumerator();
                embeds.MoveNext();
                var isStudent = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles
                    .Contains(studentRole);
                if (!isStudent)
                {
                    if (reaction.Emote.ToString().Equals(Emojis.ACCEPTEER_EMOJI.ToString()) &&
                        !reaction.User.Value.IsBot)
                    {
                        var user = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value));
                        await user.AddRoleAsync(studentRole);
                        await user.RemoveRoleAsync(notVerifiedRole);


                        var text = new StringBuilder();
                        text.Append("Jouw inzending werd zojuist goedgekeurd.");
                        text.AppendLine(
                            " Indien gewenst heb je nu de mogelijkheid om jouw verificatie-afbeelding te verwijderen.");
                        text.AppendLine(
                            "Naast deel te nemen van de server, heb je ook de mogelijkheid om deel te nemen aan de studentenvereniging van de richting! Neem eens een kijkje in onze #bovis-grafica kanaal voor meer informatie.");
                        text.Append(
                            " De volgende stap is het kiezen van jouw jaar  door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(
                            " Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(
                            " Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        var sent = await user.SendMessageAsync(text.ToString());
                        await sent.AddReactionsAsync(Emojis.emojiJaren);
                    }
                    else if (reaction.Emote.ToString().Equals(Emojis.WEIGER_EMOJI.ToString()) &&
                             !reaction.User.Value.IsBot)
                    {
                        await guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value))
                            .SendMessageAsync("Jouw inzending werd afgekeurd. Dien een nieuwe foto in.");
                    }
                }
            }
        }

        public async Task AddYearAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser()) return;

            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                Console.WriteLine("AddYearAsync");

                var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole role = null;

                if (reaction.Emote.ToString() == Emojis.JAAR_1_EMOJI.ToString())
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                else if (reaction.Emote.ToString() == Emojis.JAAR_2_EMOJI.ToString())
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                else if (reaction.Emote.ToString() == Emojis.JAAR_3_EMOJI.ToString())
                    role = guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));

                if (role != null)
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
            }
        }
    }
}