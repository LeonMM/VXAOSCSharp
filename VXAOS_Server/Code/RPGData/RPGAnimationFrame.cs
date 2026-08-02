using Math=System.Math;

namespace VXAOS_Server.RPGData {
	public class RPGAnimationFrame {
		public int cell_max = 0;
		public Table cell_data = new Table(1,8);
		public RPGAnimationFrame(int _cell_max = 0) {
			cell_max = _cell_max;
			cell_data.Resize(Math.Max(_cell_max,1),8);
		}
	}
}
