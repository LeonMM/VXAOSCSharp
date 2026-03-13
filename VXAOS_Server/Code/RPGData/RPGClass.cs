using Newtonsoft.Json;
using Math=System.Math;
using System.Collections.Generic;

namespace VXAOS_Server.RPGData {
	public class RPGClass:RPGBaseItem {
		public double[] exp_params = [30,20,30,30];
		public Table paramss = new Table(8,100);
		[JsonConverter(typeof(ListConverter<RPGClassLearning>))]
		public List<RPGClassLearning> learnings = new List<RPGClassLearning>();
		public RPGClass() {
			for(int i = 1; i <= 99; i++) {
				this.paramss[0,i] = 400 + i * 50;
				this.paramss[1,i] = 80 + i * 10;
				this.paramss[2,i] = 15 + i * 5 / 4;
				this.paramss[3,i] = 15 + i * 5 / 4;
				this.paramss[4,i] = 15 + i * 5 / 4;
				this.paramss[5,i] = 15 + i * 5 / 4;
				this.paramss[6,i] = 30 + i * 5 / 2;
				this.paramss[7,i] = 30 + i * 5 / 2;
			}
			this.features.Add(new RPGBaseItemFeature(23,0,1));
			this.features.Add(new RPGBaseItemFeature(22,0,0.95f));
			this.features.Add(new RPGBaseItemFeature(22,1,0.05f));
			this.features.Add(new RPGBaseItemFeature(22,2,0.04f));
			this.features.Add(new RPGBaseItemFeature(41,1));
			this.features.Add(new RPGBaseItemFeature(51,1));
			this.features.Add(new RPGBaseItemFeature(52,1));

		}
		public double Exp_For_Level(double lv) {
			double basis = this.exp_params[0];
			double extra = this.exp_params[1];
			double acc_a = this.exp_params[2];
			double acc_b = this.exp_params[3];
			return (double)(basis * Math.Pow((lv - 1),(0.9f + acc_a / 250f)) * lv * (lv + 1) /
				(6 + Math.Pow(lv,2) / 50f / acc_b) + (lv - 1) * extra);
		}
	}
}
