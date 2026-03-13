using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGMap {
		public string display_name = "";
		public double tileset_id = 1;
		public double width = 1;
		public double height = 1;
		public double scroll_type = 0;
		public bool specify_battleback = false;
		public string battleback1_name = "";
		public string battleback2_name = "";
		public bool autoplay_bgm = false;
		public RPGBGM bgm = new RPGBGM();
		public bool autoplay_bgs = false;
		public RPGBGS bgs = new RPGBGS();
		public bool disable_dashing = false;
		[JsonConverter(typeof(ListConverter<RPGMapEncounter>))]
		public List<RPGMapEncounter> encounter_list = new List<RPGMapEncounter>();
		public double encounter_step = 30;
		public string parallax_name = "";
		public bool parallax_loop_x = false;
		public bool parallax_loop_y = false;
		public double parallax_sx = 0;
		public double parallax_sy = 0;
		public bool parallax_show = false;
		public string note = "";
		public Table data;
		public Dictionary<int, RPGEvent> events = new Dictionary<int, RPGEvent>() { };
		public RPGMap(double _width,double _height) {
			this.width = _width;
			this.height = _height;
			this.data = new Table((int)width,(int)height,4);
		}
	}
}
