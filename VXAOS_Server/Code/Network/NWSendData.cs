using Parlot;
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
      public static void SendUseActor(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendMotd(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerData(GameClient client, int mapId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendMapPlayers(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendRemovePlayer(int clientId, int mapId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerMovement(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void PlayerChatMessage(GameClient client, string message, int colorId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void MapChatMessage(int mapId, string message, int playerId, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Enums.Chat.MAP;
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void GlobalChatMessage(string message, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Enums.Chat.GLOBAL;
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToAll(buffer.ToStringBuffer());
      }
      public static void PartyChatMessage(GameClient client,string message) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void GuildChatMessage(GameClient client,string message, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Enums.Chat.GUILD;
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void PrivateChatMessage(GameClient client, string message, string name) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void AlertMessage(GameClient client, Enums.Alert type) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendWhosOnline(GameClient client, string message) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAttackPlayer(int mapId, int hpDamage, int mpDamage, bool critical, short attackerId,
                  byte attackerType, byte aniIndex, int playerId, short animationId, bool notShowMissed) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAttackEnemy(int mapId, int hpDamage, int mpDamage, bool critical, 
         short attackerId, byte attackerType, byte aniIndex, int playerId, short animationId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAnimation(GameBattler character, short animationId, short attackerId, 
               byte attackerType, byte aniIndex, byte characterType) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(character.MapId, buffer.ToStringBuffer());
      }
      public static void SendBallon(GameBattler character, byte characterType, byte ballonId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(character.MapId, buffer.ToStringBuffer());
      }
      public static void SendEnemyBallon(GameClient client, short eventId, byte ballonId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendEnemyRevive(GameEvent @event) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendMapEvents(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendEventMovement(GameEvent @event) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendMapDrops(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAddDrop(int mapId, short itemId, byte kind, short amount, short x, short y) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendRemoveDrop(int mapId, short dropId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAddProjectile(GameClient client, short finishX, short finishY, 
               Target target, byte projectileType, byte projectileId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerVitals(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerExp(GameClient client, int exp) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerState(GameClient client, short stateId, 
               bool addState = false, float stateTime = 0) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerBuff(GameClient client, byte paramId, short buffLevel,
               float buffTime = 0, float buffMaxTime = 0) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerSwitch(GameClient client, short switchId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerVariable(GameClient client, short variableId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerSelfSwitch(GameClient client, (int MapId, int EventId, char Ch) key) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerItem(GameClient client, short itemId, byte kind, short amount,
               bool dropSound, bool popUp) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerGold(GameClient client, int amount, bool dropSound, bool popUp) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerParam(GameClient client, byte paramId, short value) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerEquip(GameClient client, byte slotId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerSkill(GameClient client, short skillId, bool learn = true) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerClass(GameClient client, short classId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerSex(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerGraphic(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerPoints(GameClient client, short points) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerHotbar(GameClient client, byte id) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerCooldown(GameClient client, short id) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTarget(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTransferPlayer(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenFriends(GameClient client, List<string> onlineFriends) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAddFriend(GameClient client, string friendName) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendRemoveFriend(GameClient client, short index) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenCreateGuild(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenGuild(GameClient client, byte onlineMembersSize) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildLeader(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildNotice(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildName(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendRemoveGuildMember(GameClient client, string name) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendJoinParty(GameClient client, GameClient player) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendLeaveParty(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToParty(client.PartyId, buffer.ToStringBuffer());
      }
      public static void SendDissolveParty(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenBank(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendBankItem(GameClient client, short itemId, byte kind, short amount) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendBankGold(GameClient client, int amount) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCloseWindow(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenShop(GameClient client, short eventId, short index) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenTeleport(GameClient client, byte teleportId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendEventCommand(GameClient client, short eventId, short initialIndex, short finalIndex) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendParallelProcessCommand(GameEvent @event, short initialIndex, short finalIndex) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendRequest(GameClient client, Enums.Request type, GameClient player) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAcceptRequest(GameClient client, Enums.Request type) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTradeItem(GameClient client, short playerId, short itemId, byte kind, short amount) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTradeGold(GameClient client, short playerId, int amount) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAddQuest(GameClient client, byte questId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFinishQuest(GameClient client, byte questId) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendVipDays(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendLogout(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAdmingCommand(GameClient client, byte command, string alertMsg = "") {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGlobalSwitch(short switchId, bool value) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         SendDataToAll(buffer.ToStringBuffer());
      }
      public static void SendGlobalSwitches(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      /*public static void Send(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Enums.Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }*/
   }
}
