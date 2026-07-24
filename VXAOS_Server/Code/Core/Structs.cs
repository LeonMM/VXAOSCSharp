using static Humanizer.In;

namespace VXAOS_Server {
   public class Hotbar {
      public Enums.Hotbar Type;
      public int ItemId;
      public Hotbar(Enums.Hotbar type, int itemId) {
         Type = type;
         ItemId = itemId;
      }
   }
   public class Target {
      public Enums.Target Type = Enums.Target.NONE;
      public int Id = -1;
   }
   public class Request {
      public Enums.Request Type = Enums.Request.NONE;
      public int Id = -1;
   }
   public class Region {
      public int X;
      public int Y;
      public Region(int x, int y) {
         X = x;
         Y = y;
      }
   }
   public class IPBlocked {
      public int Attempts;
      public DateTimeOffset Time;
   }
   public class Drop {
      public int ItemId;
      public int Kind;
      public int Amount;
      public string Name;
      public int PartyId;
      public int X;
      public int Y;
      public DateTimeOffset DespawnTime;
      public DateTimeOffset PickUpTime;
   }
   public class RewardData {
      public int ItemId;
      public int ItemKind;
      public int ItemAmount;
      public int Exp;
      public int Gold;
   }
   /*public class Interpreter {
      public int List;
      public int EventId;
      public int Index;
      public DateTimeOffset Time;
      public Interpreter(DateTimeOffset time) {
         Time = time;
      }
   }*/
   public class Guild {
      public int IdDb;
      public string Leader;
      public List<int> Flag;
      public List<string> Members;
      public string Notice;
   }
   public class Account {
      public int IdDb;
      public string Pass;
      public int Group;
      public DateTimeOffset VipTime;
      public Dictionary<int, Actor> Actors = new();
      public List<string> Friends = new();
   }
   public class Party {
      public int Id;
      public List<int> Members = new();
      public Party(int id, int leader) {
         Id = id;
         Members.Add(leader);
      }
      public IEnumerable<GameClient> Clients {
         get {
            foreach (var id in Members) {
               if (Network.Clients.TryGetValue(id, out var client))
                  yield return client;
            }
         }
      }
   }
   public class Actor {
      public int IdDb;
      public string Name = "";
      public string CharacterName = "";
      public int CharacterIndex;
      public string FaceName = "";
      public int FaceIndex;
      public int ClassId;
      public int Sex;
      public int Level;
      public int Exp;
      public int Hp;
      public int Mp;
      public int[] ParamBase = new int[8];
      public List<int> Equips = new();
      public int Points;
      public string GuildName = "";
      public int ReviveMapId;
      public int ReviveX;
      public int ReviveY;
      public int MapId;
      public int X;
      public int Y;
      public int Direction;
      public int Gold;
      public Dictionary<int,int> Items = new();
      public Dictionary<int,int> Weapons = new();
      public Dictionary<int,int> Armors = new();
      public List<int> Skills = new();
      public Dictionary<int, GameQuest> Quests = new();
      public List<Hotbar> Hotbar = new();
      public List<bool> Switches = new();
      public List<int> Variables = new();
      public Dictionary<(int MapId, int EventId, char Ch), bool> SelfSwitches = new();
      public List<int> States = new();
      public Dictionary<int, float> StatesTime = new();
   }
}
