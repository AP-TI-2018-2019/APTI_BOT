using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using APTI_BOT.Common;
using Discord.Commands;
using Discord.WebSocket;

namespace APTI_BOT.Modules
{
    [Name("Standaard commando's")]
    public class StandardModule : ModuleBase<SocketCommandContext>
    {
        [Command("herinner mij om")]
        [Alias("herinner me om", "herinner om", "herinner me", "herinner mij", "remind me")]
        [Summary(
            "Laat de bot je iets herinneren door een bericht en een datum mee te geven, bv. '!herinner mij om de planten buiten te zetten op 15/10/2020'.")]
        public async Task RemindMeToAsync([Remainder] string bericht)
        {
            await ReplyAsync("Deze functionaliteit is gebroken en wordt aan gewerkt.");
            //Console.WriteLine("RemindMeToAsync");
            //if (bericht.ToLower().Contains("morgen")
            //    || bericht.ToLower().Contains("second")
            //    || bericht.ToLower().Contains("minuten")
            //    || bericht.ToLower().Contains("minuut")
            //    || bericht.ToLower().Contains("uren")
            //    || bericht.ToLower().Contains("uur")
            //    || bericht.ToLower().Contains("dag")
            //    || bericht.ToLower().Contains("jaar")
            //    || bericht.ToLower().Contains("jaren")
            //    || bericht.ToLower().Contains("maand"))
            //{
            //    RemindNatural(bericht);
            //}
            //else
            //{
            //    var splitsing = bericht.Split("@");
            //    var message = splitsing[0];

            //    if (!DateTime.TryParse(splitsing[1], out var datum))
            //    {
            //        await ReplyAsync("De ingevoerde datum is ongeldig!");
            //    }
            //    else if (datum < DateTime.Now)
            //    {
            //        await ReplyAsync("De ingevoerde datum bevindt zich in het verleden!");
            //    }
            //    else
            //    {
            //        await ReplyAsync($"Oké: ik zal '{message}' naar je sturen op {datum}");

            //        var timer = new Timer((datum - DateTime.Now).TotalMilliseconds);
            //        timer.Elapsed += async (sender, e) => await NotifyUserAsync(sender, e, Context.User, message);
            //        timer.Enabled = true;
            //        timer.AutoReset = false;
            //    }
            //}
        }

        private async Task NotifyUserAsync(object sender, ElapsedEventArgs e, SocketUser user, string message)
        {
            if (!Context.User.IsAUser()) return;

            await ReplyAsync($"{user.Mention}, ik moest je er aan herinneren om {message}");
        }

        [Command("date")]
        [Alias("datum")]
        [Summary("Vraag de datum van vandaag op.")]
        public async Task AskDateAsync()
        {
            Console.WriteLine("AskDateAsync");
            await ReplyAsync($"{DateTime.Today.ToShortDateString()}");
        }

        [Command("time")]
        [Alias("tijd")]
        [Summary("Vraag de tijd van vandaag op.")]
        public async Task AskTimeAsync()
        {
            Console.WriteLine("AskTimeAsync");
            await ReplyAsync($"{DateTime.Now.ToShortTimeString()}");
        }

        [Command("datetime")]
        [Summary("Vraag de datum en tijd van vandaag op.")]
        public async Task AskDateTimeAsync()
        {
            Console.WriteLine("AskDateTimeAsync");
            await ReplyAsync($"{DateTime.Now}");
        }

        [Command("ap")]
        [Summary("Ping Pong Effect")]
        public async Task ApTiAsync()
        {
            Console.WriteLine("ApTiAsync");
            await ReplyAsync("ti!");
        }

        [Command("ping")]
        [Summary("Ping Pong Effect")]
        public async Task PingPongAsync()
        {
            Console.WriteLine("PingPongAsync");
            await ReplyAsync("pong!");
        }

        private int KrijgAantal(string bericht, int eindIndex)
        {
            try
            {
                bericht = bericht.Substring(0, eindIndex).TrimEnd(' ');
                return Regex.IsMatch(bericht[^1].ToString(), @"[0-9]") ? ExtractAantal(bericht, eindIndex) : 0;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("test");
                return 0;
            }
        }

        private int ExtractAantal(string bericht, int eindIndex)
        {
            var startIndex = bericht.LastIndexOf(' ');
            return int.Parse(bericht.Substring(startIndex, eindIndex - (startIndex + 1)));
        }

        private async void RemindNatural(string bericht)
        {
            var startHerinnering = bericht.IndexOf(' ');
            bericht = bericht.Substring(startHerinnering);
            var secondIndex = bericht.LastIndexOf("seconden", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("seconden", StringComparison.Ordinal)
                : bericht.LastIndexOf("second", StringComparison.Ordinal);
            var minuutIndex = bericht.LastIndexOf("minuten", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("minuten", StringComparison.Ordinal)
                : bericht.LastIndexOf("minuut", StringComparison.Ordinal);
            var uurIndex = bericht.LastIndexOf("uren", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("uren")
                : bericht.LastIndexOf("uur");
            var dagIndex = bericht.LastIndexOf("dagen", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("dagen", StringComparison.Ordinal)
                : bericht.LastIndexOf("dag", StringComparison.Ordinal);
            var weekIndex = bericht.LastIndexOf("week", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("week", StringComparison.Ordinal)
                : bericht.LastIndexOf("weken", StringComparison.Ordinal);
            var maandIndex = bericht.LastIndexOf("maanden", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("maanden", StringComparison.Ordinal)
                : bericht.LastIndexOf("maand", StringComparison.Ordinal);
            var jaarIndex = bericht.LastIndexOf("jaren", StringComparison.Ordinal) != -1
                ? bericht.LastIndexOf("jaren", StringComparison.Ordinal)
                : bericht.LastIndexOf("jaar", StringComparison.Ordinal);
            Console.WriteLine(maandIndex);
            Console.WriteLine(uurIndex);
            var seconden = KrijgAantal(bericht, secondIndex != -1 ? secondIndex : 0);
            var minuten = KrijgAantal(bericht, minuutIndex != -1 ? minuutIndex : 0);
            var uren = KrijgAantal(bericht, uurIndex != -1 ? uurIndex : 0);
            var dagen = KrijgAantal(bericht, dagIndex != -1 ? dagIndex : 0);
            dagen += bericht.LastIndexOf("morgen", StringComparison.Ordinal) == -1 ? 0 : 1;
            var weken = KrijgAantal(bericht, weekIndex != -1 ? weekIndex : 0);
            var maanden = KrijgAantal(bericht, maandIndex != -1 ? maandIndex : 0);
            var jaren = KrijgAantal(bericht, jaarIndex != -1 ? jaarIndex : 0);
            Console.WriteLine(maanden);
            Console.WriteLine(uren);
            var datumbericht = DateTime.Now.AddSeconds(seconden).AddMinutes(minuten).AddHours(uren).AddDays(dagen)
                .AddMonths(maanden).AddYears(jaren);
            Console.WriteLine(datumbericht);
            var message = bericht.Substring(0,
                bericht.LastIndexOf("over", StringComparison.Ordinal) != -1
                    ? bericht.LastIndexOf("over", StringComparison.Ordinal)
                    : bericht.LastIndexOf("binnen", StringComparison.Ordinal));
            await ReplyAsync(
                $"Oké: herinnering om {message} ingesteld op {datumbericht.ToShortDateString()} rond {datumbericht.ToShortTimeString()}.");
            var timer = new Timer((datumbericht - DateTime.Now).TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await NotifyUserAsync(sender, e, Context.User, message);
            timer.Enabled = true;
            timer.AutoReset = false;
        }
    }
}