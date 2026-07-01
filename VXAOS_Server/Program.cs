using VXAOS_Server;

internal class Program {
   private static void Main(string[] args) {
      Network.Start();
      while (true) {
         Thread.Sleep(1000);
      }
   }
}