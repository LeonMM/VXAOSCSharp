using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server.Code.Core {
   public sealed class Logger {
      private readonly Dictionary<string, StringBuilder> _text = new();

      public void Add(object type, ConsoleColor color, string text) {
         string typeName = type switch {
            int value when value == (int)Enums.Group.ADMIN => "Admin",
            int => "Monitor",
            _ => type.ToString() ?? string.Empty
         };
         string day = $"{typeName}-{DateTimeOffset.Now:dd-MMM-yyyy}";
         if (!_text.TryGetValue(day, out var builder)) {
            builder = new StringBuilder();
            _text[day] = builder;
         }
         builder.AppendLine($"{DateTimeOffset.Now:HH:mm:ss}: {text}");
         WriteColor(text, color);
      }

      public void SaveAll() {
         Directory.CreateDirectory("Logs");
         foreach (var (day, text) in _text) {
            File.AppendAllText(
                Path.Combine("Logs", $"{day}.txt"),
                text.ToString());
         }
         _text.Clear();
      }
   }
}
