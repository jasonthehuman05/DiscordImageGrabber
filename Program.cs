using Discord;
using Discord.WebSocket;

namespace DiscordImageGrabber
{
    internal class Program
    {
        public static DiscordSocketClient? client;
        static string token = File.ReadAllText("token.txt");

        static void Main(string[] args)
        {
            MainAsyncProcess();
            while (true) ;
        }

        static async void MainAsyncProcess()
        {
            Console.WriteLine("Attempting to open connection to Discord...");

            //Create the discord client and wire up all needed events
            client = new DiscordSocketClient();
            client.Log += DiscordLog;

            //Attempt first log in
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            client.Ready += Client_Ready;
            //Do nothing until program is closed
            await Task.Delay(-1);
            await client.StopAsync();
            client.Dispose();
        }

        private static Task Client_Ready()
        {
            Interaction interaction = new Interaction(client);
            Task<Task> startInteraction = interaction.StartInteraction();
            return Task.CompletedTask;
        }

        private static Task DiscordLog(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}