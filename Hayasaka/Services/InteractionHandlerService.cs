using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;


namespace Hayasaka.Services
{   
    
    public class InteractionHandlerService : DiscordClientService
    {   
        public DiscordSocketClient DiscordClient { get; set; }
        public InteractionService Interaction { get; set; }
        public IServiceProvider Provider { get; set; }


        public InteractionHandlerService(DiscordSocketClient discordClient,  InteractionService interaction, IServiceProvider provider, ILogger<InteractionHandlerService> logger) : base(discordClient, logger)
        {
            DiscordClient = discordClient;
            Interaction = interaction;
            Provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {   
            await Interaction.AddModulesAsync(Assembly.GetExecutingAssembly(), Provider);
            Client.Ready += RegisterCommandsAsync;
            Client.InteractionCreated += HandleInteractionAsync;
        }

        private async Task HandleInteractionAsync(SocketInteraction socketInteraction)
        {
            try
            {
                var context = new SocketInteractionContext(Client, socketInteraction);
                await Interaction.ExecuteCommandAsync(context, Provider);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception whilst attempting to handle interaction.");

                if (socketInteraction.Type == InteractionType.ApplicationCommand)
                {
                    var message = await socketInteraction.GetOriginalResponseAsync();

                    await message.DeleteAsync();
                }
            }
        }
        private async Task RegisterCommandsAsync()
        {   
            await Interaction.RegisterCommandsGloballyAsync();
        }

    }
}