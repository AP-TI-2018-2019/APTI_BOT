using Discord.Commands;

namespace APTI_BOT.Modules
{
    [Name("Feedback commando's")]
    public class FeedbackModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public AptiModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }
    }
}
