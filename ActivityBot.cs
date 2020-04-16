using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityBot
{
	public class ActivityBot
	{
		public Dictionary<ulong, ServerInfo> AvailableServers;
		public CancellationTokenSource CancellationTokenSource;
		public CancellationToken CancellationToken;
		private readonly DiscordSocketClient _client;
		private CommandService _commandService;
		private CommandHandler _commandHandler;
		private readonly string _botToken;

		public ActivityBot()
		{
			var SocketConfig = new DiscordSocketConfig { LogLevel = LogSeverity.Info, DefaultRetryMode = RetryMode.AlwaysRetry, MessageCacheSize = 1000000 };
			_client = new DiscordSocketClient(SocketConfig);
			_botToken = File.ReadAllText(ConfigurationManager.AppSettings["DiscordTokenFile"]).Trim();
			CancellationTokenSource = new CancellationTokenSource();
			CancellationToken = CancellationTokenSource.Token;
			AvailableServers = new Dictionary<ulong, ServerInfo>();
			BotStart().GetAwaiter().GetResult();
		}

		public async Task BotStart()
		{
			await BotLog(new LogMessage(LogSeverity.Info, "ActivityBot", "Starting Bot"));
			_commandService = new CommandService();
			_commandHandler = new CommandHandler(_client, _commandService);
			await _commandHandler.InstallCommandsAsync();
			_client.Log += BotLog;
			_client.MessageReceived += MessageReceivedAsync;
			_client.GuildAvailable += AddAvailableGuild;
			_client.GuildUnavailable += RemoveUnavaiableGuild;
			_client.JoinedGuild += JoinedGuild;
			_client.LeftGuild += LeftGuild;
			_client.UserJoined += UserJoined;
			await _client.LoginAsync(TokenType.Bot, _botToken);
			await _client.StartAsync();
		}

		public void BotStop()
		{
			BotLog(new LogMessage(LogSeverity.Info, "ActivityBot", "Stopping Bot"));
			SaveAll();
			_client.StopAsync();
			CancellationTokenSource.Cancel();
			CancellationTokenSource.Dispose();
		}

		public Task BotLog(LogMessage msg)
		{
			Console.WriteLine(msg);
			if (!new FileInfo(ConfigurationManager.AppSettings["LogFile"]).Exists)
				File.CreateText(ConfigurationManager.AppSettings["LogFile"]).Close();
			try
			{
				using StreamWriter s = File.AppendText(ConfigurationManager.AppSettings["LogFile"]);
				s.WriteLine(msg);
				s.Close();
			}
			catch { BotLog(msg); }
			return Task.CompletedTask;
		}
		public async Task Log(string msg) => await BotLog(new LogMessage(LogSeverity.Info, "ActivityBot", msg));

		private async Task MessageReceivedAsync(SocketMessage message)
		{
			if (!message.Author.IsBot)
			{
				SocketGuildUser author = message.Author as SocketGuildUser;
				SocketGuild guild = author.Guild as SocketGuild;
				AvailableServers.TryGetValue(guild.Id, out ServerInfo server);
				if (server.Enabled)
				{
					server.LastMessageTimes[author.Id] = DateTime.UtcNow;
					await UpdateServer(guild);
				}
			}
		}

		public async void SaveAll()
		{
			await Log("Saving data...");
			foreach (var server in AvailableServers.Values)
				server.WriteToDisk();
		}

		public async Task UpdateServer(SocketGuild server)
		{
			ServerInfo serverInfo = AvailableServers[server.Id];
			if (serverInfo.Enabled)
			{
				SocketRole activeRole = server.GetRole((ulong)serverInfo.ActiveRoleId);
				SocketRole inactiveRole = server.GetRole((ulong)serverInfo.InactiveRoleId);
				foreach (SocketGuildUser user in server.Users)
				{
					if (!user.IsBot)
					{
						if (!serverInfo.LastMessageTimes.ContainsKey(user.Id))
							serverInfo.LastMessageTimes.Add(user.Id, new DateTime());
						DateTime lastMessage = serverInfo.LastMessageTimes[user.Id];
						TimeSpan difference = DateTime.UtcNow - lastMessage;
						if (difference <= serverInfo.InactivityTime)
						{
							if (!user.Roles.Contains(activeRole))
								await user.AddRoleAsync(activeRole);
							if (user.Roles.Contains(inactiveRole))
								await user.RemoveRoleAsync(inactiveRole);
						}
						else
						{
							if (!user.Roles.Contains(inactiveRole))
								await user.AddRoleAsync(inactiveRole);
							if (user.Roles.Contains(activeRole))
								await user.RemoveRoleAsync(activeRole);
						}
					}
				}
			}
			serverInfo.WriteToDisk();
		}

		private async Task AddAvailableGuild(SocketGuild server)
		{
			AvailableServers.Add(server.Id, new ServerInfo(server));
			await Log($"Server {server.Name} became available");
		}
		private async Task RemoveUnavaiableGuild(SocketGuild server)
		{
			AvailableServers[server.Id].WriteToDisk();
			AvailableServers.Remove(server.Id);
			await Log($"Server {server.Name} became unavailable");
		}
		private async Task JoinedGuild(SocketGuild server)
		{
			AvailableServers.Add(server.Id, new ServerInfo(server));
			await Log($"Successfully joined server {server.Name}");
		}
		private async Task LeftGuild(SocketGuild server)
		{
			AvailableServers[server.Id].Clear();
			AvailableServers.Remove(server.Id);
			await Log($"Left server {server.Name}");
		}
		private async Task UserJoined(SocketGuildUser user)
		{
			var server = AvailableServers[user.Guild.Id];
			if (server.Enabled)
				await user.AddRoleAsync(user.Guild.GetRole((ulong)server.InactiveRoleId));
		}
	}
}
