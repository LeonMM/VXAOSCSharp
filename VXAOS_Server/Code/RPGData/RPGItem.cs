namespace VXAOS_Server.RPGData {
	public class RPGItem:RPGUsableItem {
		public double itype_id = 1;
		public double price = 0;
		public bool consumable = true;
		public RPGItem() {
			this.scope = 7;
		}
	}
}
