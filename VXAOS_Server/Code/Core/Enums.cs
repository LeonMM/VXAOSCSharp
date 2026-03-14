using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
   public class Enums {
      public enum DatabaseType {
         POSTGRESQL = 0,
         MYSQL = 1,
         SQLITE = 2
      }
      public enum Packet {
         NONE,
         LOGIN,
         FAIL_LOGIN,
         CREATE_ACCOUNT,
         CREATE_ACTOR,
         FAIL_CREATE_ACTOR,
         ACTOR,
         REMOVE_ACTOR,
         USE_ACTOR,
         MOTD,
         PLAYER_DATA,
         REMOVE_PLAYER,
         PLAYER_MOVE,
         MAP_MSG,
         CHAT_MSG,
         ALERT_MSG,
         PLAYER_ATTACK,
         ATTACK_PLAYER,
         ATTACK_ENEMY,
         USE_ITEM,
         USE_SKILL,
         ANIMATION,
         BALLOON,
         USE_HOTBAR,
         ENEMY_REVIVE,
         EVENT_DATA,
         EVENT_MOVE,
         ADD_DROP,
         REMOVE_DROP,
         ADD_PROJECTILE,
         PLAYER_VITALS,
         PLAYER_EXP,
         PLAYER_STATE,
         PLAYER_BUFF,
         PLAYER_ITEM,
         PLAYER_GOLD,
         PLAYER_PARAM,
         PLAYER_EQUIP,
         PLAYER_SKILL,
         PLAYER_CLASS,
         PLAYER_SEX,
         PLAYER_GRAPHIC,
         PLAYER_POINTS,
         PLAYER_HOTBAR,
         PLAYER_COOLDOWN,
         TARGET,
         TRANSFER,
         OPEN_FRIENDS,
         ADD_FRIEND,
         REMOVE_FRIEND,
         OPEN_CREATE_GUILD,
         CREATE_GUILD,
         OPEN_GUILD,
         GUILD_NAME,
         GUILD_LEADER,
         GUILD_NOTICE,
         REMOVE_GUILD_MEMBER,
         GUILD_REQUEST,
         LEAVE_GUILD,
         JOIN_PARTY,
         LEAVE_PARTY,
         DISSOLVE_PARTY,
         CHOICE,
         OPEN_BANK,
         BANK_ITEM,
         BANK_GOLD,
         CLOSE_WINDOW,
         OPEN_SHOP,
         BUY_ITEM,
         SELL_ITEM,
         OPEN_TELEPORT,
         CHOICE_TELEPORT,
         EVENT_COMMAND,
         NEXT_COMMAND,
         REQUEST,
         ACCEPT_REQUEST,
         DECLINE_REQUEST,
         TRADE_ITEM,
         TRADE_GOLD,
         ADD_QUEST,
         FINISH_QUEST,
         VIP_DAYS,
         LOGOUT,
         ADMIN_COMMAND,
         SWITCH,
         VARIABLE,
         SELF_SWITCH,
         NET_SWITCHES
      }
      public enum Group {
         STANDARD,
         MONITOR,
         ADMIN
      }
      public enum Dir {
         DOWN_LEFT  = 1,
         DOWN       = 2,
         DOWN_RIGHT = 3,
         LEFT       = 4,
         RIGHT      = 6,
         UP_LEFT    = 7,
         UP         = 8,
         UP_RIGHT   = 9
      }
      public enum Chat {
         MAP,
         GLOBAL,
         PARTY,
         GUILD,
         PRIVATE
      }
      public enum Login {
         SERVER_FULL,
         IP_BANNED,
         OLD_VERSION,
         ACC_BANNED,
         INVALD_USER,
         MULTI_ACCOUNT,
         INVALID_PASS,
         IP_BLOCKED,
         INACTIVITY
      }
      public enum Register {
         ACC_EXIST,
         SUCCESSFUL
      }
      public enum Alert {
         INVALID_NAME,
         TELEPORTED,
         PULLED,
         ATTACK_ADMIN,
         BUSY,
         IN_PARTY,
         IN_GUILD,
         GUILD_EXIST,
         NOT_GUILD_LEADER,
         FULL_GUILD,
         NOT_PICK_UP_DROP,
         REQUEST_DECLINED,
         TRADE_DECLINED,
         TRADE_FINISHED,
         FULL_INV,
         FULL_TRADE,
         FULL_BANK,
         MUTED
      }
      public enum Hotbar {
         NONE,
         ITEM,
         SKILL
      }
      public enum Command {
         KICK,
         TELEPORT,
         GO,
         PULL,
         ITEM,
         WEAPON,
         ARMOR,
         GOLD,
         BAN_IP,
         BAN_ACC,
         UNBAN,
         SWITCH,
         MOTD,
         MUTE,
         MSG
      }
      public enum Projectile {
         WEAPON,
         SKILL
      }
      public enum Target {
         NONE,
         PLAYER,
         ENEMY
      }
      public enum Request {
         NONE,
         TRADE,
         FINISH_TRADE,
         PARTY,
         FRIEND,
         GUILD
      }
      public enum Quest {
         IN_PROGRESS,
         FINISHED
      }
      public enum Equip {
         WEAPON,
         SHIELD,
         HELMET,
         ARMOR,
         ACESSORY,
         AMULET,
         COVER,
         GLOVE,
         BOOT
      }
      public enum Param {
         MAXHP,
         MAXMP,
         ATK,
         DEF,
         MAT,
         MDF,
         AGI,
         LUK
      }
      public enum Item {
         SCOPE_ENEMY              = 1,
         SCOPE_ALL_ALLIES         = 8,
         SCOPE_ALLIES_KNOCKED_OUT = 10,
         SCOPE_USER               = 11      
      }
      public enum Move {
         FIXED,
         RANDOM,
         TOWARD_PLAYER,
         CUSTOM
      }
   }
}
