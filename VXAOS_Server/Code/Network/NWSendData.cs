using System;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public static partial class Network {
      public static void SendDataToMap(int mapId, string msg) {
         foreach (var client in Clients.Values) {
            if (client != null && client.IsInGame() && client.MapId == mapId)
               client.Send(msg);
         }
      }
      public static void SendDataToAll(string msg) {
         foreach (var client in Clients.Values) {
            if (client != null && client.IsInGame())
               client.Send(msg);
         }
      }
      public static void SendDataToParty(int partyId, string msg) {
         if (Parties.ContainsKey(partyId)) {
            foreach (var memberId in Parties[partyId].Members) {
               Clients[memberId].Send(msg);
            }
         }
      }
      public static void SendDataToGuild(string guildName, string msg) {
         foreach (var client in Clients.Values) {
            if (client != null && client.IsInGame() && client.GuildName == guildName)
               client.Send(msg);
         }
      }
      public static void SendLogin(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Enums.Packet.LOGIN);
         buffer.WriteByte((byte)client.Group);
         buffer.WriteTime(client.VipTime);
         buffer.WriteByte((byte)client.Actors.Count);
         foreach (var (actorId, actor) in client.Actors) {
            buffer.WriteByte((byte)actorId);
            buffer.WriteString(actor.Name);
            buffer.WriteString(actor.CharacterName);
            buffer.WriteByte((byte)actor.CharacterIndex);
            buffer.WriteString(actor.FaceName);
            buffer.WriteByte((byte)actor.FaceIndex);
            buffer.WriteByte((byte)actor.Sex);
            foreach(var equip in actor.Equips) {
               buffer.WriteShort((short)equip);
            }
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFailedLogin(GameClient client, Enums.Login type) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.FAIL_LOGIN);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCreateAccount(GameClient client, Enums.Register type) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.CREATE_ACCOUNT);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCreateActor(GameClient client, int actorId, Actor actor) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.CREATE_ACCOUNT);
         buffer.WriteByte((byte)actorId);
         buffer.WriteString(actor.Name);
         buffer.WriteString(actor.CharacterName);
         buffer.WriteByte((byte)actor.CharacterIndex);
         buffer.WriteString(actor.FaceName);
         buffer.WriteByte((byte)actor.FaceIndex);
         buffer.WriteByte((byte)actor.Sex);
         foreach (var equip in actor.Equips) {
            buffer.WriteShort((short)equip);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFailedCreateActor(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.FAIL_CREATE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendRemoveActor(GameClient client, int actorId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         buffer.WriteByte((byte)actorId);
         client.Send(buffer.ToStringBuffer());
      }
   }
}
