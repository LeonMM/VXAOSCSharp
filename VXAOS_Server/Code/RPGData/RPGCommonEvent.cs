using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGCommonEvent {
		public int id = 0;
		public string name = "";
		public int trigger = 0;
		public int switch_id = 1;
		[JsonConverter(typeof(ListConverter<RPGEventCommand>))] 
		public List<RPGEventCommand> list = [new()];
		public bool IsAutorun() {
			return trigger == 1;
		}
		public bool IsParallel() {
			return trigger == 2;
		}
	}
}
