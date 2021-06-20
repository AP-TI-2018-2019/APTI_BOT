using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Services
{
    public class StartupService
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            var discordToken = _config["tokens:discord"]; // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception(
                    "Please enter your bot's token into the `_config.yml` file found in the applications root directory.");

            await _discord.LoginAsync(TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync(); // Connect to the websocket

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(),
                _provider); // Load commands and modules into the command service
        }
    }
}