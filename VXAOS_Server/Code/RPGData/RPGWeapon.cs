using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace VXAOS_Server.RPGData {
	public class RPGWeapon:RPGEquipItem {
		public double wtype_id = 0;
		public double animation_id = 0;
		public int ani_index = 0;
		public RPGWeapon() {
			this.features.Add(new RPGBaseItemFeature(31,1,0));
			this.features.Add(new RPGBaseItemFeature(22,0,0));
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         level = Note.ReadNumber("Level", note);
         ani_index = Note.ReadNumber("AniIndex", note, 8);
         vip = Note.ReadBoolean("VIP", note);
         soulbound = Note.ReadBoolean("Soulbound", note);
      }
   }
}
