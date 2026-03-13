using Math=System.Math;

namespace VXAOS_Server.RPGData {
	public class RPGAnimationFrame {
		public double cell_max = 0;
		public Table cell_data = new Table(1,8);
		public RPGAnimationFrame(double _cell_max = 0) {
			this.cell_max = _cell_max;
			this.cell_data.Resize(Math.Max((int)_cell_max,1),8);
		}
	}
}
