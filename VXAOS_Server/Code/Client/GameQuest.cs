using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public class GameQuest {
      public Enums.Quest State;
      public int Kills;
      public int SwitchId { private set; get; }
      public int VariableId{ private set; get; }
      public int VariableAmount{ private set; get; }
      public int ItemId{ private set; get; }
      public int ItemKind{ private set; get; }
      public int ItemAmount{ private set; get; }
      public int EnemyId{ private set; get; }
      public int MaxKills { private set; get; }
      public RewardData Reward { get; private set; } = new();
      public bool Repeat { get; private set; }
      public GameQuest(int id, Enums.Quest state, int kills) {
         State = state;
         Kills = kills;
         SwitchId = Quests.Data[id].SwitchId;
         VariableId = Quests.Data[id].VariableId;
         VariableAmount = Quests.Data[id].VariableAmount;
         ItemId = Quests.Data[id].ItemId;
         ItemKind = Quests.Data[id].ItemKind;
         ItemAmount = Quests.Data[id].ItemAmount;
         EnemyId = Quests.Data[id].EnemyId;
         MaxKills = Quests.Data[id].EnemyAmount;
         Reward.ItemId = Quests.Data[id].RewItemId;
         Reward.ItemKind = Quests.Data[id].RewItemKind;
         Reward.ItemAmount = Quests.Data[id].RewItemAmount;
         Reward.Exp = Quests.Data[id].RewExp;
         Reward.Gold = Quests.Data[id].RewGold;
         Repeat = Quests.Data[id].Get("Repeat", false);
      }
      public bool IsInProgress() {
         return State == Enums.Quest.IN_PROGRESS;
      }
      public bool IsFinished() {
         return State == Enums.Quest.FINISHED;
      }
   }
}
