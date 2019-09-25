using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tsugumi
{
    class Program
    {
        public static async Task Main()
            => await new Program().MainAsync();

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();

        public DateTime StartTime { private set; get; }
        public static Program P { private set; get; }
        public Db.Db BotDb { private set; get; }
        public float ServantUpdate { set; get; } // Counter for updating servant list in db

        private Program()
        {
            P = this;
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Utils.Log;
            commands.Log += Utils.LogError;
            BotDb = new Db.Db();
            ServantUpdate = 101f;
        }

        private async Task MainAsync()
        {
            await BotDb.InitAsync("Tsugumi");

            client.MessageReceived += HandleCommandAsync;

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<FateGOModule>(null);

            if (!File.Exists("Keys/token.txt"))
                throw new FileNotFoundException("Missing token.txt in Keys/");
            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.txt"));
            StartTime = DateTime.Now;
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.IsBot) return;
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix("t.", ref pos))
            {
                SocketCommandContext context = new SocketCommandContext(client, msg);
                await commands.ExecuteAsync(context, pos, null);
            }
        }
    }
}
