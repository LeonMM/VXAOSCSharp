using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGBaseItem {
		public double id = 0;
		public string name = "";
		public double icon_index = 0;
		public string description = "";
		[JsonConverter(typeof(ListConverter<RPGBaseItemFeature>))]
		public List<RPGBaseItemFeature> features = new List<RPGBaseItemFeature>();
		public string note = "";

	}
}
