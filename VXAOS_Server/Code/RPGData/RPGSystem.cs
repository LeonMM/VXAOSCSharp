using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGSystem {
		public string game_title = "";
		public int version_id = 0;
		public bool japanese = false;
		public int[] party_members = [1];
		public string currency_unit = "";
		public string[] elements = ["",""];
		public string[] skill_types = ["",""];
		public string[] weapon_types = ["",""];
		public string[] armor_types = ["",""];
		public string[] switches = ["",""];
		public string[] variables = ["",""];
		public RPGSystemVehicle boat = new RPGSystemVehicle();
		public RPGSystemVehicle ship = new RPGSystemVehicle();
		public RPGSystemVehicle airship = new RPGSystemVehicle();
		public string title1_name = "";
		public string title2_name = "";
		public bool opt_draw_title = true;
		public bool opt_use_midi = false;
		public bool opt_transparent = false;
		public bool opt_followers = true;
		public bool opt_slip_death = false;
		public bool opt_floor_death = false;
		public bool opt_display_tp = true;
		public bool opt_extra_exp = false;
		public Tone window_tone = new Tone(0,0,0);
		public RPGBGM title_bgm = new();
		public RPGBGM battle_bgm = new();
		public RPGME battle_end_me = new();
		public RPGME gameover_me = new();
		[JsonConverter(typeof(ListConverter<RPGSE>))]
		public List<RPGSE> sounds = Enumerable.Repeat(new RPGSE(),24).ToList();
		[JsonConverter(typeof(ListConverter<RPGSystemTestBattler>))]
		public List<RPGSystemTestBattler> test_battlers = new List<RPGSystemTestBattler>();
		public int test_troop_id = 1;
		public int start_map_id = 1;
		public int start_x = 0;
		public int start_y = 0;
		public RPGSystemTerms terms = new();
		public string battleback1_name = "";
		public string battleback2_name = "";
		public string battler_name = "";
		public int battler_hue = 0;
		public int edit_map_id = 1;
	}
}
