using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace PokemonGoParser
{
    public class Move
    {
        public Move(JToken token)
        {
            var data = token["data"];
            var id = data["templateId"].ToString().Replace("COMBAT_", "").Replace("V", "").Split("_")[0];
            ID = int.Parse(id);
            var m = data["combatMove"];
            Name = moveType.Replace(m["uniqueId"].ToString(), "");
            Type = m["type"].ToString().Replace("POKEMON_TYPE_", "");
            if (m["uniqueId"].ToString().EndsWith("_FAST"))
            {
                MType = MoveType.Fast;
            }
            else
            {
                MType = MoveType.Charge;
            }
            Combat = new CombatMove()
            {
                Power = double.Parse(m.TryGetString("power", "0")),
                Energy = double.Parse(m.TryGetString("energyDelta", "0")),
                Buffs = m["buffs"] == null ? null : new Buff(m["buffs"])
            };
        }

        static Regex moveType = new Regex("_FAST$", RegexOptions.Compiled);

        public Move(JToken token, string V)
        {
            var data = token["data"];
            var id = data["templateId"].ToString().Replace("COMBAT_", "").Replace("V", "").Split("_")[0];
            ID = int.Parse(id);
            var m = data["moveSettings"];
            Name = moveType.Replace(m["movementId"].ToString(), "");
            if (m["movementId"].ToString().EndsWith("_FAST"))
            {
                MType = MoveType.Fast;
            }
            else
            {
                MType = MoveType.Charge;
            }
            Type = m["pokemonType"].ToString().Replace("POKEMON_TYPE_", "");
            General = new GeneralMove()
            {
                Power = double.Parse(m.TryGetString("power", "0")),
                Energy = double.Parse(m.TryGetString("energyDelta", "0")),
                AccuracyChange = double.Parse(m.TryGetString("accuracyChance", "1")),
                StaminaLossScalar = double.Parse(m.TryGetString("staminaLossScalar", "1")),
                Duration = double.Parse(m.TryGetString("durationMs", "0")),
                DamageWindowStart = double.Parse(m.TryGetString("damageWindowStartMs", "0")),
                DamageWindowEnd = double.Parse(m.TryGetString("damageWindowEndMs", "0")),
                CriticalChance = double.Parse(m.TryGetString("criticalChance", "0"))
            };
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public MoveType MType { get; set; }
        public CombatMove Combat { get; set; }
        public GeneralMove General { get; set; }
        public int ID { get; set; }

        public enum MoveType
        {
            Fast,
            Charge
        }

        public class CombatMove
        {
            public double Power { get; set; }
            public double Energy { get; set; }
            public Buff Buffs { get; set; }
        }

        public class Buff
        {
            public Buff(JToken token)
            {
                if (token != null)
                {
                    TargetAttackStatStageChange = GetNumber(token, "targetAttackStatStageChange");
                    TargetDefenseStatStageChange = GetNumber(token, "targetDefenseStatStageChange");
                    AttackerAttackStatStageChange = GetNumber(token, "attackerAttackStatStageChange");
                    AttackerDefenseStatStageChange = GetNumber(token, "attackerDefenseStatStageChange");
                    Chance = double.Parse(token.TryGetString("buffActivationChance", "0"));
                }
            }

            private static double? GetNumber(JToken token, string key)
            {
                var n = double.Parse(token.TryGetString(key, "0"));
                if (n == 0)
                {
                    return null;
                }
                return n;
            }

            public double Chance { get; set; }
            public double? TargetAttackStatStageChange { get; set; }
            public double? TargetDefenseStatStageChange { get; set; }
            public double? AttackerAttackStatStageChange { get; set; }
            public double? AttackerDefenseStatStageChange { get; set; }
        }

        public class GeneralMove
        {
            public double Power { get; set; }
            public double Energy { get; set; }
            public double AccuracyChange { get; set; }
            public double CriticalChance { get; set; }
            public double StaminaLossScalar { get; set; }
            public double Duration { get; set; }
            public double DamageWindowStart { get; set; }
            public double DamageWindowEnd { get; set; }
        }

        internal void Merge(Move move)
        {
            if (move.Name != Name && move.ID != ID && move.Type != Type)
            {
                throw new Exception();
            }
            General = move.General;
        }
    }
}