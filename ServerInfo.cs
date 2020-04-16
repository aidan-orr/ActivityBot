using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
		public Dictionary<ulong, DateTime> LastActivityTimes;
		public TimeSpan InactivityTime;
		public bool Enabled;

		private static readonly string directory = ConfigurationManager.AppSettings["ServerStorageDirectory"];
		private static readonly string fileFormat = ConfigurationManager.AppSettings["ServerStorageFormat"];
		private static readonly string pathFormat = $"{directory}/{fileFormat}";

		public ServerInfo(SocketGuild guild)
		{
			if (!TryRetrieve(guild.Id))
			{
				GuildId = guild.Id;
				ActiveRoleId = null;
				InactiveRoleId = null;
				LastActivityTimes = new Dictionary<ulong, DateTime>();
				InactivityTime = new TimeSpan(3, 0, 0, 0, 0);
				Enabled = false;
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
					GuildId = serverInfo.GuildId;
					ActiveRoleId = serverInfo.ActiveRoleId;
					InactiveRoleId = serverInfo.InactiveRoleId;
					LastActivityTimes = serverInfo.LastActivityTimes;
					InactivityTime = serverInfo.InactivityTime;
					Enabled = serverInfo.Enabled;
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
			var path = string.Format(pathFormat, GuildId);
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
			var path = string.Format(pathFormat, GuildId);
			var f = new FileInfo(path);
			if (f.Exists)
				f.Delete();
			GuildId = 0;
			ActiveRoleId = null;
			InactiveRoleId = null;
			LastActivityTimes = null;
			InactivityTime = new TimeSpan();
			Enabled = false;
		}
	}
}