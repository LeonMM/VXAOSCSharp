using System;
using System.Linq;
using VXAOS_Server.RPGData;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public static partial class Network {
      public static void HandleMessages(GameClient client, BufferReader buffer) {
         try {
            Packet packet = (Packet)buffer.ReadByte();
            if (Enum.IsDefined(typeof(Packet), packet)) {
               if (client.IsInGame()) {
                  HandleGameMessages(client, packet, buffer);
               } else {
                  HandleMenuMessages(client, packet, buffer);
               }
            } else {
               throw new Exception("Packet Inválido / Invalid Packet");
            }
         } catch (Exception e) {
            WriteColor($"Error {e.Message}",ConsoleColor.Red);
            client.Disconnect();
         }
      }
      static void HandleMenuMessages(GameClient client, Packet packet, BufferReader buffer) {
         switch (packet) {
            case Packet.LOGIN:
               _ = HandleLogin(client, buffer);
               break;
            case Packet.CREATE_ACCOUNT:
               _ = HandleCreateAccount(client, buffer);
               break;
            case Packet.CREATE_ACTOR:
               _ = HandleCreateActor(client, buffer);
               break;
            case Packet.REMOVE_ACTOR:
               _ = HandleRemoveActor(client, buffer);
               break;
            case Packet.USE_ACTOR:
               _ = HandleUseActor(client, buffer);
               break;
         }
         client.InactivityTime = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.InactivityTime);
      }
      static void HandleGameMessages(GameClient client, Packet packet, BufferReader buffer) {
         switch (packet) {
            case Packet.PLAYER_MOVE:
               HandlePlayerMovement(client, buffer);
               break;
            case Packet.CHAT_MSG:
               HandleChatMessage(client, buffer);
               break;
            case Packet.PLAYER_ATTACK:
               HandlePlayerAttack(client);
               break;
            case Packet.USE_ITEM:
               HandleUseItem(client, buffer);
               break;
            case Packet.USE_SKILL:
               HandleUseSkill(client, buffer);
               break;
            case Packet.BALLOON:
               HandleBalloon(client, buffer);
               break;
            case Packet.USE_HOTBAR:
               HandleUseHotbar(client, buffer);
               break;
            case Packet.ADD_DROP:
               HandleAddDrop(client, buffer);
               break;
            case Packet.REMOVE_DROP:
               HandleRemoveDrop(client, buffer);
               break;
            case Packet.PLAYER_PARAM:
               HandlePlayerParam(client, buffer);
               break;
            case Packet.PLAYER_EQUIP:
               HandlePlayerEquip(client, buffer);
               break;
            case Packet.PLAYER_HOTBAR:
               HandlePlayerHotbar(client, buffer);
               break;
            case Packet.TARGET:
               HandleTarget(client, buffer);
               break;
            case Packet.OPEN_FRIENDS:
               HandleOpenFriends(client);
               break;
            case Packet.REMOVE_FRIEND:
               HandleRemoveFriend(client, buffer);
               break;
            case Packet.CREATE_GUILD:
               HandleCreateGuild(client, buffer);
               break;
            case Packet.OPEN_GUILD:
               HandleOpenGuild(client);
               break;
            case Packet.GUILD_LEADER:
               HandleGuildLeader(client, buffer);
               break;
            case Packet.GUILD_NOTICE:
               HandleGuildNotice(client, buffer);
               break;
            case Packet.REMOVE_GUILD_MEMBER:
               HandleRemoveGuildMember(client, buffer);
               break;
            case Packet.GUILD_REQUEST:
               HandleGuildRequest(client, buffer);
               break;
            case Packet.LEAVE_GUILD:
               HandleLeaveGuild(client);
               break;
            case Packet.LEAVE_PARTY:
               HandleLeaveParty(client);
               break;
            case Packet.CHOICE:
               HandleChoice(client, buffer);
               break;
            case Packet.BANK_ITEM:
               HandleBankItem(client, buffer);
               break;
            case Packet.BANK_GOLD:
               HandleBankGold(client, buffer);
               break;
            case Packet.CLOSE_WINDOW:
               HandleCloseWindow(client);
               break;
            case Packet.BUY_ITEM:
               HandleBuyItem(client, buffer);
               break;
            case Packet.SELL_ITEM:
               HandleSellItem(client, buffer);
               break;
            case Packet.CHOICE_TELEPORT:
               HandleChoiceTeleport(client, buffer);
               break;
            case Packet.NEXT_COMMAND:
               HandleNextEventCommand(client);
               break;
            case Packet.REQUEST:
               HandleRequest(client, buffer);
               break;
            case Packet.ACCEPT_REQUEST:
               HandleAcceptRequest(client);
               break;
            case Packet.DECLINE_REQUEST:
               HandleDeclineRequest(client);
               break;
            case Packet.TRADE_ITEM:
               HandleTradeItem(client, buffer);
               break;
            case Packet.TRADE_GOLD:
               HandleTradeGold(client, buffer);
               break;
            case Packet.LOGOUT:
               HandleLogout(client);
               break;
            case Packet.ADMIN_COMMAND:
               HandleAdminCommand(client, buffer);
               break;
         }
      }
      static async Task HandleLogin(GameClient client, BufferReader buffer) {
         string user = buffer.ReadString();
         string pass = buffer.ReadString();
         short version = buffer.ReadShort();
         bool exists = await DB.DoAccountExist(user);
         if (LoginHackingAttempt(client)) {
            client.Disconnect();
            return;
         } else if (version != (short)Configs.GameVersion) {
            SendFailedLogin(client, Login.OLD_VERSION);
            client.CloseAfterWriting();
            return;
         } else if (IsIpBlocked(client.Ip)) {
            SendFailedLogin(client, Login.IP_BLOCKED);
            client.CloseAfterWriting();
            return;
         } else if (!exists) {
            SendFailedLogin(client, Login.INVALD_USER);
            AddAttempt(client);
            client.CloseAfterWriting();
            return;
         } else if (MultiAccounts(user, client.Ip)) {
            SendFailedLogin(client, Login.MULTI_ACCOUNT);
            client.CloseAfterWriting();
            return;
         }
         Account? account = await DB.LoadAccount(user);
         if(account == null) {
            client.Disconnect();
            return;
         }
         if (account.Pass != pass) {
            SendFailedLogin(client, Login.INVALID_PASS);
            AddAttempt(client);
            client.CloseAfterWriting();
            return;
         } else if (IsBanned(account.IdDb.ToString())) {
            SendFailedLogin(client, Login.ACC_BANNED);
            client.CloseAfterWriting();
            return;
         }
         client.User = user;
         client.AccountIdDb = account.IdDb;
         client.Pass = account.Pass;
         client.Group = account.Group;
         client.VipTime = account.VipTime;
         client.Actors = account.Actors;
         client.Friends = account.Friends;
         client.Handshake = true;
         await DB.LoadBank(client);
         SendLogin(client);
         BlockedIps.TryRemove(client.Ip, out _);
         Console.WriteLine($"{user} logou com IP {client.Ip}");
      }
      static async Task HandleCreateAccount(GameClient client, BufferReader buffer) {
         string user = buffer.ReadString().Trim();
         string pass = buffer.ReadString();
         string email = buffer.ReadString();
         short version = buffer.ReadShort();
         bool exists = await DB.DoAccountExist(user);
         if (client.IsSpawning()) {
            return;
         } else if (CreateAccountHackingAtempt(client, user, pass, email)) {
            client.Disconnect();
            return;
         } else if (version != (short)Configs.GameVersion) {
            SendFailedLogin(client, Login.OLD_VERSION);
            client.CloseAfterWriting();
            return;
         } else if (IsIpBlocked(client.Ip)) {
            SendFailedLogin(client, Login.IP_BLOCKED);
            client.CloseAfterWriting();
            return;
         } else if (exists) {
            SendCreateAccount(client, Register.ACC_EXIST);
            client.CloseAfterWriting();
            return;
         }
         client.AntispamTime.AddSeconds(0.5);
         await DB.CreateAccount(user, pass, email);
         SendCreateAccount(client, Register.SUCCESSFUL);
         client.CloseAfterWriting();
         Console.WriteLine($"Conta {user} criada.");
      }
      static async Task HandleCreateActor(GameClient client, BufferReader buffer) {
         byte actorId = buffer.ReadByte();
         string name = Titleize(buffer.ReadString());
         byte characterIndex = buffer.ReadByte();
         short classId = buffer.ReadShort();
         byte sex = buffer.ReadByte();
         int[] @params = new int[8];
         for (int i = 0; i < 8; i++) {
            @params[i] = (int)buffer.ReadByte();
         }
         int maxParams = @params.Sum(x => x);
         int points = Configs.StartPoints - maxParams;
         if (client.IsSpawning() || !client.IsLogged() || actorId >= Configs.MaxActors ||
            client.Actors.ContainsKey(actorId) || name.Length < Configs.MinCharacters ||
            name.Length > Configs.MaxCharacters || IsInvalidName(name) || (IllegalName(name) && client.IsStandard()) ||
            classId < 1 || classId > client.MaxClasses() || sex > (byte)Sex.FEMALE ||
            characterIndex >= DataClasses[classId].graphics[sex].Count || (maxParams + points) > Configs.StartPoints
            )
            return;
         if(await DB.DoPlayerExist(name)) {
            SendFailedCreateActor(client);
         }
         client.AntispamTime.AddSeconds(0.5);
         await DB.CreatePlayer(client, actorId, name, characterIndex, classId, sex, @params, points);
         SendCreateActor(client, actorId, client.Actors[actorId]);
      }
      static async Task HandleRemoveActor(GameClient client, BufferReader buffer) {
         byte actorId = buffer.ReadByte();
         string pass = buffer.ReadString();
         if (!client.Actors.ContainsKey(actorId))
            return;
         if (client.Pass != pass) {
            SendFailedLogin(client, Login.INVALID_PASS);
            AddAttempt(client);
            return;
         }
         await DB.RemovePlayer(client.Actors[actorId].IdDb);
         client.RemoveActorGuild(client.Actors[actorId].GuildName, client.Actors[actorId].Name);
         client.Actors.Remove(actorId);
         SendRemoveActor(client, actorId);
      }
      static async Task HandleUseActor(GameClient client, BufferReader buffer) {
         try { 
         byte actorId = buffer.ReadByte();
         if (!client.Actors.ContainsKey(actorId))
            return;
         client.LoadData(actorId);
         SendPlayerData(client, client.MapId);
         Maps[client.MapId].TotalPlayers++;
         await DB.ChangeWhosOnline(client.IdDb, true);
         client.JoinGame(actorId);
         SendUseActor(client);
         client.LoadStates();
         SendGlobalSwitches(client);
         SendMapPlayers(client);
         SendMapEvents(client);
         SendMapDrops(client);
         SendMotd(client);
         } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
         }
      }
      private static void HandlePlayerMovement(GameClient client, BufferReader buffer) {
         byte d = buffer.ReadByte();
         if (d < (byte)Dir.DOWN_LEFT || d > (byte)Dir.UP_RIGHT) return;
         client.StopCount = DateTimeOffset.UtcNow.AddMilliseconds(170);
         client.MoveStraight(d);
         if (client.MoveSucceed) {
            client.CheckFloorEffect();
            client.CheckTouchEvent();
            client.CloseWindows();
         }
      }
      private static void HandleChatMessage(GameClient client, BufferReader buffer) {
         string message = buffer.ReadString();
         Chat talkType = (Chat)buffer.ReadByte();
         string name = buffer.ReadString();
         if (string.IsNullOrEmpty(message)) return;
         if (talkType == Chat.GLOBAL && client.IsGlobalChatSpawning() && message != "/who") return;
         if (client.IsSpawning()) return;
         if (client.IsMuted()) return;
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         if (message == "/who") {
            WhosOnline(client);
            return;
         }
         message = $"{client.Name}: {ChatFilter(message)}";
         switch (talkType) {
            case Chat.MAP:
               MapChatMessage(client.MapId, message, client.Id, !client.IsStandard() ? 15 + client.Group : (int)Chat.MAP);
               break;
            case Chat.GLOBAL:
               client.GlobalAntispamTime = DateTimeOffset.UtcNow.AddSeconds(Configs.GlobalAntispamTime);
               GlobalChatMessage(message, !client.IsStandard() ? 15 + client.Group : (int)Chat.GLOBAL);
               break;
            case Chat.PARTY:
               PartyChatMessage(client, message);
               break;
            case Chat.GUILD:
               GuildChatMessage(client, message);
               break;
            case Chat.PRIVATE:
               PrivateChatMessage(client, message, name);
               break;
         }
      }
      private static void HandlePlayerAttack(GameClient client) {
         if (client.IsAttacking()) return;
         if (client.IsUsingRangeWeapon()) {
            client.AttackRange();
         } else if (client.IsUsingNormalWeapon()) {
            client.AttackNormal();
         }
         if (client.IsMovable()) {
            client.CheckEventTriggerHere([0]);
            client.CheckEventTriggerThere([0, 1, 2]);
         }
      }
      private static void HandleUseItem(GameClient client, BufferReader buffer) {
         int itemId = buffer.ReadShort();
         if (client.IsUsingItem()) {
            client.UseItem(DataItems[itemId]);
         }
      }
      private static void HandleUseSkill(GameClient client, BufferReader buffer) {
         int skillId = buffer.ReadShort();
         if (client.IsUsingSkill(skillId)) {
            client.UseItem(DataSkills[skillId]);
         }         
      }
      private static void HandleBalloon(GameClient client, BufferReader buffer) {
         byte balloonId = buffer.ReadByte();
         if (balloonId > 10 || client.IsSpawning()) return;
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         SendBallon(client, (byte)Enums.Target.PLAYER, balloonId);
      }
      private static void HandleUseHotbar(GameClient client, BufferReader buffer) {
         byte id = buffer.ReadByte();
         var itemId = buffer.ReadShort();
         if (client.Hotbar[id] == null) return;
         if (client.Hotbar[id].Type == Enums.Hotbar.ITEM && client.IsUsingItem()) return;
         if (client.Hotbar[id].Type == Enums.Hotbar.SKILL && client.IsUsingSkill(itemId)) return;
         client.UseItem(client.Hotbar[id].Type == Enums.Hotbar.ITEM ? DataItems[itemId] : DataSkills[itemId]);
      }
      private static void HandleAddDrop(GameClient client, BufferReader buffer) {
         var itemId = buffer.ReadShort();
         var kind = buffer.ReadByte();
         var amount = buffer.ReadShort();
         var item = client.ItemObject(kind, itemId);
         if (item == null || client.IsInTrade() || Maps[client.MapId].IsFullDrops() || amount < 1 ||
            amount > client.ItemNumber(item) || (item is IRPGItem iitem && iitem.IsSoulbound()) || client.IsSpawning())
            return;
         client.LoseItem(item, amount);
         Maps[client.MapId].AddDrop(itemId, kind, amount, client.X, client.Y);         
      }
      private static void HandleRemoveDrop(GameClient client, BufferReader buffer) {
         var dropId = buffer.ReadByte();
         if (!Maps[client.MapId].Drops.TryGetValue(dropId, out var drop)) return;
         if (!client.Pos(drop.X, drop.Y)) return;
         if(!CanPickUpDrop(drop, client)) {
            AlertMessage(client, Alert.NOT_PICK_UP_DROP);
            return;
         }
         var item = client.ItemObject(drop.Kind, drop.ItemId);
         if (item != null && !client.IsFullInventory(item)) {
            client.GainItem(item, drop.Amount, true, true);
            Maps[client.MapId].RemoveDrop(dropId);
         }
      }
      private static void HandlePlayerParam(GameClient client, BufferReader buffer) {
         var paramId = buffer.ReadByte();
         if (client.Points == 0) return;
         client.Points--;
         if (paramId == (int)Param.MAXHP || paramId == (int)Param.MAXMP) {
            client.AddParam(paramId, 10);
         } else if (paramId >= (int)Param.ATK && paramId <= (int)Param.LUK) {
            client.AddParam(paramId, 1);
         }
      }
      private static void HandlePlayerEquip(GameClient client, BufferReader buffer) {
         var slotId = buffer.ReadByte();
         var itemId = buffer.ReadShort();
         if (client.IsSpawning() || client.IsEquipTypeFixed(slotId)) return;
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         client.ChangeEquip(slotId, itemId);
      }
      private static void HandlePlayerHotbar(GameClient client, BufferReader buffer) {
         var id = buffer.ReadByte();
         var type = (Enums.Hotbar)buffer.ReadByte();
         var itemId = buffer.ReadShort();
         if (id > Configs.MaxHotbar) return;
         client.ChangeHotbar(id, type, itemId);
      }
      private static void HandleTarget(GameClient client, BufferReader buffer) {
         var type = (Enums.Target)buffer.ReadByte();
         var targetId = buffer.ReadShort();
         client.ChangeTarget(targetId, type);
      }
      private static void HandleOpenFriends(GameClient client) {
         List<string> friends = new();
         int onlineIndex = 0;
         foreach (var name in client.Friends) {
            if (FindPlayer(name) != null) {
               friends.Insert(onlineIndex, name);
               onlineIndex++;
            } else {
               friends.Add(name);
            }
         }
         client.Friends = friends;
         client.OnlineFriendsSize = onlineIndex;
         SendOpenFriends(client, friends);
      }
      private static void HandleRemoveFriend(GameClient client, BufferReader buffer) {
         var index = buffer.ReadByte();
         client.Friends.RemoveAt(index);
         if (index <= client.OnlineFriendsSize - 1)
            client.OnlineFriendsSize--;
         SendRemoveFriend(client, index);
      }
      private static void HandleCreateGuild(GameClient client, BufferReader buffer) {
         if (!client.IsCreatingGuild() || client.IsInGuild() || client.IsSpawning()) return;
         string name = Titleize(buffer.ReadString());
         if (name.Length < Configs.MinCharacters || name.Length > Configs.MaxCharacters || IsInvalidName(name)) return;
         List<int> flag = new();
         for(int i = 0; i < 64; i++) {
            flag.Add(buffer.ReadByte());
         }
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         _ = CreateGuild(client, name, flag);
      }
      private static void HandleOpenGuild(GameClient client) {
         if (!client.IsInGuild()) return;
         OpenGuild(client);
      }
      private static void HandleGuildLeader(GameClient client, BufferReader buffer) {
         var name = buffer.ReadString();
         if (!client.IsInGuild() || !client.IsGuildLeader()) return;
         _ = ChangeGuildLeader(client, name);
      }
      private static void HandleGuildNotice(GameClient client, BufferReader buffer) {
         var notice = buffer.ReadString();
         if (!client.IsInGuild() || !client.IsGuildLeader()) return;
         if (string.IsNullOrEmpty(notice) || notice.Length > 64) return;
         if (client.IsSpawning()) return;
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         _ = ChangeGuildNotice(client, notice);
      }
      private static void HandleRemoveGuildMember(GameClient client, BufferReader buffer) {
         var name = buffer.ReadString();
         if (!client.IsInGuild() || !client.IsGuildLeader()) return;
         var member = FindGuildMember(Guilds[client.GuildName], name);
         if (member != null && Guilds[client.GuildName].Leader != member) {
            _ = RemoveGuildMember(client, member);
         } else {
            AlertMessage(client, Alert.INVALID_NAME);
         }
      }
      private static void HandleGuildRequest(GameClient client, BufferReader buffer) {
         if (!client.IsInGuild() || !client.IsGuildLeader()) return;
         if (client.IsSpawning()) return;
         var player = FindPlayer(buffer.ReadString());
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         if(player == null || player.IsInGuild()) {
            AlertMessage(client, Alert.INVALID_NAME);
            return;
         }else if(Guilds[client.GuildName].Members.Count >= Configs.MaxGuildMembers) {
            AlertMessage(client, Alert.FULL_GUILD);
         }
         player.Request.Id = client.Id;
         player.Request.Type = Enums.Request.GUILD;
         SendRequest(player, Enums.Request.GUILD, client);
      }
      private static void HandleLeaveGuild(GameClient client) {
         if (!client.IsInGuild()) return;
         if (client.IsGuildLeader()) {
            _ = RemoveGuild(client.GuildName);
         } else {
            client.LeaveGuild();
         }
      }
      private static void HandleLeaveParty(GameClient client) {
         client.LeaveParty();
      }
      private static void HandleChoice(GameClient client, BufferReader buffer) {
         var index = buffer.ReadInt();
         if (!client.HasText()) return;
         client.Choice = index;
         client.MessageInterpreter.Resume();
      }
      private static void HandleBankItem(GameClient client, BufferReader buffer) {
         if (!client.IsInBank()) return;
         var itemId = buffer.ReadShort();
         var kind = buffer.ReadByte();
         var amount = buffer.ReadShort();
         var item = client.ItemObject(kind, itemId);
         var container = client.BankItemContainer(kind);
         if (container == null || item == null) return;
         if (amount > 0 && (client.ItemNumber(item) < amount || client.IsFullBank(kind, itemId))) return;
         if (amount < 0 && (client.BankItemNumber(kind, itemId) < Math.Abs(amount) || client.IsFullInventory(item))) return;
         if (item is IRPGItem iitem && iitem.IsSoulbound()) return;
         client.GainBankItem(itemId, kind, amount);
         client.LoseItem(item, amount);
      }
      private static void HandleBankGold(GameClient client, BufferReader buffer) {
         if (!client.IsInBank()) return;
         var amount = buffer.ReadInt();
         if ((amount > 0 && client.Gold < amount) || (amount < 0 && client.BankGold < Math.Abs(amount))) return;
         client.GainBankGold(amount);
         client.LoseGold(amount);
      }
      private static void HandleCloseWindow(GameClient client) {
         client.CloseBank();
         client.CloseShop();
         client.CloseTrade();
         client.CloseCreateGuild();
         client.CloseTeleport();
      }
      private static void HandleBuyItem(GameClient client, BufferReader buffer) {
         if (!client.IsInShop()) return;
         var index = buffer.ReadByte();
         var amount = Math.Abs(buffer.ReadShort());
         if (!client.ShopGoods.HasIndex(index)) return;
         var kind = client.ShopGoods[index][0].AsByte();
         var itemId = client.ShopGoods[index][1].AsShort();
         var item = client.ItemObject(kind, itemId);
         if (item == null) return;
         var price = client.ShopGoods[index][2].AsByte() == 0 ? ((IRPGItem)item).Price() : client.ShopGoods[index][3].AsShort();
         if(client.Gold >= price * amount && (!client.IsFullInventory(item) || amount < 0)) {
            client.GainItem(item, amount);
            client.LoseGold(price * amount, true);
         }
      }
      private static void HandleSellItem(GameClient client, BufferReader buffer) {
         if (!client.IsInShop() || client.ShopGoods[0][4].AsBool()) return;         
         var itemId = buffer.ReadShort();
         var kind = buffer.ReadByte();
         var amount = Math.Abs(buffer.ReadShort());
         var item = client.ItemObject(kind, itemId);
         if(item != null && client.ItemNumber(item) >= amount) {
            client.LoseItem(item, amount);
            client.GainGold(amount * ((IRPGItem)item).Price() / 2, true);
         }
      }
      private static void HandleChoiceTeleport(GameClient client, BufferReader buffer) {
         if (!client.IsInTeleport()) return;
         var index = buffer.ReadByte();
         if (Configs.Teleports[client.TeleportId][index] == null) return;
         if (Configs.Teleports[client.TeleportId][index].Gold > client.Gold) return;
         client.LoseGold(Configs.Teleports[client.TeleportId][index].Gold);
         client.Transfer(
               Configs.Teleports[client.TeleportId][index].MapId,
               Configs.Teleports[client.TeleportId][index].X,
               Configs.Teleports[client.TeleportId][index].Y,
               (int)Dir.DOWN
            );
      }
      private static void HandleNextEventCommand(GameClient client) {
         if (!client.HasText()) return;
         var interpreter = client.MessageInterpreter;
         if (interpreter == null) return;
         client.MessageInterpreter = null;
         interpreter.Resume();
      }
      private static void HandleRequest(GameClient client, BufferReader buffer) {
         if(client.IsSpawning()) return;
         var type = (Enums.Request)buffer.ReadByte();
         int playerId = buffer.ReadShort();
         client.AntispamTime = DateTimeOffset.UtcNow.AddMilliseconds(500);
         if (IsRequestedUnavailable(client, Clients[playerId])) return;
         switch (type) {
            case Enums.Request.TRADE:
               if (client.IsInTrade() || client.IsInShop() || client.IsInBank()) return;
               if (Clients[playerId].IsInTrade() || Clients[playerId].IsInShop() || Clients[playerId].IsInBank()) {
                  AlertMessage(client, Alert.BUSY);
                  return;
               }
               break;
            case Enums.Request.FINISH_TRADE:
               if (client.IsInTrade())
                  playerId = client.TradePlayerId;
               break;
            case Enums.Request.PARTY:
               if (client.IsInParty() && Parties[client.PartyId].Members.Count >= Configs.MaxPartyMembers) return;
               if (Clients[playerId].IsInParty()) {
                  AlertMessage(client, Alert.IN_PARTY);
                  return;
               }
               break;
            case Enums.Request.FRIEND:
               if (client.Friends.Count >= Configs.MaxFriends || client.Friends.Contains(Clients[playerId].Name)) return;
               if (Clients[playerId].Friends.Contains(client.Name)) {
                  client.AddFriend(Clients[playerId]);
                  return;
               }
               break;
            case Enums.Request.GUILD:
               if (!client.IsInGuild() || Clients[playerId].IsInGuild()) return;
               if (!client.IsGuildLeader()) {
                  AlertMessage(client, Alert.NOT_GUILD_LEADER);
                  return;
               }else if (Guilds[client.GuildName].Members.Count >= Configs.MaxGuildMembers) {
                  AlertMessage(client, Alert.FULL_GUILD);
               }
               break;
         }
         Clients[playerId].Request.Id = client.Id;
         Clients[playerId].Request.Type = type;
         SendRequest(Clients[playerId], type, client);
      }
      private static void HandleAcceptRequest(GameClient client) {
         switch (client.Request.Type) {
            case Enums.Request.TRADE:
               client.OpenTrade();
               break;
            case Enums.Request.FINISH_TRADE:
               client.FinishTrade();
               break;
            case Enums.Request.PARTY:
               client.AcceptParty();
               break;
            case Enums.Request.FRIEND:
               client.AcceptFriend();
               break;
            case Enums.Request.GUILD:
               client.AcceptGuild();
               break;
         }
         client.ClearRequest();
      }
      private static void HandleDeclineRequest(GameClient client) {
         switch (client.Request.Type) {
            case Enums.Request.TRADE: case Enums.Request.PARTY: case Enums.Request.FRIEND: case Enums.Request.GUILD:
               if (Clients.TryGetValue(client.Request.Id, out var player) && player.IsInGame())
                  AlertMessage(player, Alert.REQUEST_DECLINED);
               break;
            case Enums.Request.FINISH_TRADE:
               if(client.IsInTrade())
                  AlertMessage(Clients[client.Request.Id], Alert.REQUEST_DECLINED);
               break;
         }
         client.ClearRequest();         
      }
      private static void HandleTradeItem(GameClient client, BufferReader buffer) {
         if (!client.IsInTrade()) return;
         var itemId = buffer.ReadShort();
         var kind = buffer.ReadByte();
         var amount = buffer.ReadShort();
         var item = client.ItemObject(kind, itemId);
         if (item == null) return;
         var container = client.TradeItemContainer(item);
         if (container == null) return;
         if (amount > 0 && (client.ItemNumber(item) < client.TradeItemNumber(item) + amount || client.IsFullTrade(item))) return;
         if (amount < 0 && (client.TradeItemNumber(item) < amount)) return;
         if (item is IRPGItem iitem && iitem.IsSoulbound()) return;
         client.GainTradeItem(item, amount);
         client.CloseTradeRequest();
      }
      private static void HandleTradeGold(GameClient client, BufferReader buffer) {
         if (!client.IsInTrade()) return;
         var amount = buffer.ReadInt();
         if ((amount > 0 && client.Gold < client.TradeGold + amount) || 
            (amount < 0 && client.BankGold < amount)) return;
         client.GainTradeGold(amount);
         client.CloseTradeRequest();
      }
      private static void HandleLogout(GameClient client) {
         client.LoadOriginalGraphic();
         SendLogout(client);
         client.UpdateCurrentActor();
         client.LeaveGame();
         client.InactivityTime = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.InactivityTime);
      }
      private static void HandleAdminCommand(GameClient client, BufferReader buffer) {
         var command = (Command)buffer.ReadByte();
         var str = buffer.ReadString();
         var int1 = buffer.ReadInt();
         var int2 = buffer.ReadInt();
         var int3 = buffer.ReadShort();
         if (client.IsAdmin()) {
            AdminCommands(client, command, str, int1, int2, int3);
         }else if (client.IsMonitor()) {
            MonitorCommands(client, command, str);
         }
      }
   }
}
