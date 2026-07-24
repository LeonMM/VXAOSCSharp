using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameClient : GameBattler {
      public int PartyId = -1;
      public List<GameClient> PartyMembersInMap() {
         List<GameClient> members = new List<GameClient>();
         foreach (var member in Network.Parties[PartyId].Clients) {
            if(member.MapId == MapId) {
               members.Add(member);
            }
         }
         return members;
      }
      public void PartyShare(int exp, int gold, int enemyId) {
         var members = PartyMembersInMap();
         PartyShareExp(exp, enemyId, members);
         PartyShareGold(gold, members);
      }
      public void PartyShareExp(int exp, int enemyId, List<GameClient> members) {
         if(members.Count > exp || members.Count == 1) {
            GainExp(Convert.ToInt32(exp * VipExpBonus()));
            AddKillsCount(enemyId);
            return;
         }
         int expShare = Convert.ToInt32(exp / members.Count + (exp * ServerConfig.PartyBonus[members.Count] / 100));
         int difExp = exp % members.Count;
         foreach (var member in members) {
            member.GainExp(member == this ?
               Convert.ToInt32(expShare * member.VipExpBonus() + difExp) :
               Convert.ToInt32(expShare * member.VipExpBonus())
               );
            member.AddKillsCount(enemyId);
         }
      }
      public void PartyShareGold(int gold, List<GameClient> members) {
         if (members.Count > gold || members.Count == 1) {
            GainExp(Convert.ToInt32(gold * VipGoldBonus()));
            return;
         }
         int goldShare = Convert.ToInt32(gold / members.Count + (gold * ServerConfig.PartyBonus[members.Count] / 100));
         foreach (var member in members) {
            member.GainGold(goldShare, false, true);
         }
      }
      public void ItemPartyRecovery(RPGUsableItem item) {
         if (!IsInParty()) {
            ItemRecover(item);
            return;
         }
         foreach(var member in PartyMembersInMap()) {
            if (IsInRange(member, item.range))
               member.ItemRecover(item);
         }
      }
      public void AcceptParty() {
         if (IsInParty()) return;
         if(Network.Clients.TryGetValue(Request.Id, out var client)) {
            if (client.IsInParty()) {
               if (Network.Parties[client.PartyId].Members.Count >= Configs.MaxPartyMembers) return;
               PartyId = client.PartyId;
            } else {
               CreateParty();
            }
            foreach(var member in Network.Parties[PartyId].Clients) {
               Network.SendJoinParty(member, this);
               Network.SendJoinParty(this, member);
            }
            Network.Parties[PartyId].Members.Add(Id);
         }
      }
      public void CreateParty() {
         PartyId = Network.FindPartyId();
         Network.Clients[Request.Id].PartyId = PartyId;
         Network.Parties[PartyId] = new(PartyId, Request.Id);
      }
      public void LeaveParty() {
         if (!IsInParty()) return;
         Network.SendDissolveParty(this);
         Network.Parties[PartyId].Members.Remove(Id);
         if (Network.Parties[PartyId].Members.Count == 1) {
            DissolveParty(Network.Parties[PartyId].Members[0]);
         } else {
            Network.SendLeaveParty(this);
         }
         PartyId = -1;
      }
      public void DissolveParty(int memberId) {
         Network.Parties.TryRemove(PartyId, out var _);
         Network.PartyAvaiableIds.Enqueue(PartyId);
         Network.SendDissolveParty(Network.Clients[memberId]);
         Network.Clients[memberId].PartyId = -1;
      }
   }
}
