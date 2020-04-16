using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ActivityBot.Modules
{
	[Group("activity")]
	[RequireUserPermission(GuildPermission.Administrator)]
	public class ActivityModule : ModuleBase<SocketCommandContext>
	{
		[Group("roles")]
		public class RolesModule : ModuleBase<SocketCommandContext>
		{
			[Group("active")]
			public class ActiveModule : ModuleBase<SocketCommandContext>
			{
				[Command("set")]
				public async Task SetAsync(SocketRole role)
				{
					ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
					info.ActiveRoleId = role.Id;
					if (info.ActiveRoleId != null && info.InactiveRoleId != null)
					{
						info.Enabled = true;
					}
					await ReplyAsync($"Set the active role to {role.Mention}");
				}
				[Command("clear")]
				public async Task ClearAsync()
				{
					ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
					info.ActiveRoleId = null;
					info.Enabled = false;
					await ReplyAsync("Cleared the active role");
				}
			}
			[Group("inactive")]
			public class InactiveModule : ModuleBase<SocketCommandContext>
			{
				[Command("set")]
				public async Task SetAsync(SocketRole role)
				{
					ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
					info.InactiveRoleId = role.Id;
					if (info.ActiveRoleId != null && info.InactiveRoleId != null)
					{
						info.Enabled = true;
					}
					await ReplyAsync($"Set the active role to {role.Mention}");
				}
				[Command("clear")]
				public async Task ClearAsync()
				{
					ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
					info.InactiveRoleId = null;
					info.Enabled = false;
					await ReplyAsync("Cleared the inactive role");
				}
			}
		}
		[Group("timer")]
		public class TimerModule : ModuleBase<SocketCommandContext>
		{
			[Command("set")]
			public async Task SetAsync([Remainder]string format)
			{
				ServerInfo info = Program.Watcher.AvailableServers[Context.Guild.Id];
				if (TimeSpan.TryParse(format, out info.InactivityTime))
					await ReplyAsync("Updated the inactivity time");
				else
					await ReplyAsync("Inactivity time in incorrect format");
			}
		}
		[Command("flush")]
		public async Task FlushAsync()
		{
			await Task.Run(Program.Watcher.AvailableServers[Context.Guild.Id].WriteToDisk);
			await ReplyAsync("Flushed all data to disk");
		}
		[Command("update")]
		public async Task UpdateAsync()
		{
			await Program.Watcher.UpdateServer(Context.Guild);
			await ReplyAsync("Roles updated");
		}
	}
}
