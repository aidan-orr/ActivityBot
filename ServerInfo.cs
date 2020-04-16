using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Linq;

namespace ActivityBot
{
	[Serializable]
	public class ServerInfo
	{

		public ulong ServerId;
		public ServerInfoFields Fields;

		private static readonly string directory = ConfigurationManager.AppSettings["ServerStorageDirectory"];
		private static readonly string fileFormat = ConfigurationManager.AppSettings["ServerStorageFormat"];
		private static readonly string pathFormat = $"{directory}/{fileFormat}";

		public ServerInfo(ulong serverId)
		{
			Fields = new ServerInfoFields();
			foreach (FieldInfo field in typeof(ServerInfoFields).GetFields())
			{
				field.SetValue(Fields, Activator.CreateInstance(field.FieldType));
			}
			ServerId = serverId;
		}

		public static ServerInfo GetServerInfo(ulong guildId)
		{
			string path = String.Format(pathFormat, guildId);
			if (File.Exists(path))
			{
				try
				{
					using FileStream file = File.OpenRead(path);
					return (ServerInfo)(new BinaryFormatter().Deserialize(file));
				}
				catch
				{
					return new ServerInfo(guildId);
				}
			}
			else
				return new ServerInfo(guildId);
		}

		public void WriteToDisk()
		{
			string path = string.Format(pathFormat, ServerId);
			if (!new DirectoryInfo(directory).Exists)
				Directory.CreateDirectory(directory);
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
				fileInfo.Delete();
			using FileStream file = File.Open(path, FileMode.OpenOrCreate);
			new BinaryFormatter().Serialize(file, this);
		}

		public void Clear()
		{
			string path = string.Format(pathFormat, ServerId);
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
				fileInfo.Delete();
			foreach (FieldInfo field in typeof(ServerInfo).GetFields())
			{
				field.SetValue(this, default);
			}
		}
	}
}