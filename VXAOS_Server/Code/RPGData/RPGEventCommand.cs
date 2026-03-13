using Newtonsoft.Json.Linq;
namespace VXAOS_Server.RPGData {
	public class RPGEventCommand {
		public double code = 0;
		public double indent = 0;
		public JArray parameters = new JArray();
		public RPGEventCommand(double _code = 0,double _indent = 0) {
			this.code = _code;
			this.indent = _indent;
		}
		public void SetParameters(JArray _parameters) {
			this.parameters = _parameters;
		}
	}
}
