namespace VXAOS_Server.RPGData {
	public class RPGAudioFile {
		public string name = "";
		public int volume = 100;
		public int pitch = 100;
		public RPGAudioFile(string _name = "", int _volume = 100, int _pitch = 100) {
			name = _name;
			volume = _volume;
			pitch = _pitch;
		}
	}
	public class RPGBGM:RPGAudioFile {
		public RPGBGM(string _name = "", int _volume = 100, int _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGBGS:RPGAudioFile {
		public RPGBGS(string _name = "",int _volume = 100, int _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGME:RPGAudioFile {
		public RPGME(string _name = "",int _volume = 100, int _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}

	public class RPGSE:RPGAudioFile {
		public RPGSE(string _name = "",int _volume = 100, int _pitch = 100)
				: base(_name,_volume,_pitch) { }
	}
}
