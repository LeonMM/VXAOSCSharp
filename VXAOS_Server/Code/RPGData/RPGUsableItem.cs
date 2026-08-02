using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGUsableItem:RPGBaseItem {
		public int scope = 0;
		public int occasion = 0;
		public int speed = 0;
		public int success_rate = 100;
		public int repeats = 1;
		public int tp_gain = 0;
		public int hit_type = 0;
		public int animation_id = 0;
      public int range = 0;
      public int aoe = 0;
      public int level = 0;
      public int ani_index = 0;
		public RPGUsableItemDamage damage = new RPGUsableItemDamage();
		[JsonConverter(typeof(ListConverter<RPGUsableItemEffect>))]
		public List<RPGUsableItemEffect> effects = new List<RPGUsableItemEffect>();
      public bool IsForOpponent() {
         return scope is >= 1 and <= 6;
      }
      public bool IsForFriend() {
         return scope is >= 7 and <= 11;
      }
      public bool IsForDeadFriend() {
         return scope == 9 || scope == 10;
      }
      public bool IsForUser() {
         return scope == 11;
      }
      public bool IsForOne() {
         return scope == 1 || scope == 3 || scope == 7 || scope == 9 || scope == 11;
      }
      public bool IsForRandom() {
         return scope is >= 3 and <= 6;
      }
      public int NumberOfTargets() {
         return IsForRandom() ? scope - 2 : 0;
      }
      public bool IsForAll() {
         return scope == 2 || scope == 8 || scope == 10;
      }
      public bool IsNeedSelection() {
         return scope == 1 || scope == 7 || scope == 9;
      }
      public bool IsBattleOk() {
         return occasion == 0 || occasion == 1;
      }
      public bool IsMenuOk() {
         return occasion == 0 || occasion == 2;
      }
      public bool IsCertain() {
         return hit_type == 0;
      }
      public bool IsPhysical() {
         return hit_type == 1;
      }
      public bool IsMagical() {
         return hit_type == 2;
      }
      public bool IsAoe() {
         return aoe > 0;
      }
   }
}
