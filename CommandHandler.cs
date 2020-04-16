using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace ActivityBot
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public CommandHandler(DiscordSocketClient client, CommandService commands)
		{
			_commands = commands;
			_client = client;
		}

		public async Task InstallCommandsAsync()
		{
			_client.MessageReceived += HandleCommandsAsync;
			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
		}

		private async Task HandleCommandsAsync(SocketMessage messageParam)
		{
			if (!(messageParam is SocketUserMessage message)) return;
			if (messageParam.Content.Length <= 1) return;
			if (!messageParam.Content.Contains("activity")) return;

			int argPos = 0;

			if (!(message.HasCharPrefix('!', ref argPos)) || message.Author.IsBot) return;

			var context = new SocketCommandContext(_client, message);

			var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);
			if (!result.IsSuccess)
				await messageParam.Channel.SendMessageAsync(result.ErrorReason);
		}
	}
}
