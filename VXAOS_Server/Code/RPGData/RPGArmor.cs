namespace VXAOS_Server.RPGData {
	public class RPGArmor:RPGEquipItem {
		public double atype_id = 0;
		public RPGArmor() {
			this.etype_id = 1;
			this.features.Add(new RPGBaseItemFeature(22,1,0));
		}
	}
}