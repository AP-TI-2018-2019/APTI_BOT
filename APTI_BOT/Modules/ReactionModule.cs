using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Reactie commando's")]
    public class ReactionModule : ModuleBase<SocketCommandContext>
    {
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
        private static readonly Emoji PIN_EMOJI = new Emoji("📌");

        /*
         * Emoji arrays
         */
        private readonly Emoji[] emojiJaren = new Emoji[] { JAAR_1_EMOJI, JAAR_2_EMOJI, JAAR_3_EMOJI };
        private readonly Emoji[] emojiVerificatie = new Emoji[] { ACCEPTEER_EMOJI, WEIGER_EMOJI };

        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public ReactionModule(CommandService service, IConfigurationRoot config, DiscordSocketClient client)
        {
            _service = service;
            _config = config;
            _client = client;
            _client.ReactionRemoved += YearRemovedAsync;
            _client.ReactionAdded += VerifieerFotoAsync;
            _client.ReactionAdded += YearAddedAsync;
            _client.ReactionAdded += PinAsync;
        }

        public async Task YearRemovedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IPrivateChannel)
            {
                SocketRole role;

                SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));

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

        public async Task VerifieerFotoAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            if (channel.Id == ulong.Parse(_config["ids:verificatielog"]))
            {
                System.Collections.Generic.IEnumerator<IEmbed> embeds = message.DownloadAsync().Result.Embeds.GetEnumerator();
                embeds.MoveNext();
                System.Collections.Generic.IEnumerator<SocketRole> roles = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).Roles.GetEnumerator();
                bool student = false;
                while (roles.MoveNext())
                {
                    if (roles.Current.Id == ulong.Parse(_config["ids:studentrol"]))
                    {
                        student = true;
                    }
                }
                if (!student)
                {
                    if (reaction.Emote.ToString() == ACCEPTEER_EMOJI.ToString() && !reaction.User.Value.IsBot)
                    {
                        SocketGuildUser user = guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value));
                        await user.AddRoleAsync(guild.GetRole(ulong.Parse(_config["ids:studentrol"])));

                        StringBuilder text = new StringBuilder();
                        text.Append("Jouw inzending werd zojuist goedgekeurd.");
                        text.Append(" De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht.");
                        text.Append(" Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen.");
                        text.Append(" Als je geen kanalen meer wilt zien van een jaar, dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        text.Append(" Als je jaar niet verandert, dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                        IUserMessage sent = await user.SendMessageAsync(text.ToString());
                        await sent.AddReactionsAsync(emojiJaren);
                    }
                    else if (reaction.Emote.ToString() == WEIGER_EMOJI.ToString() && !reaction.User.Value.IsBot)
                    {
                        await guild.GetUser(ulong.Parse(embeds.Current.Fields[0].Value)).SendMessageAsync("Jouw inzending werd afgekeurd. Dien een nieuwe foto in.");
                    }
                }
            }
        }

        public async Task YearAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
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
                SocketRole studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                await guild.GetUser(reaction.UserId).AddRoleAsync(studentRole);
            }
        }

        public async Task PinAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.ToString() == PIN_EMOJI.ToString())
            {
                IUserMessage messageToPin = (IUserMessage)await channel.GetMessageAsync(message.Id);
                if (!messageToPin.IsPinned)
                {
                    await messageToPin.PinAsync();
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithTitle("Pinned");
                    try
                    {
                        embedBuilder = embedBuilder.AddField("Bericht", messageToPin.Content, false);
                    }
                    catch (System.ArgumentException)
                    {
                        foreach (IAttachment attachment in messageToPin.Attachments)
                        {
                            if (attachment.IsSpoiler())
                            {
                                embedBuilder = embedBuilder.AddField("Afbeelding", $"||{attachment.Url}||", false);
                            }
                            else
                            {
                                embedBuilder = embedBuilder.WithImageUrl(attachment.Url);
                            }
                        }
                    }
                    Embed embed = embedBuilder.AddField("Kanaal", $"<#{messageToPin.Channel.Id}>", true)
                    .AddField("Door", reaction.User.Value.Mention, true)
                    .WithAuthor(messageToPin.Author.ToString(), messageToPin.Author.GetAvatarUrl(), messageToPin.GetJumpUrl())
                    .Build();
                    await ((ISocketMessageChannel)_client.GetChannel(ulong.Parse(_config["ids:pinlog"]))).SendMessageAsync("", false, embed);
                }
            }
        }
    }
}
