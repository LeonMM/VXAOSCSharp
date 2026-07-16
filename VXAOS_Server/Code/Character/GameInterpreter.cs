using Newtonsoft.Json.Linq;
using Npgsql.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public class GameInterpreter {
      private SemaphoreSlim Semaphore = new SemaphoreSlim(0, 1);
      private List<RPGEventCommand> List = new();
      private GameClient? Client;
      private int EventId = 0;
      private int Index;
      private int MapId;
      private GameEvent ObjEvent;
      private RPGCommonEvent ObjCommonEvent;
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
               break;
            case (RPGCommonEvent ev):
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
            case 113: BreakCycle(); break;
            case 115: StopEvent(); break;
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
            case 402: Choice(); break;
            case 403: Cancel(); break;
            case 411: CmdException(); break;
            case 413: RepeatAbove(); break;
            case 124 or 138 or 204 or 221 or 222 or 231 or 233 or 235 or
                  241 or 242 or 243 or 244 or 245 or 246 or 249 or 250 or
                  251 or 261 or 282 or 284:
               DefaultEventCommand(); break;
         }
      }
      internal void CommandSkip() {
         while (List[Index + 1].indent > Indent) {
            Index++;
         }
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
      public void DefaultEventCommand(int initialIndex = -1, int finalIndex = -1) {
         if (initialIndex < 0) initialIndex = Index;
         if (finalIndex < 0) finalIndex = Index + 1;
         if (Client != null) {
            Network.SendEventCommand(Client, (short)EventId, (short)initialIndex, (short)finalIndex);
         } else {
            Network.SendParallelProcessCommand(ObjEvent, (short)initialIndex, (short)finalIndex);
         }
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
         DefaultEventCommand();
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
         DefaultEventCommand();
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
         DefaultEventCommand();
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
      public void Choice() {
         if (Branch[Indent] != Params[0].AsInt()) CommandSkip();
      }
      public void Cancel() {
         if (Branch[Indent] != 4) CommandSkip();
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

               break;
         }
         Branch[Indent] = result ? 1 : 0;
         if (Branch[Indent] == 0) CommandSkip();
      }
      public void CmdException() {
         if(Branch[Indent] == 1) CommandSkip();
      }
      public void RepeatAbove() {
         do {
            Index--;
         } while (((int)List[Index].indent) != Indent);
      }
      public void BreakCycle() {
         while (true) {
            Index++;
            if (Index >= List.Count - 1) return;
            if (List[Index].code == 413 && List[Index].indent < Indent) return;
         }
      }
      public void StopEvent() {
         Index = List.Count;
      }
   }
}
