using Newtonsoft.Json;

namespace VXAOS_Server.RPGData {
	public class RPGAnimation {
		public int id = 0;
		public string name = "";
		public string animation1_name = "";
		public int animation1_hue = 0;
		public string animation2_name = "";
		public int animation2_hue = 0;
		public int position = 1;
		public int frame_max = 1;
		[JsonConverter(typeof(ListConverter<RPGAnimationFrame>))]
		public List<RPGAnimationFrame> frames = new List<RPGAnimationFrame>() { new() };
		[JsonConverter(typeof(ListConverter<RPGAnimationTiming>))]
		public List<RPGAnimationTiming> timings = new List<RPGAnimationTiming>();

	}
}
