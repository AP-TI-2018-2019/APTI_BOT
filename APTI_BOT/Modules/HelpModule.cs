using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace APTI_BOT.Modules
{
    [Name("Hulp commando's")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly string _prefix;
        private readonly CommandService _service;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;

            _prefix = _config["prefix"];
        }

        [Command("help")]
        [Summary("Samenvattingstabel van alle commando's van de APTI-bot.")]
        public async Task HelpAsync()
        {
            Console.WriteLine("HelpAsync");
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = "Dit zijn de commando's die je kan uitvoeren:"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess) description += $"{_prefix}{cmd.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Summary("Samenvattingstabel van alle commando's die met het ingevoerde woord beginnen.")]
        public async Task HelpAsync(string commando)
        {
            var result = _service.Search(Context, commando);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, wij vonden geen commando zoals **{commando}**.");
                return;
            }

            Console.WriteLine("HelpAsync(commando)");
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = $"Hier zijn alle commando's zoals **{commando}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Samenvatting: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}