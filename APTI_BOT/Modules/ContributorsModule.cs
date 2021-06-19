using System;
using System.Threading.Tasks;
using APTI_BOT.Common;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Contributor commando's")]
    public class ContributorsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public ContributorsModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("botcontributors")]
        [Summary("Vraag de contributors van de Discord bot op.")]
        public async Task AskContributorsAsync()
        {
            Console.WriteLine("AskContributorsAsync");
            await ReplyAsync($"{Contributors.GetContributors()}");
        }
    }
}