namespace VXAOS_Server.Extensions {
   public static class CSExt {
      public static void WriteColor(string texto, ConsoleColor cor) {
         var corAnterior = Console.ForegroundColor;
         Console.ForegroundColor = cor;
         Console.WriteLine(texto);
         Console.ForegroundColor = corAnterior;
      }
      public static bool HasIndex<T>(this IList<T> list, int index) {
         return index >= 0 && index < list.Count;
      }
   }
   public static class Utils {
      public static double Rand()
          => Random.Shared.NextDouble();
      public static int Rand(int max)
          => Random.Shared.Next(max);
      public static int Rand(int min, int max)
          => Random.Shared.Next(min, max);
   }
}
