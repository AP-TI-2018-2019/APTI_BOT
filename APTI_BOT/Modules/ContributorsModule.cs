using APTI_BOT.Common;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Contributor commando's")]
    public class ContributorsModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public ContributorsModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        [Command("botcontributors")]
        [Summary("Vraag de contributors van de Discord bot op.")]
        public async Task AskContributorsAsync()
        {
            await ReplyAsync($"{Contributors.GetContributors()}", false, null);
        }
    }
}
