using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VXAOS_Server.RPGData;
using static System.Net.Mime.MediaTypeNames;

namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      public string Name = "";
      public string CharacterName = "";
      public int CharacterIndex;
      public string FaceName = "";
      public int FaceIndex;
      public string OriginalCharacterName = "";
      public int OriginalCharacterIndex;
      public string OriginalFaceName = "";
      public int OriginalFaceIndex;
      public int ClassId;
      public int Sex;
      public int Exp;
      public List<int> Equips = new();
      private int _points;
      public int Points {
         get {
            return _points;
         }
         set {
            _points = value;
            Network.SendPlayerPoints(this, (short)_points);
         }
      }
      public int ReviveMapId;
      public int ReviveX;
      public int ReviveY;
      public int Gold;
      public Dictionary<int, int> Items = new();
      public Dictionary<int, int> Weapons = new();
      public Dictionary<int, int> Armors = new();
      public List<int> Skills = new();
      public List<Hotbar> Hotbar = new();
      public GameSwitches Switches;
      public GameSelfSwitches SelfSwitches;
      public List<JArray> ShopGoods = new();
      public Request Request = new();
      public Dictionary<int, GameQuest> Quests = new();
      public int PartyId = -1;
      public int TeleportId = -1;
      public GameInterpreter EventInterpreter;
      public Dictionary<int, GameInterpreter> CommonEvents = new();
      public Dictionary<int, DateTimeOffset?> ParallelEventsWating = new();
      public bool CreatingGuild = false;
      public DateTimeOffset? WaitingEvent;
      public DateTimeOffset MutedTime;
      public DateTimeOffset StopCount;
      public DateTimeOffset AntispamTime;
      public DateTimeOffset GlobalAntispamTime;
      public GameInterpreter? MessageInterpreter;
      public int Choice = -1;
      public string GuildName = "";
      public DateTimeOffset WeaponAttackTime = DateTimeOffset.UtcNow;
      public DateTimeOffset ItemAttackTime = DateTimeOffset.UtcNow;
      public Dictionary<int, DateTimeOffset> SkillCooldownTime = new();
      public DateTimeOffset RecoverTime = DateTimeOffset.UtcNow.AddSeconds(Network.Cfg.RecoverTime);
      public bool IsInShop() { return ShopGoods.Count > 0; }
      public bool IsInTrade() { return TradePlayerId >= 0; }
      public bool IsInTeleport() { return TeleportId >= 0; }
      public bool IsInGuild() { return string.IsNullOrEmpty(GuildName); }
      public bool IsInParty() { return PartyId >= 0; }
      public bool IsInBank() { return InBank; }
      public bool IsGuildLeader() { return Network.Guilds[GuildName].Leader == Name; }
      public bool HasChoice() { return Choice >= 0; }
      public bool HasText() { return MessageInterpreter != null; }
      public bool IsCreatingGuild() { return CreatingGuild; }
      public bool IsUsingNormalWeapon() { return WeaponId > 0; }
      public bool IsUsingRangeWeapon() { return Configs.RangeWeapons.ContainsKey(WeaponId); }
      public bool IsSpawning() { return AntispamTime > DateTimeOffset.UtcNow; }
      public bool IsGlobalChatSpawning() { return GlobalAntispamTime > DateTimeOffset.UtcNow; }
      public int WeaponId { get { return Equips[(int)Enums.Equip.WEAPON]; } }
      public int ShieldId { get { return Equips[(int)Enums.Equip.SHIELD]; } }
      public bool IsMovable() { return !(StopCount > DateTimeOffset.UtcNow) && !MoveRouteForcing; }
      public bool IsAttacking() { return (WeaponAttackTime > DateTimeOffset.UtcNow); }
      public bool IsUsingItem() { return (ItemAttackTime > DateTimeOffset.UtcNow); }
      public bool IsUsingSkill(int skillId) {
         return SkillCooldownTime.ContainsKey(skillId) && (SkillCooldownTime[skillId] > DateTimeOffset.UtcNow);
      }
      public bool IsMuted() { return (MutedTime > DateTimeOffset.UtcNow); }
      public void ClearRequest() {
         Request.Id = -1;
         Request.Type = Enums.Request.NONE;
      }
      public bool IsQuestInProgress(int questId) {
         if(Quests.TryGetValue(questId - 1, out var quest)){
            return quest.IsInProgress();
         }
         return false;
      }
      public bool IsQuestInFinished(int questId) {
         if(Quests.TryGetValue(questId - 1, out var quest)) {
            return quest.IsFinished();
         }
         return false;
      }
      public bool IsFinishedQuestRequirements(int questId) {
         if(Quests.TryGetValue(questId -1, out var quest)){
            if(quest.SwitchId > 0 && !Switches[quest.SwitchId])
               return false;
            if (quest.VariableId > 0 && Variables[quest.VariableId] < quest.VariableAmount)
               return false;
            var item = ItemObject(quest.ItemKind, quest.ItemId);
            if (item != null && ItemNumber(item) < quest.ItemAmount)
               return false;
            if (quest.Kills < quest.MaxKills)
               return false;
         } else {
            return false;
         }
         return true;
      }
      public float VipExpBonus() { return IsVip() ? Network.Cfg.VipExpBonus : 1; }
      public float VipGoldBonus() { return IsVip() ? Network.Cfg.VipGoldBonus : 1; }
      public float VipDropBonus() { return IsVip() ? Network.Cfg.VipDropBonus : 1; }
      public float VipRecoverBonus() { return IsVip() ? Network.Cfg.VipRecoverBonus : 1; }
      public float LoseExpRate() { return IsVip() ? Network.Cfg.LoseVipExpRate : Network.Cfg.LoseDefaultExpRate; }
      internal new IEnumerable<RPGBaseItem> FeatureObjects() {
         foreach (var obj in base.FeatureObjects())
            yield return obj;
         yield return DataActors[ClassId];
         yield return DataClasses[ClassId];
         foreach (var equip in EquipsObjects()) {
            if (equip != null)
               yield return equip;
         }
      }
      public override List<int> AtkElements() {
         List<int> set = base.AtkElements();
         if (!IsUsingNormalWeapon() && !set.Contains(1)) {
            set.Add(1);
         }
         return set;
      }
      public override bool CollideWithCharacters(int x, int y) {
         return base.CollideWithCharacters(x, y) || CollideWithPlayers(x, y) && Network.Maps[MapId].PvP;
      }
      public void Transfer(int mapId, int x, int y, byte direction) {
         StopCount = DateTimeOffset.UtcNow;
         CloseWindows();
         if(mapId == MapId) {
            ChangePosition(x, y, direction);
            Network.SendPlayerMovement(this);
         } else {
            ChangeMap(mapId, x, y, direction);
         }
      }
      private void ChangeMap(int mapId, int x, int y, byte direction) {
         int lastMapId = MapId;
         ChangePosition(x, y, direction);
         Network.SendPlayerData(this, mapId);
         MapId = mapId;
         Network.SendRemovePlayer(Id, lastMapId);
         Network.SendTransferPlayer(this);
         Network.SendMapPlayers(this);
         Network.SendMapEvents(this);
         Network.SendMapDrops(this);
         Network.Maps[lastMapId].TotalPlayers -= 1;
         Network.Maps[MapId].TotalPlayers += 1;
         ClearTargetPlayers(Enums.Target.PLAYER, lastMapId);
         ClearTarget();
         ClearRequest();
      }
      internal void ChangePosition(int x, int y, byte direction) {
         X = x; Y = y;
         Direction = direction;
      }
      internal void CheckPoint(int mapId, int x, int y) {
         ReviveMapId = mapId;
         ReviveX = x; ReviveY = y;
      }
      protected override void OnHpChanged() {
         if (!IsDead())
            Network.SendPlayerVitals(this);
      }
      protected override void OnMpChanged() {
         Network.SendPlayerVitals(this);
      }
      internal void RecoverAll() {
         ClearStates();
         ChangeVitals(Mhp, Mmp);
      }
      internal void ChangeVitals(int hp, int mp) {
         Hp = Math.Clamp(hp, 0, Mhp);
         Mp = Math.Clamp(mp, 0, Mmp);
         Network.SendPlayerVitals(this);
      }
      internal void CloseWindows() {
         CloseShop();
         CloseTrade();
         CloseBank();
         CloseTeleport();
         CloseCreateGuild();
      }
      public int ExpForLevel(int level) {
         return DataClasses[ClassId].Exp_For_Level(level);
      }
      public int CurrentLevelExp() {
         return ExpForLevel(Level);
      }
      public int NextLevelExp() {
         return ExpForLevel(Level + 1);
      }
      public bool IsMaxLevel() {
         return Level >= Configs.MaxLevel;
      }
      public List<RPGBaseItem?> EquipsObjects() {
         List<RPGBaseItem?> equips = new(Configs.MaxEquips);
         equips.Add(DataWeapons[WeaponId]);
         for (int i = 1; i < Configs.MaxEquips; i++) {
            equips.Add(DataArmors[Equips[i]]);
         }
         return equips;
      }
      public Dictionary<int, int>? ItemContainer(RPGBaseItem item) {
         switch (item) {
            case RPGItem _:
               return Items;
            case RPGWeapon _:
               return Weapons;
            case RPGArmor _:
               return Armors;
         }
         return null;
      }
      public RPGBaseItem? ItemObject(int kind, int itemId) {
         if (kind == 1) return DataItems[itemId];
         if (kind == 2) return DataWeapons[itemId];
         if (kind == 3) return DataArmors[itemId];
         return null;
      }
      public int? KindItem(RPGBaseItem item) {
         switch (item) {
            case RPGItem _:
               return 1;
            case RPGWeapon _:
               return 2;
            case RPGArmor _:
               return 3;
         }
         return null;
      }
      public RPGEquipItem? EquipObject(int slotId, int itemId) {
         return slotId == (int)Enums.Equip.WEAPON ? DataWeapons[itemId] : DataArmors[itemId];
      }
      public int ItemNumber(RPGBaseItem item) {
         var container = ItemContainer(item);
         if (container != null && container.TryGetValue((int)item.id, out var value)) {
            return value;
         }
         return 0;
      }
      public bool IsFullInventory(RPGBaseItem item) {
         int size = Items.Count + Weapons.Count + Armors.Count;
         if(size == Configs.MaxPlayerItems && !HasItem(item)) {
            Network.AlertMessage(this, Enums.Alert.FULL_INV);
            return true;
         }
         return false;
      }
      public void ChangeEquip(int slotId, int itemId) {
         if(!TradeItemWithParty(EquipObject(slotId, itemId), EquipObject(slotId, Equips[slotId]), slotId))
            return;
         Equips[slotId] = itemId;
         Network.SendPlayerEquip(this, (byte)slotId);
         Refresh();
      }
      internal bool TradeItemWithParty(RPGEquipItem? newItem, RPGEquipItem? oldItem, int slotId) {
         if (newItem == null && IsFullInventory(oldItem))
            return false;
         if (newItem != null && !IsEquippable(newItem, slotId))
            return false;
         GainItem(oldItem, 1);
         LoseItem(newItem, 1);
         if(newItem != null && IsInTrade())
            LoseTradeItem(newItem, 1);
         return true;
      }
      internal bool IsEquippable(RPGEquipItem item, int slotId) {
         if(!HasItem(item)) return false;
         if(item.etype_id != slotId) return false;
         if(item.level > Level) return false;
         if(item.vip && !IsVip()) return false;
         if (item is RPGArmor && ((RPGArmor)item).sex < 2 && ((RPGArmor)item).sex != Sex) return false;
         if (HasSealEquip(item)) return false;
         if (IsEquipTypeSealed((int)item.etype_id)) return false;
         if (item is RPGWeapon) return IsEquipWTypeOk((int)((RPGWeapon)item).wtype_id);
         if (item is RPGArmor) return IsEquipATypeOk((int)((RPGArmor)item).atype_id);
         return false;
      }
      private bool HasSealEquip(RPGEquipItem item) {
         return item.features.Any(feature =>
            feature.code == (int)Enums.Feature.EQUIP_SEAL &&
            EquipsObjects().Any(equip =>
               equip is RPGEquipItem equipItem &&
               equipItem.etype_id == feature.data_id));
      }
      public bool IsSkillWtypeOk(RPGSkill skill) {
         int wtypeId1 = (int)skill.required_wtype_id1;
         int wtypeId2 = (int)skill.required_wtype_id2;
         if (wtypeId1 == wtypeId2 && wtypeId2 == 0)
            return true;
         if (wtypeId1 > 0 && IsWtypeEquipped(wtypeId1))
            return true;
         if (wtypeId2 > 0 && IsWtypeEquipped(wtypeId2))
            return true;
         return false;
      }
      private bool IsWtypeEquipped(int wtypeId) {
         if (IsUsingNormalWeapon() && DataWeapons[WeaponId].wtype_id == wtypeId)
            return true;
         if(IsDualWield() && ShieldId > 0 && DataWeapons[ShieldId].wtype_id == wtypeId)
            return true;
         return false;
      }
      public override void AddNewState(int stateId) {
         base.AddNewState(stateId);
         float time = 0;
         if (StatesTime.TryGetValue(stateId, out var expiration))
            time = (float)(expiration - DateTimeOffset.UtcNow).TotalSeconds;
         Network.SendPlayerState(this, (short)stateId, true, time);
      }
      public void AddNewState(int stateId, float time = 0) {
         base.AddNewState(stateId);
         if(StatesTime.ContainsKey(stateId))
            StatesTime[stateId] = DateTimeOffset.UtcNow.AddSeconds(time + 0.1f);
         Network.SendPlayerState(this, (short)stateId, true, (time + 0.1f));
      }
      public override void RemoveState(int stateId) {
         base.RemoveState(stateId);
         Network.SendPlayerState(this, (short)stateId);
      }
      public override void AddBuff(int paramId, int time) {
         base.AddBuff(paramId, time);
         Network.SendPlayerBuff(this, (byte)paramId, 1, (float)(BuffsTime[paramId] - DateTimeOffset.UtcNow).TotalSeconds, time);
      }
      public override void AddDeBuff(int paramId, int time) {
         base.AddDeBuff(paramId, time);
         Network.SendPlayerBuff(this, (byte)paramId, -1, (float)(BuffsTime[paramId] - DateTimeOffset.UtcNow).TotalSeconds, time);
      }
      public override void EraseBuff(int paramId) {
         base.EraseBuff(paramId);
         Network.SendPlayerBuff(this, (byte)paramId, 0);
      }
      public override void AddParam(int paramId, int value) {
         base.AddParam(paramId, value);
         Network.SendPlayerParam(this, (byte)paramId, (short)value);
      }
      public override float ParamPlus(int paramId) {
         float value = 0;
         for (int slotId = 0; slotId < Equips.Count; slotId++) {
            int itemId = Equips[slotId];
            if (itemId > 0) {
               value += (float)EquipObject(slotId, itemId).@params[paramId];
            }
         }
         return value;
      }
      public void ResetParameters() {
         for (int paramId = 0; paramId < ParamBase.Length; paramId++) {
            int value = (int)DataClasses[ClassId].@params[paramId, 1];
            Network.SendPlayerParam(this, (byte)paramId, (short)(value - ParamBase[paramId]));
            ParamBase[paramId] = value;
         }
         Points = Configs.StartPoints + (Level - 1) * Network.Cfg.LevelUpPoints;
         Refresh();
      }
      public void ChangeTarget(int targetId, Enums.Target type) {
         if (Target.Id == targetId && Target.Type == type) return;
         Target.Id = targetId;
         Target.Type = type;
         Network.SendTarget(this);
      }
      public void ChangeHotbar(int id, Enums.Hotbar type, int itemId) {
         Hotbar[id].Type = type;
         Hotbar[id].ItemId = itemId;
         Network.SendPlayerHotbar(this, (byte)id);
      }
      public void ChangeExp(long exp) {
         if (IsMaxLevel() && exp > Exp) return;
         Network.SendPlayerExp(this, (int)(exp - Exp));
         Exp = Math.Clamp(exp, 0, ExpForLevel(Configs.MaxLevel));
         int lastLevel = Level;
         while (!IsMaxLevel() && Exp >= NextLevelExp()) LevelUp();
         while (Exp < CurrentLevelExp()) LevelDown();
         if(Level > lastLevel) {
            Network.PlayerChatMessage(this, string.Format(Vocab.LevelUp, lastLevel, Level), Configs.AlertColor);
            RecoverAll();
         }
      }
      internal void LevelUp() {
         foreach(var learning in DataClasses[ClassId].learnings) {
            if(learning.level == Level) {
               LearnSkill((int)learning.skill_id);
            }
         }
         Level += 1;
         Points += Network.Cfg.LevelUpPoints;
      }
      internal void LevelDown() {
         foreach(var forgetting in DataClasses[ClassId].learnings) {
            if(forgetting.level == Level) {
               ForgetSkill((int)forgetting.skill_id);
            }
         }
         Points -= Network.Cfg.LevelUpPoints;
         Level -= 1;
      }
      public override bool IsSkillLearned(int skillId) {
         return Skills.Contains(skillId);
      }
      public override bool HasAddedSkillType(RPGSkill skill) {
         return AddedSkillTypes().Contains((int)skill.stype_id);
      }
      public void ChangeClass(int classId) {
         ClassId = classId;
         Network.SendPlayerClass(this, (short)classId);
      }
      public void ChangeSex() {
         Sex = (int)(Sex == (int)Enums.Sex.MALE ? Enums.Sex.FEMALE : Enums.Sex.MALE);
         Network.SendPlayerSex(this);
      }
      public void SaveOriginalGraphic() {
         if(!string.IsNullOrEmpty(OriginalCharacterName)) return;
         OriginalCharacterName = CharacterName;
         OriginalCharacterIndex = CharacterIndex;
         OriginalFaceName = FaceName;
         OriginalFaceIndex = FaceIndex;
      }
      public void LoadOriginalGraphic() {
         if(string.IsNullOrEmpty(OriginalCharacterName)) return;
         CharacterName = OriginalCharacterName;
         CharacterIndex = OriginalCharacterIndex;
         FaceName = OriginalFaceName;
         FaceIndex = OriginalFaceIndex;
         OriginalCharacterName = string.Empty;
      }
      public void SetGraphic(string characterName, int characterIndex, string faceName, int faceIndex) {
         CharacterName = characterName;
         CharacterIndex = characterIndex;
         FaceName = faceName;
         FaceIndex = faceIndex;
         Network.SendPlayerGraphic(this);
      }
      public void CheckFloorEffect() {
         if (Network.Maps[MapId].IsDamageFloor(X, Y)) {
            ExecuteFloorDamage();
         }
      }
      internal void ExecuteFloorDamage() {
         int damage = (int)(10 * Fdr);
         Hp -= Math.Min(damage, MaxFloorDamage());
      }
      private int MaxFloorDamage() {
         return DataSystem.opt_floor_death ? Hp : Math.Max(Hp - 1, 0);
      }
      public void GainExp(int exp) {
         ChangeExp(Exp + exp);
      }
      public void LoseExp(int exp) {
         GainExp(-exp);
         string text = exp == 0 ? Vocab.NotLoseExp : string.Format(Vocab.Died, exp.ToString("N0").Replace(",", "."));
         Network.PlayerChatMessage(this, text, Configs.ErrorColor);
      }
      public void ChangeLevel(int level) {
         Level = Math.Clamp(level, 1, Configs.MaxLevel);
         ChangeExp(ExpForLevel(Level));
      }
      public void GainGold(int amount, bool shopSound = false, bool popup = false) {
         if (amount == 0) return;
         Gold = Math.Clamp(Gold + amount, 0, Configs.MaxGold);
         Network.SendPlayerGold(this, amount, shopSound, popup);
      }
      public void LoseGold(int amount, bool shopSound = false) {
         GainGold(-amount, shopSound);
      }
      public bool HasItem(RPGBaseItem item, bool includeEquip = false) {
         if (ItemNumber(item) > 0) return true;
         return includeEquip ? Equips.Contains((int)item.id) : false;
      }
      public void GainItem(RPGBaseItem item, int amount, bool dropSound = false, bool popup = false) {
         var container = ItemContainer(item);
         if (container == null) return;
         int itemId = (int)item.id;
         container[itemId] = Math.Clamp((ItemNumber(item) + amount), 0, Configs.MaxItems);
         if (container[itemId] == 0)
            container.Remove(itemId);
         Network.SendPlayerItem(this, (short)itemId, (byte)KindItem(item), (short)amount, dropSound, popup);
         AddItemsCount(item);
      }
      public void LoseItem(RPGBaseItem item, int amount) {
         GainItem(item, -amount);
      }
      public void LearnSkill(int skillId) {
         if (IsSkillLearned(skillId)) return;
         Skills.Add(skillId);
         Skills.Sort();
         Network.SendPlayerSkill(this, (short)skillId);
         Network.PlayerChatMessage(this, $"#{Vocab.LearnedSkill} #{DataSkills[skillId].name}.", Configs.SuccessColor);
      }
      public void ForgetSkill(int skillId) {
         Skills.Remove(skillId);
         Network.SendPlayerSkill(this, (short)skillId, false);
      }
      private void ItemEffectLearnSkill(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         LearnSkill((int)effect.data_id);
      }
      public int DropItemRate() {
         //ABILITY_DROP_ITEM_DOUBLE = 5
         return PartyAbility(5) ? 2 : 1;
      }
      public int GoldRate() {
         //ABILITY_GOLD_DOUBLE      = 4
         return PartyAbility(4) ? 2 : 1;
      }
      public void OpenShop(List<JArray> shopGoods, int eventId, int index) {
         ShopGoods = shopGoods;
         Network.SendOpenShop(this, (short)eventId, (short)index);
      }
      public void CloseShop() {
         if (!IsInShop()) return;
         ShopGoods.Clear();
         Network.SendCloseWindow(this);
         //EventInterpreter.Resume();
      }
      public void OpenTeleport(int teleportId) {
         TeleportId = teleportId;
         Network.SendOpenTeleport(this, (byte)teleportId);
      }
      public void CloseTeleport() {
         if (!IsInTeleport()) return;
         TeleportId = -1;
         Network.SendCloseWindow(this);
         //EventInterpreter.Resume();
      }
      public void CloseEventMessage() {
         Choice = -1;
         MessageInterpreter = null;
      }
      public void OpenCreateGuild() {
         if (IsCreatingGuild()) return;
         if (IsInGuild()) {
            Network.AlertMessage(this, Enums.Alert.IN_GUILD);
            return;
         }
         CreatingGuild = true;
         Network.SendOpenCreateGuild(this);
      }
      public void CloseCreateGuild() {
         if(!IsCreatingGuild()) return;
         CreatingGuild = false;
         Network.SendCloseWindow(this);
         //EventInterpreter.Resume();
      }
      public void AcceptGuild() {
         if (IsInGuild()) return;
         if (Network.Clients.TryGetValue(Request.Id, out var client) && client.IsInGame()) {
            if (!Network.Guilds.ContainsKey(client.GuildName))
               return;
         } else {
            return;
         }
         GuildName = client.GuildName;
         Network.GuildChatMessage(this, $"{Name} {Vocab.JoinGuild} {GuildName}", Configs.SuccessColor);
         Network.Guilds[GuildName].Members.Add(Name);
         Network.SendGuildName(this);
      }
      public void LeaveGuild() {
         Network.GuildChatMessage(this, $"{Name} {Vocab.LeaveGuild} {GuildName}", Configs.ErrorColor);
         Network.Guilds[GuildName].Members.Remove(Name);
         GuildName = string.Empty;
         Network.SendGuildName(this);
      }
      public void StartQuest(int questId) {
         if (Quests.ContainsKey(questId)) return;
         Quests.Add(questId, new GameQuest(questId, Enums.Quest.IN_PROGRESS, 0));
         Network.SendAddQuest(this, (byte)questId);
      }
      public void AddKillsCount(int enemyId) {
         foreach (var quest in Quests.Values) {
            if (!quest.IsInProgress() || quest.EnemyId != enemyId || quest.Kills == quest.MaxKills)
               continue;
            quest.Kills += 1;
            string text = $"#{Vocab.Killed} #{quest.Kills}/{quest.MaxKills} {DataEnemies[enemyId].name}.";
            Network.PlayerChatMessage(this, text, Configs.SuccessColor);
         }
      }
      public void AddItemsCount(RPGBaseItem item) {
         foreach (var quest in Quests.Values) {
            if (!quest.IsInProgress() || quest.ItemId != (int)item.id || ItemNumber(item) > quest.ItemAmount)
               continue;
            string text = $"#{Vocab.Have} #{ItemNumber(item)}/{quest.ItemAmount} {item.name}.";
            Network.PlayerChatMessage(this, text, Configs.SuccessColor);
         }
      }
      public void FinishQuest(int questId) {
         if (Quests.TryGetValue(questId, out var quest)) {
            quest.State = Enums.Quest.FINISHED; 
            if(quest.ItemId > 0) {
               var item = ItemObject(quest.ItemKind, quest.ItemId);
               LoseItem(item, quest.ItemAmount);
               if(IsInTrade())
                  LoseTradeItem(item, quest.ItemAmount);
            }
            GainGold(quest.Reward.Gold, false, true);
            GainExp(quest.Reward.Exp);
            if (quest.Reward.ItemId > 0) { 
               var item = ItemObject(quest.Reward.ItemKind, quest.Reward.ItemId);
               if(!IsFullInventory(item))
                  GainItem(item, quest.Reward.ItemAmount, false, true);
            }
            if(quest.Repeat)
               Quests.Remove(questId);
            Network.SendFinishQuest(this, (byte)questId);
         }
      }
      public override void SendMovement() {
         Network.SendPlayerMovement(this);
      }
      public void StartMapEvent(int x, int y, List<int> triggers, bool normal) {

      }
      public void CheckEventTriggerHere(List<int> triggers) {

      }
      public void CheckEventTriggerThere(List<int> triggers) { 

      }
      public void CheckEventTriggerTouch(int x, int y) {

      }
      public void CheckTouchEvent() {

      }
      public void UpdateGame() {
         RecoverVital();
         UpdateStBfTimers();
         UpdateCommonEvents();
         UpdateEventInterpreter();
      }
      public void RecoverVital() {
         if (RecoverTime > DateTimeOffset.UtcNow)
            return;
         RecoverTime = DateTimeOffset.UtcNow;
         if(Hp < Mhp || Mp < Mmp) {
            float n = (Agi / 100) + 1;
            ChangeVitals(
                  Convert.ToInt32(Hp + Network.Cfg.RecoverHP * VipRecoverBonus() * n),
                  Convert.ToInt32(Mp + Network.Cfg.RecoverMP * VipRecoverBonus() * n)
               );
         }
      }
      public void UpdateCommonEvents() {
      
      }
      public void UpdateEventInterpreter() {
         if (WaitingEvent == null || WaitingEvent > DateTimeOffset.UtcNow)
            return;
         WaitingEvent = null;
         EventInterpreter.Resume();
      }
   }
}
