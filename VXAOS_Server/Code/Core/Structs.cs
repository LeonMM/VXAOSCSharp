using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server.Code.Core {
   public class Account {
      public int IdDb {
         get; set;
      }

      public string Pass {
         get; set;
      }

      public int Group {
         get; set;
      }

      public DateTime VipTime {
         get; set;
      }

      public List<string> Friends { get; set; } = new();

      public Dictionary<int, string> Actors { get; set; } = new();
   }
}
