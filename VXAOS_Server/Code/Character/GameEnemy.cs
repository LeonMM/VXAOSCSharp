using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameEvent : GameBattler {
      private Dictionary<int, Func<int, int, bool>> _conditionsMetTable = new();
      internal void InitializeEnemy() {
         _conditionsMetTable = new() {
            {2, IsConditionsMetHp},
            {3, IsConditionsMetMp},
            {4, IsConditionsMetState},
            {5, IsConditionsMetLevel},
            {6, IsConditionsMetSwitch}
         };
      }
      internal void UpdateEnemy() {
         if (!IsDead())
            UpdateStBfTimers();
         if (IsInBattle()) {
            MakeActions();
         }else if (IsDead() && DateTimeOffset.UtcNow > ReviveTime) {
            Revive();
         }
      }
      internal void Revive() {
         _hp = Mhp;
         _mp = Mmp;
         Network.SendEnemyRevive(this);
         if (MoveType != (int)Enums.Move.FIXED)
            ChangePosition();
      }
      public override void Die() {
         ReviveTime = DateTimeOffset.UtcNow.AddSeconds(Enemy().revive_time);
         ClearTargetPlayers(Enums.Target.ENEMY);
         ClearStates();
         ClearBuffs();
         Treasure();
         Disable();
         ClearTarget();
      }
      internal void Treasure() {
         if(Network.Clients.TryGetValue(Target.Id, out var client)) {
            var enemy = Enemy();
            if (client.IsInParty()) {
               client.PartyShare((int)(enemy.exp * ServerConfig.ExpBonus), 
                  (int)(Rand((int)enemy.gold) * ServerConfig.GoldBonus), EnemyId);
            } else {
               client.GainExp((int)(enemy.exp * ServerConfig.ExpBonus * client.VipExpBonus()));
               client.GainGold((int)(Rand((int)enemy.gold) * ServerConfig.GoldBonus * client.GoldRate() * client.VipGoldBonus()),
                  false, true);
               client.AddKillsCount(EnemyId);
            }
            DropItems();
         }
      }
      internal void DropItems() {
         if (Network.Clients.TryGetValue(Target.Id, out var client)) {
            foreach(var drop in Enemy().drop_items) {
               if (drop.kind == 0 || Rand() * drop.denominator > (ServerConfig.DropBonus + client.DropItemRate() + client.VipDropBonus() - 2))
                  continue;
               if (Network.Maps[MapId].IsFullDrops()) break;
               Network.Maps[MapId].AddDrop((int)drop.data_id, (int)drop.kind, 1, X, Y, client.Name, client.PartyId);
            }
         }
      }
      internal void Disable() {
         if (Network.Clients.TryGetValue(Target.Id, out var client)) { 
            var enemy = Enemy();
            if (enemy.disable_variable_id > 0)
               client.Variables[enemy.disable_variable_id]++;
            if(enemy.disable_switch_id > Configs.MaxPlayerSwitches) {
               Network.Switches[enemy.disable_switch_id] = !Network.Switches[enemy.disable_switch_id];
               Network.SendEnemyRevive(this);
            } else if (enemy.disable_switch_id > 0) {
               client.Switches[enemy.disable_switch_id] = !client.Switches[enemy.disable_switch_id];
            }
         }
      }
      internal void ChangePosition() {
         for (int i = 0; i < Network.Maps[MapId].ReviveRegions[RegionId - 1].Count; i++) {
            int regionId = Rand(Network.Maps[MapId].ReviveRegions[RegionId - 1].Count);
            int x = Network.Maps[MapId].ReviveRegions[RegionId - 1][regionId].X;
            int y = Network.Maps[MapId].ReviveRegions[RegionId - 1][regionId].Y;
            if(IsPassable(x, y, 0)) {
               MoveTo(x, y);
               break;
            }
         }
      }
      internal override IEnumerable<RPGBaseItem> FeatureObjects() {
         foreach (var obj in base.FeatureObjects())
            yield return obj;
         yield return DataEnemies[EnemyId];
      }
      public void MakeActions() {
         if (Restriction() == 4 || ActionTime > DateTimeOffset.UtcNow) return;
         var actionList = ValidActions();
         if(actionList.Count > 0) {
            var action = actionList[Rand(actionList.Count)];
            ActionTime = DateTimeOffset.UtcNow.AddSeconds(Configs.AttackTime);
            if (action.skill_id == AttackSkillId) {
               AttackNormal();
            } else {
               UseItem((int)action.skill_id);
            }
         }
      }
      List<RPGEnemyAction> ValidActions() {
         var result = new List<RPGEnemyAction>();
         foreach (var action in DataEnemies[EnemyId].actions) {
            if (IsActionValid(action))
               result.Add(action);
         }
         return result;
      }
      internal bool IsActionValid(RPGEnemyAction action) {
         return IsActionConditionsMet(action) && IsUsable(DataSkills[(int)action.skill_id]);
      }
      internal bool IsActionConditionsMet(RPGEnemyAction action) {
         if (_conditionsMetTable.TryGetValue((int)action.condition_type, out var handler))
            return handler((int)action.condition_param1, (int)action.condition_param2);
         return true;
      }
      internal bool IsConditionsMetHp(int param1, int param2) {
         return HpRate() >= param1 && HpRate() <= param2;
      }
      internal bool IsConditionsMetMp(int param1, int param2) {
         return MpRate() >= param1 && MpRate() <= param2;
      }
      internal bool IsConditionsMetState(int param1, int param2) {
         return HasState(param1);
      }
      internal bool IsConditionsMetLevel(int param1, int param2) {
         if (Network.Clients.TryGetValue(Target.Id, out var client)) {
            return IsValidTarget(client) && client.Level >= param1;
         }
         return true;
      }
      internal bool IsConditionsMetSwitch(int param1, int param2) {
         if (param1 > Configs.MaxPlayerSwitches) {
            return Network.Switches[param1];
         } else if (param1 > 0) {
            if (Network.Clients.TryGetValue(Target.Id, out var client)) {
               return IsValidTarget(client) && client.Switches[param1];
            }
         }
         return true;
      }
      internal void AttackNormal() {
         foreach (var client in Network.Clients.Values) {
            if (client == null || !client.IsInGame() || client.MapId != MapId || 
               client.IsDead() || !IsInFront(client)) continue;
            int aniIndex = (DataEnemies[EnemyId].ani_index).OrDefaultValue(8, CharacterIndex);
            client.ItemApply(this, DataSkills[AttackSkillId], (int)DataSkills[AttackSkillId].animation_id, aniIndex);
            return;
         }
      }
      internal void UseItem(int itemId) {
         var item = DataSkills[itemId];
         var scope = (Enums.Item)item.scope;
         if (scope >= Enums.Item.SCOPE_ENEMY && scope <= Enums.Item.SCOPE_ALLIES_KNOCKED_OUT) {
            if (item.IsAoe()) {
               ItemAttackArea(item);
            } else {
               ItemAttackNormal(item);
            }
         } else if (scope == Enums.Item.SCOPE_USER) {
            ItemRecover(item);
         }
      }
      public void ItemAttackNormal(RPGSkill item) {
         var target = GetTarget();
         if (target == null || !IsValidTarget(target) || target.IsDead() || !IsInRange(target, (int)item.range)) return;
         var xy = MaxPassage(target);
         if (BlockedPassage(target, xy.X, xy.Y)) return;
         if (Configs.RangeSkills.ContainsKey((int)item.id))
            Network.SendAddProjectile(this, (short)xy.X, (short)xy.Y, target, (byte)Enums.Projectile.SKILL, (byte)item.id);
         target.ItemApply(this, item, (int)item.animation_id, (int)item.ani_index);
         Mp -= (int)item.mp_cost;
      }
      public void ItemAttackArea(RPGSkill item) {
         bool used = false;
         foreach (var client in Network.Clients.Values) {
            if (client != null && IsValidTarget(client) && !client.IsDead() && IsInRange(client, item.aoe)) {
               client.ItemApply(this, item, 0, 8);
               used = true;
            }
         }
         if (used) {
            Network.SendAnimation(this, (short)item.animation_id, (short)Id, 0, (byte)item.ani_index, (byte)Enums.Target.ENEMY);
            Mp -= (int)item.mp_cost;
         }
      }
      public void ItemRecover(RPGSkill item) {
         Mp -= (int)item.mp_cost;
         ItemApply(this, item, (int)item.animation_id, item.ani_index);
      }
      internal override void SendAttack(int hpDamage, int mpDamage, bool critical, int attackerId,
            int attackerType, int aniIndex, int animationId, bool notShowMissed) {
         Network.SendAttackEnemy(
            MapId,hpDamage, mpDamage, critical, 
            (short)attackerId, (byte)attackerType, (byte)aniIndex, Id, (short)animationId
         );
      }
   }
}
