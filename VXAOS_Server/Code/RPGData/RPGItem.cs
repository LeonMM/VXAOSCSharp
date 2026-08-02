using System.Runtime.Serialization;

namespace VXAOS_Server.RPGData {
	public class RPGItem:RPGUsableItem, IRPGItem {
		public int itype_id = 1;
		public int price = 0;
		public bool consumable = true;
		public bool soulbound = false;
		public RPGItem() {
		   scope = 7;
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         range = Note.ReadNumber("Range", note);
         aoe = Note.ReadNumber("AOE", note);
         level = Note.ReadNumber("Level", note);
         ani_index = Note.ReadNumber("AniIndex", note, 8);
         soulbound = Note.ReadBoolean("Soulbound", note);
      }
      public bool IsSoulbound() {
         return soulbound;
      }
      public int Price() {
         return price;
      }
   }
}
