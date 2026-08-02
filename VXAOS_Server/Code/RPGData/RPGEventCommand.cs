using Newtonsoft.Json.Linq;
namespace VXAOS_Server.RPGData {
	public class RPGEventCommand {
		public int code = 0;
		public int indent = 0;
		public JArray parameters = new JArray();
		public RPGEventCommand(int _code = 0, int _indent = 0) {
			code = _code;
			indent = _indent;
		}
		public void SetParameters(JArray _parameters) {
			parameters = _parameters;
		}
	}
}
