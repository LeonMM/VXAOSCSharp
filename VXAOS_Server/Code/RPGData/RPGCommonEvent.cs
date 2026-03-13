using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGCommonEvent {
		public double id = 0;
		public string name = "";
		public double trigger = 0;
		public double switch_id = 1;
		[JsonConverter(typeof(ListConverter<RPGEventCommand>))] 
		public List<RPGEventCommand> list = new List<RPGEventCommand>() { new() };
	}
}
