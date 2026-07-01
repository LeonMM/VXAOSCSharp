using System.Security.Principal;

namespace VXAOS_Server {
   public static partial class Network {
      public static void HandleMessages(GameClient client, BufferReader buffer) {
         try {
            Enums.Packet packet = (Enums.Packet)buffer.ReadByte();
            if (Enum.IsDefined(typeof(Enums.Packet), packet)) {
               if (client.IsInGame()) {
                  HandleGameMessages(client, packet, buffer);
               } else {
                  HandleMenuMessages(client, packet, buffer);
               }
            } else {
               throw new Exception("Packet Inválido / Invalid Packet");
            }
         } catch (Exception e) {
            CSExt.WriteColor($"Error {e.Message}",ConsoleColor.Red);
            client.Disconnect();
         }
      }
      static void HandleMenuMessages(GameClient client, Enums.Packet packet, BufferReader buffer) {
         switch (packet) {
            case Enums.Packet.LOGIN:
               _ = HandleLogin(client, buffer);
               break;
            case Enums.Packet.CREATE_ACCOUNT:
               _ = HandleCreateAccount(client, buffer);
               break;
            case Enums.Packet.CREATE_ACTOR:
               _ = HandleCreateActor(client, buffer);
               break;
            case Enums.Packet.REMOVE_ACTOR:
               _ = HandleRemoveActor(client, buffer);
               break;
            case Enums.Packet.USE_ACTOR:
               _ = HandleUseActor(client, buffer);
               break;
         }
      }
      static void HandleGameMessages(GameClient client, Enums.Packet packet, BufferReader buffer) {
         switch (packet) {
            case Enums.Packet.PLAYER_MOVE:
               HandlePlayerMovement(client, buffer);
               break;
            case Enums.Packet.CHAT_MSG:
               HandleChatMessage(client, buffer);
               break;
            case Enums.Packet.PLAYER_ATTACK:
               HandlePlayerAttack(client);
               break;
            case Enums.Packet.USE_ITEM:
               HandleUseItem(client, buffer);
               break;
            case Enums.Packet.USE_SKILL:
               HandleUseSkill(client, buffer);
               break;
            case Enums.Packet.BALLOON:
               HandleBalloon(client, buffer);
               break;
            case Enums.Packet.USE_HOTBAR:
               HandleUseHotbar(client, buffer);
               break;
            case Enums.Packet.ADD_DROP:
               HandleAddDrop(client, buffer);
               break;
            case Enums.Packet.REMOVE_DROP:
               HandleRemoveDrop(client, buffer);
               break;
            case Enums.Packet.PLAYER_PARAM:
               HandlePlayerParam(client, buffer);
               break;
            case Enums.Packet.PLAYER_EQUIP:
               HandlePlayerEquip(client, buffer);
               break;
            case Enums.Packet.PLAYER_HOTBAR:
               HandlePlayerHotbar(client, buffer);
               break;
            case Enums.Packet.TARGET:
               HandleTarget(client, buffer);
               break;
            case Enums.Packet.OPEN_FRIENDS:
               HandleOpenFriends(client);
               break;
            case Enums.Packet.REMOVE_FRIEND:
               HandleRemoveFriend(client, buffer);
               break;
            case Enums.Packet.CREATE_GUILD:
               HandleCreateGuild(client, buffer);
               break;
            case Enums.Packet.OPEN_GUILD:
               HandleOpenGuild(client);
               break;
            case Enums.Packet.GUILD_LEADER:
               HandleGuildLeader(client, buffer);
               break;
            case Enums.Packet.GUILD_NOTICE:
               HandleGuildNotice(client, buffer);
               break;
            case Enums.Packet.REMOVE_GUILD_MEMBER:
               HandleRemoveGuildMember(client, buffer);
               break;
            case Enums.Packet.GUILD_REQUEST:
               HandleGuildRequest(client, buffer);
               break;
            case Enums.Packet.LEAVE_GUILD:
               HandleLeaveGuild(client);
               break;
            case Enums.Packet.LEAVE_PARTY:
               HandleLeaveParty(client);
               break;
            case Enums.Packet.CHOICE:
               HandleChoice(client, buffer);
               break;
            case Enums.Packet.BANK_ITEM:
               HandleBankItem(client, buffer);
               break;
            case Enums.Packet.BANK_GOLD:
               HandleBankGold(client, buffer);
               break;
            case Enums.Packet.CLOSE_WINDOW:
               HandleCloseWindow(client);
               break;
            case Enums.Packet.BUY_ITEM:
               HandleBuyItem(client, buffer);
               break;
            case Enums.Packet.SELL_ITEM:
               HandleSellItem(client, buffer);
               break;
            case Enums.Packet.CHOICE_TELEPORT:
               HandleChoiceTeleport(client, buffer);
               break;
            case Enums.Packet.NEXT_COMMAND:
               HandleNextEventCommand(client);
               break;
            case Enums.Packet.REQUEST:
               HandleRequest(client, buffer);
               break;
            case Enums.Packet.ACCEPT_REQUEST:
               HandleAcceptRequest(client);
               break;
            case Enums.Packet.DECLINE_REQUEST:
               HandleDeclineRequest(client);
               break;
            case Enums.Packet.TRADE_ITEM:
               HandleTradeItem(client, buffer);
               break;
            case Enums.Packet.TRADE_GOLD:
               HandleTradeGold(client, buffer);
               break;
            case Enums.Packet.LOGOUT:
               HandleLogout(client);
               break;
            case Enums.Packet.ADMIN_COMMAND:
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
            SendFailedLogin(client, Enums.Login.OLD_VERSION);
            client.CloseAfterWriting();
            return;
         } else if (IsIpBlocked(client.Ip)) {
            SendFailedLogin(client, Enums.Login.IP_BLOCKED);
            client.CloseAfterWriting();
            return;
         } else if (!exists) {
            SendFailedLogin(client, Enums.Login.INVALD_USER);
            AddAttempt(client);
            client.CloseAfterWriting();
            return;
         } else if (MultiAccounts(user, client.Ip)) {
            SendFailedLogin(client, Enums.Login.MULTI_ACCOUNT);
            client.CloseAfterWriting();
            return;
         }
         Account account = await DB.LoadAccount(user);
         if (account.Pass != pass) {
            SendFailedLogin(client, Enums.Login.INVALID_PASS);
            AddAttempt(client);
            client.CloseAfterWriting();
            return;
         } else if (IsBanned(account.IdDb.ToString())) {
            SendFailedLogin(client, Enums.Login.ACC_BANNED);
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
            SendFailedLogin(client, Enums.Login.OLD_VERSION);
            client.CloseAfterWriting();
            return;
         } else if (IsIpBlocked(client.Ip)) {
            SendFailedLogin(client, Enums.Login.IP_BLOCKED);
            client.CloseAfterWriting();
            return;
         } else if (exists) {
            SendCreateAccount(client, Enums.Register.ACC_EXIST);
            client.CloseAfterWriting();
            return;
         }
         client.AntispamTime.AddSeconds(0.5);
         await DB.CreateAccount(user, pass, email);
         SendCreateAccount(client, Enums.Register.SUCCESSFUL);
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
            classId < 1 || classId > client.MaxClasses() || sex > (byte)Enums.Sex.FEMALE ||
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
            SendFailedLogin(client, Enums.Login.INVALID_PASS);
            AddAttempt(client);
            return;
         }
         await DB.RemovePlayer(client.Actors[actorId].IdDb);
         client.RemoveActorGuild(client.Actors[actorId].GuildName, client.Actors[actorId].Name);
         client.Actors.Remove(actorId);
         SendRemoveActor(client, actorId);
      }
      static async Task HandleUseActor(GameClient client, BufferReader buffer) {
         byte actorId = buffer.ReadByte();
         if (!client.Actors.ContainsKey(actorId))
            return;
         //client.LoadData(actorId);
         //SendPlayerData(client, client.MapId);
         //Maps[client.MapId].TotalPlayers++;
         //await DB.ChangeWhosOnline(client.IdDb, true);
         //client.JoinGame(actorId);
         //SendUseActor(client);
         //client.LoadStates();
         //SendGlobalSwitches(client);
         //SendMapPlayers(client);
         //SendMapEvents(client);
         //SendMapDrops(client);
         //SendMotd(client);
      }
      private static void HandlePlayerMovement(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleChatMessage(GameClient client, BufferReader buffer) {
         
      }
      private static void HandlePlayerAttack(GameClient client) {
         
      }
      private static void HandleUseItem(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleUseSkill(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleBalloon(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleUseHotbar(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleAddDrop(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleRemoveDrop(GameClient client, BufferReader buffer) {
         
      }
      private static void HandlePlayerParam(GameClient client, BufferReader buffer) {
         
      }
      private static void HandlePlayerEquip(GameClient client, BufferReader buffer) {
         
      }
      private static void HandlePlayerHotbar(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleTarget(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleOpenFriends(GameClient client) {
         
      }
      private static void HandleRemoveFriend(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleCreateGuild(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleOpenGuild(GameClient client) {
         
      }
      private static void HandleGuildLeader(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleGuildNotice(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleRemoveGuildMember(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleGuildRequest(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleLeaveGuild(GameClient client) {
         
      }
      private static void HandleLeaveParty(GameClient client) {
         
      }
      private static void HandleChoice(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleBankItem(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleBankGold(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleCloseWindow(GameClient client) {
         
      }
      private static void HandleBuyItem(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleSellItem(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleChoiceTeleport(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleNextEventCommand(GameClient client) {
         
      }
      private static void HandleRequest(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleAcceptRequest(GameClient client) {
         
      }
      private static void HandleDeclineRequest(GameClient client) {
         
      }
      private static void HandleTradeItem(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleTradeGold(GameClient client, BufferReader buffer) {
         
      }
      private static void HandleLogout(GameClient client) {
         
      }
      private static void HandleAdminCommand(GameClient client, BufferReader buffer) {
         
      }
   }
}
