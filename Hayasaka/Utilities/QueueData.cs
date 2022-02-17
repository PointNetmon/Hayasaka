using Discord;
using Discord.WebSocket;

namespace Hayasaka.Utilities 
{
    public class QueueData
    {
        public IUser User { get; set; }
        public ISocketMessageChannel Channel { get; set; }
        public string ThumbnailUri { get; set; }

        public QueueData(IUser user, ISocketMessageChannel channel, string thumbnailuri)
        {
            User = user;
            Channel = channel;
            ThumbnailUri = thumbnailuri;
        }
    }
}