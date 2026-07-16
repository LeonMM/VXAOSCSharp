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
      public void OpenBank() {
         if (IsInTrade() || IsInShop() || IsInBank())
            return;
         Network.SendOpenBank(this);
         InBank = true;
      }
      public void CloseBank() {
         if(!IsInBank()) return;
         Network.SendCloseWindow(this); 
         InBank = false;
      }
      public Dictionary<int, int>? BankItemContainer(int kind) {
         switch (kind) {
            case 1:
               return BankItems;
            case 2:
               return BankWeapons;
            case 3:
               return BankArmors;
         }
         return null;
      }
      public int BankItemNumber(int kind, int itemId) {
         var container = BankItemContainer(kind);
         if (container != null && container.TryGetValue(itemId, out int value)) {
            return value;
         }
         return 0;
      }
      public bool HasBankItem(int kind, int itemId) {
         return BankItemNumber(kind, itemId) > 0;
      }
      public bool IsFullBank(int kind, int itemId) {
         var container = BankItemContainer(kind);
         if (container is null)
            return true;
         return container.Count == Configs.MaxBankItems && !HasBankItem(kind, itemId);
      }
      public void GainBankItem(int itemId, int kind, int amount) {
         var container = BankItemContainer(kind);
         if (container is null)
            return;
         int lastAmount = BankItemNumber(kind, itemId);
         int newAmount = lastAmount + amount;
         container[itemId] = Math.Clamp(newAmount, 0, Configs.MaxItems);
         if (container[itemId] == 0)
            container.Remove(itemId);
         Network.SendBankItem(this, (short)itemId, (byte)kind, (short)amount);
      }
      public void GainBankGold(int amount) {
         BankGold = Math.Clamp((BankGold + amount), 0, Configs.MaxGold);
         Network.SendBankGold(this, amount);
      }
   }
}
