namespace VXAOS_Server.RPGData {
	public class RPGActor:RPGBaseItem {
		public string nickname = "";
		public int class_id = 1;
		public int initial_level = 1;
		public int max_level = 99;
		public string character_name = "";
		public int character_index = 0;
		public string face_name = "";
		public int face_index = 0;
		public int[] equips = [0,0,0,0,0];
	}
}
