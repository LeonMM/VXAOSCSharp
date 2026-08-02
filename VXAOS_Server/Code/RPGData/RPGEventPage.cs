using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGEventPage {
		public RPGEventPageCondition condition = new RPGEventPageCondition();
		public RPGEventPageGraphic graphic = new RPGEventPageGraphic();
		public int move_type = 0;
		public int move_speed = 3;
		public int move_frequency = 3;
		public RPGMoveRoute move_route = new RPGMoveRoute();
		public bool walk_anime = true;
		public bool step_anime = false;
		public bool direction_fix = false;
		public bool through = false;
		public int priority_type = 0;
		public int trigger = 0;
		[JsonConverter(typeof(ListConverter<RPGEventCommand>))] 
		public List<RPGEventCommand> list = new List<RPGEventCommand>() { new RPGEventCommand() };

	}
}
