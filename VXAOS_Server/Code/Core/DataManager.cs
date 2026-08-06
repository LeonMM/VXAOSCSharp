global using static VXAOS_Server.DataManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
	public static class DataManager {
		public static List<RPGActor?> DataActors = [];
		public static List<RPGClass?> DataClasses = [];
		public static List<RPGSkill?> DataSkills = [];
		public static List<RPGItem?> DataItems = [];
		public static List<RPGWeapon?> DataWeapons = [];
		public static List<RPGArmor?> DataArmors = [];
		public static List<RPGEnemy?> DataEnemies = [];
		public static List<RPGState?> DataStates = [];
		public static List<RPGTileset?> DataTilesets = [];
		public static List<RPGAnimation?> DataAnimations = [];
		public static List<RPGCommonEvent?> DataCommonEvents = [];
		public static RPGSystem DataSystem = new();
		public static Dictionary<int,RPGMapInfo> DataMapInfos = [];
		public static Dictionary<int,RPGMap> DataMaps = [];
		public static string Motd = "";
		public static void LoadData(string projectRootFolder) {
         if (Directory.Exists(projectRootFolder)) {
				List<string> jsons = [];
				string error = "";
				ProcessStartInfo startInfo = new() {
					FileName = "ruby",
					Arguments = $"Ruby/import.rb {Convert.ToBase64String(Encoding.UTF8.GetBytes(projectRootFolder))}",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8,
					CreateNoWindow = true
				};
				using(Process process = new ()) {
					process.EnableRaisingEvents = true;
					process.StartInfo = startInfo;
					process.Start();
					string outputStd = process.StandardOutput.ReadToEnd();
					string errorStd = process.StandardError.ReadToEnd();
					process.WaitForExit();

					if(!string.IsNullOrEmpty(outputStd)) {
						var outputLinesArray = outputStd.Split([Environment.NewLine],StringSplitOptions.RemoveEmptyEntries);
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
				JsonSerializerSettings settings = new() {
					NullValueHandling = NullValueHandling.Ignore//, 
					//DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
					//Converters = new List<JsonConverter> { new TableConverter() }
				};
				bool _success = true;
				try { 
					Console.WriteLine("Carregando Configs...");
					Configs = ModuleLoader.Load(jsons[0]);
					Console.WriteLine(Configs.RangeWeapons.TryGetValue(49, out dynamic rng2));
					Console.WriteLine(rng2.Get<int>("MpCost"));
					Console.WriteLine(Configs.RangeWeapons.ContainsKey(31));
					Console.WriteLine(Configs.AttackTime);
					Console.WriteLine("Carregando Vocab...");
					Vocab = VocabLoader.Load("vocab.ini");
					Console.WriteLine("Carregando Quests...");
					MQuests = ModuleLoader.Load(jsons[1]);
               Console.WriteLine("Carregando Heróis...");
					DataActors = JsonConvert.DeserializeObject<List<RPGActor>>(jsons[2].Remove(1,5),settings)!;
					DataActors.Insert(0, null);
               Console.WriteLine("Carregando Classes...");
					DataClasses = JsonConvert.DeserializeObject<List<RPGClass>>(jsons[3].Remove(1,5),settings)!;
					DataClasses.Insert(0, null);
               Console.WriteLine("Carregando Habilidades...");
					DataSkills = JsonConvert.DeserializeObject<List<RPGSkill>>(jsons[4].Remove(1,5),settings)!;
					DataSkills.Insert(0, null);
               Console.WriteLine("Carregando Items...");
					DataItems = JsonConvert.DeserializeObject<List<RPGItem>>(jsons[5].Remove(1,5),settings)!;
					DataItems.Insert(0, null);
					Console.WriteLine("Carregando Armas...");
					DataWeapons = JsonConvert.DeserializeObject<List<RPGWeapon>>(jsons[6].Remove(1,5),settings)!;
					DataWeapons.Insert(0, null);
               Console.WriteLine("Carregando Armaduras...");
					DataArmors = JsonConvert.DeserializeObject<List<RPGArmor>>(jsons[7].Remove(1,5),settings)!;
					DataArmors.Insert(0, null);
               Console.WriteLine("Carregando Inimigos...");
					DataEnemies = JsonConvert.DeserializeObject<List<RPGEnemy>>(jsons[8].Remove(1,5),settings)!;
					DataEnemies.Insert(0, null);
               Console.WriteLine("Carregando Estados...");
					DataStates = JsonConvert.DeserializeObject<List<RPGState>>(jsons[9].Remove(1,5),settings)!;
					DataStates.Insert(0, null);
               Console.WriteLine("Carregando Animações...");
					DataAnimations = JsonConvert.DeserializeObject<List<RPGAnimation>>(jsons[10].Remove(1, 5), settings)!;
					DataAnimations.Insert(0, null);
               Console.WriteLine("Carregando Tilesets...");
					DataTilesets = JsonConvert.DeserializeObject<List<RPGTileset>>(jsons[11].Remove(1,5),settings)!;
					DataTilesets.Insert(0, null);
               Console.WriteLine("Carregando Eventos Comuns...");
					DataCommonEvents = JsonConvert.DeserializeObject<List<RPGCommonEvent>>(jsons[12].Remove(1,5),settings)!;
					DataCommonEvents.Insert(0, null);
					foreach(var commonEvent in DataCommonEvents) {
						if (commonEvent == null) continue;
						GameInterpreter.ProcessEvalList(commonEvent.list);
					}
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
						var rpgMap = JsonConvert.DeserializeObject<RPGMap>(jsons[counter], settings);
						if (rpgMap == null) continue;
                  DataMaps.Add(id, rpgMap);
						Network.Maps.TryAdd(id, new GameMap(id, DataMaps[id]));
						counter++;
               }
            }catch (Exception ex) {
               WriteColor("Erro ao carregar dados básicos.", ConsoleColor.Red);
					WriteColor($"Erro: {ex}", ConsoleColor.Red);
            }
            LoadMotd();
            Console.WriteLine("Carregando lista de banidos...");
				try {
					_ = Network.DB.LoadBanList();
				} catch(Exception ex) {
					WriteColor("O banco de dados SQL está off-line!", ConsoleColor.Red);
					WriteColor("A lista de banidos não foi carregada!", ConsoleColor.Red);
					WriteColor($"Erro: {ex}", ConsoleColor.Red);
				}
            Console.WriteLine("Carregando guildas...");
				try {
               _ = Network.DB.LoadGuilds();
            } catch {
               WriteColor("As guildas não foram carregadas!", ConsoleColor.Red);
				}
				if(_success)
					Console.WriteLine("Dados Carregados com sucesso");
         }
		}
      public static void LoadMotd() {
         Console.WriteLine("Carregando mensagem do dia...");
         Motd = File.ReadAllText("motd.txt", Encoding.UTF8);
      }
      public static async Task SaveGameData() {
			WriteColor($"Salvando todos os dados às {DateTimeOffset.Now:H'h'mm'min.'}", ConsoleColor.Green);
			File.WriteAllText("motd.txt", Motd, Encoding.UTF8);
			File.WriteAllText("Data/switches.json", JsonConvert.SerializeObject(Network.Switches.Data));
			foreach(var client in Network.Clients.Values) {
				if (client != null && client.IsInGame())
					await Network.DB.SavePlayer(client);
			}
			await Network.DB.SaveBanList();
			Network.Log.SaveAll();
      }
   }
}
