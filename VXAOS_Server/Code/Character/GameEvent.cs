using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;

namespace VXAOS_Server {
   public partial class GameEvent : GameBattler {
      public DateTimeOffset ParallelProcessWaiting;
      public GameEvent(int id, RPGEvent @event, int mapId) { 
         Id = id;
         MapId = mapId;
      }

      internal bool Erased() {
         return false;
      }

      internal void Update() {

      }
   }
}
