using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("APTI commando's")]
    public class AptiModule : ModuleBase<SocketCommandContext>
    {
        private const string APTI_BASE_URL = @"https://apti.be/";

        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly string _prefix;

        public AptiModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;

            _prefix = _config["prefix"];
        }

        [Command("apti")]
        [Summary("Vraag de URL van onze site portaal op.")]
        public async Task AptiAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}", false, null);
        }

        [Command("yt")]
        [Alias("youtube")]
        [Summary("Vraag de URL van onze YouTube channel op.")]
        public async Task YouTubeAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}/youtube", false, null);
        }

        [Command("gt")]
        [Alias("github")]
        [Summary("Vraag de URL van onze GitHub Repository op.")]
        public async Task GitHubAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}/github", false, null);
        }

        [Command("dc")]
        [Alias("discord")]
        [Summary("Vraag de uitnodigingspagina van onze server op.")]
        public async Task DiscordAsync()
        {
            await ReplyAsync($"{APTI_BASE_URL}/discord", false, null);
        }
    }
}
