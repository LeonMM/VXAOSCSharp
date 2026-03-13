namespace VXAOS_Server.RPGData {
	public class RPGWeapon:RPGEquipItem {
		public double wtype_id = 0;
		public double animation_id = 0;
		public RPGWeapon() {
			this.features.Add(new RPGBaseItemFeature(31,1,0));
			this.features.Add(new RPGBaseItemFeature(22,0,0));
		}
	}
}
