using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGTroop {
		public int id = 0;
		public string name = "";
		[JsonConverter(typeof(ListConverter<RPGTroopMember>))] 
		public List<RPGTroopMember> members = new List<RPGTroopMember>();
		[JsonConverter(typeof(ListConverter<RPGTroopPage>))]
		public List<RPGTroopPage> pages = new List<RPGTroopPage>() { new() };
	}
}
