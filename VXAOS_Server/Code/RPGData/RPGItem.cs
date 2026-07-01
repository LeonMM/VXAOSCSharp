using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace VXAOS_Server.RPGData {
	public class RPGItem:RPGUsableItem {
		public double itype_id = 1;
		public double price = 0;
		public bool consumable = true;
		public bool soulbound = false;
		public RPGItem() {
			this.scope = 7;
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         range = Note.ReadNumber("Range", note);
         aoe = Note.ReadNumber("AOE", note);
         level = Note.ReadNumber("Level", note);
         ani_index = Note.ReadNumber("AniIndex", note, 8);
         soulbound = Note.ReadBoolean("Soulbound", note);
      }
   }
}
