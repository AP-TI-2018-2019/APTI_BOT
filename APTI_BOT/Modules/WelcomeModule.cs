using APTI_BOT.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTI_BOT.Modules
{
    [Name("Welkom commando's")]
    public class WelcomeModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;

        public WelcomeModule(IConfigurationRoot config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
            _client.UserJoined += AnnounceJoinedUser;
        }

        private async Task AnnounceJoinedUser(SocketGuildUser arg)
        {
            if (!arg.IsAUser()) return;

            await arg.SendMessageAsync(GetWelcomeText());

            SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (!arg.Roles.Contains(_studentRole))
            {
                await arg.AddRoleAsync(_notVerifiedRole);
            }
        }

        [Command("start")]
        [Summary("Start de setup procedure voor nieuwe (of bestaande) leden. Zo kan je uw naam en rol aanpassen en eenmalig uw identiteit.")]
        public async Task WelcomeCommandMessageAsync()
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await Context.User.SendMessageAsync(GetWelcomeText());

            SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (!_guild.GetUser(Context.User.Id).Roles.Contains(_studentRole))
            {
                await _guild.GetUser(Context.User.Id).AddRoleAsync(_notVerifiedRole);
            }
        }

        private string GetWelcomeText()
        {
            StringBuilder text = new StringBuilder();
            text.Append("Hey, welkom in onze server!");
            text.Append(" Ik ben de APTI-bot en mijn doel is om het toetreden tot de server eenvoudiger te maken.");
            text.Append(" We zullen beginnen met je naam op de server in te stellen.");
            text.AppendLine(" Om dit te doen type je je naam en klas in het volgende formaat: `{Naam} - {Jaar}TI{Groep}` voorafgegeaan door `!naam`.");
            text.Append("Bijvoorbeeld: `!naam Maxim - 1TIC`.");

            return text.ToString();
        }
    }
}
