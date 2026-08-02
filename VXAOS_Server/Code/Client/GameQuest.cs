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
         SwitchId = MQuests.Data[id].SwitchId;
         VariableId = MQuests.Data[id].VariableId;
         VariableAmount = MQuests.Data[id].VariableAmount;
         ItemId = MQuests.Data[id].ItemId;
         ItemKind = MQuests.Data[id].ItemKind;
         ItemAmount = MQuests.Data[id].ItemAmount;
         EnemyId = MQuests.Data[id].EnemyId;
         MaxKills = MQuests.Data[id].EnemyAmount;
         Reward.ItemId = MQuests.Data[id].RewItemId;
         Reward.ItemKind = MQuests.Data[id].RewItemKind;
         Reward.ItemAmount = MQuests.Data[id].RewItemAmount;
         Reward.Exp = MQuests.Data[id].RewExp;
         Reward.Gold = MQuests.Data[id].RewGold;
         Repeat = MQuests.Data[id].Get("Repeat", false);
      }
      public bool IsInProgress() {
         return State == Enums.Quest.IN_PROGRESS;
      }
      public bool IsFinished() {
         return State == Enums.Quest.FINISHED;
      }
   }
}
