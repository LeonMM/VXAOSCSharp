using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public partial class GameClient {
      public int Id;

      TcpClient Tcp;
      NetworkStream Stream;

      byte[] buffer = new byte[4096];

      public GameClient(int id, TcpClient tcp) {
         Id = id;
         Tcp = tcp;

         Stream = tcp.GetStream();
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

               HandleMessage(buffer, bytes);
            }
         } catch {
         }

         Disconnect();
      }

      void HandleMessage(byte[] data, int size) {
         string msg = Encoding.UTF8.GetString(data, 0, size);

         if (CheckLogin(msg)) {
            // placeholder login
         }
      }

      bool CheckLogin(string msg) {
         return true;
      }

      public async void Send(string msg) {
         byte[] data = Encoding.UTF8.GetBytes(msg);

         await Stream.WriteAsync(data, 0, data.Length);
      }

      void Disconnect() {
         Tcp.Close();
         Network.RemoveClient(Id);
      }
   }
}
