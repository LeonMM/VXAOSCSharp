using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public class Hotbar {
      public int Type;
      public int ItemId;
   }

   public class Target {
      public int Type;
      public int Id;
   }

   public class Region {
      public int X;
      public int Y;
   }

   public class IPBlocked {
      public int Attempts;
      public int Time;
   }

   public class Drop {
      public int ItemId;
      public int Kind;
      public int Amount;
      public string Name;
      public int PartyId;
      public int X;
      public int Y;
      public int DespawnTime;
      public int PickUpTime;
   }

   public class Reward {
      public int ItemId;
      public int ItemKind;
      public int ItemAmount;
      public int Exp;
      public int Gold;
   }

   public class Interpreter {
      public int List;
      public int EventId;
      public int Index;
      public int Time;
   }

   public class Guild {
      public int IdDb;
      public string Leader;
      public int[] Flag;
      public string[] Members;
      public string Notice;
   }

   public class Account {
      public int IdDb;
      public string Pass;
      public int Group;
      public DateTime VipTime;
      public Dictionary<int, Actor> Actors = new();
      public List<string> Friends = new();
   }

   public class Actor {
      public int IdDb;
      public string Name;
      public string CharacterName;
      public int CharacterIndex;
      public string FaceName;
      public int FaceIndex;
      public int ClassId;
      public int Sex;
      public int Level;
      public int Exp;
      public int Hp;
      public int Mp;
      public int[] ParamBase;
      public int Equips;
      public int Points;
      public string GuildName;
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
      public int[] Skills;
      public Dictionary<int, string> Quests; // Game_Quest
      public Dictionary<int, Hotbar> Hotbar = new();
      public bool[] Switches;
      public int[] Variables;
      public Dictionary<int, bool> SelfSwitches = new();
      public int[] States;
      public Dictionary<int, int> StatesTime = new();
   }
}
