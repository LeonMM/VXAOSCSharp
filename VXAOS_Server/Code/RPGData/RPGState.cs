namespace VXAOS_Server.RPGData {
	public class RPGState:RPGBaseItem {
		public double restriction = 0;
		public double priority = 50;
		public bool remove_at_battle_end = false;
		public bool remove_by_restriction = false;
		public double auto_removal_timing = 0;
		public double min_turns = 1;
		public double max_turns = 1;
		public bool remove_by_damage = false;
		public double chance_by_damage = 100;
		public bool remove_by_walking = false;
		public double steps_to_remove = 100;
		public string message1 = "";
		public string message2 = "";
		public string message3 = "";
		public string message4 = "";
	}
}
