using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public static class Note {
      public static List<List<Tuple<string, int>>> ReadGraphics(string note) {
         return note
             .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
             .Select(line => line.Split('=')[1])
             .Select(graphic => graphic
                 .Split(',')
                 .Select(g => Split(g))
                 .ToList())
             .ToList();
      }

      public static Tuple<string, int> ReadPaperdoll(string note) {
         var match = Regex.Match(note, @"Paperdoll=(.*)");
         var value = match.Success ? match.Groups[1].Value : "";
         return Split(value);
      }

      public static bool ReadBoolean(string str, string note) {
         var match = Regex.Match(note, $@"{str}=(....)");
         return match.Success && match.Groups[1].Value == "true";
      }

      public static int ReadNumber(string str, string note) {
         var match = Regex.Match(note, $@"{str}=(.*)");
         return match.Success ? int.Parse(match.Groups[1].Value) : 0;
      }

      private static Tuple<string, int> Split(string str) {
         var ary = str.Split('/');
         if (ary.Length == 0)
            return null;

         string first = ary[0].TrimEnd();
         int second = ary.Length > 1 ? int.Parse(ary[1]) : 0;

         return Tuple.Create(first, second);
      }
   }
}
