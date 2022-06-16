
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PokemonGoParser
{
    public static class JTokenExtensions
    {
        public static string TryGetString(this JToken token, string key, string defaultValue = "")
        {
            try
            {
                return token[key]?.ToString() ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

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

    public class All
    {
        public WeatherBonusSettings WeatherBonusSettings { get; set; }
        public IList<WeatherAffinity> WeatherAffinity { get; set; } = new List<WeatherAffinity>();
        public IList<Move> Moves { get; set; } = new List<Move>();
    }

    public class PokemonType
    {
        public string Name { get; set; }
        public IDictionary<string, double> TypeEffective { get; set; } = new Dictionary<string, double>();
    }

    public static class Program
    {
        public static void Main()
        {
            var json = File.ReadAllText("pokemon_raw.json");
            var rawData = JsonConvert.DeserializeObject(json) as JArray;
            var ignoredStartIDs = new string[]
            {
                "AVATAR_",
                "AWARDS_",
                "BADGE_",
                "BATTLE_",
                "BUDDY_",
                "CHARACTER_",
                //"COMBAT_",
                "ENABLE_",
                "ENERGY_COSTS_",
                "EVOLUTION_",
                "FORMS_",
                "FORT_",
                "FRIENDSHIP_",
                "IAP_",
                "ITEM_",
                "LPSKU_",
                "MEGA_EVOLUTION_",
                "POKECOIN_",
                "sequence_",
                "pgorelease.",
                "general1",
                "camera_",
                "bundle.",
                "COMBAT_LEAGUE_",
                "COMBAT_POKEMON_TYPE_",
                "itemleadermap",
                "hometransport.",
                "adventure_sync"
            };
            var newIgnoredStartIDs = new string[]
            {
            };
            var ignoredEndIDs = new string[]
            {
                "_SETTINGS",
                "_WHITELIST",
                "_QUEST",
                "_SETTING",
                "_AVATAR",
                "_settings"
            };
            var newIgnoredEndIDs = new string[]
            {
            };
            var ignoreMidIDs = new string[]
            {
                "_SETTINGS_"
            };
            var ignored = new List<dynamic>();
            var accepted = new List<dynamic>();
            var all = new All();
            foreach (JToken r in rawData)
            {
                var id = r["templateId"].ToString();
                if (id == "WEATHER_BONUS_SETTINGS")
                {
                    var weatherBonus = new WeatherBonusSettings(r);
                    all.WeatherBonusSettings = weatherBonus;
                }
                else if (id.StartsWith("WEATHER_AFFINITY_"))
                {
                    var weatherAfinity = new WeatherAffinity(r);
                    all.WeatherAffinity.Add(weatherAfinity);
                }
                else if (id.StartsWith("COMBAT_V"))
                {
                    var move = new Move(r);
                    if (all.Moves.Any(e => e.Name == move.Name))
                    {
                        throw new Exception();
                    }
                    all.Moves.Add(move);
                }
                else if (id.StartsWith("V") && id.Contains("_MOVE_"))
                {
                    var move = new Move(r, "v");
                    var oldMove = all.Moves.FirstOrDefault(e => e.Name == move.Name);
                    if (oldMove != null)
                    {
                        oldMove.Merge(move);
                    }
                    else
                    {
                        all.Moves.Add(move);
                    }
                }
                else if (ignoredStartIDs.Any(e => id.StartsWith(e)) ||
                    ignoredEndIDs.Any(e => id.EndsWith(e)) ||
                    ignoreMidIDs.Any(e => id.Contains(e)))
                {
                    //ignored.Add(r);
                    continue;
                }
                else if (newIgnoredStartIDs.Any(e => id.StartsWith(e)) ||
                    newIgnoredEndIDs.Any(e => id.EndsWith(e)))
                {
                    ignored.Add(r);
                    continue;
                }
                else
                {
                    accepted.Add(r);
                    //throw new InvalidDataException();
                }
            }

            //File.WriteAllText("new.json", JsonConvert.SerializeObject(ignored, Formatting.Indented));
            File.WriteAllText("accepted.json", JsonConvert.SerializeObject(accepted, Formatting.Indented));
            File.WriteAllText(@"C:\Users\g2386\Documents\GitHub\pokemon-tools\pokemon_go\all.json",
                JsonConvert.SerializeObject(all, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>() { new StringEnumConverter() }
                }));
        }
    }
}