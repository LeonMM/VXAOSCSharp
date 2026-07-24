using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace VXAOS_Server.RPGData {
	public class RPGEnemy:RPGBaseItem {
		public string battler_name = "";
		public double battler_hue = 0;
		public double[] @params = [100,0,10,10,10,10,10,10];
		public double exp = 0;
		public double gold = 0;
		[JsonConverter(typeof(ListConverter<RPGEnemyDropItem>))]
		public List<RPGEnemyDropItem> drop_items = new List<RPGEnemyDropItem>() { new(),new(),new() };
		[JsonConverter(typeof(ListConverter<RPGEnemyAction>))]
		public List<RPGEnemyAction> actions = new List<RPGEnemyAction>() { new() };
		public int revive_time = 0;
		public int disable_switch_id = 0;
		public int disable_variable_id = 0;
		public int ani_index = 0;
		public bool escape = false;
		public int sight = 3;

		public RPGEnemy() {
			this.features.Add(new RPGBaseItemFeature(22,0,0.95f));
			this.features.Add(new RPGBaseItemFeature(22,1,0.05f));
			this.features.Add(new RPGBaseItemFeature(31,1,0));
		}
		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context) {
			sight = Note.ReadNumber("Sight", note);
         int reviveTime = Note.ReadNumber("ReviveTime", note);
         revive_time = reviveTime > 0 ? reviveTime : ServerConfig.ReviveTime;
         disable_switch_id = Note.ReadNumber("SwitchID", note);
         disable_variable_id = Note.ReadNumber("VariableID", note);
         ani_index = Note.ReadNumber("AniIndex", note, 8);
         escape = Note.ReadBoolean("Escape", note);
      }
   }
}
