using Discord;
using System.Text.RegularExpressions;

namespace APTI_BOT.Common
{
    public static class Validations
    {
        public static bool IsAUser(this IUser user)
        {
            if (user == null)
            {
                return false;
            }

            return !user.IsBot && !user.IsWebhook;
        }

        public static bool IsValidName(this string name)
        {
            return Regex.Match(name, "[a-zA-Z][a-z]+ - [1-3](TI|EICT|IT)[A-Z1-9]?").Success;
        }
    }
}
