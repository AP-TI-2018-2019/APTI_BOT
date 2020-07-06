using Discord;
using Discord.WebSocket;
using Discord.API;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace APTI_BOT
{
    class Program
    {
        const string HERINNERCOMMAND = "herrinner me";
        private DiscordSocketClient _client;
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        Config config;

        public async Task MainAsync()
        {
            try
            {
                string result = System.IO.File.ReadAllText(@"config_apti.json");
                config = JsonConvert.DeserializeObject<Config>(result);
                if (config.Jaar1RolId == 0 || config.StudentRolId == 0)
                {
                    Console.WriteLine("Je configuratiebestand is verouderd. Om verder te gaan moet je nog extra gegevens invoeren.");
                    JaarRolInvoer(out ulong jaar1RolId, out ulong jaar2RolId, out ulong jaar3RolId, out ulong studentRolId);
                    config = new Config(config.DiscordToken, config.ServerId, config.PinLogId, jaar1RolId, jaar2RolId, jaar3RolId, studentRolId);
                    string json = JsonConvert.SerializeObject(config);
                    using (StreamWriter sw = File.CreateText(@"config_apti.json"))
                    {
                        sw.WriteLine(json);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                string discordToken;
                ulong serverId, pinLogId;
                Console.Write("Geef bot token: ");
                discordToken = Console.ReadLine();
                Console.Write("Geef server id: ");
                serverId = ulong.Parse(Console.ReadLine());
                Console.Write("Geef pin-log kanaal id: ");
                pinLogId = ulong.Parse(Console.ReadLine());
                JaarRolInvoer(out ulong jaar1RolId, out ulong jaar2RolId, out ulong jaar3RolId, out ulong studentRolId);
                config = new Config(discordToken, serverId, pinLogId, jaar1RolId, jaar2RolId, jaar3RolId, studentRolId);
                string json = JsonConvert.SerializeObject(config);
                using (StreamWriter sw = File.CreateText(@"config_apti.json"))
                {
                    sw.WriteLine(json);
                }
                Console.WriteLine("Configuratie compleet! De bot gaat nu starten.");
            }
            Environment.SetEnvironmentVariable("DiscordToken", config.DiscordToken);
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });
            _client.MessageReceived += MessageReceived;
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("DiscordToken"));
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        private static void JaarRolInvoer(out ulong jaar1RolId, out ulong jaar2RolId, out ulong jaar3RolId, out ulong studentRolId)
        {
            Console.Write("Geef id van rol 'Jaar 1': ");
            jaar1RolId = ulong.Parse(Console.ReadLine());
            Console.Write("Geef id van rol 'Jaar 2': ");
            jaar2RolId = ulong.Parse(Console.ReadLine());
            Console.Write("Geef id van rol 'Jaar 3': ");
            jaar3RolId = ulong.Parse(Console.ReadLine());
            Console.Write("Geef id van rol 'Student': ");
            studentRolId = ulong.Parse(Console.ReadLine());
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)//Nog deftige command handlers maken
        {
            Emoji[] emoji = new Emoji[3];
            emoji[0] = new Emoji("🥇");
            emoji[1] = new Emoji("🥈");
            emoji[2] = new Emoji("🥉");
            if (message.Content.Contains("herinner me") || message.Content.Contains("herinner"))
            {
                await message.Channel.SendMessageAsync("oke");
                string herinnering = message.Content.Substring(HERINNERCOMMAND.Length);
                int startHerinnering = herinnering.IndexOf(' ');
                startHerinnering = herinnering.IndexOf(' ', startHerinnering + 1);
                herinnering = herinnering.Substring(startHerinnering);
                if (herinnering.Contains("morgen") || herinnering.Contains("seconden") || herinnering.Contains("second") || herinnering.Contains("minuten") || herinnering.Contains("minuut") || herinnering.Contains("uren") || herinnering.Contains("uur") || herinnering.Contains("dagen") || herinnering.Contains("dag") || herinnering.Contains("jaar") || herinnering.Contains("jaren") || herinnering.Contains("maanden") || herinnering.Contains("maand"))
                {
                    int secondIndex = herinnering.LastIndexOf("seconden") != -1 ? herinnering.LastIndexOf("seconden") : herinnering.LastIndexOf("second");
                    int minuutIndex = herinnering.LastIndexOf("minuten") != -1 ? herinnering.LastIndexOf("minuten") : herinnering.LastIndexOf("minuut");
                    int uurIndex = herinnering.LastIndexOf("uren") != -1 ? herinnering.LastIndexOf("uren") : herinnering.LastIndexOf("uur");
                    int dagIndex = herinnering.LastIndexOf("dagen") != -1 ? herinnering.LastIndexOf("dagen") : herinnering.LastIndexOf("dag");
                    int weekIndex = herinnering.LastIndexOf("week") != -1 ? herinnering.LastIndexOf("week") : herinnering.LastIndexOf("weken");
                    int maandIndex = herinnering.LastIndexOf("maanden") != -1 ? herinnering.LastIndexOf("maanden") : herinnering.LastIndexOf("maand");
                    int jaarIndex = herinnering.LastIndexOf("jaren") != -1 ? herinnering.LastIndexOf("jaren") : herinnering.LastIndexOf("jaar");
                    Console.WriteLine(maandIndex);
                    Console.WriteLine(uurIndex);
                    int seconden = KrijgAantal(herinnering, secondIndex != -1 ? secondIndex : 0);
                    int minuten = KrijgAantal(herinnering, minuutIndex != -1 ? minuutIndex : 0);
                    int uren = KrijgAantal(herinnering, uurIndex != -1 ? uurIndex : 0);
                    int dagen = KrijgAantal(herinnering, dagIndex != -1 ? dagIndex : 0);
                    dagen += herinnering.LastIndexOf("morgen") == -1 ? 0 : 1;
                    int weken = KrijgAantal(herinnering, weekIndex != -1 ? weekIndex : 0);
                    int maanden = KrijgAantal(herinnering, maandIndex != -1 ? maandIndex : 0);
                    int jaren = KrijgAantal(herinnering, jaarIndex != -1 ? jaarIndex : 0);
                    Console.WriteLine(maanden);
                    Console.WriteLine(uren);
                    DateTime datumHerinnering = DateTime.Now.AddSeconds(seconden).AddMinutes(minuten).AddHours(uren).AddDays(dagen).AddMonths(maanden).AddYears(jaren);
                    Console.WriteLine(datumHerinnering);
                    await message.Channel.SendMessageAsync($"Herinnering om {herinnering.Substring(0, herinnering.LastIndexOf("over") != -1? herinnering.LastIndexOf("over") : herinnering.LastIndexOf("binnen"))} ingesteld op {datumHerinnering.ToShortDateString()} om {datumHerinnering.ToShortTimeString()}.");
                }
            }
            // Dit nog deftig implmenteren
            /*else if (message.Content == "status")
            {
                await _client.SetGameAsync("Live op YouTube", "https://www.youtube.com/channel/UCcZPpgyhpB-o1Q5fXxbN5_w", ActivityType.Streaming);
            }*/
            else if (message.Content == "!date")
            {
                await message.Channel.SendMessageAsync(DateTime.Today.ToLongDateString());
            }
            else if (message.Content == "!time")
            {
                await message.Channel.SendMessageAsync(DateTime.Now.ToLongTimeString());
            }
            else if (message.Content == "!datetime")
            {
                await message.Channel.SendMessageAsync($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            }
            else if (message.Source == MessageSource.System || message.Content == "!start")
            {
                if (message.Author.Id == _client.CurrentUser.Id)
                    await message.DeleteAsync();
                else
                {
                    await message.Author.SendMessageAsync("Hey, welkom in onze server! Ik ben de APTI-bot en mijn doel is om het toetreden tot de server eenvoudiger te maken. We zullen beginnen met je naam op de server in te stellen. Om dit te doen type je je naam en klas in het volgende formaat: {Naam} - {Jaar}TI{Groep} voorafgegeaan door `!naam`.\nBijvoorbeeld: `!naam Maxim - 1TIC`.");
                }
            }
            else if (message.Channel is IPrivateChannel && message.Source == MessageSource.User && message.Content.Contains("!naam") && message.Content.Substring(0, 5) == "!naam")
            {
                string naam = message.Content.Substring(5);
                var guild = _client.GetGuild(config.ServerId);
                var user = guild.GetUser(message.Author.Id);
                Console.Write(message.Content);
                try
                {
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = naam;
                    });
                    var sent = await message.Author.SendMessageAsync($"Je nickname is ingesteld op {naam}. De volgende stap is je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht. Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen. Als je geen kanalen meer wilt zien van een jaar dan kan je gewoon opnieuw op de emoji ervan klikken. Als je jaar niet verandert dan is de sessie van deze chat verlopen en moet je de sessie terug activeren door `!jaar` te typen.");
                    await sent.AddReactionsAsync(emoji);
                }
                catch (Discord.Net.HttpException e)
                {
                    if (e.HttpCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var sent_error = await message.Author.SendMessageAsync("Ik heb niet de machtigingen om jouw naam te veranderen, je zal dit zelf moeten doen. Als troost mag je wel kiezen in welk jaar je zit :)");
                        await sent_error.AddReactionsAsync(emoji);
                    }
                    else
                    {
                        var sent_error_unknown = await message.Author.SendMessageAsync("Het instellen van je nickname is niet gelukt. Ik weet zelf niet wat er is fout gegaan. Stuur een berichtje naar @mixxamm met een screenshot van dit bericht.\nFoutcode: " + e.HttpCode + "\n\nJe kan voorlopig al wel je jaar kiezen door te klikken op één (of meerdere) emoji onder dit bericht. Als je vakken moet meenemen, dan kan je ook het vorige jaar kiezen. Als je geen kanalen meer wilt zien van een jaar dan kan je gewoon opnieuw op de emoji ervan klikken.");
                        await sent_error_unknown.AddReactionsAsync(emoji);
                    }
                }
            }
            else if (message.Channel is IPrivateChannel && message.Source == MessageSource.User && message.Content == "!jaar")
            {
                var sent = await message.Author.SendMessageAsync("Kies je jaar door op één of meer van de emoji onder dit bericht te klikken.");
                await sent.AddReactionsAsync(emoji);
            }
            else if (message.Source == MessageSource.User)
            {
                if (message.Content.Contains("!AP"))
                {
                    await message.Channel.SendMessageAsync("TI!");
                }
                else if (message.Content == "!site")
                {
                    await message.Channel.SendMessageAsync("https://apti.be");
                }
                else if (message.Content == "!github" || message.Content == "!gh")
                {
                    await message.Channel.SendMessageAsync("https://apti.be/github");
                }
                else if (message.Content == "!youtube" || message.Content == "!yt")
                {
                    await message.Channel.SendMessageAsync("https://apti.be/youtube");
                }
                else if (message.Content == "!discord" || message.Content == "!dc")
                {
                    await message.Channel.SendMessageAsync("https://apti.be/discord");
                }
                else if (message.Content == "!help")
                {
                    await message.Channel.SendMessageAsync("!AP - TI!\n!site - geeft link naar onze site\n!github | !gh - geeft link naar onze GitHub-repo\n!youtube | !yt - geeft link naar ons YouTube-kanaal\n!discord | !dc - geeft link naar onze Discord-uitnodigingspagina\n!date - geeft de datum van vandaag\n!time - geeft de huidige tijd\n!datetime - geeft de huidige datum en tijd");
                }
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IPrivateChannel && !reaction.User.Value.IsBot)
            {
                var user = _client.GetUser(reaction.UserId);
                Console.WriteLine(user.ToString());
                var guild = _client.GetGuild(config.ServerId);
                if (reaction.Emote.ToString() == "🥇")
                {
                    var role = guild.GetRole(config.Jaar1RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == "🥈")
                {
                    var role = guild.GetRole(config.Jaar2RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == "🥉")
                {
                    var role = guild.GetRole(config.Jaar3RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).AddRoleAsync(role);
                }
                var studentRole = guild.GetRole(config.StudentRolId);
                await guild.GetUser(reaction.UserId).AddRoleAsync(studentRole);
            }
            else if (reaction.Emote.ToString() == "📌")
            {
                IUserMessage messageToPin = (IUserMessage)await channel.GetMessageAsync(message.Id);
                if (!messageToPin.IsPinned)
                {
                    await messageToPin.PinAsync();
                    var embed = new EmbedBuilder()
                        .WithTitle("Pinned")
                        .AddField("Bericht", messageToPin.Content, false)
                        .AddField("Kanaal", $"<#{messageToPin.Channel.Id}>", true)
                        .AddField("Door", reaction.User.Value.Mention, true)
                        .WithAuthor(messageToPin.Author.ToString(), messageToPin.Author.GetAvatarUrl(), messageToPin.GetJumpUrl())
                        .Build();
                    await ((ISocketMessageChannel)_client.GetChannel(config.PinLogId)).SendMessageAsync("", false, embed);
                }
            }
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is IPrivateChannel)
            {
                var user = _client.GetUser(reaction.UserId);
                Console.WriteLine(user.ToString());
                var guild = _client.GetGuild(config.ServerId);
                if (reaction.Emote.ToString() == "🥇")
                {
                    var role = guild.GetRole(config.Jaar1RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == "🥈")
                {
                    var role = guild.GetRole(config.Jaar2RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
                }
                else if (reaction.Emote.ToString() == "🥉")
                {
                    var role = guild.GetRole(config.Jaar3RolId);
                    Console.WriteLine(role.ToString());
                    await guild.GetUser(reaction.UserId).RemoveRoleAsync(role);
                }
            }
            else
            {
                //if (reaction.Emote.ToString() == "📌")
                //{
                //    Console.WriteLine(message.Id);
                //    Console.WriteLine(channel.Id);
                //    IUserMessage messageToUnPin = (IUserMessage)await channel.GetMessageAsync(message.Id);

                //    if (messageToUnPin.Reactions == null || !messageToUnPin.Reactions.ContainsKey(reaction.Emote))
                //    {
                //        await ((IUserMessage)channel.GetMessageAsync(message.Id)).UnpinAsync();
                //        await ((ISocketMessageChannel)_client.GetChannel(config.PinLogId)).SendMessageAsync("Bericht unpinned");
                //    }
                //}
            }
        }

        private static int KrijgAantal(string bericht, int eindIndex)
        {
            try
            {
                bericht = bericht.Substring(0, eindIndex).TrimEnd(' ');
                if (Regex.IsMatch(bericht[bericht.Length - 1].ToString(), @"[0-9]"))
                    return ExtractAantal(bericht, eindIndex);
                return 0;
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("test");
                return 0;
            }
        }

        private static int ExtractAantal(string bericht, int eindIndex)
        {
            int startIndex = bericht.LastIndexOf(' ');
            return int.Parse(bericht.Substring(startIndex, eindIndex - (startIndex + 1)));
        }
    }
}
