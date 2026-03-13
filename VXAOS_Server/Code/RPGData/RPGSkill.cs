namespace VXAOS_Server.RPGData {
	public class RPGSkill:RPGUsableItem {
		public double stype_id = 1;
		public double mp_cost = 0;
		public double tp_cost = 0;
		public string message1 = "";
		public string message2 = "";
		public double required_wtype_id1 = 0;
		public double required_wtype_id2 = 0;
		public RPGSkill() {
			this.scope = 1;
		}
	}
}
