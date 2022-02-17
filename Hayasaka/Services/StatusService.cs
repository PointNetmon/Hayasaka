using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;

namespace Hayasaka.Services
{
    public class StatusService : DiscordClientService
    {   
        public DiscordSocketClient DiscordClient { get; }
        public IServiceProvider Provider { get; }

        public StatusService(DiscordSocketClient discordClient, ILogger<StatusService> logger, IServiceProvider provider) : base (discordClient, logger)
        {
            DiscordClient = discordClient;
            Provider = provider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Client.WaitForReadyAsync(cancellationToken);

            Logger.LogInformation($"{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator} ({Client.CurrentUser.Id}) is now live");

            await Client.SetActivityAsync(new Game("Type '/' to view my commands")); 
        }
    }
}