﻿using System;
using System.Threading.Tasks;
using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Reactie commando's")]
    public class ReactionModule : ModuleBase<SocketCommandContext>
    {
        private const int PIN_LIMIT = 50;
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public ReactionModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.ReactionAdded += PinAsync;
            _client.MessageReceived += RemoveSystemPinMessageAsync;
        }


        public async Task PinAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (!reaction.User.Value.IsAUser()) return;

            if (reaction.Emote.ToString().Equals(Emojis.PIN_EMOJI.ToString()))
            {
                Console.WriteLine("PinAsync");
                var pinnedMessages = await channel.GetPinnedMessagesAsync();
                if (pinnedMessages.Count >= PIN_LIMIT)
                {
                    await ((ISocketMessageChannel)_client.GetChannel(channel.Id)).SendMessageAsync(
                        "Het maximaal aantal gepinde berichten is overschreden. Roep een @Beheerder om de gepinde berichten te herevalueren.");
                    return;
                }

                Console.WriteLine("PinAsync");
                var messageToPin = await message.DownloadAsync();
                if (!messageToPin.IsPinned)
                {
                    await messageToPin.PinAsync();
                    var embedBuilder = new EmbedBuilder()
                        .WithTitle("Pinned");
                    try
                    {
                        embedBuilder = embedBuilder.AddField("Bericht", messageToPin.Content);
                    }
                    catch (ArgumentException)
                    {
                        foreach (var attachment in messageToPin.Attachments)
                            if (attachment.IsSpoiler())
                                embedBuilder = embedBuilder.AddField("Afbeelding", $"||{attachment.Url}||");
                            else
                                embedBuilder = embedBuilder.WithImageUrl(attachment.Url);
                    }

                    var embed = embedBuilder.AddField("Kanaal", $"<#{messageToPin.Channel.Id}>", true)
                        .AddField("Door", reaction.User.Value.Mention, true)
                        .WithAuthor(messageToPin.Author.ToString(), messageToPin.Author.GetAvatarUrl(),
                            messageToPin.GetJumpUrl())
                        .Build();
                    await ((ISocketMessageChannel)_client.GetChannel(ulong.Parse(_config["ids:pinlog"])))
                        .SendMessageAsync("", false, embed);
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