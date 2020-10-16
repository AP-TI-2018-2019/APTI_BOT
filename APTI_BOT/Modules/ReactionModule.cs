using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Reactie commando's")]
    public class ReactionModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Emoji PIN_EMOJI = new Emoji("📌");

        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public ReactionModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionAdded += PinAsync;
            //_client.MessageReceived += RemovePinMessageAsync;
        }


        public async Task PinAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
            {
                return;
            }

            System.Console.WriteLine("PinAsync");
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
        //private async Task RemovePinMessageAsync(SocketMessage message)
        //{
        //    System.Console.WriteLine("RemovePinMessageAsync");
        //    if (message.Source == MessageSource.System && message.Author.Id == _client.CurrentUser.Id)
        //    {
        //        await message.DeleteAsync();
        //    }
        //}
    }
}
