using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VXAOS_Server.RPGData;

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
      public int Points;
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
      public List<(int, int, int, int)> ShopGoods = new();
      public Request Request = new();
      public Dictionary<int, GameQuest> Quests = new();
      public int PartyId = -1;
      public int TeleportId = -1;
      public string EventInterpreter;//interpreter
      public Dictionary<int, string> CommonEvents = new();//interpreter
      public Dictionary<int, string> ParallelEventsWating = new();//interpreter
      public bool CreatingGuild = false;
      public string? WaitingEvent;//Interpreter
      public DateTimeOffset MutedTime;
      public DateTimeOffset StopCount;
      public DateTimeOffset AntispamTime;
      public DateTimeOffset GlobalAntispamTime;
      public string? MessageInterpreter;//Interpreter
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
      public new void AddNewState(int stateId) {
         base.AddNewState(stateId);
         float time = 0;
         if (StatesTime.TryGetValue(stateId, out var expiration))
            time = (float)(expiration - DateTimeOffset.UtcNow).TotalSeconds;
         Network.SendPlayerState(this, (short)stateId, true, time);
      }
      public void AddNewState(int stateId, int time = 0) {
         base.AddNewState(stateId);
         if(StatesTime.ContainsKey(stateId))
            StatesTime[stateId] = DateTimeOffset.UtcNow.AddSeconds(time + 0.1f);
         Network.SendPlayerState(this, (short)stateId, true, (time + 0.1f));
      }
      public void LearnSkill(int skillId) {

      }
      private void ItemEffectLearnSkill(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         LearnSkill((int)effect.data_id);
      }
      public void UpdateGame() {
      }
      public void LoadOriginalGraphic() {
         if (string.IsNullOrEmpty(OriginalCharacterName))
            return;
         CharacterName = OriginalCharacterName;
         CharacterIndex = OriginalCharacterIndex;
         FaceName = OriginalFaceName;
         FaceIndex = OriginalFaceIndex;
         OriginalCharacterName = string.Empty;
      }
   }
}
