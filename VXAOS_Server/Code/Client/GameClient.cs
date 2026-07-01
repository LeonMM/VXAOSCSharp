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
      public List<bool> Switches = new();
      public Dictionary<(int MapId, int EventId, char Ch), bool> SelfSwitches = new();
      public List<(int, int, int, int)> ShopGoods = new();
      public Request Request = new();
      public Dictionary<int, GameQuest> Quests = new();
      public int PartyId = -1;
      public int TeleportId = -1;
      public string EventInterpreter;//interpreter
      public Dictionary<int, string> ParallelEventsWating = new();//interpreter
      public bool CreatingGuild = false;
      public string WaitingEvent;//Interpreter
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
      public void LearnSkill(int skillId) {

      }
      private void ItemEffectLearnSkill(GameBattler user, RPGUsableItem item, RPGUsableItemEffect effect) {
         LearnSkill((int)effect.data_id);
      }
   }
}
