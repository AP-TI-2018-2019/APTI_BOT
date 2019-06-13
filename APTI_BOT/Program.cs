using Discord;
using Discord.WebSocket;
using Discord.API;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace APTI_BOT
{
    class Program
    {
        private DiscordSocketClient _client;
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        Config config;// = new Config("", 538812803944022017, 539223601203773440);

        public async Task MainAsync()
        {
            try
            {
                string result = System.IO.File.ReadAllText(@"../../config.json");
                config = JsonConvert.DeserializeObject<Config>(result);
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
                config = new Config(discordToken, serverId, pinLogId);
                string json = JsonConvert.SerializeObject(config);
                using(StreamWriter sw = File.CreateText(@"../../config.json"))
                {
                    sw.WriteLine(json);
                }
                Console.WriteLine("Configuratie compleet! De bot gaat nu starten.");
            }
            Environment.SetEnvironmentVariable("DiscordToken", config.DiscordToken);
            _client = new DiscordSocketClient();
            _client.MessageReceived += MessageReceived;
            _client.ReactionAdded += ReactionAdded;
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("DiscordToken"));
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)//Nog deftige command handlers maken
        {
            if (message.Content == "!AP")
            {
                await message.Channel.SendMessageAsync("TI!");
            }
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
            else if (message.Source == MessageSource.System)
            {
                await message.DeleteAsync();
            }
            else if (message.Content == "!site")
            {
                await message.Channel.SendMessageAsync("https://apti.ml");
            }
            else if (message.Content == "!github" || message.Content == "!gh")
            {
                await message.Channel.SendMessageAsync("https://apti.ml/github");
            }
            else if (message.Content == "!youtube" || message.Content == "!yt")
            {
                await message.Channel.SendMessageAsync("https://apti.ml/youtube");
            }
            else if (message.Content == "!discord" || message.Content == "!dc")
            {
                await message.Channel.SendMessageAsync("https://apti.ml/discord");
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.ToString() == "📌")
            {
                IUserMessage messageToPin = (IUserMessage)await channel.GetMessageAsync(message.Id);
                if (!messageToPin.IsPinned)
                {
                    await messageToPin.PinAsync();
                    var embed = new EmbedBuilder()
                        .WithTitle("Pinned")
                        .AddField("Bericht", messageToPin.Content, false)
                        .AddField("Link", messageToPin.GetJumpUrl(), false)
                        .AddField("Kanaal", $"<#{messageToPin.Channel.Id}>", true)
                        .AddField("Door", reaction.User.Value.Mention, true)
                        .WithAuthor(messageToPin.Author.ToString(), messageToPin.Author.GetAvatarUrl(), null)
                        .Build();
                    await ((ISocketMessageChannel)_client.GetChannel(config.PinLogId)).SendMessageAsync("", false, embed);
                }
            }
        }


    }
}
