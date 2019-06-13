namespace APTI_BOT
{
    class Config
    {
        public string DiscordToken { get; set; }
        public ulong ServerId { get; set; }
        public ulong PinLogId { get; set; }

        public Config(string discordToken, ulong serverId, ulong pinLogId)
        {
            DiscordToken = discordToken;
            ServerId = serverId;
            PinLogId = pinLogId;
        }
    }
}
