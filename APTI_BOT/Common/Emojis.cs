using Discord;

namespace APTI_BOT.Common
{
    public class Emojis
    {
        /*
        *  Jaar related
        */
        public static readonly Emoji JAAR_1_EMOJI = new Emoji("🥇");
        public static readonly Emoji JAAR_2_EMOJI = new Emoji("🥈");
        public static readonly Emoji JAAR_3_EMOJI = new Emoji("🥉");

        /*
         * Actie related
         */
        public static readonly Emoji ACCEPTEER_EMOJI = new Emoji("✅");
        public static readonly Emoji WEIGER_EMOJI = new Emoji("❌");
        public static readonly Emoji PIN_EMOJI = new Emoji("📌");

        /*
         * Emoji arrays
         */
        public static readonly IEmote[] emojiJaren = { JAAR_1_EMOJI, JAAR_2_EMOJI, JAAR_3_EMOJI };
        public static readonly IEmote[] emojiVerificatie = { ACCEPTEER_EMOJI, WEIGER_EMOJI };
    }
}