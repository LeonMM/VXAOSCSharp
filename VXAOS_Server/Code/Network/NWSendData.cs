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
         if (Parties.TryGetValue(partyId, out Party? value)) {
            foreach (var memberId in value.Members) {
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
         buffer.WriteByte((byte)Packet.LOGIN);
         buffer.WriteByte(client.Group);
         buffer.WriteTime(client.VipTime);
         buffer.WriteByte(client.Actors.Count);
         foreach (var (actorId, actor) in client.Actors) {
            buffer.WriteByte(actorId);
            buffer.WriteString(actor.Name);
            buffer.WriteString(actor.CharacterName);
            buffer.WriteByte(actor.CharacterIndex);
            buffer.WriteString(actor.FaceName);
            buffer.WriteByte(actor.FaceIndex);
            buffer.WriteByte(actor.Sex);
            foreach (var equip in actor.Equips) {
               buffer.WriteShort(equip);
            }
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFailedLogin(GameClient client, Login type) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.FAIL_LOGIN);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCreateAccount(GameClient client, Register type) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CREATE_ACCOUNT);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCreateActor(GameClient client, int actorId, Actor actor) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CREATE_ACCOUNT);
         buffer.WriteByte(actorId);
         buffer.WriteString(actor.Name);
         buffer.WriteString(actor.CharacterName);
         buffer.WriteByte(actor.CharacterIndex);
         buffer.WriteString(actor.FaceName);
         buffer.WriteByte(actor.FaceIndex);
         buffer.WriteByte(actor.Sex);
         foreach (var equip in actor.Equips) {
            buffer.WriteShort(equip);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFailedCreateActor(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.FAIL_CREATE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendRemoveActor(GameClient client, int actorId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REMOVE_ACTOR);
         buffer.WriteByte(actorId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendUseActor(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.USE_ACTOR);
         buffer.WriteShort(client.Id);
         buffer.WriteString(client.Name);
         buffer.WriteString(client.CharacterName);
         buffer.WriteByte(client.CharacterIndex);
         buffer.WriteString(client.FaceName);
         buffer.WriteByte(client.FaceIndex);
         buffer.WriteShort(client.ClassId);
         buffer.WriteByte(client.Sex);
         foreach(var equip in client.Equips) {
            buffer.WriteShort(equip);
         }
         foreach(var param in client.ParamBase) {
            buffer.WriteInt(param);
         }
         buffer.WriteInt(client.Hp);
         buffer.WriteInt(client.Mp);
         buffer.WriteInt(client.Exp);
         buffer.WriteShort(client.Points);
         buffer.WriteString(client.GuildName);
         buffer.WriteInt(client.Gold);
         buffer.WriteByte(client.Items.Count);
         foreach (var (itemId, amount) in client.Items) { 
            buffer.WriteShort(itemId);
            buffer.WriteShort(amount);
         }
         buffer.WriteByte(client.Weapons.Count);
         foreach (var (weaponId, amount) in client.Weapons) { 
            buffer.WriteShort(weaponId);
            buffer.WriteShort(amount);
         }
         buffer.WriteByte(client.Armors.Count);
         foreach (var (armorId, amount) in client.Armors) { 
            buffer.WriteShort(armorId);
            buffer.WriteShort(amount);
         }
         buffer.WriteByte(client.Skills.Count);
         foreach(var skill in client.Skills) {
            buffer.WriteShort(skill);
         }
         buffer.WriteByte(client.Friends.Count);
         foreach(var skill in client.Friends) {
            buffer.WriteString(skill);
         }
         buffer.WriteByte(client.Quests.Count);
         foreach (var (questId, quest) in client.Quests) { 
            buffer.WriteByte(questId);
            buffer.WriteByte((byte)quest.State);
         }
         foreach (var hotbar in client.Hotbar) { 
            buffer.WriteByte((byte)hotbar.Type);
            buffer.WriteShort(hotbar.ItemId);
         }
         foreach(var @switch in client.Switches.Data) {
            buffer.WriteBoolean(@switch);
         }
         foreach(var variable in client.Variables.Data) {
            buffer.WriteShort(variable);
         }
         buffer.WriteShort(client.SelfSwitches.Count);
         foreach (var (key, value) in client.SelfSwitches.Data) {
            buffer.WriteShort(key.MapId);
            buffer.WriteShort(key.EventId);
            buffer.WriteString($"{key.Ch}");
            buffer.WriteBoolean(value);
         }
         buffer.WriteShort(client.MapId);
         buffer.WriteShort(client.X);
         buffer.WriteShort(client.Y);
         buffer.WriteByte(client.Direction);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendMotd(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.MOTD);
         buffer.WriteString(Motd);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerData(GameClient client, int mapId) {
         if (Maps[mapId].HasZeroPlayers()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_DATA);
         buffer.WriteShort(client.Id);
         buffer.WriteByte(client.Group);
         buffer.WriteString(client.Name);
         buffer.WriteString(client.CharacterName);
         buffer.WriteByte(client.CharacterIndex);
         buffer.WriteByte(client.Sex);
         foreach(var equip in client.Equips) {
            buffer.WriteShort(equip);
         }
         buffer.WriteInt(client.ParamBase[(int)Param.MAXHP]);
         buffer.WriteInt(client.Hp);
         buffer.WriteInt(client.Exp);
         buffer.WriteString(client.GuildName);
         buffer.WriteShort(client.X);
         buffer.WriteShort(client.Y);
         buffer.WriteByte(client.Direction);
         SendDataToMap(mapId,buffer.ToStringBuffer());
      }
      public static void SendMapPlayers(GameClient player) {
         if (Maps[player.MapId].HasZeroPlayers()) return;
         BufferWriter buffer;
         foreach(var client in Clients.Values) {
            if (client == null || !client.IsInGame() || client.MapId != player.MapId || client == player)
               continue;
            buffer = new BufferWriter();
            buffer.WriteByte((byte)Packet.PLAYER_DATA);
            buffer.WriteShort(client.Id);
            buffer.WriteByte(client.Group);
            buffer.WriteString(client.Name);
            buffer.WriteString(client.CharacterName);
            buffer.WriteByte(client.CharacterIndex);
            buffer.WriteByte(client.Sex);
            foreach (var equip in client.Equips) {
               buffer.WriteShort(equip);
            }
            buffer.WriteInt(client.ParamBase[(int)Param.MAXHP]);
            buffer.WriteInt(client.Hp);
            buffer.WriteInt(client.Exp);
            buffer.WriteString(client.GuildName);
            buffer.WriteShort(client.X);
            buffer.WriteShort(client.Y);
            buffer.WriteByte(client.Direction);
            player.Send(buffer.ToStringBuffer());
         }
      }
      public static void SendRemovePlayer(int clientId, int mapId) {
         if (Maps[mapId].HasZeroPlayers()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REMOVE_PLAYER);
         buffer.WriteShort(clientId);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerMovement(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_MOVE);
         buffer.WriteShort(client.Id);
         buffer.WriteShort(client.X);
         buffer.WriteShort(client.Y);
         buffer.WriteByte(client.Direction);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void PlayerChatMessage(GameClient client, string message, int colorId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte(colorId);
         buffer.WriteString(message);
         client.Send(buffer.ToStringBuffer());
      }
      public static void MapChatMessage(int mapId, string message, int playerId, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Chat.MAP;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.MAP_MSG);
         buffer.WriteShort(playerId);
         buffer.WriteByte(colorId);
         buffer.WriteString(message);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void GlobalChatMessage(string message, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Chat.GLOBAL;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte(colorId);
         buffer.WriteString(message);
         SendDataToAll(buffer.ToStringBuffer());
      }
      public static void PartyChatMessage(GameClient client,string message) {
         if (!client.IsInParty()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte((byte)Chat.PARTY);
         buffer.WriteString(message);
         SendDataToParty(client.PartyId, buffer.ToStringBuffer());
      }
      public static void GuildChatMessage(GameClient client,string message, int colorId = -1) {
         if (colorId < 0)
            colorId = (int)Chat.GUILD;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte(colorId);
         buffer.WriteString(message);
         SendDataToGuild(client.GuildName, buffer.ToStringBuffer());
      }
      public static void PrivateChatMessage(GameClient client, string message, string name) {
         if (client.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return;
         var player = FindPlayer(name);
         if(player == null) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte((byte)Chat.PARTY);
         buffer.WriteString(message);
         client.Send(buffer.ToStringBuffer());
         player.Send(buffer.ToStringBuffer());
      }
      public static void AlertMessage(GameClient client, Alert type) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ALERT_MSG);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendWhosOnline(GameClient client, string message) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CHAT_MSG);
         buffer.WriteByte((byte)Chat.GLOBAL);
         buffer.WriteString(message);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAttackPlayer(int mapId, int hpDamage, int mpDamage, bool critical, short attackerId,
                  byte attackerType, byte aniIndex, int playerId, short animationId, bool notShowMissed) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ATTACK_PLAYER);
         buffer.WriteShort(attackerId);
         buffer.WriteByte(attackerType);
         buffer.WriteByte(aniIndex);
         buffer.WriteShort(playerId);
         buffer.WriteInt(hpDamage); 
         buffer.WriteInt(mpDamage); 
         buffer.WriteBoolean(critical);
         buffer.WriteShort(animationId);
         buffer.WriteBoolean(notShowMissed);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAttackEnemy(int mapId, int hpDamage, int mpDamage, bool critical, 
         short attackerId, byte attackerType, byte aniIndex, int eventId, short animationId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ATTACK_ENEMY);
         buffer.WriteShort(attackerId);
         buffer.WriteByte(attackerType);
         buffer.WriteByte(aniIndex);
         buffer.WriteShort(eventId);
         buffer.WriteInt(hpDamage); 
         buffer.WriteInt(mpDamage); 
         buffer.WriteBoolean(critical);
         buffer.WriteShort(animationId);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAnimation(GameBattler character, short animationId, short attackerId, 
               byte attackerType, byte aniIndex, byte characterType) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ANIMATION);
         buffer.WriteShort(attackerId);
         buffer.WriteByte(attackerType);
         buffer.WriteByte(aniIndex);
         buffer.WriteShort(character.Id);
         buffer.WriteByte(characterType);
         buffer.WriteShort(animationId);
         SendDataToMap(character.MapId, buffer.ToStringBuffer());
      }
      public static void SendBallon(GameBattler character, byte characterType, byte ballonId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.BALLOON);
         buffer.WriteShort(character.Id);
         buffer.WriteByte(characterType);
         buffer.WriteByte(ballonId);
         SendDataToMap(character.MapId, buffer.ToStringBuffer());
      }
      public static void SendEnemyBallon(GameClient client, short eventId, byte ballonId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.BALLOON);
         buffer.WriteShort(eventId);
         buffer.WriteByte((byte)Enums.Target.ENEMY);
         buffer.WriteByte(ballonId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendEnemyRevive(GameEvent @event) {
         if (Maps[@event.MapId].HasZeroPlayers()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ENEMY_REVIVE);
         buffer.WriteShort(@event.Id);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendMapEvents(GameClient client) {
         BufferWriter buffer;
         foreach(var (eventId, @event) in Maps[client.MapId].Events) {
            buffer = new BufferWriter();
            buffer.WriteByte((byte)Packet.EVENT_DATA);
            buffer.WriteShort(eventId);
            buffer.WriteShort(@event.X);
            buffer.WriteShort(@event.Y);
            buffer.WriteByte(@event.Direction);
            buffer.WriteInt(@event.Hp);
            client.Send(buffer.ToStringBuffer());
         }
      }
      public static void SendEventMovement(GameEvent @event) {
         if (Maps[@event.MapId].HasZeroPlayers()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.EVENT_MOVE);
         buffer.WriteShort(@event.Id);
         buffer.WriteShort(@event.X);
         buffer.WriteShort(@event.Y);
         buffer.WriteByte(@event.Direction);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendMapDrops(GameClient client) {
         BufferWriter buffer;
         foreach(var (dropId, drop) in Maps[client.MapId].Drops) {
            buffer = new BufferWriter();
            buffer.WriteByte((byte)Packet.ADD_DROP);
            buffer.WriteByte(dropId);
            buffer.WriteShort(drop.ItemId);
            buffer.WriteByte(drop.Kind);
            buffer.WriteShort(drop.Amount);
            buffer.WriteShort(drop.X);
            buffer.WriteShort(drop.Y);
            client.Send(buffer.ToStringBuffer());
         }
      }
      public static void SendAddDrop(int mapId, int dropId, short itemId, byte kind, short amount, short x, short y) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ADD_DROP);
         buffer.WriteByte(dropId);
         buffer.WriteShort(itemId);
         buffer.WriteByte(kind);
         buffer.WriteShort(amount);
         buffer.WriteShort(x);
         buffer.WriteShort(y);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendRemoveDrop(int mapId, short dropId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REMOVE_DROP);
         buffer.WriteShort(dropId);
         SendDataToMap(mapId, buffer.ToStringBuffer());
      }
      public static void SendAddProjectile(GameBattler client, short finishX, short finishY, 
               GameBattler target, byte projectileType, byte projectileId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ADD_PROJECTILE);
         buffer.WriteShort(client.X);
         buffer.WriteShort(client.Y);
         buffer.WriteShort(finishX);
         buffer.WriteShort(finishY);
         buffer.WriteShort(target.X);
         buffer.WriteShort(target.Y);
         buffer.WriteByte(projectileType);
         buffer.WriteByte(projectileId);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerVitals(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_VITALS);
         buffer.WriteShort(client.Id);
         buffer.WriteInt(client.Hp);
         buffer.WriteInt(client.Mp);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerExp(GameClient client, int exp) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_EXP);
         buffer.WriteShort(client.Id);
         buffer.WriteInt(client.Exp);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerState(GameClient client, short stateId, 
               bool addState = false, float stateTime = 0) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_STATE);
         buffer.WriteShort(client.Id);
         buffer.WriteShort(stateId);
         buffer.WriteBoolean(addState);
         if(addState)
            buffer.WriteFloat(stateTime);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerBuff(GameClient client, byte paramId, short buffLevel,
               float buffTime = 0, float buffMaxTime = 0) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_BUFF);
         buffer.WriteShort(client.Id);
         buffer.WriteByte(paramId);
         buffer.WriteByte(buffLevel);
         if(buffLevel != 0) {
            buffer.WriteFloat(buffTime);
            buffer.WriteFloat(buffMaxTime);
         }
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerSwitch(GameClient client, short switchId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.SWITCH);
         buffer.WriteShort(switchId);
         buffer.WriteBoolean(client.Switches[switchId]);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerVariable(GameClient client, short variableId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.VARIABLE);
         buffer.WriteShort(variableId);
         buffer.WriteShort(client.Variables[variableId]);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerSelfSwitch(GameClient client, (int MapId, int EventId, char Ch) key) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.SELF_SWITCH);
         buffer.WriteShort(key.MapId);
         buffer.WriteShort(key.EventId);
         buffer.WriteString($"{key.Ch}");
         buffer.WriteBoolean(client.SelfSwitches[key]);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerItem(GameClient client, short itemId, byte kind, short amount,
               bool dropSound, bool popUp) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_ITEM);
         buffer.WriteShort(itemId);
         buffer.WriteByte(kind);
         buffer.WriteShort(amount);
         buffer.WriteBoolean(dropSound);
         buffer.WriteBoolean(popUp);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerGold(GameClient client, int amount, bool dropSound, bool popUp) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_GOLD);
         buffer.WriteInt(amount);
         buffer.WriteBoolean(dropSound);
         buffer.WriteBoolean(popUp);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerParam(GameClient client, byte paramId, short value) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_PARAM);
         buffer.WriteShort(client.Id);
         buffer.WriteByte(paramId);
         buffer.WriteShort(value);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerEquip(GameClient client, byte slotId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_EQUIP);
         buffer.WriteShort(client.Id);
         buffer.WriteByte(slotId);
         buffer.WriteShort(client.Equips[slotId]);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerSkill(GameClient client, short skillId, bool learn = true) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_SKILL);
         buffer.WriteShort(skillId);
         buffer.WriteBoolean(learn);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerClass(GameClient client, short classId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_CLASS);
         buffer.WriteShort(classId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerSex(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_SEX);
         buffer.WriteShort(client.Id);
         buffer.WriteByte(client.Sex);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerGraphic(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_GRAPHIC);
         buffer.WriteShort(client.Id);
         buffer.WriteString(client.CharacterName);
         buffer.WriteByte(client.CharacterIndex);
         buffer.WriteString(client.FaceName);
         buffer.WriteByte(client.FaceIndex);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendPlayerPoints(GameClient client, short points) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_POINTS);
         buffer.WriteShort(points);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerHotbar(GameClient client, byte id) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_HOTBAR);
         buffer.WriteByte(id);
         buffer.WriteByte((byte)client.Hotbar[id].Type);
         buffer.WriteShort(client.Hotbar[id].ItemId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendPlayerCooldown(GameClient client, short id) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.PLAYER_COOLDOWN);
         buffer.WriteShort(id);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTarget(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.TARGET);
         buffer.WriteByte((byte)client.Target.Type);
         buffer.WriteShort(client.Target.Id);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTransferPlayer(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.TRANSFER);
         buffer.WriteShort(client.MapId);
         buffer.WriteShort(client.X);
         buffer.WriteShort(client.Y);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenFriends(GameClient client, List<string> onlineFriends) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_FRIENDS);
         buffer.WriteByte(onlineFriends.Count);
         foreach (string name in onlineFriends) { 
            buffer.WriteString(name);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAddFriend(GameClient client, string friendName) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ADD_FRIEND);
         buffer.WriteString(friendName);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendRemoveFriend(GameClient client, byte index) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REMOVE_FRIEND);
         buffer.WriteByte(index);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenCreateGuild(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_CREATE_GUILD);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenGuild(GameClient client, byte onlineMembersSize) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_GUILD);
         buffer.WriteString(Guilds[client.GuildName].Leader);
         buffer.WriteString(Guilds[client.GuildName].Notice);
         foreach(var colorId in Guilds[client.GuildName].Flag) {
            buffer.WriteByte(colorId);
         }
         buffer.WriteByte(Guilds[client.GuildName].Members.Count);
         buffer.WriteByte(onlineMembersSize);
         foreach(string name in Guilds[client.GuildName].Members) {
            buffer.WriteString(name);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildLeader(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.GUILD_LEADER);
         buffer.WriteString(Guilds[client.GuildName].Leader);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildNotice(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.GUILD_NOTICE);
         buffer.WriteString(Guilds[client.GuildName].Notice);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGuildName(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.GUILD_NAME);
         buffer.WriteString(client.GuildName);
         buffer.WriteShort(client.Id);
         SendDataToMap(client.MapId, buffer.ToStringBuffer());
      }
      public static void SendRemoveGuildMember(GameClient client, string name) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REMOVE_GUILD_MEMBER);
         buffer.WriteString(name);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendJoinParty(GameClient client, GameClient player) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.JOIN_PARTY);
         buffer.WriteShort(player.Id);
         buffer.WriteString(player.Name);
         buffer.WriteString(player.CharacterName);
         buffer.WriteByte(player.CharacterIndex);
         buffer.WriteByte(player.Sex);
         foreach(var equip in player.Equips) {
            buffer.WriteShort(equip);
         }
         buffer.WriteInt(player.ParamBase[(int)Param.MAXHP]);
         buffer.WriteInt(player.Hp);
         buffer.WriteInt(player.Exp);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendLeaveParty(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.LEAVE_PARTY);
         buffer.WriteShort(client.Id);
         SendDataToParty(client.PartyId, buffer.ToStringBuffer());
      }
      public static void SendDissolveParty(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.DISSOLVE_PARTY);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenBank(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_BANK);
         buffer.WriteInt(client.BankGold);
         buffer.WriteByte(client.BankItems.Count);
         foreach (var (itemId, amount) in client.BankItems) {
            buffer.WriteShort(itemId);
            buffer.WriteShort(amount);
         }
         buffer.WriteByte(client.BankWeapons.Count);
         foreach (var (weaponId, amount) in client.BankWeapons) {
            buffer.WriteShort(weaponId);
            buffer.WriteShort(amount);
         }
         buffer.WriteByte(client.BankArmors.Count);
         foreach (var (armorId, amount) in client.BankArmors) {
            buffer.WriteShort(armorId);
            buffer.WriteShort(amount);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendBankItem(GameClient client, short itemId, byte kind, short amount) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.BANK_ITEM);
         buffer.WriteShort(itemId);
         buffer.WriteByte(kind);
         buffer.WriteShort(amount);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendBankGold(GameClient client, int amount) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.BANK_GOLD);
         buffer.WriteInt(amount);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendCloseWindow(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.CLOSE_WINDOW);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenShop(GameClient client, short eventId, short index) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_SHOP);
         buffer.WriteShort(eventId);
         buffer.WriteShort(index);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendOpenTeleport(GameClient client, byte teleportId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.OPEN_TELEPORT);
         buffer.WriteByte(teleportId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendEventCommand(GameClient client, short eventId, short initialIndex, short finalIndex) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.EVENT_COMMAND);
         buffer.WriteShort(eventId);
         buffer.WriteShort(initialIndex);
         buffer.WriteShort(finalIndex);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendParallelProcessCommand(GameEvent @event, short initialIndex, short finalIndex) {
         if (Maps[@event.MapId].HasZeroPlayers()) return;
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.EVENT_COMMAND);
         buffer.WriteShort(@event.Id);
         buffer.WriteShort(initialIndex);
         buffer.WriteShort(finalIndex);
         SendDataToMap(@event.MapId, buffer.ToStringBuffer());
      }
      public static void SendRequest(GameClient client, Enums.Request type, GameClient player) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.REQUEST);
         buffer.WriteByte((byte)type);
         buffer.WriteString(player.Name);
         buffer.WriteString(player.GuildName);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAcceptRequest(GameClient client, Enums.Request type) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ACCEPT_REQUEST);
         buffer.WriteByte((byte)type);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTradeItem(GameClient client, short playerId, short itemId, byte kind, short amount) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.TRADE_ITEM);
         buffer.WriteShort(playerId);
         buffer.WriteShort(itemId);
         buffer.WriteByte(kind);
         buffer.WriteShort(amount);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendTradeGold(GameClient client, short playerId, int amount) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.TRADE_GOLD);
         buffer.WriteShort(playerId);
         buffer.WriteInt(amount);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAddQuest(GameClient client, byte questId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ADD_QUEST);
         buffer.WriteByte(questId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendFinishQuest(GameClient client, byte questId) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.FINISH_QUEST);
         buffer.WriteByte(questId);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendVipDays(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.VIP_DAYS);
         buffer.WriteTime(client.VipTime.AddSeconds(client.AddedVipTime));
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendLogout(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.LOGOUT);
         buffer.WriteByte(client.ActorId);
         buffer.WriteString(client.Name);
         buffer.WriteString(client.CharacterName);
         buffer.WriteByte(client.CharacterIndex);
         buffer.WriteString(client.FaceName);
         buffer.WriteByte(client.FaceIndex);
         buffer.WriteByte(client.Sex);
         foreach (var equip in client.Equips) {
            buffer.WriteShort(equip);
         }
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendAdminCommand(GameClient client, byte command, string alertMsg = "") {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.ADMIN_COMMAND);
         buffer.WriteByte(command);
         buffer.WriteString(alertMsg);
         client.Send(buffer.ToStringBuffer());
      }
      public static void SendGlobalSwitch(short switchId, bool value) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.SWITCH);
         buffer.WriteShort(switchId);
         buffer.WriteBoolean(value);
         SendDataToAll(buffer.ToStringBuffer());
      }
      public static void SendGlobalSwitches(GameClient client) {
         BufferWriter buffer = new();
         buffer.WriteByte((byte)Packet.NET_SWITCHES);
         for(int switchId = 0; switchId < 100; switchId++) {
            buffer.WriteBoolean(Switches[switchId + Configs.MaxPlayerSwitches + 1]);
         }
         client.Send(buffer.ToStringBuffer());
      }
      /*public static void Send(GameClient client) {
         BufferWriter buffer = new BufferWriter();
         buffer.WriteByte((byte)Packet.REMOVE_ACTOR);
         client.Send(buffer.ToStringBuffer());
      }*/
   }
}
