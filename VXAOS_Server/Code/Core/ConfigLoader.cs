namespace VXAOS_Server {
   using System.IO;
   using System.Collections.Generic;
   using System.Text.RegularExpressions;

   public static class ConfigLoader {
      public static void Load(string path) {
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
         //var cfg = new ServerConfig();
         ServerConfig.ServerPort = int.Parse(data["SERVER_PORT"]);
         ServerConfig.MaxConnections = int.Parse(data["MAX_CONNECTIONS"]);
         ServerConfig.DbType = (Enums.DatabaseType)int.Parse(data["DB_TYPE"]);
         ServerConfig.DbHost = data.GetValueOrDefault("DB_HOST");
         ServerConfig.DbPort = int.Parse(data.GetValueOrDefault("DB_PORT", "0"));
         ServerConfig.DbUser = data.GetValueOrDefault("DB_USER");
         ServerConfig.DbPass = data.GetValueOrDefault("DB_PASS");
         ServerConfig.DbName = data.GetValueOrDefault("DB_NAME");
         ServerConfig.DbFile = data.GetValueOrDefault("DB_FILE");
         ServerConfig.DbPoolMin = int.Parse(data.GetValueOrDefault("DB_POOL_MIN", "1"));
         ServerConfig.DbPoolMax = int.Parse(data.GetValueOrDefault("DB_POOL_MAX", "50"));
         ServerConfig.DataPath = data.GetValueOrDefault("DATA_PATH");
         ServerConfig.AuthenticationTime = int.Parse(data["AUTHENTICATION_TIME"]);
         ServerConfig.SaveDataTime = int.Parse(data["SAVE_DATA_TIME"]);
         ServerConfig.InactivityTime = int.Parse(data["INACTIVITY_TIME"]);
         ServerConfig.MaxAttempts = int.Parse(data["MAX_ATTEMPS"]);
         ServerConfig.IpBlockingTime = int.Parse(data["IP_BLOCKING_TIME"]);
         ServerConfig.ExpBonus = float.Parse(data["EXP_BONUS"]);
         ServerConfig.GoldBonus = float.Parse(data["GOLD_BONUS"]);
         ServerConfig.DropBonus = float.Parse(data["DROP_BONUS"]);
         ServerConfig.VipExpBonus = float.Parse(data["VIP_EXP_BONUS"]);
         ServerConfig.VipGoldBonus = float.Parse(data["VIP_GOLD_BONUS"]);
         ServerConfig.VipDropBonus = float.Parse(data["VIP_DROP_BONUS"]);
         ServerConfig.VipRecoverBonus = float.Parse(data["VIP_RECOVER_BONUS"]);
         ServerConfig.LoseDefaultExpRate = int.Parse(data["LOSE_DEFAULT_EXP_RATE"]);
         ServerConfig.LoseVipExpRate = int.Parse(data["LOSE_VIP_EXP_RATE"]);
         ServerConfig.LoseGoldRate = int.Parse(data["LOSE_GOLD_RATE"]);
         ServerConfig.LevelUpPoints = int.Parse(data["LEVEL_UP_POINTS"]);
         ServerConfig.EnemyAttackBalloonId = int.Parse(data["ENEMY_ATTACK_BALLOON_ID"]);
         ServerConfig.ReviveTime = int.Parse(data["REVIVE_TIME"]);
         ServerConfig.MaxReviveRegions = int.Parse(data["MAX_REVIVE_REGIONS"]);
         ServerConfig.DropDespawnTime = int.Parse(data["DROP_DESPAWN_TIME"]);
         ServerConfig.DropPickUpTime = int.Parse(data["DROP_PICK_UP_TIME"]);
         ServerConfig.RecoverTime = int.Parse(data["RECOVER_TIME"]);
         ServerConfig.RecoverHP = int.Parse(data["RECOVER_HP"]);
         ServerConfig.RecoverMP = int.Parse(data["RECOVER_MP"]);
         ParseChatFilter(fullText);
         ParsePartyBonus(fullText);
         //return cfg;
      }

      static void ParseChatFilter(string text) {
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
               ServerConfig.ChatFilter.Add(word);
         }
      }

      static void ParsePartyBonus(string text) {
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
            ServerConfig.PartyBonus[members] = bonus;
         }
      }
   }
   public static class ServerConfig {
      // NETWORK
      public static int ServerPort;
      public static int MaxConnections;
      // DATABASE
      public static Enums.DatabaseType DbType;
      public static string DbHost = "";
      public static int DbPort;
      public static string DbUser = "";
      public static string DbPass = "";
      public static string DbName = "";
      public static string DbFile = "";
      public static int DbPoolMin;
      public static int DbPoolMax;
      // DATA
      public static string DataPath = "";
      // TIMERS
      public static int AuthenticationTime;
      public static int SaveDataTime;
      public static int InactivityTime;
      public static int IpBlockingTime;
      // LOGIN
      public static int MaxAttempts;
      // GLOBAL BONUS
      public static float ExpBonus;
      public static float GoldBonus;
      public static float DropBonus;
      // VIP BONUS
      public static float VipExpBonus;
      public static float VipGoldBonus;
      public static float VipDropBonus;
      public static float VipRecoverBonus;
      // DEATH PENALTIES
      public static int LoseDefaultExpRate;
      public static int LoseVipExpRate;
      public static int LoseGoldRate;
      // CHAT
      public static List<string> ChatFilter = new();
      // PARTY
      public static Dictionary<int, int> PartyBonus = new();
      // LEVEL
      public static int LevelUpPoints;
      // ENEMY
      public static int EnemyAttackBalloonId;
      public static int ReviveTime;
      public static int MaxReviveRegions;
      // DROP
      public static int DropDespawnTime;
      public static int DropPickUpTime;
      // RECOVERY
      public static int RecoverTime;
      public static int RecoverHP;
      public static int RecoverMP;
   }
}
