using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGMoveRoute {
		public bool repeat = true;
		public bool skippable = false;
		public bool wait = false;
		[JsonConverter(typeof(ListConverter<RPGMoveCommand>))]
		public List<RPGMoveCommand> list = new List<RPGMoveCommand>() { new RPGMoveCommand() };
	}
}
