using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameEvent : GameBattler {
      internal int PrelockDirection = 2;
      internal List<RPGEventPage> Pages;
      internal RPGEventPage? Page;
      internal DateTimeOffset StopCount;
      internal bool Escape = false;
      internal int Sight = 10;
      internal int StopCountThreshold = 1;
      internal int BattleStopCountThreshold = 1;
      internal int? Trigger = null;
      internal int MoveType = 0;
      internal int MoveFrequency = 3;
      internal RPGMoveRoute MoveRoute = new();
      internal int MoveRouteIndex;
      internal int EnemyId = 0;
      internal int FrequencyBattle;
      internal int RegionId;
      internal DateTimeOffset ActionTime;
      internal DateTimeOffset ReviveTime;
      public int CharacterIndex;
      public DateTimeOffset? ParallelProcessWaiting;
      public List<int> Locked = new();
      public GameInterpreter? Interpreter;
      public List<RPGEventCommand>? List;
      public GameEvent(int id, RPGEvent @event, int mapId) {
         InitializeEnemy();
         Id = id;
         MapId = mapId;
         X = (int)@event.x;
         Y = (int)@event.y;
         Pages = @event.pages;
         MoveSucceed = true;
         MoveRouteForcing = false;
         ParallelProcessWaiting = null;
         ClearEnemy();
         ClearTarget();
         ClearStates();
         ClearBuffs();
         Refresh();
      }
      public bool IsEnemy() {
         return EnemyId > 0;
      }
      public bool IsInBattle() {
         return Target.Id >= 0;
      }
      internal bool Erased() {
         return IsEnemy() && IsDead();
      }
      public RPGEnemy Enemy() {
         return DataEnemies[EnemyId];
      }
      public void Lock(GameClient client) {
         if(Locked.Count == 0)
            PrelockDirection = Direction;
         TurnTowardCharacter(client);
         Locked.Add(client.Id);
      }
      public void Unlock(int clientId) {
         if (!Locked.Contains(clientId)) return;
         Locked.Remove(clientId);
         if (Locked.Count == 0) {
            Direction = PrelockDirection;
            SendMovement();
         }
      }
      public void ClearEnemy() {
         ActionTime = DateTimeOffset.UtcNow.AddSeconds(Configs.AttackTime);
         ReviveTime = DateTimeOffset.UtcNow;
         Escape = false;
         _hp = 0;
         Sight = 10;
      }
      public override void Refresh() {
         var newPage = FindGlobalProperPage();
         if(newPage == null || !ReferenceEquals(newPage, Page))//newPage != Page
            SetupPage(newPage);
         StopCount = DateTimeOffset.UtcNow.AddSeconds(Rand(StopCountThreshold));
      }
      public void SetupPage(RPGEventPage? newPage) {
         Page = newPage;
         if(Page == null) {
            ClearPageSettings();
         } else {
            SetupPageSettings();
         }
      }
      public void ClearPageSettings() {
         CharacterIndex = 0;
         TileId = 0;
         Direction = (int)Enums.Dir.DOWN;
         MoveType = 0;
         MoveFrequency = 3;
         Through = false;
         Trigger = null;
         List = null;
         Interpreter = null;
         EnemyId = 0;
         StopCountThreshold = 1;
         BattleStopCountThreshold = 1;
         ClearEnemy();
         ClearTarget();
      }
      public void SetupPageSettings() {
         if(Page == null) {
            ClearPageSettings();
            return;
         }
         CharacterIndex = (int)Page.graphic.character_index;
         TileId = (int)Page.graphic.tile_id;
         Direction = (int)Page.graphic.direction;
         MoveType = (int)Page.move_type;
         MoveFrequency = (int)Page.move_frequency;
         MoveRoute = Page.move_route;
         MoveRouteIndex = 0;
         MoveRouteForcing = false;
         Through = Page.through;
         Trigger = (int)Page.trigger;
         List = Page.list;
         Interpreter = Trigger == 4 ? new GameInterpreter() : null;
         (EnemyId, FrequencyBattle, RegionId) = GetBattleParameters();
         StopCountThreshold = GetStopCountThreshold() / 40;
         BattleStopCountThreshold = GetBattleStopCountThreshold() / 40;
         if (IsEnemy()) {
            SetupEnemySettings();
         } else {
            ClearEnemy();
            ClearTarget();
         }
      }
      public void SetupEnemySettings() {
         for(int i = 0; i < 8; i++) {
            ParamBase[i] = (int)Enemy().@params[i];
         }
         Sight = Enemy().sight;
         Escape = Enemy().escape;
         _hp = Mhp;
         _mp = Mmp;
      }
      public (int, int, int) GetBattleParameters() {
         if (List != null && List[0].code == 108  && List[0].parameters[0].AsString().StartsWith("Enemy")) {
            int enemyId = int.Parse(List[0].parameters[0].AsString().Split('=')[1].Trim());
            int frequency = List[1].code == 408 ? 
               int.Parse(List[1].parameters[0].AsString().Split('=')[1].Trim()) : MoveFrequency;
            int regionId = List.Count > 2 && List[2].code == 408 ? 
               int.Parse(List[2].parameters[0].AsString().Split('=')[1].Trim()) : 1;
            return (enemyId, frequency, regionId);
         }
         return (0, 0, 0);
      }
      public override bool CollideWithCharacters(int x, int y) {
         return base.CollideWithCharacters(x, y) || CollideWithPlayers(x, y);
      }
      internal int GetStopCountThreshold() {
         return 30 * (5 - MoveFrequency);
      }
      internal int GetBattleStopCountThreshold() {
         return 30 * (5 - FrequencyBattle);
      }
      internal RPGEventPage? FindProperPage(GameClient client) {
         if(client  == null) return null;
         for (int i = Pages.Count - 1; i >= 0; i--) {
            if (ConditionsMet(client, Pages[i]))
               return Pages[i];
         }
         return null;
      }
      internal bool ConditionsMet(GameClient client, RPGEventPage page) {
         var c = page.condition;
         if (c.switch1_valid) {
            int swId = (int)c.switch1_id;
            if (swId <= Configs.MaxPlayerSwitches && !client.Switches[swId] || !Network.Switches[swId])
               return false;
         }
         if (c.switch2_valid) {
            int swId = (int)c.switch2_id;
            if (swId <= Configs.MaxPlayerSwitches && !client.Switches[swId] || !Network.Switches[swId])
               return false;
         }
         if (c.variable_valid) {
            if (client.Variables[(int)c.variable_id] < c.variable_value)
               return false;
         }
         if (c.self_switch_valid) {
            if (client.SelfSwitches[(client.MapId, Id, c.self_switch_ch[0])])
               return false;
         }
         if (c.item_valid) {
            if (client.Items.ContainsKey((int)c.item_id))
               return false;
         }
         return true;
      }
      internal RPGEventPage? FindGlobalProperPage() {
         for (int i = Pages.Count - 1; i >= 0; i--) {
            if (GlobalConditionsMet(Pages[i]))
               return Pages[i];
         }
         return null;
      }
      internal bool GlobalConditionsMet(RPGEventPage page) {
         var c = page.condition;
         if (c.switch1_valid) {
            int swId = (int)c.switch1_id;
            if (swId > Configs.MaxPlayerSwitches && !Network.Switches[swId])
               return false;
         }
         if (c.switch2_valid) {
            int swId = (int)c.switch2_id;
            if (swId > Configs.MaxPlayerSwitches && !Network.Switches[swId])
               return false;
         }
         return true;
      }
      internal void CheckEventTriggerTouch(int x, int y) {
         if (Trigger != 2 || IsEnemy()) return;
         foreach(var client in Network.Clients.Values) {
            if (client == null || !client.IsInGame() || client.MapId != MapId || client.Pos(x, y) ||
               IsNormalPriority() || !client.EventInterpreter.IsRunning) continue;
            StartClient(client);
            break;
         }
      }
      internal void Update() {
         if (!MoveRouteForcing)
            UpdateSelfMovement();
         if (IsEnemy())
            UpdateEnemy();
         UpdateParallelProcess();
      }
      internal void UpdateSelfMovement() {
         if (Erased() || StopCount > DateTimeOffset.UtcNow || Locked.Count > 0)
            return;
         StopCount = DateTimeOffset.UtcNow.AddSeconds(IsInBattle() ? BattleStopCountThreshold : StopCountThreshold);
         if(MoveType == (int)Enums.Move.FIXED && IsEnemy()) {
            SelectTarget();
         }else if (MoveType == (int)Enums.Move.RANDOM && !IsInBattle()) {
            MoveRandom();
         }else if (MoveType == (int)Enums.Move.TOWARD_PLAYER || IsInBattle()) {
            MoveTypeTowardPlayer();
         }
      }
      internal void SelectTarget() {
         if (Network.Maps[MapId].HasZeroPlayers()) return;
         var target = GetTarget();
         if(!IsNearThePlayer(target))
            target = FindTarget();
         if(target != null) {
            Target.Id = target.Id;
         } else {
            ClearTarget();
         }
      }
      internal void MoveTypeTowardPlayer() {
         if (Network.Maps[MapId].HasZeroPlayers()) {
            ClearTarget();
            MoveRandom();
            return;
         }
         var target = GetTarget();
         if(!IsNearThePlayer(target))
            target = FindTarget();
         if(target == null) {
            ClearTarget();
            MoveRandom();
            return;
         }
         Target.Id = target.Id;
         if(Escape && Hp < (Mhp / 3)) {
            MoveAwayFromCharacter(target);
         } else {
            MoveTowardCharacter(target);
         }
      }
      internal bool IsNearThePlayer(GameBattler? target) {
         return target != null && IsValidTarget(target) && IsInRange(target, Sight + 5);
      }
      public override void SendMovement() {
         Network.SendEventMovement(this);
      }
      public GameBattler? FindTarget() {
         GameBattler? target = null;
         foreach(var client in Network.Clients.Values) {
            if (client != null && client.IsInGame() && client.MapId == MapId && IsInRange(client, Sight)) {
               target = client;
               break;
            }
         }
         if (target != null && ServerConfig.EnemyAttackBalloonId > 0)
            Network.SendEnemyBallon((GameClient)target, (short)Id, (byte)ServerConfig.EnemyAttackBalloonId);
         return target;
      }
      public void UpdateParallelProcess() {
         if (Interpreter == null) return;
         if(ParallelProcessWaiting != null && DateTimeOffset.UtcNow >= ParallelProcessWaiting) {
            ParallelProcessWaiting = null;
            Interpreter.Resume();
         }else if (ParallelProcessWaiting == null) {
            Interpreter.Setup(null, List, Id, this);
         }
      }
      internal bool IsEmpty(List<RPGEventCommand>? list) {
         return list == null || list.Count <= 1;
      }
      internal bool IsTriggerIn(List<int> triggers) {
         if (Trigger == null) return false;
         return triggers.Contains((int)Trigger);
      }
      internal void StartClient(GameClient client) {
         var page = FindProperPage(client);
         if (page == null || IsEmpty(page.list)) return;
         if (IsTriggerIn([0, 1, 2]))
            Lock(client);
         client.EventInterpreter.Setup(client, page.list, Id);
      }
   }
}
