using APTI_BOT.Common;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace APTI_BOT.Modules
{
    [Name("Standaard commando's")]
    public class StandardModule : ModuleBase<SocketCommandContext>
    {
        public StandardModule()
        {
        }

        [Command("herinner mij om")]
        [Alias("herinner me om", "herinner om")]
        [Summary("Laat de bot je iets herinneren door een bericht en een datum mee te geven, bv. '!herinner mij om de planten buiten te zetten op 15/10/2020'.")]
        public async Task RemindAsync([Remainder] string bericht)
        {
            string[] splitsing = bericht.Split("op");
            string boodschap = splitsing[0];

            if (!DateTime.TryParse(splitsing[1], out DateTime datum))
            {
                await ReplyAsync($"De ingevoerde datum is ongeldig!", false, null);
            }
            else if (datum < DateTime.Now)
            {
                await ReplyAsync($"De ingevoerde datum bevindt zich in het verleden!", false, null);
            }
            else
            {
                await ReplyAsync($"Oké: ik zal '{boodschap}' naar je sturen op {datum}", false, null);

                Timer timer = new Timer((datum - DateTime.Now).TotalMilliseconds);
                timer.Elapsed += async (sender, e) => await NotifyUserAsync(sender, e, Context.User, boodschap);
                timer.Enabled = true;
                timer.AutoReset = false;
            }
        }

        private async Task NotifyUserAsync(object sender, ElapsedEventArgs e, SocketUser user, string boodschap)
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{user.Mention}, ik moest je er aan herinneren om {boodschap}", false, null);
        }

        [Command("date")]
        [Alias("datum")]
        [Summary("Vraag de datum van vandaag op.")]
        public async Task AskDateAsync()
        {
            await ReplyAsync($"{DateTime.Today.ToShortDateString()}", false, null);
        }

        [Command("time")]
        [Alias("tijd")]
        [Summary("Vraag de tijd van vandaag op.")]
        public async Task AskTimeAsync()
        {
            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}", false, null);
        }

        [Command("datetime")]
        [Summary("Vraag de datum en tijd van vandaag op.")]
        public async Task AskDateTimeAsync()
        {

            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}", false, null);
        }

        [Command("ap")]
        [Summary("Ping Pong Effect")]
        public async Task ApTiAsync()
        {
            await ReplyAsync($"ti!", false, null);
        }

        [Command("ping")]
        [Summary("Ping Pong Effect")]
        public async Task PingPongAsync()
        {
            await ReplyAsync($"pong!", false, null);
        }
    }
}
