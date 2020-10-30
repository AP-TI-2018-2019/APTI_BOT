using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Reactie commando's")]
    public class ReactionModule : ModuleBase<SocketCommandContext>
    {
        private const int PIN_LIMIT = 50;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public ReactionModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionAdded += PinAsync;
            _client.MessageReceived += RemoveSystemPinMessageAsync;
        }


        public async Task PinAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser())
            {
                return;
            }

            if (reaction.Emote.ToString().Equals(Emojis.PIN_EMOJI.ToString()))
            {
                Console.WriteLine("PinAsync");
                IReadOnlyCollection<Discord.Rest.RestMessage> pinnedMessages = await channel.GetPinnedMessagesAsync();
                if (pinnedMessages.Count >= PIN_LIMIT)
                {
                    await ((ISocketMessageChannel)_client.GetChannel(channel.Id)).SendMessageAsync($"Het maximaal aantal gepinde berichten is overschreden. Roep een @Beheerder om de gepinde berichten te herevalueren.", false, null);
                }
                else
                {
                    Console.WriteLine("PinAsync");
                    IUserMessage messageToPin = (await message.DownloadAsync());
                    if (!messageToPin.IsPinned)
                    {
                        await messageToPin.PinAsync();
                        EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithTitle("Pinned");
                        try
                        {
                            embedBuilder = embedBuilder.AddField("Bericht", messageToPin.Content, false);
                        }
                        catch (ArgumentException)
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
                        await ((ISocketMessageChannel)_client.GetChannel(channel.Id)).SendMessageAsync($"Er zijn in totaal {pinnedMessages.Count + 1} gepinde berichten. Er kunnen nog {PIN_LIMIT - pinnedMessages.Count - 1} berichten worden gepind.", false, null);
                    }
                }
            }
        }

        private async Task RemoveSystemPinMessageAsync(SocketMessage message)
        {
            if (message.Source == MessageSource.System && message.Author.Id == _client.CurrentUser.Id)
            {
                Console.WriteLine("RemovePinMessageAsync");
                await message.DeleteAsync();
            }
        }
    }
}
