using Newtonsoft.Json.Linq;

namespace VXAOS_Server.Extensions {
   public static class CSExt {
      public static bool HasIndex<T>(this IList<T> list, int index) {
         return index >= 0 && index < list.Count;
      }
      public static T GetWithFallback<T>(this IList<T> list, int index, T fallback = default) {
         return (index >= 0 && index < list.Count) ? list[index] : fallback;
      }
   }
   public static class Utils {
      public static void WriteColor(string text, ConsoleColor color) {
         var oldColor = Console.ForegroundColor;
         Console.ForegroundColor = color;
         Console.WriteLine(text);
         Console.ForegroundColor = oldColor;
      }
      public static double Rand()
          => Random.Shared.NextDouble();
      public static int Rand(int max)
          => Random.Shared.Next(max);
      public static int Rand(int min, int max)
          => Random.Shared.Next(min, max);
   }
   public static class JTokenExtensions {
      public static JArray AsArray(this JToken token) {
         return (JArray)token;
      }
      public static JObject AsObject(this JToken token) {
         return (JObject)token;
      }
      public static int AsInt(this JToken token) {
         return token.Value<int>();
      }
      public static string AsString(this JToken token) {
         return token.Value<string>();
      }
      public static bool AsBool(this JToken token) {
         return token.Value<bool>();
      }
      public static char AsChar(this JToken token) {
         return token.Value<char>();
      }
   }
   public static class JArrayExtensions {
      public static int Int(this JArray arr, int index) {
         return arr[index].Value<int>();
      }
      public static bool Bool(this JArray arr, int index) {
         return arr[index].Value<bool>();
      }
      public static string Str(this JArray arr, int index) {
         return arr[index].Value<string>();
      }
      public static JArray Array(this JArray arr, int index) {
         return (JArray)arr[index];
      }
      public static JObject Obj(this JArray arr, int index) {
         return (JObject)arr[index];
      }
      public static T To<T>(this JArray arr, int index) {
         return arr[index].ToObject<T>();
      }
   }
}
