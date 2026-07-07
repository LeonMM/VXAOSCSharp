global using VXAOS_Server.Extensions;
global using static VXAOS_Server.Extensions.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace VXAOS_Server {
   public static partial class Network {
      static TcpListener Listener;
      public static ServerConfig Cfg;
      public static ConcurrentDictionary<int, GameClient> Clients = new();
      public static ConcurrentQueue<int> ClientAvaiableIds = new();
      private static int _clientHighestIdAvailable = 0;
      public static ConcurrentDictionary<int, Party> Parties = new();
      public static ConcurrentQueue<int> PartyAvaiableIds = new();
      private static int _partyHighestIdAvailable = 0;
      public static ConcurrentDictionary<IPAddress, IPBlocked> BlockedIps = new();
      public static ConcurrentDictionary<string, DateTimeOffset> BanList = new();
      public static ConcurrentDictionary<int, GameMap> Maps = new();
      public static ConcurrentDictionary<string, Guild> Guilds = new();
      public static GameGlobalSwitches Switches = new();
      public static Database DB;
      public static void Start() {
         try {
            Cfg = ConfigLoader.Load("server.cfg");
            Console.WriteLine("Iniciando Servidor...");
            DB = new(Cfg);
            DataManager.Load(Cfg.DataPath);
            Listener = new TcpListener(IPAddress.Any, Cfg.ServerPort);
            Listener.Start();
            Console.WriteLine($"Servidor iniciado às {DateTimeOffset.Now:H'h'mm'min.'}");
            Console.WriteLine("Aguardando Clientes...");
            AcceptLoop();
         } catch (Exception ex) {
            Console.WriteLine(ex);
         }
      }
      static async void AcceptLoop() {
         while (true) {
            var tcp = await Listener.AcceptTcpClientAsync();
            int id = FindClientId();
            var client = new GameClient(id, tcp);
            Console.WriteLine($"Tentativa de conexão IP {client.Ip}");
            if (FullClients()) {
               SendFailedLogin(client, Enums.Login.SERVER_FULL);
               client.CloseAfterWriting();
               return;
            }else if (IsBanned($"{client.Ip}")) {
               SendFailedLogin(client, Enums.Login.IP_BANNED);
               client.CloseAfterWriting();
               return;
            }
            Clients[id] = client;
            await Task.Delay(200);
            client.Start();
         }
      }
      public static int FindClientId() {
         int id;
         if (ClientAvaiableIds.TryDequeue(out id)) {
            return id;
         }
         return _clientHighestIdAvailable++;
      }
      public static void RemoveClient(int id) {
         Clients.TryRemove(id, out _);
         ClientAvaiableIds.Enqueue(id);
      }
      public static int FindPartyId() {
         int id;
         if (PartyAvaiableIds.TryDequeue(out id)) {
            return id;
         }
         return _partyHighestIdAvailable;
      }
      public static void RemoveParty(int id) {
         Parties.TryRemove(id, out _);
         PartyAvaiableIds.Enqueue(id);
      }
      public static void Update() {
         UpdateClients();
         UpdateMaps();
      }
      private static void UpdateClients() {
         foreach(var client in Clients.Values) {
            if (client.IsInGame()) {
               client.UpdateGame();
            } else {
               client.UpdateMenu();
            }
         }
      }
      private static void UpdateMaps() {
         foreach(var map in Maps.Values) {
            map.Update();
         }
      }
   }
}