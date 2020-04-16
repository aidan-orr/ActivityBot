using System;
using System.Threading.Tasks;

namespace ActivityBot
{
	class Program
	{
		public static ActivityBot Watcher;
		public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelInterrupt);
			Watcher = new ActivityBot();
			while (true)
			{
				await Task.Delay(new TimeSpan(0, 5, 0));
				Watcher.SaveAll();
			}
		}

		public void CancelInterrupt(object sender, ConsoleCancelEventArgs args)
		{
			Watcher.BotStop();
			Environment.Exit(0);
		}
	}
}
