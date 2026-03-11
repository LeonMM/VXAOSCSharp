using SqlKata.Compilers;
using SqlKata.Execution;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VXAOS_Server.Code.Core;
using VXAOS_Server.Code.Database;
using VXAOS_Server.Code.Network;
using static System.Runtime.InteropServices.JavaScript.JSType;

TcpListener server = new TcpListener(IPAddress.Any, 5000);
server.Start();
Console.WriteLine("VXA-OS C# Server Port 5000");

var cfg = ConfigLoader.Load("server.cfg");
Console.WriteLine(cfg.DbType);

//var connection = Database.CreateConnection(cfg);

//var db = new QueryFactory(connection, Database.Compiler(cfg));
var Db = new Database(cfg);

while (true) {
   var client = await server.AcceptTcpClientAsync();
   _ = HandleClient(client);
}

async Task HandleClient(TcpClient client) {
   var stream = client.GetStream();
   byte[] buffer = new byte[1024];

   int bytes = await stream.ReadAsync(buffer);

   string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
   Console.WriteLine("Recebendo Mensagem do Cliente");
   Console.WriteLine((client.Client.RemoteEndPoint as IPEndPoint).Address);
   Console.WriteLine(msg);
   BufferReader reader = new BufferReader(msg);
   var type = reader.ReadByte();
   Console.WriteLine(type);
   if (type == 1) {
      var username = reader.ReadString();
      Console.WriteLine(username);
      Console.WriteLine("Recebendo dados");
      var sw = Stopwatch.StartNew(); // inicia timer
      string name = Db.LoadAccount(username).Result.Actors[0];
      sw.Stop(); // para timer
      Console.WriteLine($"Tempo: {sw.ElapsedMilliseconds} ms");
      Console.WriteLine(name);
      Console.WriteLine(reader.ReadString());
      Console.WriteLine(reader.ReadShort());
      Console.WriteLine("EOP");
   }
   BufferWriter writer = new BufferWriter();
   writer.WriteByte(2);
   writer.WriteByte(2);
   //await stream.WriteAsync(writer.ToArray());
   Console.WriteLine("Enviando erro de Login, versão diferente");
   Console.WriteLine(writer.ToStringBuffer());
   await SendPacket(stream, writer.ToStringBuffer());
}

async Task SendPacket(NetworkStream stream, string msg) {
   byte[] message = Encoding.UTF8.GetBytes(msg);

   short size = (short)message.Length;
   byte[] sizeBytes = BitConverter.GetBytes(size);

   byte[] packet = new byte[sizeBytes.Length + message.Length];

   Buffer.BlockCopy(sizeBytes, 0, packet, 0, 2);
   Buffer.BlockCopy(message, 0, packet, 2, message.Length);

   await stream.WriteAsync(packet);
}