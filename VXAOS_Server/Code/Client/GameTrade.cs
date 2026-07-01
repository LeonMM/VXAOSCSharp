using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      public Dictionary<int, int> TradeItems = new();
      public Dictionary<int, int> TradeWeapons = new();
      public Dictionary<int, int> TradeArmors = new();
      public int TradeGold = 0;
      public int TradePlayerId = -1;
   }
}
