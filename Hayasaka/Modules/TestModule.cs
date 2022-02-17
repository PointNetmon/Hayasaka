using Discord.Interactions;

namespace Hayasaka.Modules
{
    public class TestModule : InteractionModuleBase
    {
        [SlashCommand("ping", "pong")]
        public async Task PingCommand()
        {
            await RespondAsync(text:"Pong!");
        }
   
    }
}