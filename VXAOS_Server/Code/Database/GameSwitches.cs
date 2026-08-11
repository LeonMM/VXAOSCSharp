namespace VXAOS_Server {
   public class GameSwitches(GameClient client, List<bool> data) {
      public List<bool> Data {
         get;
      } = data;
      public bool this[int switchId] {
         get => Data.GetWithFallback((switchId - 1), false);
         set {
            Data[switchId - 1] = value;
            Network.SendPlayerSwitch(client, (short)switchId);
         }
      }
      public int Count { get { return Data.Count; } }
   }
   public class GameVariables(GameClient client, List<int> data) {
      private readonly GameClient? _client = client;
      public List<int> Data {
         get;
      } = data;
      public int this[int variableId] {
         get => Data.GetWithFallback((variableId - 1), 0);
         set {
            Data[variableId - 1] = value;
            if(_client is not null)
               Network.SendPlayerVariable(_client, (short)variableId);
         }
      }
      public int Count { get { return Data.Count; } }
   }
   public class GameSelfSwitches(GameClient client, Dictionary<(int MapId, int EventId, char Ch), bool> data) {
      public Dictionary<(int MapId, int EventId, char Ch), bool> Data {
         get;
      } = data;
      public bool this[(int MapId, int EventId, char Ch) key] {
         get => Data.TryGetValue(key, out bool value) && value;
         set {
            Data[key] = value;
            Network.SendPlayerSelfSwitch(client, key);
         }
      }
      public int Count { get { return Data.Count; } }
   }
   public class GameGlobalSwitches(List<bool>? data = null) {
      public List<bool> Data {
         get;
      } = data ?? [];
      public bool this[int switchId] {
         get {
            int index = switchId - Configs.MaxPlayerSwitches - 1;
            return index >= 0 && index < Data.Count && Data[index];
         }
         set {
            int index = switchId - Configs.MaxPlayerSwitches - 1;
            if (Data.Count > index) {
               Data[index] = value;
            } else {
               if (Data.Count - index > 1)
                  Data.AddRange(Enumerable.Repeat(false, (Data.Count - index)));
               Data.Add(value);
            }
            Network.SendGlobalSwitch((short)switchId, Data[index]);
            foreach (var map in Network.Maps.Values)
               map.Refresh();
         }
      }
      public int Count { get { return Data.Count; } }
   }
}
