using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXAOS_Server.RPGData;
namespace VXAOS_Server {
   public class GameMap {
      public ConcurrentDictionary<int, GameEvent> Events { get; private set; } = new();
      public List<GameEvent> TileEvents { get; private set; } = new();
      public ConcurrentDictionary<int, Drop> Drops { get; private set; } = new();
      private ConcurrentQueue<int> AvailableDropIds = new();
      private int _dropHighestIdAvailable = 0;
      public bool PvP { get; private set; }
      public List<List<Region>> ReviveRegions { get; private set; } = new();
      public int TotalPlayers = 0;
      public int FindDropId() {
         int id;
         if (AvailableDropIds.TryDequeue(out id)) {
            return id;
         }
         return _dropHighestIdAvailable;
      }
      private int Id = 0;
      private Table Data;
      private int Width = 1;
      private int Height = 1;
      private int TilesetId = 1;
      public GameMap(int id, RPGMap map) {
         Id = id;
         Data = map.data;
         Width = (int)map.width;
         Height = (int)map.height;
         TilesetId = (int)map.tileset_id;
         PvP = Note.ReadBoolean("PvP", map.note);
         for (int i = 0; i < ServerConfig.MaxReviveRegions; i++) { 
            ReviveRegions.Add(new List<Region>());
         }
         for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
               int rid = RegionId(x, y);
               if (rid > 0 && rid <= ServerConfig.MaxReviveRegions)
                  ReviveRegions[rid - 1].Add(new Region(x, y));
            }
         }
         foreach (var (eventId, ev) in map.events) {
            if (ev.name == "notupdate" || ev.name == "notglobal")
               continue;
            Events.TryAdd(eventId, new GameEvent(eventId, ev, Id));
         }
         RefreshTileEvents();
      }
      public bool HasZeroPlayers() {
         return TotalPlayers == 0;
      }
      public bool IsFullDrops() {
         return Drops.Count >= Configs.MaxMapDrops;
      }
      public int RoundXWithDirection(int x, int d) {
         return x + (d == (int)Enums.Dir.RIGHT ? 1 : d == (int)Enums.Dir.LEFT ? -1 : 0);
      }
      public int RoundYWithDirection(int y, int d) {
         return y + (d == (int)Enums.Dir.DOWN ? 1 : d == (int)Enums.Dir.UP ? -1 : 0);
      }
      public void Refresh() {
         foreach (var ev in Events.Values)
            ev.Refresh();
         RefreshTileEvents();
      }
      public void RefreshTileEvents() {
         TileEvents = Events.Values.Where(ev => ev.IsTile()).ToList();
      }
      public List<GameEvent> EventsXY(int x, int y) {
         return Events.Values.Where(ev => ev.Pos(x, y)).ToList();
      }
      public List<GameEvent> EventsXYNT(int x, int y) {
         return Events.Values.Where(ev => ev.PosNt(x, y)).ToList();
      }
      public List<GameEvent> TileEventsXY(int x, int y) {
         return TileEvents.Where(ev => ev.PosNt(x, y)).ToList();
      }
      public int EventIdXY(int x, int y) {
         var ev = Events.Values.FirstOrDefault(e => e.Pos(x, y));
         return ev?.Id ?? 0;
      }
      public bool IsValid(int x, int y) {
         return (x >= 0 && x < Width && y >= 0 && y < Height);
      }
      public bool CheckPassage(int x, int y, int bit) {
         foreach (var tileId in AllTiles(x, y)) {
            int flag = (int)DataTilesets[TilesetId].flags[tileId];
            if ((flag & 0x10) != 0)
               continue;
            if ((flag & bit) == 0)
               return true;
            if ((flag & bit) == bit)
               return false;
         }
         return false;
      }
      public int TileId(int x, int y, int z) {
         return (int)Data[x, y, z];
      }
      public int[] LayeredTiles(int x, int y) {
         return new[]
         {
            TileId(x, y, 2),
            TileId(x, y, 1),
            TileId(x, y, 0)
         };
      }
      public List<int> AllTiles(int x, int y) {
         return TileEventsXY(x, y)
             .Select(ev => ev.TileId)
             .Concat(LayeredTiles(x, y))
             .ToList();
      }
      public bool IsPassable(int x, int y, int d) {
         return CheckPassage(x, y, (1 << (d / 2 - 1)) & 0x0F);
      }
      public bool IsLayeredTilesFlag(int x, int y, int bit) {
         return LayeredTiles(x, y)
             .Any(tileId => ((int)DataTilesets[TilesetId].flags[tileId] & bit) != 0);
      }
      public bool IsLadder(int x, int y) {
         return IsValid(x, y) && IsLayeredTilesFlag(x, y, 0x20);
      }
      public bool IsCounter(int x, int y) {
         return IsValid(x, y) && IsLayeredTilesFlag(x, y, 0x80);
      }

      public bool IsDamageFloor(int x, int y) {
         return IsValid(x, y) && IsLayeredTilesFlag(x, y, 0x100);
      }

      public int TerrainTag(int x, int y) {
         if (!IsValid(x, y))
            return 0;
         foreach (var tileId in LayeredTiles(x, y)) {
            int tag = (int)DataTilesets[TilesetId].flags[tileId] >> 12;
            if (tag > 0)
               return tag;
         }
         return 0;
      }
      public int RegionId(int x, int y) {
         return IsValid(x, y) ? (int)(Data[x, y, 3] >> 8) : 0;
      }
      public void AddDrop(int itemId, int kind, int amount, int x, int y, string name = "", int partyId = -1) {
         Drop drop = new();
         drop.ItemId = itemId;
         drop.Kind = kind;
         drop.Amount = amount;
         drop.X = x;
         drop.Y = y;
         drop.Name = name;
         drop.PartyId = partyId;
         drop.DespawnTime = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.DropDespawnTime);
         drop.PickUpTime = DateTimeOffset.UtcNow.AddSeconds(ServerConfig.DropPickUpTime);
         Drops.TryAdd(FindDropId(), drop);
         Network.SendAddDrop(Id, (short)itemId, (byte)kind, (short)amount, (short)x, (short)y);
      }
      public void RemoveDrop(int dropId) {
         Drops.TryRemove(dropId, out _);
         Network.SendRemoveDrop(Id, (short)dropId);
      }
      public void Update() {
         UpdateEvents();
         UpdateDrops();
      }
      public void UpdateEvents() {
         foreach (var ev in Events.Values)
            ev.Update();
      }
      public void UpdateDrops() {
         foreach (var(dropId, drop) in Drops) {
            if (DateTimeOffset.UtcNow > drop.DespawnTime)
               RemoveDrop(dropId);
         }
      }
      public bool PvPAble() {
         return PvP && TotalPlayers > 1;
      }
   }
}
