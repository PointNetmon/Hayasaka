using Discord;
using Discord.WebSocket;
using Hayasaka.Utilities;
using Lavalink4NET;
using Lavalink4NET.Player;

namespace Hayasaka.Services
{
    public class MusicService
    {   
        public IAudioService AudioService { get; }
        public IServiceProvider Provider { get; }

        public MusicService(IServiceProvider provider)
        {
            Provider = provider;

            AudioService = Provider.GetRequiredService<IAudioService>();
        }

        public async ValueTask<MusicPlayer> GetPlayer(ulong guildId)
        {
            MusicPlayer player = AudioService.GetPlayer<MusicPlayer>(guildId) ?? null;

            if (player?.VoiceChannelId.HasValue == false)
            {
                player.Dispose();
                player = null;
            }

            return player;
        }

        // Joins a voice channel
        // Returns 'true' on success
        // Returns 'false' with an error message on failure 
        public async ValueTask<(bool, string)> JoinAsync(IGuildUser client, ulong guildId, ulong voiceChannelId)
        {   
            if (client?.GuildPermissions is not {Speak: true, Connect: true})
            {
                return (false, "‼ I'm missing `Connect` and `Speak` permissions.");
            }

            await AudioService.JoinAsync<MusicPlayer>(guildId, voiceChannelId, true);
            return (true, null);
        }

        // Adds track to queue and returns a boolean
        // Returns 'true' on success
        // Returns 'false' with an error message on failure 
        public async ValueTask<(bool, string)> AddTrackAsync(ulong guildId, LavalinkTrack track)
        {   
            var player = await GetPlayer(guildId);

            try
            {   
                await player?.PlayAsync(track, true);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }

            return (true, null);
        }

        public async ValueTask<string> GetThumbnail(LavalinkTrack track)
        {   
            string DEFAULT_URI = "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Ftse3.mm.bing.net%2Fth%3Fid%3DOIP.ct4xZzaT6oLv4UbIteoZuQHaEK%26pid%3DApi&f=1";

            if (track?.Source is null)
            {
                return DEFAULT_URI;
            }

            string uri = $"https://img.youtube.com/vi/{track.TrackIdentifier}/maxresdefault.jpg";

            return uri;
        }

        public async ValueTask<EmbedBuilder> GetQueueMenuEmbed(ulong guildId, string[] options)
        {
            var player = await GetPlayer(guildId);
            var queue = player.Queue.ToArray();
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Magenta);

            int count = player.Queue.Count;

            foreach (string option in options)
            {   
                if (option == player.CurrentTrack?.Title && player.CurrentTrack?.Context is QueueData data)
                {       
                        embed.WithDescription($"**Track `{player.CurrentTrack?.Title}`**")
                            .WithImageUrl(await GetThumbnail(player.CurrentTrack))
                        
                            .AddField(name: "Added by", value: $"`{data.User.Username}#{data.User.Discriminator}`", inline: true)
                            .AddField(name: "Queue Position", value: $"`1`", inline: true)

                            .AddField(name: "Duration", value: $"`{player.CurrentTrack?.Duration}`", inline: true)
                            .AddField(name: "Author", value: $"`{player.CurrentTrack?.Author}`", inline: true)

                            .AddField(name: "Source", value: $"['Redirect']({player.CurrentTrack?.Source})", inline: true)
                            .AddField(name: "ID", value: $"`{player.CurrentTrack?.TrackIdentifier}`", inline: true);                    
                }

                foreach (LavalinkTrack track in queue)
                {
                    if (option == track.Title && track.Context is QueueData queue_data)
                    {
                        count += 1;

                        embed.WithDescription($"**Track `{track.Title}`**")
                            .WithImageUrl(await GetThumbnail(track))
                        
                            .AddField(name: "Added by", value: $"`{queue_data.User.Username}#{queue_data.User.Discriminator}`", inline: true)
                            .AddField(name: "Queue Position", value: $"`{count}`", inline: true)

                            .AddField(name: "Duration", value: $"`{track.Duration}`", inline: true)
                            .AddField(name: "Author", value: $"`{track.Author}`", inline: true)

                            .AddField(name: "Source", value: $"['Redirect']({track.Source})", inline: true)
                            .AddField(name: "ID", value: $"`{track.TrackIdentifier}`", inline: true);
                    }
                }
            }
        
        return embed;
        }

    }
}