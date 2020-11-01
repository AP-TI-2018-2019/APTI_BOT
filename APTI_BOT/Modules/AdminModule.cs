﻿using Discord;
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
            bool hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }
            else
            {
                Console.WriteLine("CorrectUserRolesByCourseAsync");
                SocketRole tiRole = Context.Guild.GetRole(ulong.Parse(_config["ids:toegepasteinformatierol"]));
                SocketRole eictRole = Context.Guild.GetRole(ulong.Parse(_config["ids:elektronicaictrol"]));
                IReadOnlyCollection<SocketGuildUser> users = Context.Guild.Users;

                foreach (SocketGuildUser user in users)
                {
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
            bool hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }
            else
            {
                Console.WriteLine("MakeReminderAsync");
                SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
                SocketTextChannel channel = guild.GetTextChannel(channelId);

                if (channel == null)
                {
                    await ReplyAsync("Er is geen tekstkanaal gevonden met de gegeven channel id.");
                }
                else
                {
                    await channel.SendMessageAsync(text);
                }
            }
        }

        [Command("remindnonverified")]
        [Summary("Laat de bot de mensen met een niet geverifieerd rol aan herinneren zich te verifiëren.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemindNonVerifiedUsersAsync()
        {
            SocketGuildUser userToCheck = Context.User as SocketGuildUser;
            SocketRole adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            bool hasAdminRole = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!hasAdminRole)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }
            else
            {
                Console.WriteLine("RemindNonVerifiedUsersAsync");
                SocketGuild guild = Context.Guild;
                SocketTextChannel channel = guild.GetTextChannel(ulong.Parse(_config["ids:nietgeverifieerdlog"]));
                SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));

                if (channel == null)
                {
                    await ReplyAsync("Er is geen 'niet-geverifieerd' tekstkanaal gevonden!");
                    return;
                }
                else
                {
                    await channel.SendMessageAsync("@everyone Vergeet niet om je te laten verifiëren! Lukt dit niet, contacteer dan een van de beheerders. Wij ruimen namelijk alle gebruikers van deze server op die zich nog niet hebben geverifieerd.");

                    IReadOnlyCollection<SocketGuildUser> users = channel.Users;
                    IEnumerable<SocketGuildUser> nonVerifiedUsers = users.Where(user => user.Roles.Contains(notVerifiedRole));
                    foreach (SocketGuildUser nonVerifiedUser in nonVerifiedUsers)
                    {
                        Console.WriteLine($"DM verstuurd naar {nonVerifiedUser.Nickname}!");
                        await nonVerifiedUser.SendMessageAsync($"Hey {nonVerifiedUser.Nickname}, ik wilde je even laten herinneren dat je je nog niet geverifieerd hebt in onze APTI-server. Probeer dit zo snel mogelijk te doen!\n\nps: Een sessie kan je starten door `!start` naar mij te sturen! :)");
                    }
                }
            }
        }


        [Command("downloadusers")]
        [Summary("Laad alle gebruikers van de server in zodat de botcommando's deftig functioneren.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DownloadAllUserDataAsync()
        {
            SocketGuildUser userToCheck = Context.User as SocketGuildUser;
            SocketRole adminRole = Context.Guild.GetRole(ulong.Parse(_config["ids:beheerderrol"]));
            bool role = (userToCheck as IGuildUser).Guild.Roles.Contains(adminRole);

            if (!role)
            {
                await ReplyAsync("U heeft niet voldoende rechten om dit commando uit te voeren!");
                return;
            }
            else
            {
                Console.WriteLine("Start user download...");
                await Context.Guild.DownloadUsersAsync();
                Console.WriteLine("Download completed.");
            }
        }
    }
}
