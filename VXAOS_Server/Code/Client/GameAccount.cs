using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameClient {
      public IPAddress Ip { get; private set; }
      public int ActorId = -1;
      public int IdDb = -1;
      public bool Handshake = false;
      public DateTimeOffset InactivityTime = DateTimeOffset.UtcNow;
      public int AccountIdDb = -1;
      public string User = "";
      public string Pass = "";
      public int Group = 0;
      public int AddedVipTime = 0;
      public DateTimeOffset VipTime = DateTimeOffset.UtcNow;
      public Dictionary<int, Actor> Actors = new();
      public List<string> Friends = new();
      public int OnlineFriendsSize = 0;
      TcpClient Tcp;
      NetworkStream Stream;
      byte[] buffer = new byte[4096];
      public bool Disconnected = false;
      ConcurrentQueue<byte[]> SendQueue = new();
      bool Sending;
      bool DisconnectPending;
      public GameClient(int id, TcpClient tcp) {
         Id = id;
         Tcp = tcp;
         Stream = tcp.GetStream();
         _ = StartHandshakeTimeout();
         AntispamTime = DateTimeOffset.UtcNow;
         Ip = (Tcp.Client.RemoteEndPoint as IPEndPoint).Address;
      }
      public void Start() {
         _ = ReceiveLoop();
      }
      async Task ReceiveLoop() {
         try {
            while (true) {
               int bytes = await Stream.ReadAsync(buffer, 0, buffer.Length);
               if (bytes == 0)
                  break;
               BufferReader bufferreader = new BufferReader(Encoding.Latin1.GetString(buffer, 0, bytes));
               Network.HandleMessages(this, bufferreader);
            }
         } catch (Exception ex) {
            Console.WriteLine(ex);
         } finally {
            Disconnect();
         }
      }
      public void Send(string msg) {
         byte[] message = Encoding.Latin1.GetBytes(msg);
         ushort size = (ushort)message.Length;
         byte[] sizeBytes = BitConverter.GetBytes(size);
         byte[] packet = new byte[sizeBytes.Length + message.Length];
         Buffer.BlockCopy(sizeBytes, 0, packet, 0, 2);
         Buffer.BlockCopy(message, 0, packet, 2, message.Length);
         SendQueue.Enqueue(packet);
         if (!Sending)
            _ = ProcessSendQueue();
      }
      async Task ProcessSendQueue() {
         Sending = true;
         while (SendQueue.TryDequeue(out var packet)) {
            await Stream.WriteAsync(packet);
         }
         Sending = false;
         if (DisconnectPending)
            Disconnect();
      }
      public void Disconnect() {
         if (Disconnected)
            return;
         Disconnected = true;
         try {
            Tcp.Client.Shutdown(SocketShutdown.Both);
         } catch { }
         Tcp.Close();
         if (IsInGame()) {
            LoadOriginalGraphic();
            LeaveGame();
         }
         if (IsLogged()) {
            Console.WriteLine($"{User} saiu.");
         }
         Network.RemoveClient(Id);
      }
      public void CloseAfterWriting() {
         DisconnectPending = true;
         if (!Sending && SendQueue.IsEmpty)
            Disconnect();
      }
      public async Task StartHandshakeTimeout() {
         await Task.Delay(TimeSpan.FromSeconds(ServerConfig.AuthenticationTime));
         if (!Handshake) {
            Disconnect();
         }
      }
      public bool IsConnected() { return (Id >= 0); }
      public bool IsLogged() { return (User.Count() > 0); }
      public new bool IsInGame() { return ActorId >= 0; }
      public bool IsStandard() { return Group == (int)Enums.Group.STANDARD; }
      public bool IsAdmin() { return Group == (int)Enums.Group.ADMIN; }
      public bool IsMonitor() { return Group == (int)Enums.Group.MONITOR; }
      public bool IsVip() { return VipTime.AddSeconds(AddedVipTime) > DateTimeOffset.UtcNow; }
      public int MaxClasses() { return IsVip() ? Configs.MaxVipClasses : Configs.MaxDefaultClasses; }
      public Actor Actor() { return Actors[ActorId]; }
      public void AddVipDays(int days) {
         AddedVipTime += days * 86400;
         Network.PlayerChatMessage(this, string.Format(Vocab.AddVIPDays, days), Configs.SuccessColor);
         Network.SendVipDays(this);
      }
      public void AcceptFriend() {
         if (Network.Clients.TryGetValue(Request.Id, out GameClient client) && client.IsInGame()) {
            if (client.Friends.Count >= Configs.MaxFriends || Friends.Count >= Configs.MaxFriends)
               return;
            client.AddFriend(this);
            AddFriend(client);
         }
      }
      public void AddFriend(GameClient friend) {
         if (Friends.Count >= Configs.MaxFriends || Friends.Contains(friend.Name))
            return;
         Friends.Insert(OnlineFriendsSize, friend.Name);
         OnlineFriendsSize += 1;
         Network.SendAddFriend(this, friend.Name);
      }
      internal void RemoveActorGuild(string guildName, string name) {
         if (string.IsNullOrEmpty(guildName))
            return;
         if (Network.Guilds[guildName].Leader == name) {
            Network.RemoveGuild(guildName);
         } else {
            Network.Guilds[guildName].Members.Remove(name);
         }
      }
      public void LoadData(int actorId) {
         Name = Actors[actorId].Name;
         CharacterName = Actors[actorId].CharacterName;
         CharacterIndex = Actors[actorId].CharacterIndex;
         FaceName = Actors[actorId].FaceName;
         FaceIndex = Actors[actorId].FaceIndex;
         ClassId = Actors[actorId].ClassId;
         Sex = Actors[actorId].Sex;
         Level = Actors[actorId].Level;
         Exp = Actors[actorId].Exp;
         Hp = Actors[actorId].Hp;
         Mp = Actors[actorId].Mp;
         ParamBase = Actors[actorId].ParamBase;
         Equips = Actors[actorId].Equips;
         Points = Actors[actorId].Points;
         GuildName = (Network.IsMemberInGuild(Actors[actorId].GuildName, Name) ? Actors[actorId].GuildName : "");
         ReviveMapId = Actors[actorId].ReviveMapId;
         ReviveX = Actors[actorId].ReviveX;
         ReviveY = Actors[actorId].ReviveY;
         MapId = Actors[actorId].MapId;
         X = Actors[actorId].X;
         Y = Actors[actorId].Y;
         Direction = Actors[actorId].Direction;
         Gold = Actors[actorId].Gold;
         Items = Actors[actorId].Items;
         Weapons = Actors[actorId].Weapons;
         Armors = Actors[actorId].Armors;
         Skills = Actors[actorId].Skills;
         Quests = Actors[actorId].Quests;
         Hotbar = Actors[actorId].Hotbar;
         Switches = new GameSwitches(this, Actors[actorId].Switches);
         Variables = new GameVariables(this, Actors[actorId].Variables);
         SelfSwitches = new GameSelfSwitches(this, Actors[actorId].SelfSwitches);
      }
      public void JoinGame(int actorId) {
         ActorId = actorId;
         RecoverTime = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.RecoverTime);
         GlobalAntispamTime = DateTimeOffset.UtcNow;
         WeaponAttackTime = DateTimeOffset.UtcNow;
         ItemAttackTime = DateTimeOffset.UtcNow;
         SkillCooldownTime = new();
         MutedTime = DateTimeOffset.UtcNow;
         StopCount = DateTimeOffset.UtcNow;
         OriginalCharacterName = string.Empty;
         OriginalCharacterIndex = 0;
         OriginalFaceName = "";
         OriginalFaceIndex = 0;
         OnlineFriendsSize = 0;
         TeleportId = -1;
         PartyId = -1;
         CommonEvents = new();
         ParallelEventsWating = new();
         CreatingGuild = false;
         InBank = false;
         MessageInterpreter = null;
         WaitingEvent = null;
         ShopGoods = new();
         Choice = -1;
         ClearTarget();
         ClearRequest();
         ClearStates();
         ClearBuffs();
      }
      public void LoadStates() {
         foreach(var state in Actors[ActorId].States) {
            float time = 0;
            if (Actors[ActorId].StatesTime.ContainsKey(state))
               time = Actors[ActorId].StatesTime[state];
            AddNewState(state, time);
         }
      }
      public void LeaveGame() {
      }
      public void UpdateCurrentActor() {
         Actor().CharacterName = CharacterName;
         Actor().CharacterIndex = CharacterIndex;
         Actor().FaceName = FaceName;
         Actor().FaceIndex = FaceIndex;
         Actor().ClassId = ClassId;
         Actor().Sex = Sex;
         Actor().Level = Level;
         Actor().Exp = Exp;
         Actor().Hp = Hp;
         Actor().Mp = Mp;
         Actor().ParamBase = ParamBase;
         Actor().Equips = Equips;
         Actor().Points = Points;
         Actor().GuildName = GuildName;
         Actor().ReviveMapId = ReviveMapId;
         Actor().ReviveX = ReviveX;
         Actor().ReviveY = ReviveY;
         Actor().MapId = MapId;
         Actor().X = X;
         Actor().Y = Y;
         Actor().Direction = Direction;
         Actor().Gold = Gold;
         Actor().Items = Items;
         Actor().Weapons = Weapons;
         Actor().Armors = Armors;
         Actor().Skills = Skills;
         Actor().Quests = Quests;
         Actor().Hotbar = Hotbar;
         Actor().Switches = Switches.Data;
         Actor().Variables = Variables.Data;
         Actor().SelfSwitches = SelfSwitches.Data;
      }
      public void UpdateMenu() {
         if(IsLogged() && DateTimeOffset.UtcNow > InactivityTime) {
            Network.SendFailedLogin(this, Enums.Login.INACTIVITY);
            CloseAfterWriting();
         }
      }
   }
}
