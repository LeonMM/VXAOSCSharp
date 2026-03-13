using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGEnemy:RPGBaseItem {
		public string battler_name = "";
		public double battler_hue = 0;
		public double[] paramss = [100,0,10,10,10,10,10,10];
		public double exp = 0;
		public double gold = 0;
		[JsonConverter(typeof(ListConverter<RPGEnemyDropItem>))]
		public List<RPGEnemyDropItem> drop_items = new List<RPGEnemyDropItem>() { new(),new(),new() };
		[JsonConverter(typeof(ListConverter<RPGEnemyAction>))]
		public List<RPGEnemyAction> actions = new List<RPGEnemyAction>() { new() };


		public RPGEnemy() {
			this.features.Add(new RPGBaseItemFeature(22,0,0.95f));
			this.features.Add(new RPGBaseItemFeature(22,1,0.05f));
			this.features.Add(new RPGBaseItemFeature(31,1,0));
		}
	}
}
