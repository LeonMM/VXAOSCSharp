using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VXAOS_Server.Code.Core.Enums;

namespace VXAOS_Server.Code.Core {
   public static class ConfigLoader {
      public static ServerConfig Load(string path) {
         var lines = File.ReadAllLines(path);

         var data = new Dictionary<string, string>();

         foreach (var line in lines) {
            if (string.IsNullOrWhiteSpace(line))
               continue;

            if (line.StartsWith("#"))
               continue;

            var parts = line.Split('=');

            if (parts.Length != 2)
               continue;

            data[parts[0].Trim().ToLower()] = parts[1].Trim();
         }

         return new ServerConfig {
            DbType = (DatabaseType)int.Parse(data["dbtype"]),
            DbHost = data.GetValueOrDefault("dbhost"),
            DbPort = int.Parse(data.GetValueOrDefault("dbport", "0")),
            DbUser = data.GetValueOrDefault("dbuser"),
            DbPass = data.GetValueOrDefault("dbpass"),
            DbName = data.GetValueOrDefault("dbname"),
            DbPoolMin = int.Parse(data.GetValueOrDefault("dbpoolmin", "1")),
            DbPoolMax = int.Parse(data.GetValueOrDefault("dbpoolmax", "50"))
         };
      }
   }

   public class ServerConfig {
      public Enums.DatabaseType DbType {
         get; set;
      }
      public string DbHost {
         get; set;
      }

      public int DbPort {
         get; set;
      }

      public string DbUser {
         get; set;
      }

      public string DbPass {
         get; set;
      }

      public string DbName {
         get; set;
      }

      public int DbPoolMin {
         get; set;
      }

      public int DbPoolMax {
         get; set;
      }
   }
}
