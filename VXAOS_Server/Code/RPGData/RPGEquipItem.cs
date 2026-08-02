namespace VXAOS_Server.RPGData {
	public class RPGEquipItem:RPGBaseItem, IRPGItem {
		public int price = 0;
		public int etype_id = 0;
		public int[] @params = [0,0,0,0,0,0,0,0];
		public int level = 0;
		public bool vip = false;
		public bool soulbound = false;
		public bool IsSoulbound() {
			return soulbound;
		}
		public int Price() {
			return price;
		}
	}
	internal interface IRPGItem {
		internal bool IsSoulbound();
		internal int Price();
	}
}
