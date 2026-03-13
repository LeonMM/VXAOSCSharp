using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGEvent {
		public double id = 0;
		public string name = "";
		public double x = 1;
		public double y = 1;
		[JsonConverter(typeof(ListConverter<RPGEventPage>))] 
		public List<RPGEventPage> pages = new List<RPGEventPage>() { new RPGEventPage() };
		public RPGEvent(double _x,double _y) {
			this.x = _x;
			this.y = _y;
		}
	}
}
