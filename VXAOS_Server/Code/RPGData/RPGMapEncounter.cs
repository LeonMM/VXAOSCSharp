using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGMapEncounter {
		public int troop_id = 1;
		public int weight = 10;
		[JsonConverter(typeof(ListConverter<int>))]
		public List<int> region_set = new List<int>();
	}
}
