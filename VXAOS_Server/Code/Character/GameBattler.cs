using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public class GameBattler : GameCharacter {
      private readonly Dictionary<int, Action<GameBattler, RPGUsableItem, RPGUsableItemEffect>>
            _itemEffectTable;
      public GameBattler() {
         _itemEffectTable = new() {
               { 11, ItemEffectRecoverHP },
               { 12, ItemEffectRecoverMP },
               { 21, ItemEffectAddState },
               { 22, ItemEffectRemoveState },
               { 31, ItemEffectAddBuff },
               { 32, ItemEffectAddDebuff },
               { 33, ItemEffectRemoveBuff },
               { 34, ItemEffectRemoveDebuff },
               { 42, ItemEffectGrow },
               { 43, ItemEffectLearnSkill }
            };
      }
      public Target Target = new();
      public bool IsInFront(GameBattler target) {
         int x = Network.Maps[MapId].RoundXWithDirection(X, Direction);
         int y = Network.Maps[MapId].RoundYWithDirection(Y, Direction);
         return target.Pos(x, y);
      }
      public bool IsInRange(GameBattler target, int range) {
         return Math.Abs(DistanceXFrom(target.X)) <= range && Math.Abs(DistanceYFrom(target.Y)) <= range;
      }
      public void ClearTarget() {
         Target.Id = -1;
         Target.Type = Enums.Target.NONE;
      }
      public GameBattler? GetTarget() {
         if (Target.Type == Enums.Target.ENEMY)
            return Network.Maps[MapId].Events[Target.Id];
         if (Target.Id >= 0)
            return Network.Clients[Target.Id];
         return null;
      }
      public bool IsInGame() { return true; }
      public bool IsValidTarget(GameBattler target) {
         bool result = target.IsInGame() && target.MapId == MapId;
         if (!result)
            ClearTarget();
         return result;
      }
      public float ApplyVariance(float damage, float variance) {
         int amp = Convert.ToInt32(MathF.Max(MathF.Abs(damage) * variance / 100f, 0));
         int var = Rand(amp + 1) + Rand(amp + 1) - amp;
         return damage >= 0 ? damage + var : damage - var;
      }
      public void MakeDamageValue(GameBattler user, RPGUsableItem item, bool critical, int animationId, int aniIndex) {
         float value = item.damage.Eval(user, this, user.Variables);
         value *= ItemElementRate(user, item);
         if (item.IsPhysical())
            value *= Pdr;
         if (item.IsMagical())
            value *= Mdr;
         if (item.damage.IsRecover())
            value *= Rec;
         if (critical)
            value *= 3;
         value = ApplyVariance(value, (float)item.damage.variance);
         value = ApplyGuard(value);
         MakeDamage(value, item, user, critical, animationId, aniIndex);
      }
      private void MakeDamage(float value, RPGUsableItem item, GameBattler user, bool critical, int animationId, int aniIndex) {
         if (item.damage.IsToHp())
            ExecuteHPDamage(-value, critical, user, animationId, aniIndex, item.damage.IsRecover());
         if (item.damage.IsToMp())
            ExecuteMPDamage(-value, false, user, animationId, aniIndex, item.damage.IsRecover());
         if (item.damage.IsDrain() && item.damage.IsToHp())
            user.ExecuteHPDamage(value, critical, user, animationId, aniIndex, true);
         if (item.damage.IsDrain() && item.damage.IsToMp())
            user.ExecuteMPDamage(value, false, user, animationId, aniIndex, true);
      }
      private void ExecuteHPDamage(float damage, bool critical, GameBattler attacker, int animationId, int aniIndex, bool notShowMissed) {
         if (damage < 0 && MathF.Abs(damage) > Hp)
            damage = -Hp;
         if (damage > 0 && Hp + damage > Mhp)
            damage = Mhp - Hp;
         SendAttack((int)damage, 0, critical, attacker.Id, attacker is GameClient ? 0 : 1, aniIndex, animationId, notShowMissed);
         Hp += (int)damage;
         Refresh();
         RemoveStatesByDamage();
         if (IsDead())
            Die();
      }
      private void ExecuteMPDamage(float damage, bool critical, GameBattler attacker, int animationId, int aniIndex, bool notShowMissed) {
         if (damage < 0 && MathF.Abs(damage) > Mp)
            damage = -Mp;
         if (damage > 0 && Mp + damage > Mmp)
            damage = Mmp - Mp;
         SendAttack(0, (int)damage, critical, attacker.Id, attacker is GameClient ? 0 : 1, aniIndex, animationId, notShowMissed);
         Mp += (int)damage;
         Refresh();
      }
      private void SendAttack(int v1, int damage, bool critical, int id, int v2, int aniIndex, int animationId, bool notShowMissed) {
      }
      private float ApplyGuard(float damage) {
         return damage / (damage > 0 && IsGuard() ? 2 * Grd : 1);
      }
      public float ItemHit(GameBattler user, RPGUsableItem item) {
         float rate = (float)item.success_rate * 0.01f;
         if (item.IsPhysical())
            rate *= user.Hit;
         return rate;
      }
      public float ItemEva(GameBattler user, RPGUsableItem item) {
         if (item.IsPhysical())
            return Eva;
         if (item.IsMagical())
            return Mev;
         return 0;
      }
      public float ItemCri(GameBattler user, RPGUsableItem item) {
         return item.damage.critical ? user.Cri * (1 - Cev) : 0;
      }
      private float ItemElementRate(GameBattler user, RPGUsableItem item) {
         if(item.damage.element_id < 0) {
            if (user.AtkElements().Count == 0)
               return 1.0f;
            return ElementsMaxRate(user.AtkElements());
         } else {
            ElementRate((int)item.damage.element_id);
         }
         return 1f;
      }
      private float ElementsMaxRate(List<int> elements) {
         float max = 0f;
         foreach (var i in elements) {
            max = MathF.Max(max, ElementRate(i));
         }
         return max;
      }
      public void ItemApply(GameBattler user, RPGUsableItem item, int animationId, int aniIndex) {
         bool missed = (Rand() >= ItemHit(user, item));
         bool evaded = (!missed && (Rand() < ItemEva(user, item)));
         if (missed || evaded)
            return;
         if (!item.damage.IsNone()) {
            bool critical = (Rand() < ItemCri(user, item));
            MakeDamageValue(user, item, critical, animationId, aniIndex);
         } else if (item.animation_id > 0) {
            byte attackerType = (byte)(user is GameClient ? 0 : 1);
            byte characterType = (byte)(this is GameClient ? 0 : 1);
            //Network.SendAnimation(this, item.animation_id, user.Id, attackerType, aniIndex, characterType);
         }
         if (IsDead())
            return;
         foreach (var effect in item.effects) {
            ItemEffectApply(user, item, effect);
         }
      }
      public void ItemEffectApply(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         if (_itemEffectTable.TryGetValue((int)effect.code, out var handler))
            handler(user, item, effect);
      }
      private void ItemEffectRecoverHP(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         float value = (Mhp * (float)effect.value1 + (float)effect.value2) * Rec;
         if (item is RPGItem)
            value *= user.Pha;
         ExecuteHPDamage(Convert.ToInt32(value), false, user, (int)item.animation_id, item.ani_index, true);
      }
      private void ItemEffectRecoverMP(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         float value = (Mmp * (float)effect.value1 + (float)effect.value2) * Rec;
         if (item is RPGItem)
            value *= user.Pha;
         ExecuteMPDamage(Convert.ToInt32(value), false, user, (int)item.animation_id, item.ani_index, true);
      }
      private void ItemEffectAddState(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         if(effect.data_id == 0) {
            ItemEffectAddStateAttack(user, item, effect);
         } else {
            ItemEffectAddStateNormal(user, item, effect);
         }
      }
      private void ItemEffectAddStateAttack(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         foreach(var stateId in user.AtkStates()) {
            float chance = (float)effect.value1;
            chance *= StateRate(stateId);
            chance *= user.AtkStatesRate(stateId);
            chance *= LukEffectRate(user);
            if (Rand() < chance)
               AddState(stateId);
         }
      }
      private void ItemEffectAddStateNormal(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         int stateId = (int)effect.data_id;
         float chance = (float)effect.value1;
         if(this is GameClient && !(user is GameClient)) {
            chance *= StateRate(stateId);
            chance *= LukEffectRate(user);
         }
         if (Rand() < chance)
            AddState(stateId);
      }
      private void ItemEffectRemoveState(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         float chance = (float)(effect.value1);
         if (Rand() < chance)
            RemoveState((int)effect.data_id);
      }
      private void ItemEffectAddBuff(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         AddBuff((int)effect.data_id, (int)effect.value1);
      }
      private void ItemEffectAddDebuff(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         int debuffId = (int)effect.data_id;
         float chance = DebuffRate(debuffId) * LukEffectRate(user);
         if(Rand() < chance)
            AddDeBuff(debuffId, (int)effect.value1);
      }
      private void ItemEffectRemoveBuff(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         int buffId = (int)effect.data_id;
         if (Buffs[buffId] > 0)
            RemoveBuff(buffId);
      }
      private void ItemEffectRemoveDebuff(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         int debuffId = (int)effect.data_id;
         if (Buffs[debuffId] < 0)
            RemoveBuff(debuffId);
      }
      private void ItemEffectGrow(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         AddParam((int)effect.data_id, (int)effect.value1);
      }
      private void ItemEffectLearnSkill(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {

      }
      private float LukEffectRate(GameBattler user) {
         return MathF.Max(0.0f, (1.0f + (user.Luk - Luk) * 0.001f));
      }
      public Point MaxPassage(GameCharacter target) {
         if (Pos(target.X, target.Y))
            return new Point(target.X, target.Y);
         float radians = MathF.Atan2(target.X - X, target.Y - Y);
         float speedX = MathF.Sin(radians);
         float speedY = MathF.Cos(radians);
         int resultX = target.X;
         int resultY = target.Y;
         int rangeX = Math.Abs(target.X - X);
         int rangeY = Math.Abs(target.Y - Y);
         int direction = ProjectileDirection(target);
         float x = X;
         float y = Y;
         while (true) {
            x += speedX;
            y += speedY;
            int x2 = (int)x;
            int y2 = (int)y;
            if (Math.Abs(DistanceXFrom(x2)) > rangeX ||
                Math.Abs(DistanceYFrom(y2)) > rangeY)
               break;
            if (!MapPassable(x2, y2, direction)) {
               resultX = x2;
               resultY = y2;
               break;
            }
         }
         return new Point(resultX, resultY);
      }
      public int ProjectileDirection(GameCharacter target) {
         int sx = DistanceXFrom(target.X);
         int sy = DistanceYFrom(target.Y);
         if (Math.Abs(sx) > Math.Abs(sy))
            return sx > 0 ? (int)(int)Enums.Dir.LEFT : (int)Enums.Dir.RIGHT;
         if (sy != 0)
            return sy > 0 ? (int)Enums.Dir.UP : (int)Enums.Dir.DOWN;
         return Direction;
      }
      public bool BlockedPassage(GameCharacter target, int x, int y) {
         return !target.Pos(x, y);
      }
      public bool MapPassable(int x, int y, int d) {
         return Network.Maps[MapId].IsValid(x, y) &&
                Network.Maps[MapId].IsPassable(x, y, d);
      }
      public void ClearTargetPlayers(Enums.Target type, int mapId = -1) {
         if(mapId < 0) mapId = MapId;
         if (Network.Maps[mapId].HasZeroPlayers())
            return;
         foreach(var client in Network.Clients.Values) {
            if (client.IsInGame() && client.MapId == mapId) {
               if (client.Target.Type == type && client.Target.Id == Id)
                  client.ClearTarget();
            }
         }
      }
   }
}
