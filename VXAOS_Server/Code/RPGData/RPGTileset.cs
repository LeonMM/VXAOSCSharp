namespace VXAOS_Server.RPGData {
	public class RPGTileset {
		public int id = 0;
		public int mode = 1;
		public string name = "";
		public string[] tileset_names = ["","","","","","","","",""];
		public Table flags = new Table(8192);
		public string note = "";

		public RPGTileset() {
			flags[0] = 0x0010;
			for(int i = 2048; i <= 2815; i++) {
				flags[i] = 0x000F;
			}
			for(int i = 4352; i <= 8191; i++) {
				flags[i] = 0x000F;
			}
		}
	}
}
