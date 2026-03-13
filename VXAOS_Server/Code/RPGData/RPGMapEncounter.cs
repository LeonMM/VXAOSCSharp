using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGMapEncounter {
		public double troop_id = 1;
		public double weight = 10;
		[JsonConverter(typeof(ListConverter<double>))]
		public List<double> region_set = new List<double>();
	}
}
