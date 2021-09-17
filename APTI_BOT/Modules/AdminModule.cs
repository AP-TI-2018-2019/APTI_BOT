using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Admin commando's")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        public static string VERSION_NR = "2.0.1";

        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;

        public AdminModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }


        [Command("corrigeerrichtingen")]
        [Summary(
            "Geeft alle bestaande gebruikers automatisch de juiste richting op basis van hun naam. OPGELET: gebruik dit zuinig en enkel als dit uw laatste keuze is.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CorrectUserRolesByCourseAsync()
        {
            var userToCheck = Context.User as SocketGuildUser;
            var adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            var hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }

            Console.WriteLine("CorrectUserRolesByCourseAsync");
            var tiRole = Context.Guild.GetRole(ulong.Parse(_config["ids:toegepasteinformatierol"]));
            var eictRole = Context.Guild.GetRole(ulong.Parse(_config["ids:elektronicaictrol"]));
            var progRole = Context.Guild.GetRole(ulong.Parse(_config["ids:programmerenrol"]));
            var users = Context.Guild.Users;

            foreach (var user in users)
            {
                if (user?.Nickname == null) continue;

                if (user.Nickname.Contains("EICT"))
                {
                    await user.AddRoleAsync(eictRole);
                    await user.RemoveRolesAsync(new List<IRole> { tiRole, progRole });
                }
                else if (user.Nickname.Contains("TI") || user.Nickname.Contains("IT"))
                {
                    await user.AddRoleAsync(tiRole);
                    await user.RemoveRolesAsync(new List<IRole> { eictRole, progRole });
                }
                else if (user.Nickname.Contains("PROG"))
                {
                    await user.AddRoleAsync(progRole);
                    await user.RemoveRolesAsync(new List<IRole> { eictRole, tiRole });
                }
            }
        }

        [Command("botmessage")]
        [Alias("dm")]
        [Summary("Laat de bot een bericht schrijven in een bepaald kanaal.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task LetBotMessageAsync(ulong channelId, [Remainder] string text)
        {
            var userToCheck = Context.User as SocketGuildUser;
            var adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            var hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }

            Console.WriteLine("MakeReminderAsync");
            var guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            var channel = guild.GetTextChannel(channelId);

            if (channel == null)
                await ReplyAsync("Er is geen tekstkanaal gevonden met de gegeven channel id.");
            else
                await channel.SendMessageAsync(text);
        }

        [Command("remindnonverified")]
        [Summary("Laat de bot de mensen met een niet geverifieerd rol aan herinneren zich te verifiëren.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemindNonVerifiedUsersAsync()
        {
            var userToCheck = Context.User as SocketGuildUser;
            var adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            var hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }

            Console.WriteLine("RemindNonVerifiedUsersAsync");
            var guild = Context.Guild;
            var channel = guild.GetTextChannel(ulong.Parse(_config["ids:nietgeverifieerdlog"]));
            var notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));

            if (channel == null)
            {
                await ReplyAsync("Er is geen 'niet-geverifieerd' tekstkanaal gevonden!");
                return;
            }

            await channel.SendMessageAsync(
                "@everyone Vergeet niet om je te laten verifiëren! Lukt dit niet, contacteer dan een van de beheerders. Wij ruimen namelijk alle gebruikers van deze server op die zich nog niet hebben geverifieerd.");

            var users = channel.Users;
            var nonVerifiedUsers = users.Where(user => user.Roles.Contains(notVerifiedRole));
            foreach (var nonVerifiedUser in nonVerifiedUsers)
            {
                Console.WriteLine($"DM verstuurd naar {nonVerifiedUser.Nickname}!");
                await nonVerifiedUser.SendMessageAsync(
                    $"Hey {nonVerifiedUser.Nickname}, ik wilde je even laten herinneren dat je je nog niet geverifieerd hebt in onze APTI-server. Probeer dit zo snel mogelijk te doen!\n\nps: Een sessie kan je starten door `!start` naar mij te sturen! :)");
            }
        }


        [Command("downloadusers")]
        [Summary("Laad alle gebruikers van de server in zodat de botcommando's deftig functioneren.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DownloadAllUserDataAsync()
        {
            var userToCheck = Context.User as SocketGuildUser;
            var adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            var role = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!role)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }

            Console.WriteLine("Start user download...");
            await Context.Guild.DownloadUsersAsync();
            Console.WriteLine("Download completed.");
        }

        [Command("version")]
        [Alias("v")]
        [Summary("Toon de versie van de bot.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GetBotVersion()
        {
            var userToCheck = Context.User as SocketGuildUser;
            var adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            var hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }

            Console.WriteLine("GetBotVersion");
            await ReplyAsync($"De bot draait op versie: {VERSION_NR}");
        }
    }
}