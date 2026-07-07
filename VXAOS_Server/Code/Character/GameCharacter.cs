using Newtonsoft.Json.Linq;
using System;
using VXAOS_Server.RPGData;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public class GameCharacter {
      public const int FLAG_ID_GUARD = 1;
      public int Id = -1;
      public int Direction;
      public int MapId = -1;
      public int X = 0;
      public int Y = 0;
      public bool Through = false;
      public int PriorityType = 1;
      public int TileId = 0;
      public bool MoveSucceed { get; private set; } = false;
      public bool MoveRouteForcing = false;
      public GameVariables Variables = new GameVariables(null, new List<int>());
      public List<int> States = new();
      public Dictionary<int, DateTimeOffset> StatesTime = new();
      public int[] ParamBase = new int[8];
      public int[] Buffs = {0,0,0,0,0,0,0,0,0};
      public DateTimeOffset[] BuffsTime = new DateTimeOffset[8];
      private int _hp;
      private int _mp;
      private long _exp;
      public int Hp { 
         get { return _hp; }
         set { 
            _hp = Math.Clamp(value, 0, Mhp);
            if (IsDead())
               Die();
         }
      }
      public int Mp {
         get { return _mp; }
         set {
            _mp = Math.Clamp(value, 0, Mmp);
         }
      }
      public int Level = 1;
      public int Tp = 1;
      public int Mhp { get { return Param(0); } }
      public int Mmp { get { return Param(1); } }
      public int Atk { get { return Param(2); } }
      public int Def { get { return Param(3); } }
      public int Mat { get { return Param(4); } }
      public int Mdf { get { return Param(5); } }
      public int Agi { get { return Param(6); } }
      public int Luk { get { return Param(7); } }
      public float Hit { get { return XParam(0); } }
      public float Eva { get { return XParam(1); } }
      public float Cri { get { return XParam(2); } }
      public float Cev { get { return XParam(3); } }
      public float Mev { get { return XParam(4); } }
      public float Grd { get { return SParam(1); } }
      public float Rec { get { return SParam(2); } }
      public float Pha { get { return SParam(3); } }
      public float Pdr { get { return SParam(6); } }
      public float Mdr { get { return SParam(7); } }
      public float Fdr { get { return SParam(8); } }
      public int AttackSkillId { get { return 1; } }
      public int GuardSkillId { get { return 2; } }
      public int DeathStateId { get { return 1; } }
      public List<RPGState> RPGStates() {
         List<RPGState> states = new();
         foreach(var state in States) {
            states.Add(DataStates[state]);
         }
         return states;
      }
      public void ClearStates() {
         States = new();
         StatesTime = new();
      }
      public void ClearBuffs() {
         Array.Fill(Buffs, 0);
         Array.Fill(BuffsTime, DateTimeOffset.UtcNow);
      }
      public void RemoveStatesOnDeath() {
         foreach (var state in RPGStates()) {
            if (state.remove_at_battle_end)
               RemoveState((int)state.id);
         }
         for (int buffId = 0; buffId < 8; buffId++)
            EraseBuff(buffId);
      }
      public bool HasState(int stateId) {
         return States.Contains(stateId);
      }
      public bool HasDeathState() {
         return HasState(DeathStateId);
      }
      public void AddState(int stateId) {
         if (IsStateAddable(stateId)) {
            AddNewState(stateId);
         }
      }
      private bool IsStateAddable(int stateId) {
         return !HasDeathState() && DataStates.HasIndex(stateId) && 
            !IsStateResist(stateId) && !IsStateRestrict(stateId);
      }
      public void AddNewState(int stateId) {
         if(stateId == DeathStateId) {
            Die();
            return;
         }
         if (DataStates.HasIndex(stateId)) {
            States.Add(stateId);
            if (DataStates[stateId].auto_removal_timing > 0)
               StatesTime.Add(stateId, DateTimeOffset.UtcNow.AddSeconds(DataStates[stateId].min_turns + 0.1f));
            if (Restriction() > 0)
               OnRestrict();
         }
      }
      private void OnRestrict() {
         foreach(var state in RPGStates()) {
            if (state.remove_by_restriction)
               RemoveState((int)state.id);
         }
      }
      public void RemoveState(int stateId) {
         if (States.Contains(stateId)) {
            States.Remove(stateId);
            StatesTime.Remove(stateId);
         }
      }
      public void AddBuff(int paramId, int time) {
         if (HasDeathState())
            return;
         if(!IsBuffMaxed(paramId))
            Buffs[paramId]++;
         BuffsTime[paramId] = DateTimeOffset.UtcNow.AddSeconds(time + 0.1f);
         if (IsDebuffed(paramId))
            EraseBuff(paramId);
         Refresh();
      }
      public void AddDeBuff(int paramId, int time) {
         if (HasDeathState())
            return;
         if (!IsDebuffMaxed(paramId))
            Buffs[paramId]--;
         BuffsTime[paramId] = DateTimeOffset.UtcNow.AddSeconds(time + 0.1f);
         if (IsBuffed(paramId))
            EraseBuff(paramId);
         Refresh();
      }
      public void RemoveBuff(int paramId) {
         if (HasDeathState() || Buffs[paramId] == 0)
            return;
         EraseBuff(paramId);
         Refresh();
      }
      private void EraseBuff(int paramId) {
         Buffs[paramId] = 0;
         BuffsTime[paramId] = DateTimeOffset.UtcNow;
      }
      private bool IsDebuffed(int paramId) {
         return Buffs[paramId] < 0;
      }
      private bool IsBuffMaxed(int paramId) {
         return Buffs[paramId] == 2;
      }
      private bool IsBuffed(int paramId) {
         return Buffs[paramId] > 0;
      }
      private bool IsDebuffMaxed(int paramId) {
         return Buffs[paramId] == -2;
      }
      public void RemoveStatesByDamage() {
         foreach (var state in RPGStates()) {
            if (state.remove_by_damage && Random.Shared.Next(100) < state.chance_by_damage)
               RemoveState((int)state.id);
         }
      }
      public int Param(int paramId) {
         float value = ParamBase[paramId] + ParamPlus(paramId);
         value *= ParamRate(paramId) * ParamBuffRate(paramId);
         return Math.Clamp((int)value, ParamMin(paramId), Configs.MaxParams);
      }
      public float ParamPlus(int paramId) {
         return 0;
      }
      public float ParamRate(int paramId) {
         return FeaturesPi((int)Enums.Feature.PARAM, paramId);
      }
      public float ParamBuffRate(int paramId) {
         return Buffs[paramId] * 0.25f + 1.0f;
      }
      public float ParamMin(int paramId) {
         return paramId == (int)Enums.Param.MAXMP ? 0 : 1;
      }
      public float FeaturesPi(int code, int id) {
         float result = 1.0f;
         foreach (var ft in FeaturesWithId(code, id)) {
            result *= (float)ft.value;
         }
         return result;
      }
      public float FeaturesSum(int code, int id) {
         float result = 0.0f;
         foreach (var ft in FeaturesWithId(code, id)) {
            result += (float)ft.value;
         }
         return result;
      }
      public IEnumerable<RPGBaseItemFeature> FeaturesWithId(int code, int id) {
         foreach (var ft in AllFeatures()) {
            if (ft.code == code && ft.data_id == id)
               yield return ft;
         }
      }
      public IEnumerable<RPGBaseItemFeature> Features(int code) {
         foreach (var ft in AllFeatures()) {
            if (ft.code == code)
               yield return ft;
         }
      }
      public IEnumerable<RPGBaseItemFeature> AllFeatures() {
         foreach (var obj in FeatureObjects()) {
            foreach (var feature in obj.features) {
               yield return feature;
            }
         }
      }
      internal IEnumerable<RPGBaseItem> FeatureObjects() {
         foreach (var state in RPGStates())
            yield return state;
      }
      public List<int> FeaturesSet(int code) {
         HashSet<int> result = new();

         foreach (var feature in Features(code)) {
            result.Add((int)feature.data_id);
         }

         return result.ToList();
      }
      public void AddParam(int paramId, int value) {
         ParamBase[paramId] += value;
      }
      public float XParam(int xparamId) {
         return FeaturesSum((int)Enums.Feature.XPARAM, xparamId);
      }
      public float SParam(int sparamId) {
         return FeaturesPi((int)Enums.Feature.SPARAM, sparamId);
      }
      public float ElementRate(int elementId) {
         return FeaturesPi((int)Enums.Feature.ELEMENT_RATE, elementId);
      }
      public float DebuffRate(int paramId) {
         return FeaturesPi((int)Enums.Feature.DEBUFF_RATE, paramId);
      }
      public float StateRate(int stateId) {
         return FeaturesPi((int)Enums.Feature.STATE_RATE, stateId);
      }
      public List<int> StateResistSet() {
         return FeaturesSet((int)Enums.Feature.STATE_RESIST);
      }
      public bool IsStateResist(int stateId) {
        return StateResistSet().Contains(stateId);
      }
      public List<int> AtkElements() {
         return FeaturesSet((int)Enums.Feature.ATK_ELEMENT);
      }
      public List<int> AtkStates() {
         return FeaturesSet((int)Enums.Feature.ATK_STATE);
      }
      public float AtkStatesRate(int stateId) {
         return FeaturesSum((int)Enums.Feature.ATK_STATE, stateId);
      }
      public List<int> AddedSkillTypes() {
         return FeaturesSet((int)Enums.Feature.STYPE_ADD);
      }
      public bool IsSkillTypeSealed(int stypeId) {
         return FeaturesSet((int)Enums.Feature.STYPE_SEAL).Contains(stypeId);
      }
      public List<int> AddedSkills() {
         return FeaturesSet((int)Enums.Feature.SKILL_ADD);
      }
      public bool IsSkillSealed(int skillId) {
         return FeaturesSet((int)Enums.Feature.SKILL_SEAL).Contains(skillId);
      }
      public bool IsEquipWTypeOk(int wtypeId) {
         return FeaturesSet((int)Enums.Feature.EQUIP_WTYPE).Contains(wtypeId);
      }
      public bool IsEquipATypeOk(int atypeId) {
         return FeaturesSet((int)Enums.Feature.EQUIP_ATYPE).Contains(atypeId);
      }
      public bool IsEquipTypeFixed(int etypeId) {
         return FeaturesSet((int)Enums.Feature.EQUIP_FIX).Contains(etypeId);
      }
      public bool IsEquipTypeSealed(int etypeId) {
         return FeaturesSet((int)Enums.Feature.EQUIP_SEAL).Contains(etypeId);
      }
      public int SlotType() {
         List<int> set = FeaturesSet((int)Enums.Feature.SLOT_TYPE);
         return set.Count > 0 ? set.Max() : 0;
      }
      public bool IsDualWield() {
         return SlotType() == 1;
      }
      public bool SpecialFlag(int flagId) {
         return Features((int)Enums.Feature.SPECIAL_FLAG).Any(ft => ft.data_id == flagId);
      }
      public bool PartyAbility(int abilityId) {
         return Features((int)Enums.Feature.PARTY_ABILITY).Any(ft => ft.data_id == abilityId);
      }
      public bool IsGuard() {
         return SpecialFlag(FLAG_ID_GUARD) && Restriction() < 4;
      }
      public int Restriction() {
         return RPGStates().Select(state => (int)state.restriction).Append(0).Max();
      }
      private bool IsStateRestrict(int stateId) {
         return DataStates[stateId].remove_by_restriction && Restriction() > 0;
      }
      public bool IsSkillLearned(int skillId) {
         return true;
      }
      public bool IsSkillWTypeOk(RPGSkill skill) {
         return true;
      }
      public bool HasAddedSkillType(RPGSkill skill) {
         return true;
      }
      public bool UsableItemConditionsMet(RPGUsableItem item) {
         return Restriction() < 4 && item.occasion < 3;
      }
      public bool SkillConditionsMet(RPGSkill skill) {
         return (IsSkillLearned((int)skill.id) || AddedSkills().Contains((int)skill.id))
            && UsableItemConditionsMet(skill)
            && Mp >= skill.mp_cost
            && IsSkillWTypeOk(skill)
            && !IsSkillSealed((int)skill.id)
            && !IsSkillTypeSealed((int)skill.stype_id)
            && HasAddedSkillType(skill);
      }
      public bool ItemConditionsMet(RPGItem item) {
         return UsableItemConditionsMet(item) && HasItem(item);
      }
      private bool HasItem(RPGItem item) {
         return true;
      }
      public bool Usable(RPGUsableItem item) {
         if (item is RPGSkill skill)
            return SkillConditionsMet(skill);
         if (item is RPGItem inventoryItem)
            return ItemConditionsMet(inventoryItem);
         return false;
      }
      public bool IsDead() {
         return Hp <= 0;
      }
      public float HpRate() {
         return ((float)Hp) / ((float)Mhp);
      }
      public float MpRate() {
         if(Mhp > 0)
            return ((float)Mp) / ((float)Mmp);
         return 0;
      }
      public void Refresh() {
         Hp = Math.Clamp(Hp, 0, Mhp);
         Mp = Math.Clamp(Mp, 0, Mmp);
      }
      public void Die() {
      }
      public bool Pos(int x, int y) {
         return X == x && Y == y;
      }
      public bool PosNt(int x, int y) {
         return Pos(x, y) && !Through;
      }
      public bool IsNormalPriority() {
         return PriorityType == 1;
      }
      public int ReverseDir(int dir) {
         return 10 - dir;
      }
      public bool IsPassable(int x, int y, int d) {
         int x2 = Network.Maps[MapId].RoundXWithDirection(x, d);
         int y2 = Network.Maps[MapId].RoundYWithDirection(y, d);
         if (!Network.Maps[MapId].IsValid(x2, y2))
            return false;
         if (Through)
            return true;
         if (!Network.Maps[MapId].IsPassable(x2, y2, ReverseDir(d)))
            return false;
         if (CollideWithCharacters(x2, y2))
            return false;
         return true;
      }
      public bool IsDiagonalPassable(int x, int y, int horz, int vert) {
         int x2 = Network.Maps[MapId].RoundXWithDirection(x, horz);
         int y2 = Network.Maps[MapId].RoundYWithDirection(y, vert);
         return ((IsPassable(x, y, vert) && IsPassable(x, y2, horz)) || 
                  IsPassable(x, y, horz) && IsPassable(x2, y, vert));
      }
      public bool CollideWithCharacters(int x, int y) {
         return CollideWithEvents(x, y);
      }
      internal bool CollideWithEvents(int x, int y) {
         return Network.Maps[MapId]
             .EventsXYNT(x, y)
             .Any(ev => ev.IsNormalPriority() && !ev.Erased());
      }
      internal bool CollideWithPlayers(int x, int y) {
         return Network.Clients.Values
             .Any(client =>
                 client != null &&
                 client.IsInGame() &&
                 client.MapId == MapId &&
                 client.PosNt(x, y));
      }
      internal bool IsTile() {
         return TileId > 0 && PriorityType == 0;
      }
      public void CheckEventTriggerTouchFront() {
         int x2 = Network.Maps[MapId].RoundXWithDirection(X, Direction);
         int y2 = Network.Maps[MapId].RoundYWithDirection(Y, Direction);
         CheckEventTriggerTouchFront(x2, y2);
      }
      private void CheckEventTriggerTouchFront(int x2, int y2) {
      }
      public void MoveStraight(int d, bool turnOk = true) {
         MoveSucceed = IsPassable(X, Y, d);
         if (MoveSucceed) {
            Direction = d;
            X = Network.Maps[MapId].RoundXWithDirection(X, d);
            Y = Network.Maps[MapId].RoundYWithDirection(Y, d);
            if (Network.Maps[MapId].IsLadder(X, Y))
               Direction = 8;
            SendMovement();
         }else if (turnOk) {
            Direction = d;
            SendMovement();
            CheckEventTriggerTouchFront();
         }
      }
      public void MoveDiagonal(int d) {
         int horz;
         int vert;
         if (d < 7) {
            horz = d + 3;
            vert = 2;
         } else {
            horz = d - 3;
            vert = 8;
         }
         MoveSucceed = IsDiagonalPassable(X, Y, horz, vert);
         if (MoveSucceed) {
            X = Network.Maps[MapId].RoundXWithDirection(X, horz);
            Y = Network.Maps[MapId].RoundYWithDirection(Y, vert);
            if (Network.Maps[MapId].IsLadder(X, Y))
               Direction = 8;
         }
         if (Direction == ReverseDir(horz))
            Direction = horz;
         if (Direction == ReverseDir(vert))
            Direction = vert;
         SendMovement();
      }
      public void MoveTo(int x, int y) {
         X = x;
         Y = y;
         SendMovement();
      }
      private void SendMovement() {}
      public int DistanceXFrom(int x) { return X - x; }
      public int DistanceYFrom(int y) { return Y - y; }
      public void Swap(GameCharacter character) {
         int newX = character.X;
         int newY = character.Y;
         character.MoveTo(X, Y);
         MoveTo(newX, newY); 
      }
      public void UpdateStBfTimers() {
         for (int i = 0; i < BuffsTime.Length; i++) {
            if (Buffs[i] == 0)
               continue;
            if (DateTimeOffset.UtcNow >= BuffsTime[i])
               RemoveBuff(i);
         }
         foreach(var (state_id, timer) in StatesTime) {
            if (DateTimeOffset.UtcNow >= timer) 
               RemoveState(state_id);
         }
      }
   }
}
