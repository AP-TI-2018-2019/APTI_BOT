using APTI_BOT.Common;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Text.RegularExpressions;
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
        [Alias("herinner me om", "herinner om", "herinner me", "herinner mij")]
        [Summary("Laat de bot je iets herinneren door een bericht en een datum mee te geven, bv. '!herinner mij om de planten buiten te zetten op 15/10/2020'.")]
        public async Task RemindMeToAsync([Remainder] string bericht)
        {
            Console.WriteLine("RemindMeToAsync");
            if (bericht.Contains("morgen") || bericht.Contains("seconden") || bericht.Contains("second") || bericht.Contains("minuten") || bericht.Contains("minuut") || bericht.Contains("uren") || bericht.Contains("uur") || bericht.Contains("dagen") || bericht.Contains("dag") || bericht.Contains("jaar") || bericht.Contains("jaren") || bericht.Contains("maanden") || bericht.Contains("maand"))
            {
                RemindNatural(bericht);
            }
            else
            {
                string[] splitsing = bericht.Split("op");
                string message = splitsing[0];

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
                    await ReplyAsync($"Oké: ik zal '{message}' naar je sturen op {datum}", false, null);

                    Timer timer = new Timer((datum - DateTime.Now).TotalMilliseconds);
                    timer.Elapsed += async (sender, e) => await NotifyUserAsync(sender, e, Context.User, message);
                    timer.Enabled = true;
                    timer.AutoReset = false;
                }
            }
        }

        private async Task NotifyUserAsync(object sender, ElapsedEventArgs e, SocketUser user, string message)
        {
            if (!Context.User.IsAUser())
            {
                return;
            }

            await ReplyAsync($"{user.Mention}, ik moest je er aan herinneren om {message}", false, null);
        }

        [Command("date")]
        [Alias("datum")]
        [Summary("Vraag de datum van vandaag op.")]
        public async Task AskDateAsync()
        {
            Console.WriteLine("AskDateAsync");
            await ReplyAsync($"{DateTime.Today.ToShortDateString()}", false, null);
        }

        [Command("time")]
        [Alias("tijd")]
        [Summary("Vraag de tijd van vandaag op.")]
        public async Task AskTimeAsync()
        {
            Console.WriteLine("AskTimeAsync");
            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}", false, null);
        }

        [Command("datetime")]
        [Summary("Vraag de datum en tijd van vandaag op.")]
        public async Task AskDateTimeAsync()
        {
            Console.WriteLine("AskDateTimeAsync");
            await ReplyAsync($"{DateTime.Now}", false, null);
        }

        [Command("ap")]
        [Summary("Ping Pong Effect")]
        public async Task ApTiAsync()
        {
            Console.WriteLine("ApTiAsync");
            await ReplyAsync($"ti!", false, null);
        }

        [Command("ping")]
        [Summary("Ping Pong Effect")]
        public async Task PingPongAsync()
        {
            Console.WriteLine("PingPongAsync");
            await ReplyAsync($"pong!", false, null);
        }

        private int KrijgAantal(string bericht, int eindIndex)
        {
            try
            {
                bericht = bericht.Substring(0, eindIndex).TrimEnd(' ');
                if (Regex.IsMatch(bericht[bericht.Length - 1].ToString(), @"[0-9]"))
                {
                    return ExtractAantal(bericht, eindIndex);
                }

                return 0;
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("test");
                return 0;
            }
        }

        private int ExtractAantal(string bericht, int eindIndex)
        {
            int startIndex = bericht.LastIndexOf(' ');
            return int.Parse(bericht.Substring(startIndex, eindIndex - (startIndex + 1)));
        }

        private async void RemindNatural(string bericht)
        {
            int startHerinnering = bericht.IndexOf(' ');
            bericht = bericht.Substring(startHerinnering);
            int secondIndex = bericht.LastIndexOf("seconden") != -1 ? bericht.LastIndexOf("seconden") : bericht.LastIndexOf("second");
            int minuutIndex = bericht.LastIndexOf("minuten") != -1 ? bericht.LastIndexOf("minuten") : bericht.LastIndexOf("minuut");
            int uurIndex = bericht.LastIndexOf("uren") != -1 ? bericht.LastIndexOf("uren") : bericht.LastIndexOf("uur");
            int dagIndex = bericht.LastIndexOf("dagen") != -1 ? bericht.LastIndexOf("dagen") : bericht.LastIndexOf("dag");
            int weekIndex = bericht.LastIndexOf("week") != -1 ? bericht.LastIndexOf("week") : bericht.LastIndexOf("weken");
            int maandIndex = bericht.LastIndexOf("maanden") != -1 ? bericht.LastIndexOf("maanden") : bericht.LastIndexOf("maand");
            int jaarIndex = bericht.LastIndexOf("jaren") != -1 ? bericht.LastIndexOf("jaren") : bericht.LastIndexOf("jaar");
            Console.WriteLine(maandIndex);
            Console.WriteLine(uurIndex);
            int seconden = KrijgAantal(bericht, secondIndex != -1 ? secondIndex : 0);
            int minuten = KrijgAantal(bericht, minuutIndex != -1 ? minuutIndex : 0);
            int uren = KrijgAantal(bericht, uurIndex != -1 ? uurIndex : 0);
            int dagen = KrijgAantal(bericht, dagIndex != -1 ? dagIndex : 0);
            dagen += bericht.LastIndexOf("morgen") == -1 ? 0 : 1;
            int weken = KrijgAantal(bericht, weekIndex != -1 ? weekIndex : 0);
            int maanden = KrijgAantal(bericht, maandIndex != -1 ? maandIndex : 0);
            int jaren = KrijgAantal(bericht, jaarIndex != -1 ? jaarIndex : 0);
            Console.WriteLine(maanden);
            Console.WriteLine(uren);
            DateTime datumbericht = DateTime.Now.AddSeconds(seconden).AddMinutes(minuten).AddHours(uren).AddDays(dagen).AddMonths(maanden).AddYears(jaren);
            Console.WriteLine(datumbericht);
            string message = bericht.Substring(0, bericht.LastIndexOf("over") != -1 ? bericht.LastIndexOf("over") : bericht.LastIndexOf("binnen"));
            await ReplyAsync($"Oké: herinnering om {message} ingesteld op {datumbericht.ToShortDateString()} rond {datumbericht.ToShortTimeString()}.");
            Timer timer = new Timer((datumbericht - DateTime.Now).TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await NotifyUserAsync(sender, e, Context.User, message);
            timer.Enabled = true;
            timer.AutoReset = false;
        }
    }
}
