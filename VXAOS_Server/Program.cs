using System.Runtime.Loader;
using VXAOS_Server;
internal class Program {

   private static void Main(string[] args) {
      AssemblyLoadContext.Default.Resolving += (context, assemblyName) => {
         string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", $"{assemblyName.Name}.dll");
         if (File.Exists(assemblyPath)) {
            return context.LoadFromAssemblyPath(assemblyPath);
         }
         return null;
      };
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
         Console.WriteLine("Pressione qualquer tecla para sair");
         Console.ReadKey();
      }
   }
}