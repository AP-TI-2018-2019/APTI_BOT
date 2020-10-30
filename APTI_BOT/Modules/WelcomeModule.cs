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
            _client.UserJoined += AnnounceJoinedUserAsync;
            //_client.MessageReceived += AnnounceJoinedUserMessageAsync;
        }

        private async Task AnnounceJoinedUserMessageAsync(SocketMessage msg)
        {
            SocketGuild _guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            var channel = _guild.GetChannel(ulong.Parse(_config["ids:welkomlog"]));
            if (msg.Source.Equals(MessageSource.System) && msg.Channel.Equals(channel))
            {
                await msg.Author.SendMessageAsync(GetWelcomeText());

                SocketRole _studentRole = _guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole _notVerifiedRole = _guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                var guildUser = _guild.GetUser(msg.Author.Id);
                
                if (!guildUser.Roles.Contains(_studentRole))
                {
                    await guildUser.AddRoleAsync(_notVerifiedRole);
                }
            }
        }

        private async Task AnnounceJoinedUserAsync(SocketGuildUser arg)
        {
            if (!arg.IsAUser())
            {
                return;
            }

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
            text.AppendLine(" Om dit te doen type je je naam en klas in het volgende formaat: `!naam {Naam} - {Jaar}(TI|EICT){Groep}`.");
            text.AppendLine("Voorbeeld 1: `!naam Maxim - 1TIC`");
            text.Append("Voorbeeld 2: `!naam Dana - 1EICT2`");

            return text.ToString();
        }
    }
}
