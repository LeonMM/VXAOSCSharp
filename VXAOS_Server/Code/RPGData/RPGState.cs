using System.Runtime.Serialization;

namespace VXAOS_Server.RPGData {
	public class RPGState:RPGBaseItem {
		public int restriction = 0;
		public int priority = 50;
		public bool remove_at_battle_end = false;
		public bool remove_by_restriction = false;
		public int auto_removal_timing = 0;
		public int min_turns = 1;
		public int max_turns = 1;
		public bool remove_by_damage = false;
		public int chance_by_damage = 100;
		public bool remove_by_walking = false;
		public int steps_to_remove = 100;
		public string message1 = "";
		public string message2 = "";
		public string message3 = "";
		public string message4 = "";
		public bool save = false;
		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context) {
			save = Note.ReadBoolean("Save", note);
		}
	}
}
