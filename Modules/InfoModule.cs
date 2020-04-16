using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace ActivityBot.Modules
{
	[Group("activity")]
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		public Task PingAsync() => ReplyAsync("pong!");

		[Command("info")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task InfoAsync()
		{
			ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
			string builder = "";
			TimeSpan inactivity = info.InactivityTime;
			builder += $"Active Role: {(info.ActiveRoleId == null ? "None" : Context.Guild.GetRole((ulong)info.ActiveRoleId).Name)}\n";
			builder += $"Inactive Role: {(info.InactiveRoleId == null ? "None" : Context.Guild.GetRole((ulong)info.InactiveRoleId).Name)}\n";
			builder += $"Inactivity Time: {((inactivity.Days != 0) ? $"{inactivity.Days} Days" : "")} {((inactivity.Hours != 0) ? $"{inactivity.Hours} Hours" : "")} {((inactivity.Minutes != 0) ? $"{inactivity.Hours} Minutes" : "")}\n";
			await ReplyAsync(builder);
		}
	}
}
