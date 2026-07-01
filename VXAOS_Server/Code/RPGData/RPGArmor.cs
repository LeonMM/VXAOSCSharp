using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace VXAOS_Server.RPGData {
	public class RPGArmor:RPGEquipItem {
		public double atype_id = 0;
		public int sex = 2;
		public RPGArmor() {
			this.etype_id = 1;
			this.features.Add(new RPGBaseItemFeature(22,1,0));
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         level = Note.ReadNumber("Level", note);
         vip = Note.ReadBoolean("VIP", note);
         soulbound = Note.ReadBoolean("Soulbound", note);
         sex = Note.ReadNumber("Sex", note, 2);
      }
   }
}