namespace VXAOS_Server.RPGData {
	public class RPGAudioFile {
		public string name = "";
		public double volume = 100;
		public double pitch = 100;
		public RPGAudioFile(string _name = "",double _volume = 100,double _pitch = 100) {
			this.name = _name;
			this.volume = _volume;
			this.pitch = _pitch;
		}
	}
	public class RPGBGM:RPGAudioFile {
		public RPGBGM(string _name = "",double _volume = 100,double _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGBGS:RPGAudioFile {
		public RPGBGS(string _name = "",double _volume = 100,double _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGME:RPGAudioFile {
		public RPGME(string _name = "",double _volume = 100,double _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGSE:RPGAudioFile {
		public RPGSE(string _name = "",double _volume = 100,double _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}
}
