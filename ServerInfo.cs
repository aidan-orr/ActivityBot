using System;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ActivityBot
{
	[Serializable]
	public class ServerInfo
	{

		public ulong GuildId;
		public ulong? ActiveRoleId;
		public ulong? InactiveRoleId;
		public Dictionary<ulong, DateTime> LastMessageTimes;
		public TimeSpan InactivityTime;
		public bool Enabled;

		private static readonly string directory = ConfigurationManager.AppSettings["ServerStorageDirectory"];
		private static readonly string fileFormat = ConfigurationManager.AppSettings["ServerStorageFormat"];
		private static readonly string pathFormat = $"{directory}/{fileFormat}";

		public ServerInfo(SocketGuild guild)
		{
			if (!TryRetrieve(guild.Id))
			{
				this.GuildId = guild.Id;
				this.ActiveRoleId = null;
				this.InactiveRoleId = null;
				this.LastMessageTimes = new Dictionary<ulong, DateTime>();
				this.InactivityTime = new TimeSpan(3, 0, 0, 0, 0);
				this.Enabled = false;
			}
		}

		private bool TryRetrieve(ulong guildId)
		{
			var path = String.Format(pathFormat, guildId);
			if (File.Exists(path))
			{
				try
				{
					BinaryFormatter formatter = new BinaryFormatter();
					ServerInfo serverInfo;
					using (var file = File.OpenRead(path)) serverInfo = (ServerInfo)formatter.Deserialize(file);
					this.GuildId = serverInfo.GuildId;
					this.ActiveRoleId = serverInfo.ActiveRoleId;
					this.InactiveRoleId = serverInfo.InactiveRoleId;
					this.LastMessageTimes = serverInfo.LastMessageTimes;
					this.InactivityTime = serverInfo.InactivityTime;
					this.Enabled = serverInfo.Enabled;
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
				return false;
		}
		public void WriteToDisk()
		{
			var path = string.Format(pathFormat, this.GuildId);
			if (!new DirectoryInfo(directory).Exists)
				Directory.CreateDirectory(directory);
			var formatter = new BinaryFormatter();
			var fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
				fileInfo.Delete();
			using var file = File.Open(path, FileMode.OpenOrCreate);
			formatter.Serialize(file, this);
		}

		public void Clear()
		{
			var path = string.Format(pathFormat, this.GuildId);
			var f = new FileInfo(path);
			if (f.Exists)
				f.Delete();
			this.GuildId = 0;
			this.ActiveRoleId = null;
			this.InactiveRoleId = null;
			this.LastMessageTimes = null;
			this.InactivityTime = new TimeSpan();
			this.Enabled = false;
		}
	}
}