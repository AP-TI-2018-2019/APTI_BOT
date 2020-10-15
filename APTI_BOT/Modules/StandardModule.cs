using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace APTI_BOT.Modules
{
    [Name("Standaard commando's")]
    public class StandardModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly string _prefix;

        public StandardModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;

            _prefix = _config["prefix"];
        }

        [Command("herinner mij om")]
        [Alias("herinner me om", "herinner om")]
        [Summary("Laat de bot je iets herinneren door een bericht en een datum mee te geven, bv. '!herinner mij om de planten buiten te zetten op 15/10/2020'.")]
        public async Task HerinnerAsync([Remainder] string bericht)
        {
            string[] splitsing = bericht.Split("op");
            string boodschap = splitsing[0];

            if (!DateTime.TryParse(splitsing[1], out DateTime datum))
            {
                await ReplyAsync($"De ingevoerde datum is ongeldig!", false, null);
                return;
            }

            await ReplyAsync($"Oké: ik zal '{boodschap}' naar je sturen op {datum}", false, null);

            Timer timer = new Timer((datum - DateTime.Now).TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await NotifyUser(sender, e, Context.User, boodschap);
            timer.Enabled = true;
            timer.AutoReset = false;
        }

        private async Task NotifyUser(object sender, ElapsedEventArgs e, SocketUser user, string boodschap)
        {
            await ReplyAsync($"{user.Mention}, ik moest je er aan herinneren om {boodschap}", false, null);
        }

        [Command("date")]
        [Alias("datum")]
        [Summary("Vraag de datum van vandaag op.")]
        public async Task VraagDatumOpAsync()
        {
            await ReplyAsync($"{DateTime.Today.ToShortDateString()}", false, null);
        }

        [Command("time")]
        [Alias("tijd")]
        [Summary("Vraag de tijd van vandaag op.")]
        public async Task VraagTijdOpAsync()
        {
            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}", false, null);
        }

        [Command("datetime")]
        [Summary("Vraag de datum en tijd van vandaag op.")]
        public async Task VraagDateTimeOpAsync()
        {
            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}", false, null);
        }
    }
}
