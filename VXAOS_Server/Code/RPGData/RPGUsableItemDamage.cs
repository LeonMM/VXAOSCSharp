using NCalc;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace VXAOS_Server.RPGData {
	public class RPGUsableItemDamage {
		public double type = 0;
		public double element_id = 0;
		public string formula = "0";
		public double variance = 20;
		public bool critical = false;
      public bool IsNone() {
         return type == 0;
      }
      public bool IsToHp() {
         return type == 1 || type == 3 || type == 5;
      }
      public bool IsToMp() {
         return type == 2 || type == 4 || type == 6;
      }
      public bool IsRecover() {
         return type == 3 || type == 4;
      }
      public bool IsDrain() {
         return type == 5 || type == 6;
      }
      public int Sign() {
         return IsRecover() ? - 1 : 1;
      }
      private static readonly Dictionary<string, Func<GameBattler, float>> BattlerParameters =
          new(StringComparer.OrdinalIgnoreCase) {
             ["atk"] = b => b.Atk,
             ["def"] = b => b.Def,
             ["mat"] = b => b.Mat,
             ["mdf"] = b => b.Mdf,
             ["agi"] = b => b.Agi,
             ["luk"] = b => b.Luk,
             ["mhp"] = b => b.Mhp,
             ["mmp"] = b => b.Mmp,
             ["hp"] = b => b.Hp,
             ["mp"] = b => b.Mp,
             ["tp"] = b => b.Tp,
             ["level"] = b => b.Level
          };
      private Expression _expression;
      private GameBattler _a = null!;
      private GameBattler _b = null!;
      private List<int> _v = null!;
      [OnDeserialized]
      internal void OnDeserialized(StreamingContext context) {
         var parsed = formula;
         parsed = Regex.Replace(parsed, @"\ba\.([A-Za-z_]\w*)", "a_$1", RegexOptions.IgnoreCase);
         parsed = Regex.Replace(parsed, @"\bb\.([A-Za-z_]\w*)", "b_$1", RegexOptions.IgnoreCase);
         parsed = Regex.Replace(parsed, @"\bv\[(\d+)\]", "v_$1", RegexOptions.IgnoreCase);
         _expression = new Expression(parsed);
         RegisterBattlerParameters();
         RegisterVariableParameters(formula);
         RegisterFunctions();
      }
      private void RegisterBattlerParameters() {
         foreach (var kv in BattlerParameters) {
            string member = kv.Key;
            var getter = kv.Value;
            _expression.DynamicParameters["a_" + member] = _ => getter(_a);
            _expression.DynamicParameters["b_" + member] = _ => getter(_b);
         }
      }
      private void RegisterVariableParameters(string formula) {
         var variables = new HashSet<int>();
         foreach (Match match in Regex.Matches(formula, @"\bv_(\d+)\b"))
            variables.Add(int.Parse(match.Groups[1].Value));
         foreach (int id in variables) {
            int index = id;
            _expression.DynamicParameters["v_" + index] = _ =>
               index >= 0 && index < _v.Count
                  ? _v[index]
                  : 0;
         }
      }

      private void RegisterFunctions() {
         _expression.Functions["rand"] = args => {
            if (args.Count == 0)
               return Random.Shared.NextSingle();
            float max = Convert.ToSingle(args.Evaluate(0));
            if (max <= 0)
               return 0f;
            return Random.Shared.NextSingle() * max;
         };
         _expression.Functions["min"] = args =>
            Math.Min(
               Convert.ToSingle(args.Evaluate(0)),
               Convert.ToSingle(args.Evaluate(1)));
         _expression.Functions["max"] = args =>
            Math.Max(
               Convert.ToSingle(args.Evaluate(0)),
               Convert.ToSingle(args.Evaluate(1)));
         _expression.Functions["abs"] = args =>
            Math.Abs(
               Convert.ToSingle(args.Evaluate(0)));
      }
      public float Eval(GameBattler a, GameBattler b, List<int> variables) {
         _a = a;
         _b = b;
         _v = variables;
         try {
            float result = Convert.ToSingle(_expression.Evaluate(), CultureInfo.InvariantCulture);
            return MathF.Max(result, 0) * Sign();
         } catch {
            return 0f;
         }
      }
   }
}
