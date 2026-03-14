
using Newtonsoft.Json;
using VXAOS_Server.RPGData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
	public class DataManager {
		public List<RPGActor> data_actors = new List<RPGActor>();
		public List<RPGClass> data_classes = new List<RPGClass>();
		public List<RPGSkill> data_skills = new List<RPGSkill>();
		public List<RPGItem> data_items = new List<RPGItem>();
		public List<RPGWeapon> data_weapons = new List<RPGWeapon>();
		public List<RPGArmor> data_armors = new List<RPGArmor>();
		public List<RPGEnemy> data_enemies = new List<RPGEnemy>();
		public List<RPGTroop> data_troops = new List<RPGTroop>();
		public List<RPGState> data_states = new List<RPGState>();
		public List<RPGAnimation> data_animations = new List<RPGAnimation>();
		public List<RPGTileset> data_tilesets = new List<RPGTileset>();
		public List<RPGCommonEvent> data_common_events = new List<RPGCommonEvent>();
		public RPGSystem data_system = new RPGSystem();
		public Dictionary<int,RPGMapInfo> data_mapinfos = new Dictionary<int,RPGMapInfo> { };
		public Dictionary<int,RPGMap> data_maps = new Dictionary<int,RPGMap>() { };

		public DataManager(string projectRootFolder) {
         if (Directory.Exists(projectRootFolder)) {
				List<string> jsons = new List<string>();
				string error = "";
				ProcessStartInfo startInfo = new ProcessStartInfo {
					FileName = "ruby",
					Arguments = $"Ruby/import.rb {Convert.ToBase64String(Encoding.UTF8.GetBytes(projectRootFolder))}",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8,
					CreateNoWindow = true
				};
				using(Process process = new Process()) {
					process.EnableRaisingEvents = true;
					process.StartInfo = startInfo;
					process.Start();
					string outputStd = process.StandardOutput.ReadToEnd();
					string errorStd = process.StandardError.ReadToEnd();
					process.WaitForExit();

					if(!string.IsNullOrEmpty(outputStd)) {
						var outputLinesArray = outputStd.Split(new[] { Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
						jsons.AddRange(outputLinesArray);
					}
					if(jsons.Count > 0) {
						if(outputStd.Contains("error1",StringComparison.OrdinalIgnoreCase)) {
							error = "Wrong folder/Missing Files.";
						} else if(outputStd.Contains("error2",StringComparison.OrdinalIgnoreCase)) {
							error = "The directory does not exist.";
						}
					}
					if(!string.IsNullOrEmpty(errorStd)) {
						error = errorStd;
					}
				}
				if(!string.IsNullOrEmpty(error)) {
					Console.WriteLine($"{error}");
					Console.WriteLine("Aperte qualquer tecla para sair");
               Console.ReadKey();
					Environment.Exit(0);
					return;
				}
				if(jsons.Count <= 0) {
					return;
				}
				JsonSerializerSettings settings = new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore//, // Ignore null values
																											//DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, // Populate default values
																											//Converters = new List<JsonConverter> { new TableConverter() } // Add custom converter
				};
				data_actors = JsonConvert.DeserializeObject<List<RPGActor>>(jsons[0].Remove(1,5),settings)!;
				data_classes = JsonConvert.DeserializeObject<List<RPGClass>>(jsons[1].Remove(1,5),settings)!;
				data_skills = JsonConvert.DeserializeObject<List<RPGSkill>>(jsons[2].Remove(1,5),settings)!;
				data_items = JsonConvert.DeserializeObject<List<RPGItem>>(jsons[3].Remove(1,5),settings)!;
				data_weapons = JsonConvert.DeserializeObject<List<RPGWeapon>>(jsons[4].Remove(1,5),settings)!;
				data_armors = JsonConvert.DeserializeObject<List<RPGArmor>>(jsons[5].Remove(1,5),settings)!;
				data_enemies = JsonConvert.DeserializeObject<List<RPGEnemy>>(jsons[6].Remove(1,5),settings)!;
				data_troops = JsonConvert.DeserializeObject<List<RPGTroop>>(jsons[7].Remove(1,5),settings)!;
				data_states = JsonConvert.DeserializeObject<List<RPGState>>(jsons[8].Remove(1,5),settings)!;
				data_tilesets = JsonConvert.DeserializeObject<List<RPGTileset>>(jsons[9].Remove(1,5),settings)!;
				data_common_events = JsonConvert.DeserializeObject<List<RPGCommonEvent>>(jsons[10].Remove(1,5),settings)!;
				data_system = JsonConvert.DeserializeObject<RPGSystem>(jsons[11],settings)!;
				data_mapinfos = JsonConvert.DeserializeObject<Dictionary<int,RPGMapInfo>>(jsons[12],settings)!;
				int counter = 13;
				foreach(int id in data_mapinfos.Keys) {
					data_maps.Add(id,JsonConvert.DeserializeObject<RPGMap>(jsons[counter],settings));
					counter++;
				}
				Console.WriteLine("Dados Carregados com sucesso");
			}
		}
	}
}
