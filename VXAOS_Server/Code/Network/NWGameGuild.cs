using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public static partial class Network {
      internal static async Task RemoveGuild(string guildName) {
         string message = string.Format(Vocab.RemoveGuild, guildName);
         foreach (var player in Network.Clients.Values) {
            if (player == null ||
                !player.IsInGame() ||
                player.GuildName != guildName) {
               continue;
            }
            player.GuildName = string.Empty;
            //SendGuildName(player);
            //PlayerChatMessage(
             //   player,
            //    message,
            //    Configs.ErrorColor);
         }
         if (Network.Guilds.TryRemove(guildName, out var guild)) {
            await DB.RemoveGuild(guild);
         }
      }
   }
}
