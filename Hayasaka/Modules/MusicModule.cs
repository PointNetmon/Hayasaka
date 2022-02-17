using Discord;
using Discord.Interactions;
using Hayasaka.Utilities;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Hayasaka.Services;
using Discord.WebSocket;

namespace Hayasaka.Modules
{
    public class MusicModule : InteractionModuleBase
    {

        public IAudioService AudioService { get; }
        public MusicService Music { get; }
    
        public MusicModule(IServiceProvider provider)
        {
            AudioService = provider.GetRequiredService<IAudioService>();

            Music = provider.GetRequiredService<MusicService>();
        }

        [SlashCommand("play", "Enqueue a track.")]
        public async Task PlayCommand(string query)
        {
            await DeferAsync();

            var client = await Context.Guild.GetCurrentUserAsync();

            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await FollowupAsync("⁉ You are not connected to a voice channel.");
                return;
            }
            var player = await Music.GetPlayer(Context.Guild.Id);


            if (player is null)
            {
                (bool join_success, string join_error) = await Music.JoinAsync(client, Context.Guild.Id, voiceState.VoiceChannel.Id);

                if (join_success == false && join_error != null)
                {
                    await FollowupAsync(join_error);
                    return;
                }
            }

            var track = await AudioService.GetTrackAsync(query, SearchMode.YouTube);

            (bool play_success, string play_error) = await Music.AddTrackAsync(Context.Guild.Id, track);

            if (play_success == false && play_error is not null)
            {
                await FollowupAsync($"🛠 Failed to play the track, command raised an exception. \n ```cs\n{play_error}\n```");
            }
            var thumbnail = await Music.GetThumbnail(track);

            track.Context = new QueueData(Context.User, Context.Channel as ISocketMessageChannel, thumbnail);

            var embed = new EmbedBuilder();
            embed.WithDescription($"**Track`{track.Title}` has been enqueued.**");

            embed.AddField(name: "Author", value: $"`{track.Author}`", inline: true);
            embed.AddField(name: "Duration", value: $"`{track.Duration}`", inline: true);
            embed.AddField(name: "Source", value: $"['Redirect']({track.Source})", inline: true);

            embed.WithColor(Color.Magenta);
            embed.WithThumbnailUrl(thumbnail);

            await FollowupAsync(embed: embed.Build());
        }

        [SlashCommand("pause", "Tempoarily pauses the player")]
        public async Task PauseCommand()
        {
            await DeferAsync();
            var player = await Music.GetPlayer(Context.Guild.Id);

            if (player.State == PlayerState.Paused)
            {
                await player.ResumeAsync();

                var embed = new EmbedBuilder();
                embed.WithDescription($"Resumed.");
                embed.WithColor(Color.Magenta);

                await FollowupAsync(embed: embed.Build());
            }
            else
            {
                await player.PauseAsync();
                var embed = new EmbedBuilder();
                embed.WithDescription($"Paused.");
                embed.WithColor(Color.Magenta);

                await FollowupAsync(embed: embed.Build());
            }
        }

        [SlashCommand("dequeue", "Dequeue all the tracks.")]
        public async Task DequeueCommand()
        {
            await DeferAsync();

            var player = await Music.GetPlayer(Context.Guild.Id);

            if (player is null)
            {
                await FollowupAsync($"‼ I'm not connected to a voice channel.");
                return;
            }

            player.Queue.Clear();
            await FollowupAsync($"🧹 Tracks have been dequeued.");
        }

        [SlashCommand("next", "Skips to next track in queue.")]
        public async Task NextCommand()
        {
            await DeferAsync();

            var player = await Music.GetPlayer(Context.Guild.Id);

            if (player is null)
            {
                await FollowupAsync($"‼ I'm not connected to a voice channel.");
                return;
            }

            await player.SkipAsync();
            await FollowupAsync($"⏭ Skipped the track");
        }

        [SlashCommand("disconnect", "Disconnects from the voice channel.")]
        public async Task DisconnectCommand()
        {
            await DeferAsync(ephemeral: true);

            var player = await Music.GetPlayer(Context.Guild.Id);

            if (player is null)
            {
                await FollowupAsync("‼ I'm not connected to a voice channel.");
                return;
            }

            await player.DestroyAsync();
            await player.DisconnectAsync();
            await FollowupAsync($"👋");

        }

        [SlashCommand("queue", "View the tracks enqueued.")]
        public async Task QueueCommand()
        {
            await DeferAsync();
            var player = await Music.GetPlayer(Context.Guild.Id);

            if (player is null)
            {
                await FollowupAsync($"‼ I'm not connected to a voice channel.");
                return;
            }

            if (player.Queue.IsEmpty && player.CurrentTrack is null)
            {
                await FollowupAsync("‼ There are no tracks enqueued");
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription($"**Current Track `{player.CurrentTrack?.Title}`**")
                .WithImageUrl(await Music.GetThumbnail(player.CurrentTrack))
                .WithColor(Color.Magenta);

            if (player.CurrentTrack?.Context is QueueData data)
            {

                embed.AddField(name: "Added by", value: $"`{data.User.Username}#{data.User.Discriminator}`", inline: true);
                embed.AddField(name: "Queue Position", value: "`1`", inline: true);

                embed.AddField(name: "Duration", value: $"`{player.CurrentTrack?.Duration}`", inline: true);
                embed.AddField(name: "Author", value: $"`{player.CurrentTrack?.Author}`", inline: true);

                embed.AddField(name: "Source", value: $"['Redirect']({player.CurrentTrack?.Source})", inline: true);
                embed.AddField(name: "ID", value: $"`{player.CurrentTrack?.TrackIdentifier}`", inline: true);
            }

            var menu = new SelectMenuBuilder()
                .WithPlaceholder($"{player.CurrentTrack?.Title}")
                .WithCustomId("queue-menu")
                .AddOption(player.CurrentTrack?.Title, player.CurrentTrack?.Title, player.CurrentTrack?.Author);

            var builder = new ComponentBuilder();

            foreach (LavalinkTrack track in player.Queue)
            {
                menu.AddOption(track.Title, track.Title, track.Author);
            }

            builder.WithSelectMenu(menu);

            await FollowupAsync(embed: embed.Build(), components: builder.Build());
        }

        [ComponentInteraction("queue-menu*")]
        public async Task QueueMenu(string id, string[] options)
        {
            await DeferAsync(ephemeral: true);
            var embed = await Music.GetQueueMenuEmbed(Context.Guild.Id, options);

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}