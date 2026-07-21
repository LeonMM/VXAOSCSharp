using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public class GameInterpreter {
      private static readonly MemoryCache _globalScriptCache = new(new MemoryCacheOptions());
      private static readonly ScriptOptions _scriptOptions = ScriptOptions.Default.
         WithReferences(typeof(GameInterpreter).Assembly, typeof(DataManager).Assembly).
         WithImports("System", "System.Math", "VXAOS_Server", "static VXAOS_Server.DataManager");
      private SemaphoreSlim Semaphore = new SemaphoreSlim(0, 1);
      private List<RPGEventCommand> List = new();
      private GameClient? Client;
      private int EventId = 0;
      private int Index;
      private int MapId;
      private GameEvent? ObjEvent;
      private RPGCommonEvent? ObjCommonEvent;
      private JArray Params;
      private int Indent;
      private Dictionary<int, int> Branch;
      public bool IsRunning = false;
      private List<int> CmdExcludeList = new List<int>() {
            111, 113, 115, 117, 119, 121, 124, 138, 203, 204,
            213, 221, 222, 223, 224, 225, 230, 231, 232, 233,
            234, 235, 236, 241, 242, 243, 244, 245, 246, 249,
            250, 251, 261, 282, 284, 355, 411, 413 };
      public void Setup(GameClient? client, List<RPGEventCommand> list, int eventId = 0, object? obj = null) {
         Client = client;
         if (Client != null)
            MapId = Client.MapId;
         EventId = eventId;
         List = list;
         Branch = new();
         Index = 0;
         switch (obj) {
            case (GameEvent ev):
               ObjEvent = ev;
               ObjCommonEvent = null;
               break;
            case (RPGCommonEvent ev):
               ObjEvent = null;
               ObjCommonEvent = ev;
               break;
         }
         IsRunning = true;
         Task.Run(async () => await RunAsync());
      }
      private async Task RunAsync() {
         while (Index < List.Count) {
            await ExecuteCommand();
            Index++;
         }
         FinalizeRun();
      }
      private void FinalizeRun() {
         IsRunning = false;
         if (EventId > 0 && Client != null) {
            //Network.Maps[MapId].Events[EventId].Unlock(Client.Id);
         }
      }
      public void Resume() {
         Semaphore.Release();
      }
      private async Task ExecuteCommand() {
         if (Client == null && CmdExcludeList.Contains((int)List[Index].code))
            return;
         var command = List[Index];
         Params = command.parameters;
         Indent = (int)command.indent;
         switch ((int)command.code) {
            case 101: await ShowMessage(); break;
            case 102: await ShowChoice(); break;
            case 103: await StoreNumber(); break;
            case 104: await SelectItem(); break;
            case 105: await ShowScrollingText(); break;
            case 111: await Condition(); break;
            case 113: await BreakCycle(); break;
            case 115: await StopEvent(); break;
            case 117: await CommonEvent(); break;
            case 119: await GoToLabel(); break;
            case 121: await ChangeSwitch(); break;
            case 122: await ChangeVariable(); break;
            case 123: await ChangeSelfSwitches(); break;
            case 125: await ChangeGold(); break;
            case 126: await ChangeItem(); break;
            case 127: await ChangeWeapon(); break;
            case 128: await ChangeArmor(); break;
            case 201: await TransferPlayer(); break;
            case 203: await ChangeEventPosition(); break;
            case 212: await ShowAnimation(); break;
            case 213: await ShowBalloon(); break;
            case 223: await ScreenTint(); break;
            case 224: await FlashEffect(); break;
            case 225: await TremorEffect(); break;
            case 230: await Wait(); break;
            case 232: await MoveImage(); break;
            case 234: await ImageTone(); break;
            case 236: await ClimateOptions(); break;
            case 285: await PositionInformation(); break;
            case 302: await OpenShop(); break;
            case 311: await ChangeHp(); break;
            case 312: await ChangeMp(); break;
            case 313: await ChangeState(); break;
            case 314: await RecoverAll(); break;
            case 315: await ChangeExp(); break;
            case 316: await ChangeLevel(); break;
            case 317: await ChangeParam(); break;
            case 318: await ChangeSkill(); break;
            case 319: await ChangeEquip(); break;
            case 321: await ChangeClass(); break;
            case 322: await ChangeGraphic(); break;
            case 353: await GameOver(); break;
            case 355: await CallScript(); break;
            case 402: await Choice(); break;
            case 403: await Cancel(); break;
            case 411: await CmdException(); break;
            case 413: await RepeatAbove(); break;
            case 124 or 138 or 204 or 221 or 222 or 231 or 233 or 235 or
                  241 or 242 or 243 or 244 or 245 or 246 or 249 or 250 or
                  251 or 261 or 282 or 284:
               await DefaultEventCommand(); break;
         }
      }
      internal Task CommandSkip() {
         while (List[Index + 1].indent > Indent) {
            Index++;
         }
         return Task.CompletedTask;
      }
      internal int? NextEventCode() {
         if (Index + 1 < List.Count) {
            return (int)List[Index + 1].code;
         }
         return 0;
      }
      internal GameBattler? GetCharacter(int param) {
         if (param < 0)
            return Client;
         return Network.Maps[MapId].Events[param > 0 ? param : EventId];
      }
      internal float OperateValue(int operation, int operandType, int operand) {
         float value = operandType == 0 ? operand : Client.Variables[operand];
         return operation == 0 ? value : -value;
      }
      public Task DefaultEventCommand(int initialIndex = -1, int finalIndex = -1) {
         if (initialIndex < 0) initialIndex = Index;
         if (finalIndex < 0) finalIndex = Index + 1;
         if (Client != null) {
            Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)finalIndex);
         } else {
            Network.SendParallelProcessCommand(ObjEvent, (short)initialIndex, (short)finalIndex);
         }
         return Task.CompletedTask;
      }
      public async Task ShowMessage() {
         int initialIndex = Index;
         if (Client == null) return;
         Client.MessageInterpreter = this;
         while (NextEventCode() == 401) Index++;
         switch (NextEventCode()) {
            case 102:
               Index++;
               Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)Index);
               await SetupChoices(List[Index].parameters);
               break;
            case 103:
               Index++;
               Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)Index);
               await SetupNumInput(List[Index].parameters);
               break;
            case 104:
               Index++;
               Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)Index);
               await SetupItemChoice(List[Index].parameters);
               break;
            default:
               Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)Index);
               await Semaphore.WaitAsync();
               break;
         }
      }
      public async Task ShowChoice() {
         if (Client == null) return;
         Client.MessageInterpreter = this;
         await DefaultEventCommand();
         await SetupChoices(Params);
      }
      internal async Task SetupChoices(JArray @params) {
         await Semaphore.WaitAsync();
         Branch[Indent] = Math.Min(Client.Choice, @params[0].AsArray().Count);
         Client.CloseEventMessage();
      }
      public async Task StoreNumber() {
         if (Client == null) return;
         Client.MessageInterpreter = this;
         await DefaultEventCommand();
         await SetupNumInput(Params);
      }
      internal async Task SetupNumInput(JArray @params) {
         await Semaphore.WaitAsync();
         Client.Variables[@params[0].AsInt()] = Math.Min(Client.Choice, 99999999);
         Client.CloseEventMessage();
      }
      public async Task SelectItem() {
         if (Client == null) return;
         Client.MessageInterpreter = this;
         await DefaultEventCommand();
         await SetupItemChoice(Params);
      }
      internal async Task SetupItemChoice(JArray @params) {
         await Semaphore.WaitAsync();
         Client.Variables[@params[0].AsInt()] = Math.Min(Client.Choice, DataItems.Count);
         Client.CloseEventMessage();
      }
      public async Task ShowScrollingText() {
         int initialIndex = Index;
         while (NextEventCode() == 405) Index++;
         Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)Index);
      }
      public async Task Choice() {
         if (Branch[Indent] != Params[0].AsInt()) await CommandSkip();
      }
      public async Task Cancel() {
         if (Branch[Indent] != 4) await CommandSkip();
      }
      public async Task Condition() {
         bool result = false;
         switch (Params[0].AsInt()) {
            case 0:
               int swId = Params[1].AsInt();
               if(swId <= Configs.MaxPlayerSwitches) {
                  result = Client.Switches[swId] == (Params[2].AsInt() == 0);
               } else {
                  result = Network.Switches[swId] == (Params[2].AsInt() == 0);
               }
               break;
            case 1:
               int value1 = Params[1].AsInt();
               int value2 = Params[2].AsInt() == 0 ? Params[3].AsInt() : Client.Variables[Params[3].AsInt()];
               switch (Params[4].AsInt()) {
                  case 0:
                     result = value1 == value2;
                     break;
                  case 1:
                     result = value1 >= value2;
                     break;
                  case 2:
                     result = value1 <= value2;
                     break;
                  case 3:
                     result = value1 > value2;
                     break;
                  case 4:
                     result = value1 < value2;
                     break;
                  case 5:
                     result = value1 != value2;
                     break;
               }
               break;
            case 2:
               if(EventId > 0) {
                  result = Client.SelfSwitches[(MapId, EventId, Params[1].AsChar())] == (Params[2].AsInt() == 0);
               }
               break;
            case 4:
               switch (Params[2].AsInt()) {
                  case 1:
                     result = Client.Name == Params[3].AsString();
                     break;
                  case 2:
                     result = Client.ClassId == Params[3].AsInt();
                     break;
                  case 3:
                     result = Client.IsSkillLearned(Params[3].AsInt());
                     break;
                  case 4:
                     result = Client.WeaponId == Params[3].AsInt();
                     break;
                  case 5:
                     result = Client.Equips.GetRange(1, Configs.MaxEquips - 1).Contains(Params[3].AsInt());
                     break;
                  case 6:
                     result = Client.HasState(Params[3].AsInt());
                     break;
               }
               break;
            case 6:
               var character = GetCharacter(Params[1].AsInt());
               if(character != null) {
                  result = character.Direction == Params[2].AsInt();
               }
               break;
            case 7:
               switch (Params[2].AsInt()) {
                  case 0:
                     result = Client.Gold >= Params[1].AsInt();
                     break;
                  case 1:
                     result = Client.Gold <= Params[1].AsInt();
                     break;
                  case 2:
                     result = Client.Gold < Params[1].AsInt();
                     break;
               }
               break;
            case 8:
               result = Client.HasItem(DataItems[Params[1].AsInt()]);
               break;
            case 9:
               result = Client.HasItem(DataWeapons[Params[1].AsInt()], Params[2].AsBool());
               break;
            case 10:
               result = Client.HasItem(DataArmors[Params[1].AsInt()], Params[2].AsBool());
               break;
            case 12:
               result = await EvaluateCondition(Params[1].AsString());
               break;
         }
         Branch[Indent] = result ? 1 : 0;
         if (Branch[Indent] == 0) await CommandSkip();
      }
      public async Task CmdException() {
         if(Branch[Indent] == 1) await CommandSkip();
      }
      public Task RepeatAbove() {
         do {
            Index--;
         } while (((int)List[Index].indent) != Indent);
         return Task.CompletedTask;
      }
      public Task BreakCycle() {
         while (true) {
            Index++;
            if (Index >= List.Count - 1) return Task.CompletedTask;
            if (List[Index].code == 413 && List[Index].indent < Indent) return Task.CompletedTask;
         }
      }
      public Task StopEvent() {
         Index = List.Count;
         return Task.CompletedTask;
      }
      public Task CommonEvent() {
         if(Client == null) return Task.CompletedTask;
         var commonEvent = DataCommonEvents[Params[0].AsInt()];
         if(commonEvent == null) return Task.CompletedTask;
         var child = new GameInterpreter();
         child.Setup(Client, commonEvent.list, EventId);
         return Task.CompletedTask;
      }
      public Task GoToLabel() {
         var labelName = Params[0].AsString();
         for(int i = 0; i < List.Count; i++) {
            var cmd = List[i];
            if (cmd.code == 118 && cmd.parameters[0].AsString() == labelName) {
               Index = i;
               break;
            }
         }
         return Task.CompletedTask;
      }
      public Task ChangeSwitch() {
         var value = (Params[2].AsInt() == 0);
         for (int swId = Params[0].AsInt(); swId < Params[1].AsInt(); swId++) {
             if(swId <= Configs.MaxPlayerSwitches) {
               if (Client == null) return Task.CompletedTask;
               Client.Switches[swId] = value;
            } else {
               Network.Switches[swId] = value;
            }
         }
         return Task.CompletedTask;
      }
      public async Task ChangeVariable() {
         int value = 0;
         switch (Params[3].AsInt()) {
            case 0:
               value = Params[4].AsInt();
               break;
            case 1:
               if (Client == null) return;
               value = Client.Variables[Params[4].AsInt()];
               break;
            case 2:
               value = Params[4].AsInt() + Rand(Params[5].AsInt() - Params[4].AsInt() + 1);
               break;
            case 3:
               value = await GameDataOperand(Params[4].AsInt(), Params[5].AsInt(), Params[6].AsInt());
               break;
            case 4:
               value = await EvaluateInteger(Params[4].AsString());
               break;
         }
         await OperateVariable(Params[0].AsInt(), Params[2].AsInt(), value);
      }
      public Task<int> GameDataOperand(int type, int param1, int param2) {
         if(Client == null) return Task.FromResult(0);
         switch (type) {
            case 0:
               return Task.FromResult(Client.ItemNumber(DataItems[param1]));
            case 1:
               return Task.FromResult(Client.ItemNumber(DataWeapons[param1]));
            case 2:
               return Task.FromResult(Client.ItemNumber(DataArmors[param1]));
            case 3:
               switch (param2) {
                  case 0:
                     return Task.FromResult(Client.Level);
                  case 1:
                     return Task.FromResult(Client.Exp);
                  case 2:
                     return Task.FromResult(Client.Hp);
                  case 3:
                     return Task.FromResult(Client.Mp);
                  case (>= 4 and <= 11):
                     return Task.FromResult(Client.Param(param2 - 4));
               }
               break;
            case 5:
               var character = GetCharacter(param1);
               if(character != null) {
                  switch (param2) {
                     case 0:
                        return Task.FromResult(character.X);
                     case 1:
                        return Task.FromResult(character.Y);
                     case 2:
                        return Task.FromResult(character.Direction);
                  }
               }
               break;
            case 7:
               switch (param1) {
                  case 0:
                     return Task.FromResult(Client.MapId);
                  case 2:
                     return Task.FromResult(Client.Gold);
               }
               break;
         }
         return Task.FromResult(0);
      }
      public Task OperateVariable(int variableId, int opType, int value) {
         if (Client == null) return Task.CompletedTask;
         try {
            switch (opType) {
               case 0:
                  Client.Variables[variableId] = value;
                  break;
               case 1:
                  Client.Variables[variableId] += value;
                  break;
               case 2:
                  Client.Variables[variableId] -= value;
                  break;
               case 3:
                  Client.Variables[variableId] *= value;
                  break;
               case 4:
                  Client.Variables[variableId] /= value;
                  break;
               case 5:
                  Client.Variables[variableId] %= value;
                  break;
            }
         } catch {
            Client.Variables[variableId] = 0;
         }
         return Task.CompletedTask;
      }
      public Task ChangeSelfSwitches() {
         if (EventId == 0 || Client == null) return Task.CompletedTask;
         var key = (MapId, EventId, Params[0].AsChar());
         Client.SelfSwitches[key] = (Params[1].AsInt() == 0);
         return Task.CompletedTask;
      }
      public Task ChangeGold() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[0].AsInt(), Params[1].AsInt(), Params[2].AsInt());
         Client.GainGold(value, false, (Params[0].AsInt() == 0));
         return Task.CompletedTask;
      }
      public Task ChangeItem() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[1].AsInt(), Params[2].AsInt(), Params[3].AsInt());
         if (Client.IsFullInventory(DataItems[Params[0].AsInt()]) && value > 0){
            Network.Maps[Client.MapId].AddDrop(Params[0].AsInt(), 1, value, Client.X, Client.Y, Client.Name);
            return Task.CompletedTask;
         }
         Client.GainItem(DataItems[Params[0].AsInt()], value, false, (value > 0));
         if (Client.IsInTrade() && value < 0)
            Client.LoseTradeItem(DataItems[Params[0].AsInt()], Math.Abs(value));
         return Task.CompletedTask;
      }
      public Task ChangeWeapon() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[1].AsInt(), Params[2].AsInt(), Params[3].AsInt());
         if (Client.IsFullInventory(DataWeapons[Params[0].AsInt()]) && value > 0){
            Network.Maps[Client.MapId].AddDrop(Params[0].AsInt(), 2, value, Client.X, Client.Y, Client.Name);
            return Task.CompletedTask;
         }
         Client.GainItem(DataWeapons[Params[0].AsInt()], value, false, (value > 0));
         if (Client.IsInTrade() && value < 0)
            Client.LoseTradeItem(DataWeapons[Params[0].AsInt()], Math.Abs(value));
         return Task.CompletedTask;
      }
      public Task ChangeArmor() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[1].AsInt(), Params[2].AsInt(), Params[3].AsInt());
         if (Client.IsFullInventory(DataArmors[Params[0].AsInt()]) && value > 0){
            Network.Maps[Client.MapId].AddDrop(Params[0].AsInt(), 3, value, Client.X, Client.Y, Client.Name);
            return Task.CompletedTask;
         }
         Client.GainItem(DataArmors[Params[0].AsInt()], value, false, (value > 0));
         if (Client.IsInTrade() && value < 0)
            Client.LoseTradeItem(DataArmors[Params[0].AsInt()], Math.Abs(value));
         return Task.CompletedTask;
      }
      public Task TransferPlayer() {
         if (Client == null) return Task.CompletedTask;
         int mapId, x, y;
         if (Params[0].AsInt() == 0) {
            mapId = Params[1].AsInt();
            x = Params[2].AsInt();
            y = Params[3].AsInt();
         } else {
            mapId = Client.Variables[Params[1].AsInt()];
            x = Client.Variables[Params[2].AsInt()];
            y = Client.Variables[Params[3].AsInt()];
         }
         Client.Transfer(mapId, x, y, Params[4].AsByte());
         return Task.CompletedTask;
      }
      public Task ChangeEventPosition() {
         var character = GetCharacter(Params[0].AsInt());
         if (character == null) return Task.CompletedTask;
         if (Params[4].AsInt() > 0) 
            character.Direction = Params[4].AsInt();
         if (Params[1].AsInt() == 0) {
            character.MoveTo(Params[2].AsInt(), Params[3].AsInt());
         }else if(Params[1].AsInt() == 1) {
            if (Client == null) return Task.CompletedTask;
            character.MoveTo(
                  Client.Variables[Params[2].AsInt()],
                  Client.Variables[Params[3].AsInt()]
               );
         } else {
            var character2 = GetCharacter(Params[2].AsInt());
            if(character2 != null)
               character.Swap(character2);
         }

         return Task.CompletedTask;
      }
      public async Task ShowAnimation() {
         var character = GetCharacter(Params[0].AsInt());
         if(character != null) {
            var type = (int)(Params[0].AsInt() >= 0 ? Enums.Target.ENEMY : Enums.Target.PLAYER);
            Network.SendAnimation(character, Params[1].AsShort(), (short)character.Id, (byte)(type - 1), 8, (byte)type);
         }
         if (Params[2].AsBool())
            await Wait((int)DataAnimations[Params[1].AsInt()].frame_max * 4 + 1);
      }
      public async Task ShowBalloon() {
         var character = GetCharacter(Params[0].AsInt());
         if(character != null) {
            var type = (byte)(Params[0].AsInt() >= 0 ? Enums.Target.ENEMY : Enums.Target.PLAYER);
            Network.SendBallon(character, type, Params[1].AsByte());
         }
         if (Params[2].AsBool())
            await Wait(76);
      }
      public async Task ScreenTint() {
         await DefaultEventCommand();
         if (Params[2].AsBool())
            await Wait(Params[1].AsInt());
      }
      public async Task FlashEffect() {
         await DefaultEventCommand();
         if (Params[2].AsBool())
            await Wait(Params[1].AsInt());
      }
      public async Task TremorEffect() {
         await DefaultEventCommand();
         if (Params[2].AsBool())
            await Wait(Params[1].AsInt());
      }
      public async Task Wait(int duration = -1) {
         if (duration < 0) duration = Params[0].AsInt();
         if(ObjEvent != null) {
            ObjEvent.ParallelProcessWaiting = DateTimeOffset.UtcNow.AddSeconds(duration / 60);
         }else if (EventId > 0 && ObjEvent == null && Client != null) {
            Client.WaitingEvent = DateTimeOffset.UtcNow.AddSeconds(duration / 60);
         }else if (ObjCommonEvent != null && Client != null) {
            Client.ParallelEventsWating[(int)ObjCommonEvent.id] = DateTimeOffset.UtcNow.AddSeconds(duration / 60);
         }
         await Semaphore.WaitAsync();
      }
      public async Task MoveImage() {
         await DefaultEventCommand();
         if (Params[11].AsBool())
            await Wait(Params[10].AsInt());
      }
      public async Task ImageTone() {
         await DefaultEventCommand();
         if (Params[3].AsBool())
            await Wait(Params[2].AsInt());
      }
      public async Task ClimateOptions() {
         await DefaultEventCommand();
         if (Params[3].AsBool())
            await Wait(Params[2].AsInt());
      }
      public Task PositionInformation() {
         if (Client == null) return Task.CompletedTask;
         int x, y, value;
         if (Params[2].AsInt() == 0) {
            x = Params[3].AsInt();
            y = Params[4].AsInt();
         } else {
            x = Client.Variables[Params[3].AsInt()];
            y = Client.Variables[Params[4].AsInt()];
         }
         switch (Params[1].AsInt()) {
            case 0:
               value = Network.Maps[MapId].TerrainTag(x, y);
               break;
            case 1:
               value = Network.Maps[MapId].EventIdXY(x, y);
               break;
            case 2: case 3: case 4:
               value = Network.Maps[MapId].TileId(x, y, Params[1].AsInt() - 2);
               break;
            default:
               value = Network.Maps[MapId].RegionId(x, y);
               break;
         }
         Client.Variables[Params[0].AsInt()] = value;
         return Task.CompletedTask;
      }
      public async Task OpenShop() {
         if (Client == null) return;
         List<JArray> goods = new List<JArray>() {Params};
         int initialIndex = Index;
         while(NextEventCode() == 605) {
            Index++;
            goods.Add(List[Index].parameters);
         }
         if (!Client.IsInTrade() && !Client.IsInBank()) {
            Client.OpenShop(goods, EventId, initialIndex);
            await Semaphore.WaitAsync();
         }
      }
      public Task ChangeHp() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[2].AsInt(), Params[3].AsInt(), Params[4].AsInt());
         Client.Hp += value;
         return Task.CompletedTask;
      }
      public Task ChangeMp() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[2].AsInt(), Params[3].AsInt(), Params[4].AsInt());
         Client.Mp += value;
         return Task.CompletedTask;
      }
      public Task ChangeState() {
         if (Client == null) return Task.CompletedTask;
         if (Params[2].AsInt() == 0) {
            Client.AddState(Params[3].AsInt());
         }else {
            Client.RemoveState(Params[3].AsInt());
         }
         return Task.CompletedTask;
      }
      public Task RecoverAll() {
         if (Client == null) return Task.CompletedTask;
         Client.RecoverAll();
         return Task.CompletedTask;
      }
      public Task ChangeExp() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[2].AsInt(), Params[3].AsInt(), Params[4].AsInt());
         Client.ChangeExp(Client.Exp + value);
         return Task.CompletedTask;
      }
      public Task ChangeLevel() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[2].AsInt(), Params[3].AsInt(), Params[4].AsInt());
         Client.ChangeLevel(Client.Level + value);
         return Task.CompletedTask;
      }
      public Task ChangeParam() {
         if (Client == null) return Task.CompletedTask;
         int value = (int)OperateValue(Params[3].AsInt(), Params[4].AsInt(), Params[5].AsInt());
         Client.AddParam(Params[2].AsInt(), value);
         return Task.CompletedTask;
      }
      public Task ChangeSkill() {
         if (Client == null) return Task.CompletedTask;
         if (Params[2].AsInt() == 0) {
            Client.LearnSkill(Params[3].AsInt());
         }else {
            Client.ForgetSkill(Params[3].AsInt());
         }
         return Task.CompletedTask;
      }
      public Task ChangeEquip() {
         if (Client == null) return Task.CompletedTask;
         Client.ChangeEquip(Params[1].AsInt(), Params[2].AsInt());
         return Task.CompletedTask;
      }
      public Task ChangeClass() {
         if (Client == null) return Task.CompletedTask;
         Client.ChangeClass(Params[1].AsInt());
         return Task.CompletedTask;
      }
      public Task ChangeGraphic() {
         if (Client == null) return Task.CompletedTask;
         Client.SetGraphic(Params[1].AsString(),Params[2].AsInt(),Params[3].AsString(),Params[4].AsInt());
         return Task.CompletedTask;
      }
      public Task GameOver() {
         if (Client == null) return Task.CompletedTask;
         Client.Die();
         return Task.CompletedTask;
      }
      public async Task CallScript() {
         int initialIndex = Index;
         string script = $"{List[Index].parameters[0].AsString()}\n";
         while(NextEventCode() == 655) {
            Index++;
            script += $"{List[Index].parameters[0].AsString()}\n";
         }
         if(script.StartsWith("[NG]", StringComparison.CurrentCultureIgnoreCase)) {
            await DefaultEventCommand(initialIndex, Index);
         } else {
            await EvaluateAction(script);
         }
      }
      public void ChatAdd(string message, int colorId) {
         if (Client == null) return;
         Network.PlayerChatMessage(Client, message, colorId);
      }
      public void CheckPoint(int mapId, int x, int y) {
         if (Client == null) return;
         Client.CheckPoint(mapId, x, y);
      }
      public void StartQuest(int questId) {
         if (Client == null) return;
         Client.StartQuest(questId - 1);
      }
      public void FinishQuest(int questId) {
         if (Client == null) return;
         Client.FinishQuest(questId - 1);
      }
      public async Task OpenTeleport(int teleportId) {
         if (Client == null) return;
         Client.OpenTeleport(teleportId - 1);
         await Semaphore.WaitAsync();
      }
      public async Task OpenBank() {
         if (Client == null) return;
         await Network.DB.LoadDistributor(Client);
         Client.OpenBank();
         await Semaphore.WaitAsync();
      }
      public async Task OpenCreateGuild() {
         if (Client == null) return;
         Client.OpenCreateGuild();
         if(Client.IsCreatingGuild())
            await Semaphore.WaitAsync();
      }
      public async Task<bool> EvaluateCondition(string code) {
         if(string.IsNullOrEmpty(code)) return false;
         string cacheKey = $"{code}_bool";
         try {
            if (!_globalScriptCache.TryGetValue(cacheKey, out ScriptRunner<bool> runner)) {
               var script = CSharpScript.Create<bool>(
                  code, _scriptOptions, globalsType: typeof(GameInterpreter)
                  );
               script.Compile();
               runner = script.CreateDelegate();
               var cacheOptions = new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(TimeSpan.FromMinutes(10));
               _globalScriptCache.Set(cacheKey, runner, cacheOptions);
            }
            return await runner(globals: this);
         } catch (Exception e) {
            return false;
         }
      }
      public async Task<int> EvaluateInteger(string code) {
         if(string.IsNullOrEmpty(code)) return 0;
         string cacheKey = $"{code}_int";
         try {
            if (!_globalScriptCache.TryGetValue(cacheKey, out ScriptRunner<int> runner)) {
               var script = CSharpScript.Create<int>(
                  code, _scriptOptions, globalsType: typeof(GameInterpreter)
                  );
               script.Compile();
               runner = script.CreateDelegate();
               var cacheOptions = new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(TimeSpan.FromMinutes(10));
               _globalScriptCache.Set(cacheKey, runner, cacheOptions);
            }
            return await runner(globals: this);
         } catch (Exception e) {
            return 0;
         }
      }
      public async Task EvaluateAction(string code) {
         if(string.IsNullOrEmpty(code)) return;
         string cacheKey = $"{code}_action";
         try {
            if (!_globalScriptCache.TryGetValue(cacheKey, out ScriptRunner<object> runner)) {
               var script = CSharpScript.Create<object>(
                  code, _scriptOptions, globalsType: typeof(GameInterpreter)
                  );
               script.Compile();
               runner = script.CreateDelegate();
               var cacheOptions = new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(TimeSpan.FromMinutes(10));
               _globalScriptCache.Set(cacheKey, runner, cacheOptions);
            }
            await runner(globals: this);
         } catch (Exception e) {
         }
      }
   }
}
