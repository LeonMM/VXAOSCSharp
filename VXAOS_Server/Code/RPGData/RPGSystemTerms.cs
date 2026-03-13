using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
namespace VXAOS_Server.RPGData {
	public class RPGSystemTerms {
		[JsonConverter(typeof(ListConverter<string>))]
		public List<string> basic = Enumerable.Repeat("",8).ToList();
		[JsonConverter(typeof(ListConverter<string>))]
		public List<string> paramss = Enumerable.Repeat("",8).ToList();
		[JsonConverter(typeof(ListConverter<string>))]
		public List<string> etypes = Enumerable.Repeat("",5).ToList();
		[JsonConverter(typeof(ListConverter<string>))]
		public List<string> commands = Enumerable.Repeat("",23).ToList();
	}
}
