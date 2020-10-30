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
            //_client.UserJoined += AnnounceJoinedUserAsync;
            _client.MessageReceived += AnnounceJoinedUserMessageAsync;
        }

        private async Task AnnounceJoinedUserMessageAsync(SocketMessage msg)
        {
            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketGuildChannel channel = guild.GetChannel(ulong.Parse(_config["ids:welkomlog"]));
            if (msg.Source.Equals(MessageSource.System) && msg.Channel.Equals(channel))
            {
                await msg.Author.SendMessageAsync(GetWelcomeText());

                SocketRole studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
                SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
                SocketGuildUser guildUser = guild.GetUser(msg.Author.Id);

                if (!guildUser.Roles.Contains(studentRole))
                {
                    await guildUser.AddRoleAsync(notVerifiedRole);
                }
            }
        }

        private async Task AnnounceJoinedUserAsync(SocketGuildUser arg)
        {
            if (!arg.IsAUser())
            {
                return;
            }

            Console.WriteLine("AnnounceJoinedUserAsync");
            await arg.SendMessageAsync(GetWelcomeText());

            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketTextChannel welcomeChannel = guild.GetTextChannel(ulong.Parse(_config["ids:welkomlog"]));
            SocketRole studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (!arg.Roles.Contains(studentRole))
            {
                await arg.AddRoleAsync(notVerifiedRole);
            }
        }

        [Command("start")]
        [Summary("Start de setup procedure voor nieuwe (of bestaande) leden. Zo kan je uw naam en rol aanpassen en eenmalig uw identiteit.")]
        public async Task WelcomeCommandMessageAsync()
        {
            Console.WriteLine("WelcomeCommandMessageAsync");
            await Context.User.SendMessageAsync(GetWelcomeText());

            SocketGuild guild = _client.GetGuild(ulong.Parse(_config["ids:server"]));
            SocketRole studentRole = guild.GetRole(ulong.Parse(_config["ids:studentrol"]));
            SocketRole notVerifiedRole = guild.GetRole(ulong.Parse(_config["ids:nietgeverifieerdrol"]));
            if (!guild.GetUser(Context.User.Id).Roles.Contains(studentRole))
            {
                await guild.GetUser(Context.User.Id).AddRoleAsync(notVerifiedRole);
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
