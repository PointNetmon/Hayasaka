using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Hayasaka.Services;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;

namespace Hayasaka.Utilities
{
    public class MusicPlayer : VoteLavalinkPlayer
    {   
        public RestUserMessage Message { get; set; }

        public override async Task OnTrackStartedAsync(TrackStartedEventArgs args)
        {   
            if (CurrentTrack?.Context is QueueData data)
            {   
                var embed = new EmbedBuilder();
                embed.WithDescription($"**Now Playing `{CurrentTrack?.Title}`; Added by `{data.User.Username}#{data.User.Discriminator}`.**");
                embed.WithImageUrl(data.ThumbnailUri);

                embed.WithFooter("To keep this text-channel clean, this message will self-destruct after the track ends.");

                embed.WithColor(Color.Magenta);

                Message = await data.Channel.SendMessageAsync(embed: embed.Build());
                
            }

            await base.OnTrackStartedAsync(args);
        }

        public override async Task OnTrackEndAsync(TrackEndEventArgs args)
        {
            if (CurrentTrack?.Context is QueueData data)
            {   
                await Message.DeleteAsync();
                Task _ = Task.Run(async () => 
                {
                    await Task.Delay(10000);
                    if (State != PlayerState.Playing && Queue.IsEmpty)
                    {   
                        await DisconnectAsync();
                    }
                });
            }

            await base.OnTrackEndAsync(args);
        }
    }
}