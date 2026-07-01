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
         Ip = (Tcp.Client.RemoteEndPoint as IPEndPoint).Address;
         _ = StartHandshakeTimeout();
         AntispamTime = DateTimeOffset.UtcNow;
      }
      public void Start() {
         ReceiveLoop();
      }
      async void ReceiveLoop() {
         try {
            while (true) {
               int bytes = await Stream.ReadAsync(buffer, 0, buffer.Length);
               if (bytes == 0)
                  break;
               BufferReader bufferreader = new BufferReader(Encoding.Latin1.GetString(buffer, 0, bytes));
               Network.HandleMessages(this, bufferreader);
            }
         } catch {
         }
         Disconnect();
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
         Network.RemoveClient(Id);
      }
      public void CloseAfterWriting() {
         DisconnectPending = true;
         if (!Sending && SendQueue.IsEmpty)
            Disconnect();
      }
      public async Task StartHandshakeTimeout() {
         await Task.Delay(TimeSpan.FromSeconds(Network.Cfg.AuthenticationTime));
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
      public void LeaveGame() {
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
   }
}
