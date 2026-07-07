using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace VXAOS_Server {
   public static partial class Network {
      public static bool FullClients() {
         return Clients.Count == Cfg.MaxConnections && ClientAvaiableIds.IsEmpty;
      }
      public static GameClient? FindPlayer(string name) {
         foreach (var client in Clients.Values) {
            if (client != null && client.Name == name) return client;
         }
         return null;
      }
      public static string? FindGuildMember(Guild guild, string name) {
         foreach (var member in guild.Members) {
            if (name == member)
               return member;
         }
         return null;
      }
      public static bool IsMemberInGuild(string guildName, string name) {
         if(Guilds.ContainsKey(guildName))
            return FindGuildMember(Guilds[guildName], name) != null;
         return false;
      }
      public static bool LoginHackingAttempt(GameClient client) {
         return !client.IsConnected() || client.IsLogged();
      }
      public static int FindGuildIdDb(string name) {
         return string.IsNullOrEmpty(name) || !Guilds.ContainsKey(name) ? 0 : Guilds[name].IdDb;
      }
      public static string FindGuildName(int idDb) {
         if (idDb == 0)
            return "";
         foreach(var (name, guild) in Guilds) {
            if (guild.IdDb == idDb)
               return name;
         }
         return "";
      }
      public static string Titleize(string str) {
         return string.Join(" ", str.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower())));
      }
      public static bool IsInvalidEmail(string email) {
         return !Regex.IsMatch(email,@"^([\w+\-].?)+@[a-z\d\-]+(\.[a-z]+)*\.[a-z]+$",
             RegexOptions.IgnoreCase);
      }
      public static bool IsInvalidUser(string user) {
         return Regex.IsMatch(user, @"[\/\\\""*<>|]");
      }
      public static bool IsInvalidName(string name) {
         return Regex.IsMatch(name, @"[^A-Za-z0-9 ]");
      }
      static bool IsIpBlocked(IPAddress ip) {
         bool result = BlockedIps.ContainsKey(ip) && BlockedIps[ip].Attempts == Cfg.MaxAttempts;
         if(result && DateTimeOffset.UtcNow > BlockedIps[ip].Time) {
            BlockedIps.TryRemove(ip, out _);
            result = false;
         }
         return result;
      }
      static void AddAttempt(GameClient client) {
         if (!BlockedIps.ContainsKey(client.Ip) || DateTimeOffset.UtcNow > BlockedIps[client.Ip].Time)
            BlockedIps.TryAdd(client.Ip, new IPBlocked());
         BlockedIps[client.Ip].Attempts++;
         if (BlockedIps[client.Ip].Attempts == Cfg.MaxAttempts) {
            BlockedIps[client.Ip].Time = DateTimeOffset.UtcNow.AddSeconds(Cfg.IpBlockingTime);
            SendFailedLogin(client, Enums.Login.IP_BLOCKED);
            client.CloseAfterWriting();
         } else {
            BlockedIps[client.Ip].Time = DateTimeOffset.UtcNow.AddSeconds(60);
         }
      }
      static bool MultiAccounts(string user, IPAddress ip) {
         var client = Clients.Values.FirstOrDefault(c =>
            c != null &&
            string.Equals(c.User, user, StringComparison.OrdinalIgnoreCase)
         );
         if (client != null && client.Ip == ip) { 
            if(client.IsInGame())
               client.LeaveGame();
            client.Disconnect();
            return false;
         }
         return client != null;
      }
      static bool IsBanned(string key) {
         bool banned = BanList.ContainsKey(key);
         if (banned && DateTimeOffset.UtcNow > BanList[key]) {
            BanList.TryRemove(key, out _);
            return false;
         }
         return banned;
      }
      public static bool CreateAccountHackingAtempt(GameClient client, string user, string pass, string email) {
         return !client.IsConnected() || 
               client.IsLogged() ||
               user.Length < Configs.MinCharacters ||
               user.Length > Configs.MaxCharacters ||
               pass.Length < Configs.MinCharacters ||
               pass.Length > 32 ||
               IsInvalidUser(user) ||
               IsInvalidEmail(email) ||
               email.Length > 40;
      }
      public static bool IllegalName(string name) {
         foreach (string word in (IEnumerable<string>)Configs.ForbiddenNames) {
            if (name.Contains(word, StringComparison.OrdinalIgnoreCase))
               return true;
         }
         return false;
      }
   }
}
