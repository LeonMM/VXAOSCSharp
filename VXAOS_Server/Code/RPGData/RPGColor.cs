using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VXAOS_Server.RPGData {
	public class RPGColor {
		public double red { get; set; }
		public double green { get; set; }
		public double blue { get; set; }
		public double alpha { get; set; }

		public RPGColor(double _red,double _green,double _blue,double _alpha = 255) {
			red = _red;
			green = _green;
			blue = _blue;
			alpha = _alpha;
		}

		public Color ToDrawingColor() {
			return Color.FromArgb((byte)alpha,(byte)red,(byte)green,(byte)blue);
		}

		public static RPGColor FromDrawingColor(Color color) {
			return new RPGColor(color.R,color.G,color.B,color.A);
		}
	}
	public class Tone {
		private double _red;
		private double _green;
		private double _blue;
		private double _gray;
		public double red {
			get => _red;
			set => _red = Math.Clamp(value,-255,255);
		}

		public double green {
			get => _green;
			set => _green = Math.Clamp(value,-255,255);
		}

		public double blue {
			get => _blue;
			set => _blue = Math.Clamp(value,-255,255);
		}
		public double gray {
			get => _gray;
			set => _gray = Math.Clamp(value,0,255);
		}

		public Tone(double _red = 0,double _green = 0,double _blue = 0,double _gray = 0) {
			red = _red;
			green = _green;
			blue = _blue;
			gray = _gray;
		}

		public void Set(double _red,double _green,double _blue,double _gray = 0) {
			red = _red;
			green = _green;
			blue = _blue;
			gray = _gray;
		}

		public void Adjust(double redOffset,double greenOffset,double blueOffset,double grayOffset = 0) {
			red += redOffset;
			green += greenOffset;
			blue += blueOffset;
			gray += grayOffset;
		}
		public override string ToString() {
			return $"Tone(red: {red}, green: {green}, blue: {blue}, gray: {gray})";
		}
	}
}
