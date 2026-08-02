using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace VXAOS_Server {
   public static partial class Network {
      public static bool FullClients() {
         return Clients.Count == ServerConfig.MaxConnections && ClientAvaiableIds.IsEmpty;
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
         if(Guilds.TryGetValue(guildName, out Guild? value))
            return FindGuildMember(value, name) != null;
         return false;
      }
      public static int FindGuildIdDb(string name) {
         return string.IsNullOrEmpty(name) || !Guilds.TryGetValue(name, out Guild? value) ? 0 : value.IdDb;
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
         return !InvalidEmailRegex().IsMatch(email);
      }
      public static bool IsInvalidUser(string user) {
         return InvalidUserRegex().IsMatch(user);
      }
      public static bool IsInvalidName(string name) {
         return InvalidNameRegex().IsMatch(name);
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
      public static bool LoginHackingAttempt(GameClient client) {
         return !client.IsConnected() || client.IsLogged();
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
      public static bool IsRequestedUnavailable(GameClient client, GameClient? requested) {
         if (requested == null || !requested.IsInGame()) return true;
         if (client.Id == requested.Id) return true;
         if (client.MapId != requested.MapId) return true;
         if (!client.IsInRange(requested, 10)) return true;
         return false;
      }
      public static bool CanPickUpDrop(Drop drop, GameClient client) {
         if (string.IsNullOrEmpty(drop.Name)) return true;
         if (drop.Name == client.Name) return true;
         if (drop.PartyId > -1 && drop.PartyId == client.PartyId) return true;
         if (DateTimeOffset.UtcNow >= drop.PickUpTime) return true;
         return false;
      }
      static bool IsBanned(string key) {
         bool banned = BanList.ContainsKey(key);
         if (banned && DateTimeOffset.UtcNow > BanList[key]) {
            BanList.TryRemove(key, out _);
            return false;
         }
         return banned;
      }
      static bool IsIpBlocked(IPAddress ip) {
         bool result = BlockedIps.ContainsKey(ip) && BlockedIps[ip].Attempts == ServerConfig.MaxAttempts;
         if(result && DateTimeOffset.UtcNow > BlockedIps[ip].Time) {
            BlockedIps.TryRemove(ip, out _);
            result = false;
         }
         return result;
      }
      static void AddAttempt(GameClient client) {
         if (!BlockedIps.TryGetValue(client.Ip, out IPBlocked? value) || DateTimeOffset.UtcNow > value.Time)
            BlockedIps.TryAdd(client.Ip, new IPBlocked());
         BlockedIps[client.Ip].Attempts++;
         if (BlockedIps[client.Ip].Attempts == ServerConfig.MaxAttempts) {
            BlockedIps[client.Ip].Time = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.IpBlockingTime);
            SendFailedLogin(client, Enums.Login.IP_BLOCKED);
            client.CloseAfterWriting();
         } else {
            BlockedIps[client.Ip].Time = DateTimeOffset.UtcNow.AddSeconds(60);
         }
      }
      public static string ChatFilter(string message) {
         foreach( var word in ServerConfig.ChatFilter) {
            message = message.Replace(word, new('*', word.Length));
         }
         return message;
      }
      public static void WhosOnline(GameClient player) {
         List<string> names = [];
         foreach(var client in Clients.Values) {
            if (client != null && client.IsInGame())
               names.Add($"{client.Name} [{client.Level}]");
         }
         if(names.Count > 1) {
            SendWhosOnline(player, string.Format(Vocab.Connected, names.Count, string.Join(", ", names.Take(40))));
         } else {
            SendWhosOnline(player, Vocab.NobodyConnected);
         }
      }

      [GeneratedRegex(@"^([\w+\-].?)+@[a-z\d\-]+(\.[a-z]+)*\.[a-z]+$", RegexOptions.IgnoreCase, "pt-BR")]
      private static partial Regex InvalidEmailRegex();
      [GeneratedRegex(@"[\/\\\""*<>|]")]
      private static partial Regex InvalidUserRegex();
      [GeneratedRegex(@"[^A-Za-z0-9 ]")]
      private static partial Regex InvalidNameRegex();
   }
}
