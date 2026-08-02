using System.Net;
using VXAOS_Server.RPGData;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public static partial class Network {
      internal static void AdminCommands(GameClient client, Command command, string str, int int1, int int2, int int3) {
         switch (command) {
            case Command.KICK:
               KickPlayer(client, str);
               break;
            case Command.TELEPORT:
               TeleportPlayer(client, str, int1, int2, int3);
               break;
            case Command.GO:
               GoToPlayer(client, str);
               break;
            case Command.PULL:
               PullPlayer(client, str);
               break;
            case Command.ITEM:
               GiveItem(client, DataItems[int1], str, int2);
               break;
            case Command.WEAPON:
               GiveItem(client, DataWeapons[int1], str, int2);
               break;
            case Command.ARMOR:
               GiveItem(client, DataArmors[int1], str, int2);
               break;
            case Command.GOLD:
               GiveGold(client, str, int2);
               break;
            case Command.BAN_IP: case Command.BAN_ACC:
               _ = Ban(client, command, str, int1);
               break;
            case Command.UNBAN:
               _ = DB.Unban(client, str);
               break;
            case Command.SWITCH:
               ChangeGlobalSwitch(Convert.ToInt32(str), int1 == 1);
               break;
            case Command.MOTD:
               ChangeMotd(client, str);
               break;
            case Command.MUTE:
               MutePlayer(client, str);
               break;
            case Command.MSG:
               AdminMessage(client, str);
               break;
         }
      }
      internal static void MonitorCommands(GameClient client, Command command, string name) {
         switch (command) {
            case Command.GO:
               GoToPlayer(client, name);
               break;
            case Command.PULL:
               PullPlayer(client, name);
               break;
            case Command.MUTE:
               MutePlayer(client, name);
               break;
         }
      }
      private static void KickPlayer(GameClient client, string name) {
         var player = FindPlayer(name);
         if(player == null || player.IsAdmin()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }
         GlobalChatMessage($"{player.Name} {Vocab.Kicked}");
         SendAdminCommand(player, (byte)Command.KICK);
         Log.Add(client.Group, ConsoleColor.Blue, $"{client.User} expulsou {player.Name}.");
         player.CloseAfterWriting();
      }
      private static void TeleportPlayer(GameClient player, string name, int mapId, int x, int y) {
         foreach(var client in Clients.Values) {
            if(client == null) continue;
            if (name == "all" && client.IsInGame()) {
               client.Transfer(mapId, x, y, (byte)client.Direction);
               AlertMessage(client, Alert.TELEPORTED);
            } else if (client.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && client.IsInGame()) {
               client.Transfer(mapId, x, y, (byte)client.Direction);
               AlertMessage(client, Alert.TELEPORTED);
               if(client != player)
                  PlayerChatMessage(player, $"{string.Format(Vocab.Teleported,client.Name,x,y)} {mapId}", Configs.SuccessColor);
               break;
            }
         }
         Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} transportou {name} para as coordenadas {x} e {y} do mapa {mapId}.");
      }
      private static void GoToPlayer(GameClient client, string name) {
         var player = FindPlayer(name);
         if(player == null || player.IsAdmin()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }
         client.Transfer(player.MapId, player.X, player.Y, (byte)client.Direction);
         Log.Add(player.Group, ConsoleColor.Blue, $"{client.User} foi até {name}, nas coordenadas {client.X} e {client.Y} do mapa {client.MapId}.");
      }
      private static void PullPlayer(GameClient player, string name) {
         foreach(var client in Clients.Values) {
            if(client == null) continue;
            if (name == "all" && client.IsInGame() && client != player) {
               client.Transfer(player.MapId, player.X, player.Y, (byte)client.Direction);
               AlertMessage(client, Alert.PULLED);
            } else if (client.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && client.IsInGame()) {
               client.Transfer(player.MapId, player.X, player.Y, (byte)client.Direction);
               AlertMessage(client, Alert.PULLED);
               break;
            }
         }
         Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} puxou {name} para as coordenadas {player.X} e {player.Y} do mapa {player.MapId}.");
      }
      private static void GiveItem(GameClient player, RPGBaseItem item, string name, int amount) {
         foreach(var client in Clients.Values) {
            if(client == null) continue;
            if (name == "all" && client.IsInGame()) {
               if (!client.IsFullInventory(item) && amount > 0)
                  client.GainItem(item, amount, false, true);
            } else if (client.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && client.IsInGame()) {
               if (client.IsFullInventory(item) && amount > 0) {
                  PlayerChatMessage(player, string.Format(Vocab.FullInventory,client.Name), Configs.ErrorColor);
                  Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} tentou dar {amount} para {name}, mas o inventário deste estava cheio.");
                  return;
               } else {
                  client.GainItem(item, amount, false, true);
                  PlayerChatMessage(player, $"{string.Format(Vocab.GaveItem, amount, item.name)} {client.Name}", Configs.SuccessColor);
                  break;
               }
            }
         }
         Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} deu {amount} {item.name} para {name}.");
      }
      private static void GiveGold(GameClient player, string name, int amount) {
         foreach(var client in Clients.Values) {
            if(client == null) continue;
            if (name == "all" && client.IsInGame()) {
               client.GainGold(amount, false, true);
            } else if (client.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && client.IsInGame()) {
               client.GainGold(amount, false, true);               
               PlayerChatMessage(player, $"{string.Format(Vocab.GaveGold, amount)} {client.Name}", Configs.SuccessColor);
               break;               
            }
         }
         Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} deu {amount} moeda(s) de ouro para {name}.");
      }
      private static async Task Ban(GameClient client, Command type, string name, int days) {
         var player = FindPlayer(name);
         if (player == null || player.IsAdmin()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }
         var time = DateTimeOffset.UtcNow.AddDays(days);
         if (player == null && type == Command.BAN_ACC && (await DB.DoPlayerExist(name))) {
            var result = await DB.LoadSomePlayerData(name);
            int accIdDb = result.Value.AccountId;
            name = result.Value.Name;
            BanList.AddOrUpdate($"{accIdDb}", time, (_, _) => time);
            GlobalChatMessage($"{name} {Vocab.Banned}");
            Log.Add(client.Group, ConsoleColor.Blue, $"{client.User} baniu {name} por {days} dia(s).");
            return;
         } else if (player == null || player.IsAdmin()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         } else if (type == Command.BAN_ACC) {
            BanList.AddOrUpdate($"{player.AccountIdDb}", time, (_, _) => time);
            SendAdminCommand(player, (byte)type);
            player.CloseAfterWriting();
         } else {
            BanList.AddOrUpdate($"{player.Ip}", time, (_, _) => time);
            KickBannedIp(player.Ip);
         }
         GlobalChatMessage($"{player.Name} {Vocab.Banned}");
         Log.Add(client.Group, ConsoleColor.Blue, $"{client.User} baniu {player.Name} por {days} dia(s).");
      }
      private static void KickBannedIp(IPAddress bannedIp) {
         foreach(var client in Clients.Values) {
            if (client == null || client.Ip != bannedIp || client.IsAdmin()) continue;
            SendAdminCommand(client, (byte)Command.BAN_IP);
            client.CloseAfterWriting();
         }
      }
      private static void ChangeGlobalSwitch(int switchId, bool value) {
         if(switchId > Configs.MaxPlayerSwitches) {
            Switches[switchId] = value;
         }
      }
      private static void ChangeMotd(GameClient client, string motd) {
         Motd = motd;
         GlobalChatMessage(motd);
         Log.Add(client.Group, ConsoleColor.Blue, $"{client.User} mudou a mensagem do dia para: {motd}.");
      }
      private static void MutePlayer(GameClient client, string name) {
         var player = FindPlayer(name);
         if (player == null || player.IsAdmin()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }
         player.MutedTime = DateTime.UtcNow.AddSeconds(30);
         AlertMessage(player, Alert.MUTED);
         Log.Add(client.Group, ConsoleColor.Blue, $"{client.User} silenciou {name} por 30 segundos.");
      }
      private static void AdminMessage(GameClient player, string message) {
         foreach (var client in Clients.Values) {
            if (client != null && client.IsInGame())
               SendAdminCommand(client, (byte)Command.MSG, message);
         }
         GlobalChatMessage(message, Configs.AdmMsgColor);
         Log.Add(player.Group, ConsoleColor.Blue, $"{player.User} enviou a mensagem {message}.");
      }
   }
}