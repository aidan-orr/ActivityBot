using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityBot
{
	[Serializable]
	public class ServerInfoFields
	{
		public ulong? ActiveRoleId;
		public ulong? InactiveRoleId;
		public Dictionary<ulong, DateTime> LastActivityTimes;
		public TimeSpan InactivityTime;
		public bool Enabled;
	}
}
