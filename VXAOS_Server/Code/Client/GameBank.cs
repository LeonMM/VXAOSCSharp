using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      public Dictionary<int, int> BankItems = new();
      public Dictionary<int, int> BankWeapons = new();
      public Dictionary<int, int> BankArmors = new();
      public int BankGold = 0;
      public int BankIdDb = -1;
      private bool InBank = false;
   }
}
