using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public static partial class Network {
      static TcpListener listener;
      public static ConcurrentDictionary<int, GameClient> Clients = new();
      public static ServerConfig Cfg;
      public static void Start(int port) {
         Cfg = ConfigLoader.Load("server.cfg");
         listener = new TcpListener(IPAddress.Any, Cfg.ServerPort);
         listener.Start();
         AcceptLoop();
      }

      static async void AcceptLoop() {
         while (true) {
            var tcp = await listener.AcceptTcpClientAsync();

            int id = FindClientId();

            var client = new GameClient(id, tcp);

            Clients[id] = client;

            client.Start();
         }
      }

      public static int FindClientId() {
         // placeholder
         return 0;
      }

      public static void RemoveClient(int id) {
         GameClient client;
         Clients.TryRemove(id, out client);
      }
   }
}
