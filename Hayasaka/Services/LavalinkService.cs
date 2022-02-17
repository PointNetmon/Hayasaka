using Discord.Addons.Hosting;
using Discord.WebSocket;
using Lavalink4NET;

namespace Hayasaka.Services
{   
    
    public class LavalinkService : DiscordClientService
    {   
        public DiscordSocketClient DiscordClient { get; }
        public IServiceProvider Provider { get; }
        public IAudioService AudioService { get; }

        public LavalinkService(DiscordSocketClient discordClient,  IServiceProvider provider, ILogger<LavalinkService> logger) : base(discordClient, logger)
        {
            DiscordClient = discordClient;
            Provider = provider;

            AudioService = Provider.GetRequiredService<IAudioService>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {   
            Client.Ready += () => AudioService.InitializeAsync();
        }  

    }
}