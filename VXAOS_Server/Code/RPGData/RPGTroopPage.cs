using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGTroopPage {
		public RPGTroopPageCondition condition = new();
		public int span = 0;
		[JsonConverter(typeof(ListConverter<RPGEventCommand>))]
		public List<RPGEventCommand> list = new List<RPGEventCommand>() { new() };

	}
}
