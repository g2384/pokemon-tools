
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PokemonGoParser
{
    public class All
    {
        public WeatherBonusSettings WeatherBonusSettings { get; set; }
        public IList<WeatherAffinity> WeatherAffinity { get; set; } = new List<WeatherAffinity>();
        public IList<Move> Moves { get; set; } = new List<Move>();
        public IList<TypeEffective> TypeChart { get; set; } = new List<TypeEffective>();
        public Dictionary<string, PokemonBasicData> Pokemon { get; set; } = new Dictionary<string, PokemonBasicData>();
        public Dictionary<string, PokemonCombatData> Combat { get; set; } = new Dictionary<string, PokemonCombatData>();
        public IList<double> CPMultiplier { get; set; }
    }

    public class MoveSum
    {
        public static MoveSum[] Calculate(IList<Move> moves, PokemonCombatData pokemon, IList<double> cpMultiplier, IList<string> pokemonTypes)
        {
            var quickMove = pokemon.QuickMoves ?? new List<string>();
            var quickMoveElite = pokemon.QuickMovesElite ?? new List<string>();
            var chargeMove = pokemon.ChargeMoves ?? new List<string>();
            var chargeMoveElite = pokemon.ChargeMovesElite ?? new List<string>();
            var shadowMove = pokemon.Shadow?.ShadowChargeMove ?? "";
            var purifiedMove = pokemon.Shadow?.PurifiedChargeMove ?? "";
            var results = new List<MoveSum>();
            var allQuick = quickMove.ToList().Concat(quickMoveElite).ToArray();
            var allCharge = chargeMove.ToList().Concat(chargeMoveElite).Concat(new List<string>() { shadowMove }).Concat(new List<string>() { purifiedMove }).ToArray();
            foreach (var q in allQuick)
            {
                if (string.IsNullOrEmpty(q))
                {
                    continue;
                }
                var qm = moves.First(e => e.Name == q);
                foreach (var c in allCharge)
                {
                    if (string.IsNullOrEmpty(c))
                    {
                        continue;
                    }
                    var cm = moves.First(e => e.Name == c);
                    var atk = CalculateStat(pokemon.BaseStats.Attack, 15, 40, 1, cpMultiplier);
                    var def = CalculateStat(pokemon.BaseStats.Defense, 15, 40, 1, cpMultiplier);
                    var sta = CalculateStat(pokemon.BaseStats.Stamina, 15, 40, 1, cpMultiplier);
                    var bar = CalculateBar(cm.Combat.Energy, true);
                    var dps = CalculateAveDps(qm, cm, atk, def, sta, bar, pokemonTypes, c == shadowMove);
                }
            }
            //QuickMove = quickMove;
            //ChargeMove = chargeMove;
            return results.ToArray();
        }

        private static object CalculateAveDps(Move qm, Move cm, int atk, int def, int sta, int bar, IList<string> pokemonTypes, bool isShadow)
        {
            throw new NotImplementedException();
        }

        private static int CalculateBar(double energy, bool isRaid)
        {
            if (isRaid)
            {
                var bar = Math.Ceiling((double)100 / Math.Abs(energy));
                return bar > 3 ? 3 : (int)bar;
            }
            else
            {
                return Math.Abs(energy) > 50 ? 1 : 2;
            }
        }

        private static int CalculateStat(int b, int iv, int level, double addition, IList<double> cpMultiplier)
        {
            var result = (b + iv) * cpMultiplier[level];
            if (addition > 0)
            {
                result *= addition;
            }
            return (int)Math.Floor(result);
        }

        public string QuickMove { get; set; }
        public string QuickMoveType { get; set; }
        public string ChargeMove { get; set; }
        public string ChargeMoveType { get; set; }
        public double DPS { get; set; }
        public double TDO { get; set; }
        public double WeightedValue { get; set; }
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
                "adventure_sync",
                "QUEST_",
                "RECOMMENDED_",
                "STICKER_",
                "TRAINER_",
                "TEMPORARY_EVOLUTION_"
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
                "_SETTINGS_",
                "_REWARDS_"
            };
            var ignoredPokemon = new string[]
            {
                "V0351_POKEMON_CASTFORM_HOME_FORM_REVERSION",
                "V0487_POKEMON_GIRATINA_HOME_REVERSION",
                "V0555_POKEMON_DARMANITAN_HOME_FORM_REVERSION",
                "V0647_POKEMON_KELDEO_HOME_FORM_REVERSION",
                "V0648_POKEMON_MELOETTA_HOME_FORM_REVERSION",
                "V0649_POKEMON_GENESECT_HOME_FORM_REVERSION"
            };
            var ignored = new List<dynamic>();
            var accepted = new List<dynamic>();
            var all = new All();
            var spawnsData = new List<JToken>();
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
                else if (id.StartsWith("POKEMON_TYPE_"))
                {
                    var type = new TypeEffective(r);
                    all.TypeChart.Add(type);
                }
                else if (id.StartsWith("VS_SEEKER_"))
                {
                    continue;
                }
                else if (id.StartsWith("SPAWN_V"))
                {
                    spawnsData.Add(r);
                }
                else if (id == "PLAYER_LEVEL_SETTINGS")
                {
                    var d = r["data"]["playerLevel"]["cpMultiplier"].ToArray<double>();
                    all.CPMultiplier = d.ToList();
                }
                else if (ignoredPokemon.All(e => e != id) && id.StartsWith("V") && id.Contains("_POKEMON_"))
                {
                    var sd = spawnsData.FirstOrDefault(e => e["templateId"].ToString().Replace("SPAWN_", "") == id);
                    var p = PokemonBasicData.Convert(r, sd);
                    var combat = PokemonCombatData.Convert(r);
                    AddToAll(all, p);
                    AddToAll(all, combat);
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

            //var dpsTable = new Dictionary<string, MoveSum[]>();
            //foreach (var p in all.Combat)
            //{
            //    var poke = p.Value;
            //    var sum = MoveSum.Calculate(all.Moves, poke, all.CPMultiplier, all.Pokemon[poke.UniqueName].Types);
            //}

            //File.WriteAllText("new.json", JsonConvert.SerializeObject(ignored, Formatting.Indented));
            File.WriteAllText("accepted.json", JsonConvert.SerializeObject(accepted, Formatting.Indented));
            File.WriteAllText(@"C:\Users\g2386\Documents\GitHub\pokemon-tools\pokemon_go\all.json",
                JsonConvert.SerializeObject(all, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>() { new StringEnumConverter() }
                }));
        }

        private static void AddToAll(All all, PokemonBasicData p)
        {
            var matched = all.Pokemon.Where(e => e.Value.Name == p.Name).ToArray();
            foreach (var m in matched)
            {
                if (m.Value.IsEqual(p))
                {
                    if (string.IsNullOrEmpty(p.Form))
                    {
                        throw new Exception();
                    }
                    if (m.Value.Forms == null)
                    {
                        m.Value.Forms = new List<string>();
                    }
                    m.Value.Forms.Add(p.Form);
                    return;
                }
            }

            all.Pokemon.Add(p.UniqueName, p);
        }

        private static void AddToAll(All all, PokemonCombatData p)
        {
            var matched = all.Combat.Where(e => e.Value.Name == p.Name).ToArray();
            foreach (var m in matched)
            {
                if (m.Value.IsEqual(p))
                {
                    if (string.IsNullOrEmpty(p.Form))
                    {
                        throw new Exception();
                    }
                    if (m.Value.Forms == null)
                    {
                        m.Value.Forms = new List<string>();
                    }
                    m.Value.Forms.Add(p.Form);
                    return;
                }
            }
            all.Combat.Add(p.UniqueName, p);
        }
    }
}