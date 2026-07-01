global using static VXAOS_Server.DataManager;
using Newtonsoft.Json;
using VXAOS_Server.RPGData;
using System.Diagnostics;
using System.Text;

namespace VXAOS_Server {
	public static class DataManager {
		public static List<RPGActor> DataActors = new();
		public static List<RPGClass> DataClasses = new();
		public static List<RPGSkill> DataSkills = new();
		public static List<RPGItem> DataItems = new();
		public static List<RPGWeapon> DataWeapons = new();
		public static List<RPGArmor> DataArmors = new();
		public static List<RPGEnemy> DataEnemies = new();
		public static List<RPGState> DataStates = new();
		public static List<RPGTileset> DataTilesets = new();
		public static List<RPGAnimation> DataAnimations = new();
		public static List<RPGCommonEvent> DataCommonEvents = new();
		public static RPGSystem DataSystem = new();
		public static Dictionary<int,RPGMapInfo> DataMapInfos = new();
		public static Dictionary<int,RPGMap> DataMaps = new();
		public static string Motd = "";
		public static void Load(string projectRootFolder) {
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
				bool _success = true;
				try { 
					Console.WriteLine("Carregando Configs...");
					Configs = ModuleLoader.Load(jsons[0]);
					Console.WriteLine("Carregando Vocab...");
					Vocab = VocabLoader.Load("vocab.ini");
					Console.WriteLine("Carregando Quests...");
					Quests = ModuleLoader.Load(jsons[1]);
					Console.WriteLine("Carregando Heróis...");
					DataActors = JsonConvert.DeserializeObject<List<RPGActor>>(jsons[2].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Classes...");
					DataClasses = JsonConvert.DeserializeObject<List<RPGClass>>(jsons[3].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Habilidades...");
					DataSkills = JsonConvert.DeserializeObject<List<RPGSkill>>(jsons[4].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Items...");
					DataItems = JsonConvert.DeserializeObject<List<RPGItem>>(jsons[5].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Armas...");
					DataWeapons = JsonConvert.DeserializeObject<List<RPGWeapon>>(jsons[6].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Armaduras...");
					DataArmors = JsonConvert.DeserializeObject<List<RPGArmor>>(jsons[7].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Inimigos...");
					DataEnemies = JsonConvert.DeserializeObject<List<RPGEnemy>>(jsons[8].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Estados...");
					DataStates = JsonConvert.DeserializeObject<List<RPGState>>(jsons[9].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Animações...");
					DataStates = JsonConvert.DeserializeObject<List<RPGState>>(jsons[10].Remove(1, 5), settings)!;
					Console.WriteLine("Carregando Tilesets...");
					DataTilesets = JsonConvert.DeserializeObject<List<RPGTileset>>(jsons[11].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Eventos Comuns...");
					DataCommonEvents = JsonConvert.DeserializeObject<List<RPGCommonEvent>>(jsons[12].Remove(1,5),settings)!;
					Console.WriteLine("Carregando Sistema...");
					DataSystem = JsonConvert.DeserializeObject<RPGSystem>(jsons[13],settings)!;
					if (File.Exists("Data/switches.json")) {
						Console.WriteLine("Carregando switches globais...");
						var data = JsonConvert.DeserializeObject<List<bool>>(File.ReadAllText("Data/switches.json"))!;
						Network.Switches = new GameGlobalSwitches(data);
					}
					Console.WriteLine("Carregando Mapas...");
					DataMapInfos = JsonConvert.DeserializeObject<Dictionary<int,RPGMapInfo>>(jsons[14],settings)!;
					int counter = 15;
					foreach(int id in DataMapInfos.Keys) {
						DataMaps.Add(id,JsonConvert.DeserializeObject<RPGMap>(jsons[counter],settings));
						Network.Maps.TryAdd(id, new GameMap(id, DataMaps[id]));
						counter++;
					}
            }catch (Exception ex) {
               CSExt.WriteColor("Erro ao carregar dados básicos.", ConsoleColor.Red);
					CSExt.WriteColor($"Erro: {ex}", ConsoleColor.Red);
            }
            LoadMotd();
            Console.WriteLine("Carregando lista de banidos...");
				try {
					_ = Network.DB.LoadBanList();
				} catch(Exception ex) {
					CSExt.WriteColor("O banco de dados SQL está off-line!", ConsoleColor.Red);
					CSExt.WriteColor("A lista de banidos não foi carregada!", ConsoleColor.Red);
					CSExt.WriteColor($"Erro: {ex}", ConsoleColor.Red);
				}
            Console.WriteLine("Carregando guildas...");
				try {
               _ = Network.DB.LoadGuilds();
            } catch {
               CSExt.WriteColor("As guildas não foram carregadas!", ConsoleColor.Red);
				}
				if(_success)
					Console.WriteLine("Dados Carregados com sucesso");
         }
		}
      public static void LoadMotd() {
         Console.WriteLine("Carregando mensagem do dia...");
         Motd = File.ReadAllText("motd.txt", Encoding.UTF8);
      }
   }
}
