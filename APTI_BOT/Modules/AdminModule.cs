using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Admin commando's")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public AdminModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }


        [Command("corrigeerrichtingen")]
        [Summary("Geeft alle bestaande gebruikers automatisch de juiste richting op basis van hun naam. OPGELET: gebruik dit zuinig en enkel als dit uw laatste keuze is.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CorrectUserRolesByCourseAsync()
        {
            SocketGuildUser userToCheck = Context.User as SocketGuildUser;
            SocketRole adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            bool role = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!role)
            {
                await ReplyAsync("U heeft geen recht om dit commando uit te voeren!");
                return;
            }
            else
            {

                await Context.Guild.DownloadUsersAsync();
                IReadOnlyCollection<SocketGuildUser> users = Context.Guild.Users;

                foreach (SocketGuildUser user in users)
                {
                    SocketRole tiRole = Context.Guild.GetRole(ulong.Parse(_config["ids:toegepasteinformatierol"]));
                    SocketRole eictRole = Context.Guild.GetRole(ulong.Parse(_config["ids:elektronicaictrol"]));

                    if (user == null || user.Nickname == null)
                    {
                        continue;
                    }

                    if (user.Nickname.Contains("EICT"))
                    {
                        await user.AddRoleAsync(eictRole);
                        await user.RemoveRoleAsync(tiRole);
                    }
                    else if (user.Nickname.Contains("TI"))
                    {
                        await user.AddRoleAsync(tiRole);
                        await user.RemoveRoleAsync(eictRole);
                    }
                }
            }
        }

        [Command("reminder")]
        [Summary("Laat de bot een bericht schrijven in een bepaald kanaal.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MakeReminderAsync(ulong channelId, [Remainder] string text)
        {
            SocketGuildUser userToCheck = Context.User as SocketGuildUser;
            SocketRole adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            bool role = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!role)
            {
                await ReplyAsync("U heeft geen recht om dit commando uit te voeren!");
                return;
            }
            else
            {
                SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                var channel = _guild.GetTextChannel(channelId);

                await channel.SendMessageAsync(text);
            }
        }
    }
}
