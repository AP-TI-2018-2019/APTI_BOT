using System.Collections.Generic;
using System.Linq;

namespace APTI_BOT.Common
{
    public class Contributors
    {
        /*
         * Contributors
         */
        private static readonly string DISCORD_CHARACTER = "@";
        private static readonly string DISCORD_MAXIM = DISCORD_CHARACTER + "mixxamm#7308";
        private static readonly string DISCORD_DANA = DISCORD_CHARACTER + "Ding Dong Gaming#8988";

        public static readonly List<string> BOT_CONTRIBUTORS = new List<string> { DISCORD_MAXIM, DISCORD_DANA };

        public static string GetContributors()
        {
            return BOT_CONTRIBUTORS.Count() > 1
                ? string.Join(", ", BOT_CONTRIBUTORS.Take(BOT_CONTRIBUTORS.Count() - 1)) + " en " +
                  BOT_CONTRIBUTORS.Last()
                : BOT_CONTRIBUTORS.FirstOrDefault();
        }
    }
}