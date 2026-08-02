using Newtonsoft.Json.Linq;
namespace VXAOS_Server.RPGData {
	public class RPGMoveCommand {
		public int code = 0;
		public JArray parameters = new JArray();
		public RPGMoveCommand(int _code = 0) {
			code = _code;
		}

	}
}
