using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ActivityBot.Modules
{
	[Group("activity")]
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		public Task PingAsync() => ReplyAsync("pong!");

		[Group("info")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public class Info : ModuleBase<SocketCommandContext>
		{
			[Command]
			[Priority(0)]
			public async Task InfoAsync()
			{
				ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
				string builder = "";
				TimeSpan inactivity = info.InactivityTime;
				builder += $"Active Role: {(info.ActiveRoleId == null ? "Not Set" : "@" + Context.Guild.GetRole((ulong)info.ActiveRoleId).Name)}\n";
				builder += $"Inactive Role: {(info.InactiveRoleId == null ? "Not Set" : "@" + Context.Guild.GetRole((ulong)info.InactiveRoleId).Name)}\n";
				builder += $"Inactivity Time: {(inactivity.Days != 0 ? $"{inactivity.Days} Days" : "")} {(inactivity.Hours != 0 ? $"{inactivity.Hours} Hours" : "")} {(inactivity.Minutes != 0 ? $"{inactivity.Hours} Minutes" : "")} {(inactivity.Seconds != 0 ? $"{inactivity.Seconds} Seconds" : "")}\n";
				await ReplyAsync(builder);
			}
			[Command]
			[Priority(1)]
			public async Task InfoUserAsync(SocketUser user)
			{
				SocketGuildUser guildUser = user as SocketGuildUser;
				ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
				TimeSpan inactivity = DateTime.UtcNow - info.LastActivityTimes[user.Id];
				string builder = "";
				builder += $"User: {guildUser.Nickname}\n";
				builder += $"Inactivity Time: {(inactivity.Days != 0 ? $"{inactivity.Days} Days" : "")} {(inactivity.Hours != 0 ? $"{inactivity.Hours} Hours" : "")} {(inactivity.Minutes != 0 ? $"{inactivity.Hours} Minutes" : "")} {(inactivity.Seconds != 0 ? $"{inactivity.Seconds} Seconds" : "")}\n";
				await ReplyAsync(builder);

			}
		}
	}
}
