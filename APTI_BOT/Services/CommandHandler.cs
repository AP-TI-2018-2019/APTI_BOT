using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace APTI_BOT.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        public CommandHandler(
            DiscordSocketClient client,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _client = client;
            _commands = commands;
            _config = config;
            _provider = provider;

            _client.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            // Ensure the message is from a user/bot
            if (!(s is SocketUserMessage msg))
            {
                return;
            }

            if (msg.Author.Id == _client.CurrentUser.Id)
            {
                return;     // Ignore self when checking commands
            }

            SocketCommandContext context = new SocketCommandContext(_client, msg);     // Create the command context

            int argPos = 0;     // Check if the message has a valid command prefix
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _provider);     // Execute the command

                if (!result.IsSuccess)     // If not successful, reply with the error.
                {
                    await context.Channel.SendMessageAsync("Dit commando werd niet gevonden! Kijk na of je typefouten hebt gemaakt.");
                }
            }
        }
    }
}
