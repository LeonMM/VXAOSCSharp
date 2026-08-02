namespace VXAOS_Server {
   public class Hotbar(Enums.Hotbar type, int itemId) {
      public Enums.Hotbar Type = type;
      public int ItemId = itemId;
   }
   public class Target {
      public Enums.Target Type = Enums.Target.NONE;
      public int Id = -1;
   }
   public class Request {
      public Enums.Request Type = Enums.Request.NONE;
      public int Id = -1;
   }
   public class Region(int x, int y) {
      public int X = x;
      public int Y = y;
   }
   public class IPBlocked {
      public int Attempts;
      public DateTimeOffset Time;
   }
   public class Drop {
      public int ItemId;
      public int Kind;
      public int Amount;
      public string Name = "";
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
      public string Leader = "";
      public List<int> Flag = [];
      public List<string> Members = [];
      public string Notice = "";
   }
   public class Account {
      public int IdDb;
      public string Pass = "";
      public int Group;
      public DateTimeOffset VipTime;
      public Dictionary<int, Actor> Actors = [];
      public List<string> Friends = [];
   }
   public class Party {
      public int Id;
      public List<int> Members = [];
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
      public List<int> Equips = [];
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
      public Dictionary<int,int> Items = [];
      public Dictionary<int,int> Weapons = [];
      public Dictionary<int,int> Armors = [];
      public List<int> Skills = [];
      public Dictionary<int, GameQuest> Quests = [];
      public List<Hotbar> Hotbar = [];
      public List<bool> Switches = [];
      public List<int> Variables = [];
      public Dictionary<(int MapId, int EventId, char Ch), bool> SelfSwitches = [];
      public List<int> States = []  ;
      public Dictionary<int, float> StatesTime = [];
   }
}
