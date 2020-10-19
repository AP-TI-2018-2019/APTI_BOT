using Discord;

namespace APTI_BOT.Common
{
    public static class Validations
    {
        public static bool IsAUser(this IUser user)
        {
            return !user.IsBot && !user.IsWebhook;
        }
    }
}
