﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace APTI_BOT.Services
{
    public class LoggingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;

        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            LogDirectory = Path.Combine(Directory.GetCurrentDirectory() + "/../../..", "logs");

            _discord = discord;
            _commands = commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private string LogDirectory { get; }
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(LogDirectory)) // Create the log directory if it doesn't exist
                Directory.CreateDirectory(LogDirectory);

            if (!File.Exists(LogFile)) // Create today's log file if it doesn't exist
                File.Create(LogFile).Dispose();

            var logText =
                $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(LogFile, logText + "\n"); // Write the log text to a file

            return Console.Out.WriteLineAsync(logText); // Write the log text to the console
        }
    }
}