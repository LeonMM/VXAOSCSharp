using Newtonsoft.Json.Linq;
using System;
using VXAOS_Server.RPGData;
using static VXAOS_Server.Enums;

namespace VXAOS_Server {
   public partial class GameCharacter {
      public void MoveRandom() {
         MoveStraight(Rand(4) * 2 + 2, false);
      }
      public void MoveTowardCharacter(GameBattler character) {
         if (IsInFront(character)) return;
         int sx = DistanceXFrom(character.X);
         int sy = DistanceYFrom(character.Y);
         if(Math.Abs(sx) > Math.Abs(sy)) {
            MoveStraight((int)(sx > 0 ? Dir.LEFT : Dir.RIGHT));
            if (!MoveSucceed && sy != 0)
               MoveStraight((int)(sy > 0 ? Dir.UP : Dir.DOWN));
         } else {
            MoveStraight((int)(sy > 0 ? Dir.UP : Dir.DOWN));
            if (!MoveSucceed && sx != 0)
               MoveStraight((int)(sx > 0 ? Dir.LEFT : Dir.RIGHT));
         }
      }
      public void MoveAwayFromCharacter(GameBattler character) {
         if (IsInFront(character)) return;
         int sx = DistanceXFrom(character.X);
         int sy = DistanceYFrom(character.Y);
         if(Math.Abs(sx) > Math.Abs(sy)) {
            MoveStraight((int)(sx > 0 ? Dir.RIGHT : Dir.LEFT));
            if (!MoveSucceed && sy != 0)
               MoveStraight((int)(sy > 0 ? Dir.DOWN : Dir.UP));
         } else {
            MoveStraight((int)(sy > 0 ? Dir.DOWN : Dir.UP));
            if (!MoveSucceed && sx != 0)
               MoveStraight((int)(sx > 0 ? Dir.RIGHT : Dir.LEFT));
         }
      }
      public void TurnTowardCharacter(GameBattler character) {
         if (IsInFront(character)) return;
         int sx = DistanceXFrom(character.X);
         int sy = DistanceYFrom(character.Y);
         if(Math.Abs(sx) > Math.Abs(sy)) {
            Direction = (int)(sx > 0 ? Dir.LEFT : Dir.RIGHT);
            SendMovement();
         } else {
            Direction = (int)(sy > 0 ? Dir.UP : Dir.DOWN);
            SendMovement();
         }
      }
   }
}
