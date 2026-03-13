using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGUsableItem:RPGBaseItem {
		public double scope = 0;
		public double occasion = 0;
		public double speed = 0;
		public double success_rate = 100;
		public double repeats = 1;
		public double tp_gain = 0;
		public double hit_type = 0;
		public double animation_id = 0;
		public RPGUsableItemDamage damage = new RPGUsableItemDamage();
		[JsonConverter(typeof(ListConverter<RPGUsableItemEffect>))]
		public List<RPGUsableItemEffect> effects = new List<RPGUsableItemEffect>();

	}
}
