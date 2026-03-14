using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   using System;
   using System.IO;
   using System.Collections.Generic;
   using System.Text.RegularExpressions;

   public static class ConfigLoader {

      public static ServerConfig Load(string path) {

         var lines = File.ReadAllLines(path);

         var data = new Dictionary<string, string>();
         var fullText = File.ReadAllText(path);

         foreach (var raw in lines) {

            var line = raw;

            if (string.IsNullOrWhiteSpace(line))
               continue;

            var commentIndex = line.IndexOf('#');

            if (commentIndex >= 0)
               line = line.Substring(0, commentIndex);

            if (string.IsNullOrWhiteSpace(line))
               continue;

            if (!line.Contains("="))
               continue;

            var parts = line.Split('=', 2);

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            data[key] = value;
         }

         var cfg = new ServerConfig();

         cfg.ServerPort = int.Parse(data["SERVER_PORT"]);
         cfg.MaxConnections = int.Parse(data["MAX_CONNECTIONS"]);

         cfg.DbType = (Enums.DatabaseType)int.Parse(data["DB_TYPE"]);
         cfg.DbHost = data.GetValueOrDefault("DB_HOST");
         cfg.DbPort = int.Parse(data.GetValueOrDefault("DB_PORT", "0"));
         cfg.DbUser = data.GetValueOrDefault("DB_USER");
         cfg.DbPass = data.GetValueOrDefault("DB_PASS");
         cfg.DbName = data.GetValueOrDefault("DB_NAME");
         cfg.DbFile = data.GetValueOrDefault("DB_FILE");
         cfg.DbPoolMin = int.Parse(data.GetValueOrDefault("DB_POOL_MIN", "1"));
         cfg.DbPoolMax = int.Parse(data.GetValueOrDefault("DB_POOL_MAX", "50"));

         cfg.DataPath = data.GetValueOrDefault("DATA_PATH");

         cfg.SaveDataTime = int.Parse(data["SAVE_DATA_TIME"]);
         cfg.InactivityTime = int.Parse(data["INACTIVITY_TIME"]);
         cfg.MaxAttempts = int.Parse(data["MAX_ATTEMPS"]);
         cfg.IpBlockingTime = int.Parse(data["IP_BLOCKING_TIME"]);

         cfg.ExpBonus = float.Parse(data["EXP_BONUS"]);
         cfg.GoldBonus = float.Parse(data["GOLD_BONUS"]);
         cfg.DropBonus = float.Parse(data["DROP_BONUS"]);

         cfg.VipExpBonus = float.Parse(data["VIP_EXP_BONUS"]);
         cfg.VipGoldBonus = float.Parse(data["VIP_GOLD_BONUS"]);
         cfg.VipDropBonus = float.Parse(data["VIP_DROP_BONUS"]);
         cfg.VipRecoverBonus = float.Parse(data["VIP_RECOVER_BONUS"]);

         cfg.LoseDefaultExpRate = int.Parse(data["LOSE_DEFAULT_EXP_RATE"]);
         cfg.LoseVipExpRate = int.Parse(data["LOSE_VIP_EXP_RATE"]);
         cfg.LoseGoldRate = int.Parse(data["LOSE_GOLD_RATE"]);

         cfg.LevelUpPoints = int.Parse(data["LEVEL_UP_POINTS"]);

         cfg.EnemyAttackBalloonId = int.Parse(data["ENEMY_ATTACK_BALLOON_ID"]);
         cfg.ReviveTime = int.Parse(data["REVIVE_TIME"]);
         cfg.MaxReviveRegions = int.Parse(data["MAX_REVIVE_REGIONS"]);

         cfg.DropDespawnTime = int.Parse(data["DROP_DESPAWN_TIME"]);
         cfg.DropPickUpTime = int.Parse(data["DROP_PICK_UP_TIME"]);

         cfg.RecoverTime = int.Parse(data["RECOVER_TIME"]);
         cfg.RecoverHP = int.Parse(data["RECOVER_HP"]);
         cfg.RecoverMP = int.Parse(data["RECOVER_MP"]);

         ParseChatFilter(fullText, cfg);
         ParsePartyBonus(fullText, cfg);

         return cfg;
      }

      static void ParseChatFilter(string text, ServerConfig cfg) {

         var match = Regex.Match(text,
            @"CHAT_FILTER\s*=\s*\[(.*?)\]",
            RegexOptions.Singleline);

         if (!match.Success)
            return;

         var content = match.Groups[1].Value;

         var words = content.Split(',');

         foreach (var w in words) {

            var word = w.Trim().Trim('\'', '"');

            if (!string.IsNullOrWhiteSpace(word))
               cfg.ChatFilter.Add(word);
         }
      }

      static void ParsePartyBonus(string text, ServerConfig cfg) {

         var match = Regex.Match(text,
            @"PARTY_BONUS\s*=\s*\{(.*?)\}",
            RegexOptions.Singleline);

         if (!match.Success)
            return;

         var content = match.Groups[1].Value;

         var lines = content.Split(',');

         foreach (var l in lines) {

            if (l.Contains("#"))
               continue;

            var parts = l.Split("=>");

            if (parts.Length != 2)
               continue;

            var members = int.Parse(parts[0].Trim());
            var bonus = int.Parse(parts[1].Trim());

            cfg.PartyBonus[members] = bonus;
         }
      }
   }
   public class ServerConfig {

      // NETWORK
      public int ServerPort;
      public int MaxConnections;

      // DATABASE
      public Enums.DatabaseType DbType;
      public string DbHost;
      public int DbPort;
      public string DbUser;
      public string DbPass;
      public string DbName;
      public string DbFile;
      public int DbPoolMin;
      public int DbPoolMax;

      // DATA
      public string DataPath;

      // TIMERS
      public int SaveDataTime;
      public int InactivityTime;
      public int IpBlockingTime;

      // LOGIN
      public int MaxAttempts;

      // GLOBAL BONUS
      public float ExpBonus;
      public float GoldBonus;
      public float DropBonus;

      // VIP BONUS
      public float VipExpBonus;
      public float VipGoldBonus;
      public float VipDropBonus;
      public float VipRecoverBonus;

      // DEATH PENALTIES
      public int LoseDefaultExpRate;
      public int LoseVipExpRate;
      public int LoseGoldRate;

      // CHAT
      public List<string> ChatFilter = new();

      // PARTY
      public Dictionary<int, int> PartyBonus = new();

      // LEVEL
      public int LevelUpPoints;

      // ENEMY
      public int EnemyAttackBalloonId;
      public int ReviveTime;
      public int MaxReviveRegions;

      // DROP
      public int DropDespawnTime;
      public int DropPickUpTime;

      // RECOVERY
      public int RecoverTime;
      public int RecoverHP;
      public int RecoverMP;
   }
}
