using Newtonsoft.Json;
using System.Runtime.Serialization;
using Math = System.Math;

namespace VXAOS_Server.RPGData {
	public class RPGClass:RPGBaseItem {
		public int[] exp_params = [30,20,30,30];
		public Table @params = new Table(8,100);
		public List<List<Tuple<string, int>>> graphics = new();
      [JsonConverter(typeof(ListConverter<RPGClassLearning>))]
		public List<RPGClassLearning> learnings = new List<RPGClassLearning>();
		public RPGClass() {
			for(int i = 1; i <= 99; i++) {
				@params[0,i] = 400 + i * 50;
				@params[1,i] = 80 + i * 10;
				@params[2,i] = 15 + i * 5 / 4;
				@params[3,i] = 15 + i * 5 / 4;
				@params[4,i] = 15 + i * 5 / 4;
				@params[5,i] = 15 + i * 5 / 4;
				@params[6,i] = 30 + i * 5 / 2;
				@params[7,i] = 30 + i * 5 / 2;
			}
			features.Add(new RPGBaseItemFeature(23,0,1));
			features.Add(new RPGBaseItemFeature(22,0,0.95f));
			features.Add(new RPGBaseItemFeature(22,1,0.05f));
			features.Add(new RPGBaseItemFeature(22,2,0.04f));
			features.Add(new RPGBaseItemFeature(41,1));
			features.Add(new RPGBaseItemFeature(51,1));
			features.Add(new RPGBaseItemFeature(52,1));

		}
		public int Exp_For_Level(double lv) {
			double basis = exp_params[0];
			double extra = exp_params[1];
			double acc_a = exp_params[2];
			double acc_b = exp_params[3];
			return (int)(basis * Math.Pow((lv - 1),(0.9f + acc_a / 250f)) * lv * (lv + 1) /
				(6 + Math.Pow(lv,2) / 50f / acc_b) + (lv - 1) * extra);
		}
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
			graphics = Note.ReadGraphics(note);
      }
   }
}
