
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PokemonGoParser
{
    public enum PokemonTypes
    {
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fair
    }

    public class WeatherBonusSettings
    {
        private readonly JToken t;
        public WeatherBonusSettings(JToken token)
        {
            t = token["data"]["weatherBonusSettings"];
        }

        public JToken Token => t;
    }

    public class WeatherAffinity
    {
        public WeatherAffinity(JToken token)
        {
            var a = token["data"]["weatherAffinities"];
            Name = a["weatherCondition"].ToString();
            PokemonTypes = JsonConvert.DeserializeObject<string[]>(JsonConvert.SerializeObject(a["pokemonType"])).Select(e => e.Replace("POKEMON_TYPE_", "")).ToArray();
        }
        public string Name { get; set; }
        public string[] PokemonTypes { get; set; }
    }

    public class All
    {
        public WeatherBonusSettings WeatherBonusSettings { get; set; }
        public IList<WeatherAffinity> WeatherAffinity { get; set; } = new List<WeatherAffinity>();
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
                if (id.StartsWith("WEATHER_AFFINITY_"))
                {
                    var weatherAfinity = new WeatherAffinity(r);
                    all.WeatherAffinity.Add(weatherAfinity);
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
            File.WriteAllText("all.json", JsonConvert.SerializeObject(all, Formatting.Indented));
        }
    }
}