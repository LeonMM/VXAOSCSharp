using Newtonsoft.Json;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGAnimation {
		public double id = 0;
		public string name = "";
		public string animation1_name = "";
		public double animation1_hue = 0;
		public string animation2_name = "";
		public double animation2_hue = 0;
		public double position = 1;
		public double frame_max = 1;
		[JsonConverter(typeof(ListConverter<RPGAnimationFrame>))]
		public List<RPGAnimationFrame> frames = new List<RPGAnimationFrame>() { new() };
		[JsonConverter(typeof(ListConverter<RPGAnimationTiming>))]
		public List<RPGAnimationTiming> timings = new List<RPGAnimationTiming>();

	}
}
