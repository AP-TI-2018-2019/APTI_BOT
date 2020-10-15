using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    public class VerificatieModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly string _prefix;

        public VerificatieModule(CommandService service, IConfigurationRoot config, DiscordSocketClient client)
        {
            _service = service;
            _config = config;
            _client = client;
            _client.MessageReceived += MessageReceivedAsync;

            _prefix = _config["prefix"];
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {

        }
    }
}
