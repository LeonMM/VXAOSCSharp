using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGEvent {
		public int id = 0;
		public string name = "";
		public int x = 1;
		public int y = 1;
		[JsonConverter(typeof(ListConverter<RPGEventPage>))] 
		public List<RPGEventPage> pages = new List<RPGEventPage>() { new RPGEventPage() };
		public RPGEvent(int _x, int _y) {
			x = _x;
			y = _y;
		}
	}
}
