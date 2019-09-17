using Discord.Commands;
using DiscordUtils;
using System.Threading.Tasks;

namespace Tsugumi
{
    public class CommunicationModule : ModuleBase
    {
        [Command("Info")]
        public async Task Info()
        {
            await ReplyAsync("", false, Utils.GetBotInfo(Program.P.StartTime, "Tsugumi", Program.P.client.CurrentUser));
        }
    }
}
