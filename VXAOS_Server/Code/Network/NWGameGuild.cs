using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public static partial class Network {
      internal static void OpenGuild(GameClient client) {
         List<string> members = new();
         int onlineIndex = 0;
         foreach(var name in Guilds[client.GuildName].Members) {
            if (FindPlayer(name) != null) {
               members.Insert(onlineIndex, name);
               onlineIndex++;
            } else {
               members.Add(name);
            }
         }
         Guilds[client.GuildName].Members = members;
         SendOpenGuild(client, (byte)onlineIndex);
      }
      internal static async Task CreateGuild(GameClient client, string name, List<int> flag) {
         if (Guilds.ContainsKey(name)) {
            AlertMessage(client, Enums.Alert.GUILD_EXIST);
            client.CloseCreateGuild();
            return;
         }
         client.GuildName = name;
         Guilds.TryAdd(name, new());
         Guilds[name].Leader = client.Name;
         Guilds[name].Flag = flag;
         Guilds[name].Members = new() { client.Name };
         Guilds[name].Notice = "";
         await DB.CreateGuild(name);
         SendGuildName(client);
         PlayerChatMessage(client, string.Format(Vocab.NewGuild, name), Configs.SuccessColor);
         client.CreatingGuild = false;
         client.EventInterpreter.Resume();
      }
      internal static async Task ChangeGuildLeader(GameClient client, string name) {
         var member = FindGuildMember(Guilds[client.GuildName], name);
         if (string.IsNullOrEmpty(member) || Guilds[client.GuildName].Leader == name) {
            AlertMessage(client, Enums.Alert.INVALID_NAME);
            return;
         }
         Guilds[client.GuildName].Leader = member;
         GuildChatMessage(client, $"{member} {Vocab.ChangeLeader}", Configs.GuildColor);
         await DB.SaveGuild(Guilds[client.GuildName]);
         SendGuildLeader(client);
      }
      internal static async Task ChangeGuildNotice(GameClient client, string notice) {
         Guilds[client.GuildName].Notice = notice;
         GuildChatMessage(client, $"{Vocab.ChangeNotice} {notice}", Configs.GuildColor);
         await DB.SaveGuild(Guilds[client.GuildName]);
         SendGuildNotice(client);
      }
      internal static async Task RemoveGuildMember(GameClient client, string memberName) {
         var player = FindPlayer(memberName);
         if (player != null) {
            player.LeaveGuild();
         } else {
            Guilds[client.GuildName].Members.Remove(memberName);
            await DB.SaveGuild(Guilds[client.GuildName]);
         }
         SendRemoveGuildMember(client, memberName);
      }
      internal static async Task RemoveGuild(string guildName) {
         string message = string.Format(Vocab.RemoveGuild, guildName);
         foreach (var player in Clients.Values) {
            if (player == null ||
                !player.IsInGame() ||
                player.GuildName != guildName) {
               continue;
            }
            player.GuildName = string.Empty;
            SendGuildName(player);
            PlayerChatMessage(
                player,
                message,
                Configs.ErrorColor);
         }
         if (Guilds.TryRemove(guildName, out var guild)) {
            await DB.RemoveGuild(guild);
         }
      }
   }
}
