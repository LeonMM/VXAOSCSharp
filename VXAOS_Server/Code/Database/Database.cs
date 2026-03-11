using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.Code.Core;
using static VXAOS_Server.Code.Core.Enums;

namespace VXAOS_Server.Code.Database {
   public class Database {
      private ServerConfig cfg;
      private readonly Compiler compiler;
      public Database(ServerConfig cfg) {
         this.cfg = cfg;
         compiler = GetCompiler();
         Console.WriteLine(cfg.DbType);
         try {
            var sw = Stopwatch.StartNew(); // inicia timer
            var banlist = Db().Query("ban_list").Get();
            Console.WriteLine(banlist.Count());
            sw.Stop(); // para timer
            Console.WriteLine($"Tempo: {sw.ElapsedMilliseconds} ms");
         } catch (Exception ex) { 
            Console.WriteLine(ex.Message);
         }
      }
      public IDbConnection CreateConnection() {
         switch (cfg.DbType) {
            case DatabaseType.PostgreSQL:
               var pgConn =
                   $"Host={cfg.DbHost};" +
                   $"Port={cfg.DbPort};" +
                   $"Database={cfg.DbName};" +
                   $"Username={cfg.DbUser};" +
                   $"Password={cfg.DbPass};" +
                   $"Minimum Pool Size={cfg.DbPoolMin};" +
                   $"Maximum Pool Size={cfg.DbPoolMax};" +
                   $"Pooling=True;Max Auto Prepare=50;Auto Prepare Min Usages=2;";
               return new NpgsqlConnection(pgConn);
            case DatabaseType.MySQL:
               var myConn =
                   $"Server={cfg.DbHost};" +
                   $"Port={cfg.DbPort};" +
                   $"Database={cfg.DbName};" +
                   $"User ID={cfg.DbUser};" +
                   $"Password={cfg.DbPass};" +
                   $"MinimumPoolSize={cfg.DbPoolMin};" +
                   $"MaximumPoolSize={cfg.DbPoolMax};" +
                   $"Pooling=True;ConnectionIdleTimeout=60";
               return new MySqlConnection(myConn);
            case DatabaseType.SQLite:
               var dbPath = Path.Combine("Data", cfg.DbName);
               var sqliteConn =
                   //$"Data Source={dbPath};";
                   $"Data Source=Data/{cfg.DbName}.db;" +
                   $"Pooling=True;";
               return new SqliteConnection(sqliteConn);

            default:
               throw new Exception("Unsupported database type");
         }
      }
      public Compiler GetCompiler() {
         switch (cfg.DbType) {
            case DatabaseType.PostgreSQL:
               return new PostgresCompiler();
            case DatabaseType.MySQL:
               return new MySqlCompiler();
            case DatabaseType.SQLite:
               Console.WriteLine("Sqlit");
               return new SqliteCompiler();
            default:
               throw new Exception("Unsupported compiler type");
         }
      }
      private QueryFactory Db() {
         var conn = CreateConnection();
         //var compiler = GetCompiler();
         return new QueryFactory(conn, compiler);
      }
      public async Task CreateAccount(
          string user,
          string pass,
          string email) {
         var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
         var db = Db();
         await db.Query("accounts").InsertAsync(new {
            username = user,
            password = pass,
            email = email,
            vip_time = now,
            creation_date = now,
            cash = 0
         });

         var accountId = await db.Query("accounts")
             .Where("username", user)
             .Select("id")
             .FirstAsync<int>();

         await db.Query("banks").InsertAsync(new {
            account_id = accountId
         });
      }
      public async Task<Account> LoadAccount(
          string user) {
         var db = Db();
         var row = await db.Query("accounts")
             .Where("username", user)
             .FirstAsync();
         if (row == null)
            return null;
         var account = new Account();

         account.IdDb = row.id;
         account.Pass = row.password;
         account.Group = row.group;
         Console.WriteLine(account.IdDb);

         var vipTime = DateTimeOffset.FromUnixTimeSeconds(row.vip_time).UtcDateTime;

         account.VipTime = vipTime > DateTime.UtcNow
             ? vipTime
             : DateTime.UtcNow;

         var friends = await db.Query("account_friends")
             .Where("account_id", account.IdDb)
             .Select("name")
             .GetAsync<string>();

         account.Friends = friends.ToList();

         var actors = await db.Query("actors")
             .Where("account_id", account.IdDb)
             .GetAsync();

         foreach (var rowActor in actors) {
            int slot = rowActor.slot_id;
            Console.WriteLine(rowActor.id);
            account.Actors[slot] = rowActor.name;//LoadPlayer(rowActor);
         }

         return account;
      }
   }
}
