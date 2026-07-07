using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public class GameSwitches {
      private readonly GameClient _client;
      public List<bool> Data {
         get;
      }

      public GameSwitches(GameClient client, List<bool> data) {
         _client = client;
         Data = data;
      }

      public bool this[int switchId] {
         get => Data.GetWithFallback((switchId - 1), false);//[switchId - 1];
         set {
            Data[switchId - 1] = value;
            Network.SendPlayerSwitch(_client, (short)switchId);
         }
      }
   }
   public class GameVariables {
      private readonly GameClient? _client;
      public List<int> Data {
         get;
      }

      public GameVariables(GameClient client, List<int> data) {
         _client = client;
         Data = data;
      }

      public int this[int variableId] {
         get => Data.GetWithFallback((variableId - 1), 0);//[variableId - 1];
         set {
            Data[variableId - 1] = value;
            if(_client is not null)
               Network.SendPlayerVariable(_client, (short)variableId);
         }
      }
   }
   public class GameSelfSwitches {
      private readonly GameClient _client;
      public Dictionary<(int MapId, int EventId, char Ch), bool> Data {
         get;
      }

      public GameSelfSwitches(GameClient client, Dictionary<(int MapId, int EventId, char Ch), bool> data) {
         _client = client;
         Data = data;
      }

      public bool this[(int MapId, int EventId, char Ch) key] {
         get => Data.TryGetValue(key, out bool value) && value;
         set {
            Data[key] = value;
            Network.SendPlayerSelfSwitch(_client, key);
         }
      }
   }
   public class GameGlobalSwitches {
      public List<bool> Data {
         get;
      }

      public GameGlobalSwitches(List<bool>? data = null) {
         Data = data ?? new();
      }

      public bool this[int switchId] {
         get {
            int index = switchId - Configs.MaxPlayerSwitches - 1;
            return index >= 0 && index < Data.Count && Data[index];
         }
         set {
            int index = switchId - Configs.MaxPlayerSwitches - 1;
            Data[index] = value;
            Network.SendGlobalSwitch((short)switchId, value);
            foreach (var map in Network.Maps.Values)
               map.Refresh();
         }
      }
   }
}
