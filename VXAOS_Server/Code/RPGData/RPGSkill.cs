using System.Runtime.Serialization;

namespace VXAOS_Server.RPGData {
	public class RPGSkill:RPGUsableItem {
		public int stype_id = 1;
		public int mp_cost = 0;
		public int tp_cost = 0;
		public string message1 = "";
		public string message2 = "";
		public int required_wtype_id1 = 0;
		public int required_wtype_id2 = 0;
      public int cooldown;
		public RPGSkill() {
			scope = 1;
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         range = Note.ReadNumber("Range", note);
         aoe = Note.ReadNumber("AOE", note);
         level = Note.ReadNumber("Level", note);
         int cd = Note.ReadNumber("Cooldown", note);
         cooldown = cd > 0 ? cd : Configs.CooldownSkillTime;
         ani_index = Note.ReadNumber("AniIndex", note, 8);
      }
   }
}
