using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Verificatie commando's")]
    public class VerificatieModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

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
            if (!message.Author.IsAUser())
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
                Console.WriteLine("I'm making another embed!");
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                ISocketMessageChannel verificationLogChannel = ((ISocketMessageChannel)_guild.GetChannel(ulong.Parse(_config["ids:verificatielog"])));
                RestUserMessage verificationEmbed = await verificationLogChannel.SendMessageAsync("", false, embed);
                await verificationEmbed.AddReactionsAsync(Emojis.emojiVerificatie);
            }
        }

        public async Task VerifyIdAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser())
            {
                return;
            }

            if (reaction.Channel.Id == ulong.Parse(_config["ids:verificatielog"]))
            {
                Console.WriteLine("VerifyIdAsync");

                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                IEnumerator<IEmbed> embeds = message.DownloadAsync().Result.Embeds.GetEnumerator();
                embeds.MoveNext();
                bool isStudent = _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.Contains(_studentRole);
                if (!isStudent)
                {
                    if (reaction.Emote.ToString().Equals(Emojis.ACCEPTEER_EMOJI.ToString()) && !reaction.User.Value.IsBot)
                    {
                        SocketGuildUser user = _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value));
                        await user.AddRoleAsync(_studentRole);
                        await user.RemoveRoleAsync(_notVerifiedRole);


                        StringBuilder text = new StringBuilder();
                        text.Append("Jouw inzending werd zojuist goedgekeurd.");
                        text.Append(" Indien gewenst heb je nu de mogelijkheid om jouw verificatie-afbeelding te verwijderen.");
                        text.Append(" De volgende stap is het kiezen van jouw jaar  door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(" Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        IUserMessage sent = await user.SendMessageAsync(text.ToString());
                        await sent.AddReactionsAsync(Emojis.emojiJaren);
                    }
                    else if (reaction.Emote.ToString().Equals(Emojis.WEIGER_EMOJI.ToString()) && !reaction.User.Value.IsBot)
                    {
                        await _guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).SendMessageAsync("Jouw inzending werd afgekeurd. Dien een nieuwe foto in.");
                    }
                }
            }

            return;
        }

        public async Task AddYearAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser())
            {
                return;
            }

            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                Console.WriteLine("AddYearAsync");

                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                if (reaction.Emote.ToString() == Emojis.JAAR_1_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar1rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == Emojis.JAAR_2_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar2rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == Emojis.JAAR_3_EMOJI.ToString())
                {
                    SocketRole role = _guild.GetRole(ulong.Parse(_config["ids:jaar3rol"]));
                    await _guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
            }
        }
    }
}
