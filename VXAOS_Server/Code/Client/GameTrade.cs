using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      public Dictionary<int, int> TradeItems = new();
      public Dictionary<int, int> TradeWeapons = new();
      public Dictionary<int, int> TradeArmors = new();
      public int TradeGold = 0;
      public int TradePlayerId = -1;
      public void OpenTrade() {
         if(Network.Clients.TryGetValue(Request.Id, out var tradePlayer) && tradePlayer.IsInGame()) {
            if (MapId! != tradePlayer.MapId) return;
            if (tradePlayer.IsInTrade() || tradePlayer.IsInShop() || tradePlayer.IsInBank()) return;
            if (IsInTrade() || IsInShop() || IsInBank()) return;
            TradePlayerId = Request.Id;
            tradePlayer.TradePlayerId = Id;
            Network.SendAcceptRequest(this, Request.Type);
            Network.SendAcceptRequest(tradePlayer, Request.Type);
         }
      }
      public void CloseTrade() {
         if(!IsInTrade()) return;
         Network.SendCloseWindow(this);
         Network.SendCloseWindow(Network.Clients[TradePlayerId]);
         Network.Clients[TradePlayerId].ClearTradeItems();
         ClearTradeItems();
      }
      public void ClearTradeItems() {
         TradePlayerId = -1;
         TradeItems.Clear();
         TradeWeapons.Clear();
         TradeArmors.Clear();
         TradeGold = 0;
      }
      public void CloseTradeRequest() {
         if (Request.Type != Enums.Request.FINISH_TRADE) return;
         ClearRequest();
      }
      public Dictionary<int, int>? TradeItemContainer(RPGBaseItem item) {
         switch (item) {
            case RPGItem _:
               return TradeItems;
            case RPGWeapon _:
               return TradeWeapons;
            case RPGArmor _:
               return TradeArmors;
         }
         return null;
      }
      public int TradeItemNumber(RPGBaseItem item) {
         var container = ItemContainer(item);
         if (container != null && container.TryGetValue((int)item.id, out var value)) {
            return value;
         }
         return 0;
      }
      public bool HasTradeItem(RPGBaseItem item) {
         return TradeItemNumber(item) > 0;
      }
      public bool IsFullTrade(RPGBaseItem item) {
         int size = TradeItems.Count + TradeWeapons.Count + TradeArmors.Count;
         return size == Configs.MaxTradeItems && !HasTradeItem(item);
      }
      public void GainTradeItem(RPGBaseItem item, int amount) {
         var container = TradeItemContainer(item);
         if (container == null) return;
         int itemId = (int)item.id;
         int lastNumber = TradeItemNumber(item);
         int newNumber = lastNumber + amount;
         container[itemId] = Math.Clamp(newNumber, 0, Configs.MaxItems);
         if (container[itemId] == 0)
            container.Remove(itemId);
         Network.SendTradeItem(this, (short)Id, (short)itemId, (byte)KindItem(item), (short)amount);
         Network.SendTradeItem(Network.Clients[TradePlayerId], (short)Id, (short)itemId, (byte)KindItem(item), (short)amount);
      }
      public void LoseTradeItem(RPGBaseItem item, int amount) {
         GainTradeItem(item, -amount);
      }
      public void GainTradeGold(int amount) {
         TradeGold = Math.Clamp(TradeGold + amount, 0, Configs.MaxGold);
         Network.SendTradeGold(this, (short)Id, amount);
         Network.SendTradeGold(Network.Clients[TradePlayerId], (short)Id, amount);
      }
      public void FinishTrade() {
         if (!IsInTrade()) return;
         if (!Network.Clients.TryGetValue(TradePlayerId, out var tradePlayer)) return;
         foreach(var (itemId, amount) in TradeItems) {
            var item = DataItems[itemId];
            if(item != null && tradePlayer.IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               LoseItem(item, amnt);
               tradePlayer.GainItem(item, amount);
            }
         }
         foreach(var (weaponId, amount) in TradeWeapons) {
            var item = DataWeapons[weaponId];
            if(item != null && tradePlayer.IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               LoseItem(item, amnt);
               tradePlayer.GainItem(item, amount);
            }
         }
         foreach(var (armorId, amount) in TradeArmors) {
            var item = DataArmors[armorId];
            if(item != null && tradePlayer.IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               LoseItem(item, amnt);
               tradePlayer.GainItem(item, amount);
            }
         }
         foreach (var (itemId, amount) in tradePlayer.TradeItems) {
            var item = DataItems[itemId];
            if(item != null && IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               GainItem(item, amount);
               tradePlayer.LoseItem(item, amnt);
            }
         }
         foreach(var (weaponId, amount) in tradePlayer.TradeWeapons) {
            var item = DataWeapons[weaponId];
            if(item != null && IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               GainItem(item, amount);
               tradePlayer.LoseItem(item, amnt);
            }
         }
         foreach(var (armorId, amount) in tradePlayer.TradeArmors) {
            var item = DataArmors[armorId];
            if(item != null && IsFullInventory(item)) {
               int amnt = Math.Min(amount, Configs.MaxItems - tradePlayer.ItemNumber(item));
               GainItem(item, amount);
               tradePlayer.LoseItem(item, amnt);
            }
         }
         GainGold(tradePlayer.TradeGold - TradeGold);
         tradePlayer.GainGold(TradeGold - tradePlayer.TradeGold);
         Network.AlertMessage(this, Enums.Alert.TRADE_FINISHED);
         Network.AlertMessage(tradePlayer, Enums.Alert.TRADE_FINISHED);
         CloseTrade();
      }
   }
}
