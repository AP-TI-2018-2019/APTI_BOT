namespace APTI_BOT
{
    class Config
    {
        public string DiscordToken { get; set; }
        public ulong ServerId { get; set; }
        public ulong PinLogId { get; set; }
        public ulong Jaar1RolId { get; set; }
        public ulong Jaar2RolId { get; set; }
        public ulong Jaar3RolId { get; set; }
        public ulong StudentRolId { get; set; }
        public ulong VerificatieId { get; set; }

        public Config(string discordToken, ulong serverId, ulong pinLogId, ulong verificatieId, ulong jaar1RolId, ulong jaar2RolId, ulong jaar3RolId, ulong studentRolId)
        {
            DiscordToken = discordToken;
            ServerId = serverId;
            PinLogId = pinLogId;
            VerificatieId = verificatieId;
            Jaar1RolId = jaar1RolId;
            Jaar2RolId = jaar2RolId;
            Jaar3RolId = jaar3RolId;
            StudentRolId = studentRolId;
        }
    }
}
