using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      private const int _effectCommonEvent = 44;
      public void AttackNormal() {
         if (Restriction() == 4) return;
         WeaponAttackTime = DateTimeOffset.UtcNow.AddSeconds(Configs.AttackTime);
         int aniIndex = (DataWeapons[WeaponId].ani_index).OrDefaultValue(8, CharacterIndex);
         foreach(var enemy in Network.Maps[MapId].Events.Values) {
            if (enemy == null || enemy.IsDead() || !IsInFront(enemy)) continue;
            HitEnemy(enemy, (int)DataWeapons[WeaponId].animation_id, aniIndex, DataSkills[AttackSkillId]);
            return;
         }
         if (!Network.Maps[MapId].PvPAble()) return;
         foreach(var client in Network.Clients.Values) {
            if (client == null || !client.IsInGame() || client.MapId != MapId || client.IsDead() ||
               !IsInFront(client) || client.IsAdmin() || HasProtectionLevel(client)) continue;
            HitPlayer(client, (int)DataWeapons[WeaponId].animation_id, aniIndex, DataSkills[AttackSkillId]);
            return;
         }
      }
      public void AttackRange() {
         if (Restriction() == 4) return;
         WeaponAttackTime = DateTimeOffset.UtcNow.AddSeconds(Configs.AttackTime);
         int itemId = Configs.RangeWeapons[WeaponId].ItemId;
         if (itemId > 0 && !HasItem(DataItems[itemId])) return;
         int? mpCost = Configs.RangeWeapons[WeaponId].MpCost;
         if (mpCost != null && Mp < mpCost) return;
         var target = GetTarget();
         if (target == null || !IsInRange(target, Configs.RangeWeapons[WeaponId].Range)) return;
         if (itemId > 0) LoseItem(DataItems[itemId], 1);
         if (mpCost != null) Mp -= (int)mpCost;
         var xy = MaxPassage(target);
         Network.SendAddProjectile(this, (short)xy.X, (short)xy.Y, target, (byte)Enums.Projectile.WEAPON, (byte)WeaponId);
         if(BlockedPassage(target, xy.X, xy.Y)) return;
         int aniIndex = (DataWeapons[WeaponId].ani_index).OrDefaultValue(8, CharacterIndex);
         if(Target.Type == Enums.Target.PLAYER && IsValidTarget(target) && Network.Maps[MapId].PvP && 
            !((GameClient)target).IsAdmin() && !HasProtectionLevel((GameClient)target)) {
            HitPlayer((GameClient)target, (int)DataWeapons[WeaponId].animation_id, aniIndex, DataSkills[AttackSkillId]);
         } else if (Target.Type == Enums.Target.ENEMY && !target.IsDead()) {
            HitEnemy((GameEvent)target, (int)DataWeapons[WeaponId].animation_id, aniIndex, DataSkills[AttackSkillId]);
         }
      }
      public void UseItem(RPGUsableItem usableItem) {
         if(!IsUsable(usableItem) || usableItem.level > Level) return;
         if(usableItem is RPGSkill skill) {
            SkillCooldownTime.TryAdd((int)usableItem.id, DateTimeOffset.UtcNow.AddSeconds(skill.cooldown));
            Network.SendPlayerCooldown(this, (short)usableItem.id);
            Mp -= (int)skill.mp_cost;
         }else if (usableItem is RPGItem item) {
            ItemAttackTime = DateTimeOffset.UtcNow.AddSeconds(Configs.CooldownItemTime);
            ConsumeItem(item);
         }
         foreach(var effect in usableItem.effects) {
            ItemGlobalEffectApply(effect);
         }
         var scope = (Enums.Item)usableItem.scope;
         if (scope == Enums.Item.SCOPE_ALL_ALLIES) {
            ItemPartyRecovery(usableItem);
         } else if (
               scope >= Enums.Item.SCOPE_ENEMY &&
               scope <= Enums.Item.SCOPE_ALLIES_KNOCKED_OUT) {
            if (usableItem.IsAoe()) {
               ItemAttackArea(usableItem);
            } else {
               ItemAttackNormal(usableItem);
            }
         } else if (scope == Enums.Item.SCOPE_USER) {
            ItemRecover(usableItem);
         }
      }
      public void ConsumeItem(RPGItem item) {
         if (!item.consumable) return;
         LoseItem(item, 1);
         if (IsInTrade() && TradeItemNumber(item) > ItemNumber(item))
            LoseTradeItem(item, 1);
      }
      public void ItemGlobalEffectApply(RPGUsableItemEffect effect) {
         if (effect.code != _effectCommonEvent) return;
         int effectId = (int)effect.data_id;
         if (CommonEvents.TryGetValue(effectId, out var cmnEv) &&
            cmnEv != null && cmnEv.IsRunning) return;
         CommonEvents[effectId] = new GameInterpreter();
         CommonEvents[effectId].Setup(this, DataCommonEvents[effectId].list, -effectId, DataCommonEvents[effectId]);
         if(!CommonEvents[effectId].IsRunning)
            CommonEvents.Remove(effectId);
      }
      public void ItemAttackNormal(RPGUsableItem item) {
         var target = GetTarget();
         if (target == null || target.IsDead() || !IsInRange(target, (int)item.range) ||
            Target.Type == Enums.Target.ENEMY && item.IsForFriend()) {
            if (item.IsForFriend())
               ItemApply(this, item, (int)item.animation_id, item.ani_index);
            return;
         }
         var xy = MaxPassage(target);
         if(item is RPGSkill && Configs.RangeSkills.ContainsKey((int)item.id))
            Network.SendAddProjectile(this, (short)xy.X, (short)xy.Y, target, (byte)Enums.Projectile.SKILL, (byte)item.id);
         if (BlockedPassage(target, xy.X, xy.Y)) return;
         if (Target.Type == Enums.Target.PLAYER && IsValidTarget(target)){
            if(item.IsForFriend() || Network.Maps[MapId].PvP &&
               !((GameClient)target).IsAdmin() && !HasProtectionLevel((GameClient)target))
               HitPlayer((GameClient)target, (int)item.animation_id, item.ani_index, item);
         } else if (Target.Type == Enums.Target.ENEMY && !target.IsDead()) {
            HitEnemy((GameEvent)target, (int)item.animation_id, item.ani_index, item);
         }
      }
      public void ItemAttackArea(RPGUsableItem item) {
         foreach(var enemy in Network.Maps[MapId].Events.Values) {
            if (enemy.IsDead() || !IsInRange(enemy, item.aoe)) continue;
            HitEnemy(enemy, 0, 8, item);
         }
         if (Network.Maps[MapId].PvPAble()) {
            foreach(var client in Network.Clients.Values) {
               if (client == null || !client.IsInGame() || client.MapId != MapId || client.IsDead() ||
               !IsInRange(client, item.aoe) || client.IsAdmin() || HasProtectionLevel(client)) continue;
               HitPlayer(client, 0, 8, item);
            }
         }
         Network.SendAnimation(this, (short)item.animation_id, (short)Id, 0, (byte)item.ani_index, (byte)Enums.Target.PLAYER);
      }
      public void ItemRecover(RPGUsableItem item) {
         ItemApply(this, item, (int)item.animation_id, item.ani_index);
      }
      public bool HasProtectionLevel(GameClient target) {
         return Level < Configs.MinLevelPvp || target.Level < Configs.MinLevelPvp;
      }
      public void HitPlayer(GameClient? client, int animationId, int aniIndex, RPGUsableItem item) {
         ChangeTarget(client.Id, Enums.Target.PLAYER);
         client.ItemApply(this, item, animationId, aniIndex);
      }
      public void HitEnemy(GameEvent? enemy, int animationId, int aniIndex, RPGUsableItem item) {
         ChangeTarget(enemy.Id, Enums.Target.ENEMY);
         enemy.Target.Id = Id;
         enemy.ItemApply(this, item, animationId, aniIndex);
      }
      internal override void SendAttack(int hpDamage, int mpDamage, bool critical, int attackerId,
            int attackerType, int aniIndex, int animationId, bool notShowMissed) {
         Network.SendAttackPlayer(MapId, hpDamage, mpDamage, critical, (short)attackerId, 
            (byte)attackerType, (byte)aniIndex, Id, (short)animationId, notShowMissed);
      }
      public override void Die() {
         LoseGold(Gold * ServerConfig.LoseGoldRate / 100);
         LoseExp(Convert.ToInt32(Exp * LoseExpRate() / 100));
         RecoverAll();
         RemoveStatesOnDeath();
         Transfer(ReviveMapId, ReviveX, ReviveY, (byte)Enums.Dir.DOWN);
         if (HasText()) {
            EventInterpreter.FinalizeRun();
            CloseEventMessage();
         }
      }
   }
}
