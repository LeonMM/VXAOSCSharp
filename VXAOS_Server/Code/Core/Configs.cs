global using static VXAOS_Server.Modules;
using System.Dynamic;
using System.Text.Json;
namespace VXAOS_Server {
   public class DynamicModule : DynamicObject {
      private readonly Dictionary<string, object?> _data;
      public DynamicModule(Dictionary<string, object?> data) {
         _data = data;
      }
      public override bool TryGetMember(GetMemberBinder binder,out object? result) {
         return _data.TryGetValue(binder.Name, out result);
      }
      public object? this[string key] {
         get {
            _data.TryGetValue(key, out var value);
            return value;
         }
      }
      public T Get<T>(
          string name,
          T defaultValue = default!) {
         if (!_data.TryGetValue(name, out var value))
            return defaultValue;

         return (T)Convert.ChangeType(
             value,
             typeof(T));
      }
   }

   public static class ModuleLoader {
      public static dynamic Load(string json) {
         using var doc = JsonDocument.Parse(json);
         return ConvertObject(doc.RootElement);
      }
      private static DynamicModule ConvertObject(JsonElement element) {
         var dict =new Dictionary<string, object?>();
         foreach (var property in element.EnumerateObject()) {
            dict[property.Name] =ConvertValue(property.Value);
         }
         return new DynamicModule(dict);
      }
      private static object? ConvertValue(JsonElement element) {
         switch (element.ValueKind) {
            case JsonValueKind.Object:
               if (IsNumericDictionary(element)) {
                  var dict = new Dictionary<int, object?>();
                  foreach (var prop in element.EnumerateObject()) {
                     dict[int.Parse(prop.Name)] = ConvertValue(prop.Value);
                  }
                  return dict;
               }
               return ConvertObject(element);
            case JsonValueKind.Array:
               return element.EnumerateArray().Select(ConvertValue).ToList();
            case JsonValueKind.String:
               return element.GetString();
            case JsonValueKind.True:
               return true;
            case JsonValueKind.False:
               return false;
            case JsonValueKind.Number:
               if (element.TryGetInt32(out var i))
                  return i;
               if (element.TryGetInt64(out var l))
                  return l;
               return element.GetDouble();
            default:
               return null;
         }
      }
      private static bool IsNumericDictionary(JsonElement element) {
         foreach (var prop in element.EnumerateObject()) {
            if (!int.TryParse( prop.Name, out _)) {
               return false;
            }
         }
         return true;
      }
   }

   public static class VocabLoader {
      public static dynamic Load(string filePath) {
         var data =
             new Dictionary<string, object?>();
         foreach (var line in File.ReadLines(filePath)) {
            var text = line.Trim();
            if (string.IsNullOrWhiteSpace(text))
               continue;
            if (text.StartsWith("#"))
               continue;
            var separator =
                text.IndexOf('=');
            if (separator < 0)
               continue;
            var key =
                text[..separator].Trim();
            var value =
                text[(separator + 1)..].Trim();
            if (value.StartsWith("'") &&
                value.EndsWith("'")) {
               value =
                   value[1..^1];
            }
            data[key] = value;
         }

         return new DynamicModule(data);
      }
   }
   public static class Modules {
      public static dynamic Configs;
      public static dynamic Quests;
      public static dynamic Vocab;
   }
}
