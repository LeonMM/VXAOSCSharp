using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using Npgsql.Internal.Postgres;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Security.Principal;
using System.Xml.Linq;
using VXAOS_Server.RPGData;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public class Database {
      //private ServerConfig cfg;
      private readonly Compiler compiler;
      public Database() {
         //this.cfg = cfg;
         compiler = GetCompiler();
         Console.WriteLine($"Banco de Dados {ServerConfig.DbType} incializado.");
      }
      public IDbConnection CreateConnection() {
         switch (ServerConfig.DbType) {
            case Enums.DatabaseType.POSTGRESQL:
               var pgConn =
                   $"Host={ServerConfig.DbHost};" +
                   $"Port={ServerConfig.DbPort};" +
                   $"Database={ServerConfig.DbName};" +
                   $"Username={ServerConfig.DbUser};" +
                   $"Password={ServerConfig.DbPass};" +
                   $"Minimum Pool Size={ServerConfig.DbPoolMin};" +
                   $"Maximum Pool Size={ServerConfig.DbPoolMax};" +
                   $"Pooling=True;Max Auto Prepare=50;Auto Prepare Min Usages=2;";
               return new NpgsqlConnection(pgConn);
            case Enums.DatabaseType.MYSQL:
               var myConn =
                   $"Server={ServerConfig.DbHost};" +
                   $"Port={ServerConfig.DbPort};" +
                   $"Database={ServerConfig.DbName};" +
                   $"User ID={ServerConfig.DbUser};" +
                   $"Password={ServerConfig.DbPass};" +
                   $"MinimumPoolSize={ServerConfig.DbPoolMin};" +
                   $"MaximumPoolSize={ServerConfig.DbPoolMax};" +
                   $"Pooling=True;ConnectionIdleTimeout=60";
               return new MySqlConnection(myConn);
            case Enums.DatabaseType.SQLITE:
               var sqliteConn =
                   $"Data Source=Data/{ServerConfig.DbFile}.db;" +
                   $"Pooling=True;";
               return new SqliteConnection(sqliteConn);

            default:
               throw new Exception("Unsupported database type");
         }
      }
      public Compiler GetCompiler() {
         switch (ServerConfig.DbType) {
            case Enums.DatabaseType.POSTGRESQL:
               return new PostgresCompiler();
            case Enums.DatabaseType.MYSQL:
               return new MySqlCompiler();
            case Enums.DatabaseType.SQLITE:
               return new SqliteCompiler();
            default:
               throw new Exception("Unsupported compiler type");
         }
      }
      private QueryFactory Query() {
         var conn = CreateConnection();
         return new QueryFactory(conn, compiler);
      }
      internal async Task CreateAccount(string user, string pass, string nEmail) {
         var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
         var qry = Query();
         try {
            var accountId = await qry.Query("accounts").InsertGetIdAsync<int>(new {
               username = user,
               password = pass,
               email = nEmail,
               vip_time = now,
               creation_date = now,
               cash = 0
            });
            await qry.Query("banks").InsertAsync(new { account_id = accountId });
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task<Account> LoadAccount(string user) {
         var qry = Query();
         var account = new Account();
         try {
            var row = await qry.Query("accounts")
                .Where("username", user)
                .FirstOrDefaultAsync();
            if (row == null)
               return null;
            account.IdDb = Convert.ToInt32(row.id);
            account.Pass = row.password;
            account.Group = Convert.ToInt32(row.group);
            var vipTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(row.vip_time));
            account.VipTime = vipTime > DateTimeOffset.UtcNow
                ? vipTime
                : DateTimeOffset.UtcNow;
            var friends = await qry.Query("account_friends")
                .Where("account_id", account.IdDb)
                .Select("name")
                .GetAsync<string>();
            account.Friends = friends.ToList();
            var actors = await qry.Query("actors")
                .Where("account_id", account.IdDb)
                .GetAsync();
            foreach (var rowActor in actors) {
               int slot = Convert.ToInt32(rowActor.slot_id);
               Actor actor = await LoadPlayer(qry, rowActor);
               account.Actors.Add(slot, actor);
            }
         } finally { qry.Connection.Dispose(); }
         return account;
      }
      internal async Task SaveAccount(GameClient client, QueryFactory qry) {
         long? vipTimeUnix = await qry.Query("accounts")
             .Where("id", client.AccountIdDb)
             .Select("vip_time")
             .FirstOrDefaultAsync<long?>();
         DateTimeOffset vipTime = DateTimeOffset.FromUnixTimeSeconds(
             Math.Max(vipTimeUnix ?? 0, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
         );
         DateTimeOffset newVipTime = vipTime.AddSeconds(client.AddedVipTime);
                  await qry.Query("accounts")
             .Where("id", client.AccountIdDb)
             .UpdateAsync(new {
                vip_time = newVipTime.ToUnixTimeSeconds()
             });
         client.VipTime = newVipTime;
         client.AddedVipTime = 0;
         var friends = (await qry.Query("account_friends")
             .Where("account_id", client.AccountIdDb)
             .Select("name")
             .GetAsync<string>())
             .ToList();
         foreach (string name in friends.Except(client.Friends)) {
            await qry.Query("account_friends")
                .Where("account_id", client.AccountIdDb)
                .Where("name", name)
                .DeleteAsync();
         }
         foreach (string name in client.Friends.Except(friends)) {
            await qry.Query("account_friends")
                .InsertAsync(new {
                   account_id = client.AccountIdDb,
                   name
                });
         }
      }
      internal async Task<bool> DoAccountExist(string user) {
         var qry = Query();
         bool result = false;
         try {
            result = await qry.Query("accounts")
            .Where("username", user)
            .ExistsAsync();
         } finally { qry.Connection.Dispose(); }
         return result;
      }
      internal async Task CreatePlayer(GameClient client, byte actorId, string name, byte characterIndex, short classId, byte sex, int[] @params, int points) {
         Actor actor = new();
         actor.Name = name;
         actor.CharacterName = DataClasses[classId].graphics[sex][characterIndex].Item1;
         actor.CharacterIndex = DataClasses[classId].graphics[sex][characterIndex].Item2;
         actor.FaceName = DataClasses[classId].graphics[sex + 2][characterIndex].Item1;
         actor.FaceIndex = DataClasses[classId].graphics[sex + 2][characterIndex].Item2;
         actor.ClassId = classId;
         actor.Sex = sex;
         actor.Level = (int)DataActors[classId].initial_level;
         actor.Exp = DataClasses[classId].Exp_For_Level(actor.Level);
         int maxHp = @params[(int)Enums.Param.MAXHP] * 10 + (int)DataClasses[classId].@params[(int)Enums.Param.MAXHP, actor.Level];
         int maxMp = @params[(int)Enums.Param.MAXMP] * 10 + (int)DataClasses[classId].@params[(int)Enums.Param.MAXMP, actor.Level];
         actor.ParamBase[(int)Enums.Param.MAXHP] = maxHp;
         actor.ParamBase[(int)Enums.Param.MAXMP] = maxMp;
         for (int paramId = (int)Enums.Param.ATK; paramId <= (int)Enums.Param.LUK; paramId++) {
            actor.ParamBase[paramId] = (int)DataClasses[classId].@params[paramId, actor.Level] + @params[paramId];
         }
         foreach (double equip in DataActors[classId].equips) {
            actor.Equips.Add((int)equip);
         }
         while (actor.Equips.Count < Configs.MAX_EQUIPS) {
            actor.Equips.Add(0);
         }
         actor.Points = points;
         actor.GuildName = "";
         actor.ReviveMapId = actor.MapId = (int)DataSystem.start_map_id;
         actor.ReviveX = actor.X = (int)DataSystem.start_x;
         actor.ReviveY = actor.Y = (int)DataSystem.start_y;
         actor.Direction = (int)Enums.Dir.DOWN;
         actor.Gold = 0;
         actor.States = new();
         actor.StatesTime = new();
         actor.Items = new();
         actor.Weapons = new();
         actor.Armors = new();
         foreach (var learning in DataClasses[classId].learnings) {
            if (learning.level <= actor.Level)
               actor.Skills.Add((int)learning.skill_id);
         }
         actor.Quests = new();
         for (int i = 0; i < Configs.MaxHotbar; i++) {
            actor.Hotbar.Add(new Hotbar(0, 0));
         }
         actor.Switches = Enumerable.Repeat(false, Configs.MaxPlayerSwitches).ToList();
         actor.Variables = Enumerable.Repeat(0, Configs.MaxPlayerVariables).ToList();
         var qry = Query();
         bool success = false;
         try {
            var id = await qry.Query("actors").InsertGetIdAsync<int>(new {
               slot_id = actorId,
               account_id = client.AccountIdDb,
               name = actor.Name,
               character_name = actor.CharacterName,
               character_index = actor.CharacterIndex,
               face_name = actor.FaceName,
               face_index = actor.FaceIndex,
               class_id = actor.ClassId,
               sex = actor.Sex,
               level = actor.Level,
               exp = actor.Exp,
               hp = actor.Hp,
               mp = actor.Mp,
               mhp = actor.ParamBase[0],
               mmp = actor.ParamBase[1],
               atk = actor.ParamBase[2],
               def = actor.ParamBase[3],
               @int = actor.ParamBase[4],
               res = actor.ParamBase[5],
               agi = actor.ParamBase[6],
               luk = actor.ParamBase[7],
               points = actor.Points,
               revive_map_id = actor.ReviveMapId,
               revive_x = actor.ReviveX,
               revive_y = actor.ReviveY,
               map_id = actor.MapId,
               x = actor.X,
               y = actor.Y,
               direction = actor.Direction,
               creation_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
               last_login = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            actor.IdDb = id;
            for (int slotId = 0; slotId < actor.Equips.Count; slotId++) {
               await qry.Query("actor_equips").InsertAsync(new {
                  actor_id = actor.IdDb,
                  slot_id = slotId,
                  equip_id = actor.Equips[slotId]
               });
            }
            foreach (var skillId in actor.Skills) {
               await qry.Query("actor_skills").InsertAsync(new {
                  actor_id = actor.IdDb,
                  skill_id = skillId
               });
            }
            for (int slotId = 0; slotId < Configs.MaxHotbar; slotId++) {
               await qry.Query("actor_hotbars").InsertAsync(new {
                  actor_id = actor.IdDb,
                  slot_id = slotId
               });
            }
            var switches = new List<object>();
            for (int switchId = 1; switchId <= Configs.MaxPlayerSwitches; switchId++) {
               switches.Add(new {
                  actor_id = actor.IdDb,
                  switch_id = switchId
               });
            }
            await qry.Query("actor_switches").InsertAsync(switches);
            var variables = new List<object>();
            for (int variableId = 1; variableId <= Configs.MaxPlayerVariables; variableId++) {
               variables.Add(new {
                  actor_id = actor.IdDb,
                  variable_id = variableId
               });
            }
            await qry.Query("actor_variables").InsertAsync(variables);
            success = true;
         } finally { qry.Connection.Dispose(); }
         if (success)
            client.Actors[actorId] = actor;
      }
      internal async Task<(int AccountId, string Name)?> LoadSomePlayerData(string name) {
         var qry = Query();
         try {
            dynamic row = await qry.Query("actors")
                .Where("name", name)
                .Select("account_id", "name")
                .FirstOrDefaultAsync();
            if (row == null)
               return null;
            return ((int)row.account_id, (string)row.name);
         } finally {
            qry.Connection.Dispose();
         }
      }
      internal async Task<Actor> LoadPlayer(QueryFactory qry, dynamic rowActor) {
         Actor actor = new();
         actor.IdDb = Convert.ToInt32(rowActor.id);
         actor.Name = rowActor.name;
         actor.CharacterName = rowActor.character_name;
         actor.CharacterIndex = Convert.ToInt32(rowActor.character_index);
         actor.FaceName = rowActor.face_name;
         actor.FaceIndex = Convert.ToInt32(rowActor.face_index);
         actor.ClassId = Convert.ToInt32(rowActor.class_id);
         actor.Sex = Convert.ToInt32(rowActor.sex);
         actor.Level = Convert.ToInt32(rowActor.level);
         actor.Exp = Convert.ToInt32(rowActor.exp);
         actor.Hp = Convert.ToInt32(rowActor.hp);
         actor.Mp = Convert.ToInt32(rowActor.mp);
         actor.ParamBase[0] = Convert.ToInt32(rowActor.mhp);
         actor.ParamBase[1] = Convert.ToInt32(rowActor.mmp);
         actor.ParamBase[2] = Convert.ToInt32(rowActor.atk);
         actor.ParamBase[3] = Convert.ToInt32(rowActor.def);
         actor.ParamBase[4] = Convert.ToInt32(rowActor.@int);
         actor.ParamBase[5] = Convert.ToInt32(rowActor.res);
         actor.ParamBase[6] = Convert.ToInt32(rowActor.agi);
         actor.ParamBase[7] = Convert.ToInt32(rowActor.luk);
         actor.Points = Convert.ToInt32(rowActor.points);
         actor.GuildName = Network.FindGuildName(Convert.ToInt32(rowActor.guild_id));
         actor.ReviveMapId = Convert.ToInt32(rowActor.revive_map_id);
         actor.ReviveX = Convert.ToInt32(rowActor.revive_x);
         actor.ReviveY = Convert.ToInt32(rowActor.revive_y);
         int map_id = Convert.ToInt32(rowActor.map_id);
         bool mapExists = Network.Maps.ContainsKey(map_id);
         actor.MapId = mapExists ? map_id : Convert.ToInt32(actor.ReviveMapId);
         actor.X = mapExists ? rowActor.x : Convert.ToInt32(actor.ReviveX);
         actor.Y = mapExists ? rowActor.y : Convert.ToInt32(actor.ReviveY);
         actor.Direction = Convert.ToInt32(rowActor.direction);
         actor.Gold = Convert.ToInt32(rowActor.gold);
         var skills = await qry.Query("actor_skills")
             .Where("actor_id", actor.IdDb)
             .Select("skill_id")
             .GetAsync<int>();
         actor.Skills = skills.ToList();
         var switches = await qry.Query("actor_switches")
             .Where("actor_id", actor.IdDb)
             .Select("value")
             .GetAsync<int>();
         actor.Switches = switches.Select(v => v == 1).ToList();
         var variables = await qry.Query("actor_variables")
                      .Where("actor_id", actor.IdDb)
                      .Select("value")
                      .GetAsync<int>();
         actor.Variables = variables.ToList();
         var rows = await qry.Query("actor_self_switches")
                  .Where("actor_id", actor.IdDb)
                  .GetAsync();
         foreach (var row in rows) {
            actor.SelfSwitches[
                (Convert.ToInt32(row.map_id),
                 Convert.ToInt32(row.event_id),
                 Convert.ToChar(row.ch))
            ] = row.value == 1;
         }
         await LoadPlayerEquips(qry, actor);
         await LoadPlayerItems(qry, actor);
         await LoadPlayerWeapons(qry, actor);
         await LoadPlayerArmors(qry, actor);
         await LoadPlayerQuests(qry, actor);
         await LoadPlayerHotbar(qry, actor);
         await LoadPlayerStates(qry, actor);
         return actor;
      }
      internal async Task LoadPlayerEquips(QueryFactory qry, Actor actor) {
         actor.Equips = new();
         var equips = await qry.Query("actor_equips")
             .Where("actor_id", actor.IdDb)
             .Select("equip_id", "slot_id")
             .GetAsync();
         foreach (var equip in equips) {
            int equipId = Convert.ToInt32(equip.equip_id);
            int slotId = Convert.ToInt32(equip.slot_id);
            if ((slotId == (int)Enums.Equip.WEAPON && !DataManager.DataWeapons.HasIndex(equipId)) ||
               (slotId > (int)Enums.Equip.WEAPON && !DataManager.DataArmors.HasIndex(equipId)))
               equipId = 0;
            actor.Equips.Add(equipId);
         }
      }
      internal async Task LoadPlayerItems(QueryFactory qry, Actor actor) {
         actor.Items = new();
         var items = await qry.Query("actor_items")
                      .Where("actor_id", actor.IdDb)
                      .Select("item_id", "amount")
                      .GetAsync();
         foreach (var item in items) {
            int itemId = Convert.ToInt32(item.item_id);
            int amount = Convert.ToInt32(item.amount);
            if (DataManager.DataItems.HasIndex(itemId))
               actor.Items.Add(itemId, amount);
         }
      }
      internal async Task LoadPlayerWeapons(QueryFactory qry, Actor actor) {
         actor.Weapons = new();
         var weapons = await qry.Query("actor_weapons")
                      .Where("actor_id", actor.IdDb)
                      .Select("weapon_id", "amount")
                      .GetAsync();
         foreach (var weapon in weapons) {
            int weaponId = Convert.ToInt32(weapon.weapon_id);
            int amount = Convert.ToInt32(weapon.amount);
            if (DataManager.DataWeapons.HasIndex(weaponId))
               actor.Weapons.Add(weaponId, amount);
         }
      }
      internal async Task LoadPlayerArmors(QueryFactory qry, Actor actor) {
         actor.Armors = new();
         var armors = await qry.Query("actor_armors")
                      .Where("actor_id", actor.IdDb)
                      .Select("armor_id", "amount")
                      .GetAsync();
         foreach (var armor in armors) {
            int armorId = Convert.ToInt32(armor.armor_id);
            int amount = Convert.ToInt32(armor.amount);
            if (DataArmors.HasIndex(armorId))
               actor.Armors.Add(armorId, amount);
         }
      }
      internal async Task LoadPlayerQuests(QueryFactory qry, Actor actor) {
         var quests = await qry.Query("actor_quests")
                      .Where("actor_id", actor.IdDb)
                      .GetAsync();
         foreach (var quest in quests) {
            int questId = Convert.ToInt32(quest.quest_id);
            int state = Convert.ToInt32(quest.state);
            int kills = Convert.ToInt32(quest.kills);
            actor.Quests.Add(questId, new(questId, (Enums.Quest)state, kills));
         }
      }
      internal async Task LoadPlayerHotbar(QueryFactory qry, Actor actor) {
         actor.Hotbar = new();
         var hotbar = await qry.Query("actor_hotbars")
                      .Where("actor_id", actor.IdDb)
                      .Select("item_id", "type")
                      .GetAsync();
         foreach (var row in hotbar) {
            Enums.Hotbar type;
            int itemId = Convert.ToInt32(row.item_id);
            int value = Convert.ToInt32(row.type);
            if (Enum.IsDefined(typeof(Enums.Hotbar), value)) {
               type = (Enums.Hotbar)value;
            } else {
               type = Enums.Hotbar.NONE;
               itemId = 0;
            }
            if ((type == Enums.Hotbar.ITEM && !DataManager.DataItems.HasIndex(itemId)) ||
               (type == Enums.Hotbar.SKILL && !DataManager.DataSkills.HasIndex(itemId))) {
               type = Enums.Hotbar.NONE;
               itemId = 0;
            }
            actor.Hotbar.Add(new(type, itemId));
         }
      }
      internal async Task LoadPlayerStates(QueryFactory qry, Actor actor) {
         actor.States = new();
         actor.StatesTime = new();
         var states = await qry.Query("actor_states")
                      .Where("actor_id", actor.IdDb)
                      .Select("state_id", "state_time")
                      .GetAsync();
         foreach (var state in states) {
            int stateId = Convert.ToInt32(state.state_id);
            float stateTime = Convert.ToDecimal(state.state_time);
            actor.States.Add(stateId);
            if (stateTime > 0)
               actor.StatesTime.Add(stateId, stateTime);
         }
      }
      internal async Task SavePlayer(GameClient client) {
         var qry = Query();
         try {
            await qry.Query("actors")
               .Where("id", client.IdDb)
               .UpdateAsync(new {
                  character_name = client.CharacterName, character_index = client.CharacterIndex,
                  face_name = client.FaceName, face_index = client.FaceIndex,
                  class_id = client.ClassId, sex = client.Sex,
                  level = client.Level, exp = client.Exp,
                  hp = client.Hp, mp = client.Mp,
                  mhp = client.ParamBase[0], mmp = client.ParamBase[1],
                  atk = client.ParamBase[2], def = client.ParamBase[3],
                  @int = client.ParamBase[4], res = client.ParamBase[5],
                  agi = client.ParamBase[6], luk = client.ParamBase[7],
                  points = client.Points, guild_id = Network.FindGuildIdDb(client.GuildName),
                  revive_map_id = client.ReviveMapId, revive_x = client.ReviveX,
                  revive_y = client.ReviveY, map_id = client.MapId,
                  x = client.X, y = client.Y, direction = client.Direction,
                  gold = client.Gold, last_login = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
               });
            for (int slotId = 0; slotId < client.Equips.Count; slotId++) {
               await qry.Query("actor_equips").
                  Where("actor_id", client.IdDb).Where("slot_id",slotId).
                  UpdateAsync(new {
                     equip_id = client.Equips[slotId]
                  });
            }
            for (int slotId = 0; slotId < client.Hotbar.Count; slotId++) {
               await qry.Query("actor_hotbars").
                  Where("actor_id", client.IdDb).Where("slot_id",slotId).
                  UpdateAsync(new {
                     type = (int)client.Hotbar[slotId].Type,
                     item_id = client.Hotbar[slotId].ItemId
                  });
            }
            for (int switchId = 0; switchId < client.Switches.Count; switchId++) {
               await qry.Query("actor_switches").
                  Where("actor_id", client.IdDb).Where("switch_id", (switchId + 1)).
                  UpdateAsync(new {
                     value = (client.Switches[switchId] ? 1 : 0)
                  });
            }
            for (int variableId = 0; variableId < client.Variables.Count; variableId++) {
               await qry.Query("actor_variables").
                  Where("actor_id", client.IdDb).Where("variable_id", variableId).
                  UpdateAsync(new {
                     value = client.Variables[variableId]
                  });
            }
            await SaveItems(client, qry, client.Items, "item");
            await SaveItems(client, qry, client.Weapons, "weapon");
            await SaveItems(client, qry, client.Armors, "armor");
            await SavePlayerSkills(client, qry);
            await SavePlayerQuests(client, qry);
            await SavePlayerSelfSwitches(client, qry);
            await SavePlayerStates(client, qry);
            await SaveAccount(client, qry);
            await SaveBank(client, qry);
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task SaveItems(GameClient client, QueryFactory qry, Dictionary<int, int> actItems, string iType, bool isBank = false) {
         int idDb = isBank ? client.AccountIdDb : client.IdDb;
         string objectId = isBank ? "bank_id" : "actor_id";
         string table = isBank ? $"bank_{iType}s" : $"actor_{iType}s";
         string iTypeId = $"{iType}_id";
         var items = (await qry.Query(table)
                        .Where(objectId, idDb)
                        .SelectRaw($"{iTypeId} as ItemId")
                        .Select("amount")
                        .GetAsync<(int ItemId, int Amount)>())
                        .ToDictionary(x => x.ItemId, x => x.Amount);
         foreach (var (itemId, amnt) in actItems) {
            if (!items.TryGetValue(itemId, out var dbAmount)) {
               await qry.Query(table).InsertAsync(new Dictionary<string, object> {
                  [objectId] = idDb,
                  [iTypeId] = itemId,
                  ["amount"] = amnt
               });
               continue;
            } else if (amnt != dbAmount) {
               await qry.Query(table).
                  Where(objectId, idDb).
                  Where(iTypeId, itemId).
                  UpdateAsync(new {
                     amount = amnt
                  });
            }
            items.Remove(itemId);
         }
         foreach (var itemId in items.Keys) {
            await qry.Query(table).
                  Where(objectId, idDb).
                  Where(iTypeId, itemId).
                  DeleteAsync();
         }
      }
      internal async Task SavePlayerSkills(GameClient client, QueryFactory qry) {
         var skills = (await qry.Query("actor_skills").
            Where("actor_id", client.IdDb).
            Select("skill_id").
            GetAsync<int>()).ToList();
         foreach(var skillId in skills.Except(client.Skills)) {
            await qry.Query("actor_skills").
               Where("actor_id", client.IdDb).
               Where("skill_id", skillId).
               DeleteAsync();
         }
         foreach (var skillId in client.Skills.Except(skills)) {
            await qry.Query("actor_skills").
               InsertAsync(new {
                  actor_id = client.IdDb,
                  skill_id = skillId
               });
         }
      }
      internal async Task SavePlayerQuests(GameClient client, QueryFactory qry) {
         var quests = (await qry.Query("actor_quests").
            Where("actor_id", client.IdDb).
            Select("quest_id").
            GetAsync<int>()).ToList();
         foreach(var (questId, quest) in client.Quests) {
            if (quests.Contains(questId)) {
               await qry.Query("actor_quests").
                  Where("actor_id", client.IdDb).
                  Where("quest_id", questId).
                  UpdateAsync(new {
                     state = (int)quest.State,
                     kills = quest.Kills
                  });
               quests.Remove(questId);
            } else {
               await qry.Query("actor_quests").InsertAsync(new {
                  actor_id = client.IdDb,
                  quest_id = questId,
                  state = (int)quest.State,
                  kills = quest.Kills
               });
            }
         }
         foreach(var questId in quests) {
            await qry.Query("actor_quests").
               Where("actor_id", client.IdDb).
               Where("quest_id", questId).
               DeleteAsync();
         }
      }
      internal async Task SavePlayerSelfSwitches(GameClient client, QueryFactory qry) {
         //(int MapId, int EventId, char Ch)
         Dictionary<(int MapId, int EventId, char Ch), bool> selfSwitches = new();
         var rows = await qry.Query("actor_self_switches")
                  .Where("actor_id", client.IdDb)
                  .GetAsync();
         foreach (var row in rows) {
            selfSwitches[
                (Convert.ToInt32(row.map_id),
                 Convert.ToInt32(row.event_id),
                 Convert.ToChar(row.ch))
            ] = row.value == 1;
         }
         foreach(var (key, value) in client.SelfSwitches.Data) {
            if (selfSwitches.TryGetValue(key, out bool nValue) && value != nValue) {
               await qry.Query("actor_self_switches").
                  Where("actor_id", client.IdDb).
                  Where("map_id", key.MapId).
                  Where("event_id", key.EventId).
                  Where("ch", key.Ch).
                  UpdateAsync(new {
                     value = (value ? 1 : 0)
                  });
            } else if (!selfSwitches.ContainsKey(key)) {
               await qry.Query("actor_self_switches").
                  InsertAsync(new {
                     actor_id = client.IdDb,
                     map_id = key.MapId,
                     event_id = key.EventId,
                     ch = key.Ch,
                     value = (value ? 1 : 0)
                  });
            }
         }
      }
      internal async Task SavePlayerStates(GameClient client, QueryFactory qry) {
         await qry.Query("actor_states").
            Where("actor_id", client.IdDb).
            DeleteAsync();
         foreach(var stateId in client.States) {
            if (!DataStates[stateId].save)
               continue;
            float time = -1f;
            if(client.StatesTime.TryGetValue(stateId, out DateTimeOffset value)) {
               time = (float)(value - DateTimeOffset.UtcNow).TotalSeconds;
            }
            await qry.Query("actor_states").
               InsertAsync(new {
                  actor_id = client.IdDb, 
                  state_id = stateId,
                  state_time = time
               });
         }
      }
      internal async Task<bool> DoPlayerExist(string name) {
         var qry = Query();
         bool result = false;
         try {
            result = await qry.Query("actors")            
            .Where("name", name)
            .ExistsAsync();
         } finally { qry.Connection.Dispose(); }
         return result;
      }
      internal async Task RemovePlayer(int actorIdDb) {
         var qry = Query();
         try {
            qry.Connection.Open();
            using var transaction = qry.Connection.BeginTransaction();
            await qry.Query("actors")
                .Where("id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_equips")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_items")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_weapons")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_armors")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_skills")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_quests")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_hotbars")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_switches")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_variables")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_self_switches")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            await qry.Query("actor_states")
                .Where("actor_id", actorIdDb)
                .DeleteAsync(transaction);
            transaction.Commit();
         } catch {
            throw;
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task LoadDistributor(GameClient client) {
         var qry = Query();
         try {
            var items = await qry.Query("distributor").
                Where("account_id", client.AccountIdDb).
                GetAsync();
            foreach(var item in items) {
               var container = client.BankItemContainer(item.kind);
               container[(int)item.item_id] = Math.Min(Configs.MaxItems,
                     client.BankItemNumber(item.kind, item.item_id) + (int)item.amount);
            }
            await qry.Query("distributor").
                Where("account_id", client.AccountIdDb).
                DeleteAsync();
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task LoadBank(GameClient client) {
         var qry = Query();
         try { 
            var bank = await qry.Query("banks")
                        .Where("account_id", client.AccountIdDb)
                        .FirstAsync();
            client.BankIdDb = bank.id;
            client.BankGold = bank.gold;
            client.BankItems = new Dictionary<int, int>();
            var items = await qry.Query("bank_items")
                .Where("bank_id", client.BankIdDb)
                .Select("item_id", "amount")
                .GetAsync();
            foreach (var item in items) {
               client.BankItems[(int)item.item_id] = (int)item.amount;
            }
            client.BankWeapons = new Dictionary<int, int>();
            var weapons = await qry.Query("bank_weapons")
                .Where("bank_id", client.BankIdDb)
                .Select("weapon_id", "amount")
                .GetAsync();
            foreach (var weapon in weapons) {
               client.BankWeapons[(int)weapon.weapon_id] = (int)weapon.amount;
            }
            client.BankArmors = new Dictionary<int, int>();
            var armors = await qry.Query("bank_armors")
                .Where("bank_id", client.BankIdDb)
                .Select("armor_id", "amount")
                .GetAsync();
            foreach (var armor in armors) {
               client.BankArmors[(int)armor.armor_id] = (int)armor.amount;
            }
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task SaveBank(GameClient client, QueryFactory qry) {
         await qry.Query("banks").
            Where("account_id", client.AccountIdDb).
            UpdateAsync(new {
               gold = client.BankGold
            });
         await SaveItems(client, qry, client.BankItems, "item", true);
         await SaveItems(client, qry, client.BankWeapons, "weapon", true);
         await SaveItems(client, qry, client.BankArmors, "armor", true);
      }
      internal async Task CreateGuild(string gName) {
         var qry = Query();
         try {
            var guildIdDb = await qry.Query("guilds").InsertGetIdAsync<int>(new {
               name = gName,
               leader = Network.Guilds[gName].Leader,
               flag = string.Join(",", Network.Guilds[gName].Flag),
               creation_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            Network.Guilds[gName].IdDb = guildIdDb;
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task LoadGuilds() {
         var qry = Query();
         try { 
            var rows = await qry.Query("guilds").GetAsync();
            foreach (var row in rows) {
               string name = row.name;
               string flag = row.flag;
               var guild = new Guild {
                  IdDb = row.id,
                  Leader = row.leader,
                  Notice = row.notice,
                  Flag = flag.Split(',')
                        .Select(int.Parse)
                        .ToList()
               };
               var members = await qry.Query("actors")
                  .Where("guild_id", guild.IdDb)
                  .Select("name")
                  .GetAsync<string>();
               guild.Members = members.ToList();
               Network.Guilds[name] = guild;
            }
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task SaveGuild(Guild guild) {
         var qry = Query();
         try {
            await qry.Query("guilds")
               .Where("guild_id", guild.IdDb)
               .UpdateAsync(new {
                  leader = guild.Leader,
                  notice = guild.Notice
               });
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task RemoveGuild(Guild guild) {
         var qry = Query();
         try {
            await qry.Query("guilds")
               .Where("id", guild.IdDb)
               .DeleteAsync();
            await qry.Query("actors")
               .Where("guild_id", guild.IdDb)
               .UpdateAsync( new {
                  guild_id = 0
               });
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task RemoveGuildMember(string memberName) {
         var qry = Query();
         try { 
            await qry.Query("actors")
               .Where("name", memberName)
               .UpdateAsync(new {
                  guild_id = 0
               });
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task LoadBanList() {
         var qry = Query();
         try { 
            var rows = await qry.Query("ban_list").GetAsync();
            foreach (var row in rows) {
               object key = row.account_id > 0
                   ? row.account_id
                   : row.ip;
               Network.BanList[$"{key}"] =
                   DateTimeOffset.FromUnixTimeSeconds((long)row.time);
            }
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task SaveBanList() {
         var qry = Query();
         try { 
            var dbBanList = new Dictionary<string, DateTimeOffset>();
            var rows = await qry.Query("ban_list").GetAsync();
            foreach (var row in rows) {
               string key = row.account_id > 0
                   ? row.account_id.ToString()
                   : row.ip;
               dbBanList[key] =
                   DateTimeOffset.FromUnixTimeSeconds((long)row.time);
            }
            foreach (var pair in dbBanList) {
               if (Network.BanList.ContainsKey(pair.Key))
                  continue;
               if (TryGetAccountId(pair.Key, out int accountId)) {
                  await qry.Query("ban_list")
                      .Where("account_id", accountId)
                      .DeleteAsync();
               } else {
                  await qry.Query("ban_list")
                      .Where("ip", pair.Key)
                      .DeleteAsync();
               }
            }
            foreach (var pair in Network.BanList) {
               if (dbBanList.ContainsKey(pair.Key))
                  continue;
               var data = new Dictionary<string, object> {
                  ["time"] = pair.Value.ToUnixTimeSeconds(),
                  ["ban_date"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
               };
               if (TryGetAccountId(pair.Key, out int accountId)) {
                  data["account_id"] = accountId;
               } else { 
                  data["ip"] = pair.Key;
               }
               await qry.Query("ban_list").InsertAsync(data);
            }
         } finally { qry.Connection.Dispose(); }
      }
      internal static bool TryGetAccountId(string key, out int accountId) {
         return int.TryParse(key, out accountId);
      }
      internal async Task Unban(GameClient client, string playerName) {
         var qry = Query();
         try { 
            int? accountId = await qry.Query("actors")
                         .Where("name", playerName)
                         .Select("account_id")
                         .FirstAsync<int?>();
            if(accountId.HasValue) {
               Network.BanList.TryRemove($"{accountId}", out _);
            }
         } finally { qry.Connection.Dispose(); }
      }
      internal async Task ChangeWhosOnline(int idDb, bool online) {
         var qry = Query();
         try {
            await qry.Query("actors")
               .Where("id", idDb)
               .UpdateAsync(new {
                  online = (online ? 1 : 0)
               });
         } finally { qry.Connection.Dispose(); }
      }
      /*
      internal async Task Example() {
         var qry = Query();
         try {
            await qry.Query("tabela")
               .Where("", )
         } finally { qry.Connection.Dispose(); }
      }
      */
   }
}
