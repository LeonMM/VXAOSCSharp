using VXAOS_Server;

internal class Program {
   private static void Main(string[] args) {
      bool noError = true;
      try {
         Network.Start();
         while (true) {
            Thread.Sleep(1000);
         }
      } catch (Exception ex) { 
         Console.WriteLine(ex.ToString());
         noError = false;
      } finally {
         if (noError)
            _ = SaveGameData();
      }
   }
}