using Discord;
using System.Threading.Tasks;

namespace APTI_BOT
{
    internal class Program
    {
        /*
         * Contributors
         */
        private const string DISCORD_CHARACTER = "@";
        private const string DISCORD_MAXIM = DISCORD_CHARACTER + "mixxamm";
        private const string DISCORD_DANA = DISCORD_CHARACTER + "Ding Dong Gaming";
        private static readonly string BOT_CONTRIBUTORS = $"{DISCORD_MAXIM} of {DISCORD_DANA}";

        /*
         *  Jaar related
         */
        private static readonly Emoji JAAR_1_EMOJI = new Emoji("🥇");
        private static readonly Emoji JAAR_2_EMOJI = new Emoji("🥈");
        private static readonly Emoji JAAR_3_EMOJI = new Emoji("🥉");

        /*
         * Actie related
         */
        private static readonly Emoji ACCEPTEER_EMOJI = new Emoji("✅");
        private static readonly Emoji WEIGER_EMOJI = new Emoji("❌");
        private static readonly Emoji PIN_EMOJI = new Emoji("📌");

        /*
         * Emoji arrays
         */
        private static readonly Emoji[] emojiJaren = new Emoji[] { JAAR_1_EMOJI, JAAR_2_EMOJI, JAAR_3_EMOJI };
        private static readonly Emoji[] emojiVerificatie = new Emoji[] { ACCEPTEER_EMOJI, WEIGER_EMOJI };


        public static Task Main(string[] args)
        {
            return Startup.RunAsync(args);
        }
    }
}
