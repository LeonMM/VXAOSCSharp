using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGBaseItem {
		public int id = 0;
		public string name = "";
		public int icon_index = 0;
		public string description = "";
		[JsonConverter(typeof(ListConverter<RPGBaseItemFeature>))]
		public List<RPGBaseItemFeature> features = new List<RPGBaseItemFeature>();
		public string note = "";

	}
}
