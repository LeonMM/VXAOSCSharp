namespace VXAOS_Server.RPGData {
	public class RPGEquipItem:RPGBaseItem, IRPGItem {
		public double price = 0;
		public double etype_id = 0;
		public double[] @params = [0,0,0,0,0,0,0,0];
		public int level = 0;
		public bool vip = false;
		public bool soulbound = false;
		public bool IsSoulbound() {
			return soulbound;
		}
		public int Price() {
			return (int)price;
		}
	}
	internal interface IRPGItem {
		internal bool IsSoulbound();
		internal int Price();
	}
}
