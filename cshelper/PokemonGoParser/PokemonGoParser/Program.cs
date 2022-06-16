
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
    }

    public class TypeEffective
    {
        public TypeEffective(JToken token)
        {
            Name = token["data"]["templateId"].ToString().Replace("POKEMON_TYPE_", "");
            var s = token["data"]["typeEffective"]["attackScalar"].ToArray<double>();
            for(var i = 0; i < s.Length; i++)
            {
                var type = (PokemonType)i;
                var value = s[i];
                Effective.Add(type, value);
            }
        }


        public string Name { get; set; }
        public IDictionary<PokemonType, double> Effective { get; set; } = new Dictionary<PokemonType, double>();
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
                "RECOMMENDED_"
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
                else if (id.StartsWith("POKEMON_TYPE_"))
                {
                    var type = new TypeEffective(r);
                    all.TypeChart.Add(type);
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