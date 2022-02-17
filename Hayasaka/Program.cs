using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Discord.Addons.Hosting;
using Hayasaka.Services;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;


var host = Host.CreateDefaultBuilder()   
    .ConfigureDiscordHost((context, config) =>
    {
        config.SocketConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 200,
            UseInteractionSnowflakeDate = false,
        };

        config.Token = context.Configuration["Bot:Token"];
    })
    .UseInteractionService((context, config) =>
    {   
        config.DefaultRunMode = RunMode.Async;
        config.ThrowOnError = true;
        config.LogLevel = LogSeverity.Verbose;
        config.UseCompiledLambda = true;
    })
    .ConfigureServices((context, services) =>
    {   
        services.AddSingleton<IAudioService, LavalinkNode>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
            .AddSingleton(new LavalinkNodeOptions
            {
                RestUri = context.Configuration["Lavalink:RestUri"],
                WebSocketUri = context.Configuration["Lavalink:WebSocketUri"],
                Password = context.Configuration["Lavalink:Password"],
            });
        services.AddSingleton<MusicService>();
        
        // DiscordClientService
        services.AddHostedService<StatusService>();
        services.AddHostedService<LavalinkService>();
        services.AddHostedService<InteractionHandlerService>();
        }).Build();
  
await host.RunAsync();