using Newtonsoft.Json.Linq;
namespace VXAOS_Server.RPGData {
	public class RPGMoveCommand {
		public double code = 0;
		public JArray parameters = new JArray();
		public RPGMoveCommand(double _code = 0) {
			this.code = _code;
		}

	}
}
