
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PokemonGoParser
{
    public class PokemonCombatData : ICustomEqual<PokemonCombatData>
    {
        public static PokemonCombatData Convert(JToken token)
        {
            var data = token["data"];
            var settings = data["pokemonSettings"];
            var p = new PokemonCombatData();
            p.ID = int.Parse(data["templateId"].ToString().Split("_")[0].Replace("V", ""));
            p.Name = settings["pokemonId"].ToString();
            p.UniqueName = nameRegex.Replace(data["templateId"].ToString(), "");
            if (p.Name.Contains(p.UniqueName))
            {
                p.UniqueName = p.Name;
            }
            p.Form = settings["form"]?.ToString();
            p.Shadow = settings["shadow"] == null ? null : ShadowInfo.Convert(settings["shadow"]);
            p.ThirdMove = settings["thirdMove"] == null ? null : Cost.Convert(settings["thirdMove"], "stardustToUnlock", "candyToUnlock");
            p.BaseStats = settings["stats"] == null ? null : Stats.Convert(settings["stats"]);

            p.QuickMoves = settings["quickMoves"]?.ToArray<string>().Select(e => fastRegex.Replace(e, "")).ToList();
            p.ChargeMoves = settings["cinematicMoves"]?.ToArray<string>().ToList();
            p.QuickMovesElite = settings["eliteQuickMove"]?.ToArray<string>().Select(e => fastRegex.Replace(e, "")).ToList();
            p.ChargeMovesElite = settings["eliteCinematicMove"]?.ToArray<string>().ToList();
            return p;
        }

        static JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public bool IsEqual(PokemonCombatData pokemon)
        {
            var from = JsonConvert.SerializeObject(this, jSettings);
            var fromP = JsonConvert.DeserializeObject<PokemonCombatData>(from);
            SetToNull(fromP);
            from = JsonConvert.SerializeObject(fromP);
            var pokemonJson = JsonConvert.SerializeObject(pokemon, jSettings);
            var pokemonJsonP = JsonConvert.DeserializeObject<PokemonCombatData>(pokemonJson);
            SetToNull(pokemonJsonP);
            pokemonJson = JsonConvert.SerializeObject(pokemonJsonP);
            return from == pokemonJson;
        }

        private static void SetToNull(PokemonCombatData fromP)
        {
            fromP.Form = null;
            fromP.Forms = null;
            fromP.UniqueName = null;
        }

        static Regex nameRegex = new Regex(@"V[\d]+_POKEMON_", RegexOptions.Compiled);
        static Regex fastRegex = new Regex(@"_FAST$", RegexOptions.Compiled);

        public List<string> QuickMoves { get; set; }
        public List<string> ChargeMoves { get; set; }
        public List<string> ChargeMovesElite { get; set; }
        public List<string> QuickMovesElite { get; set; }
        public Cost ThirdMove { get; set; }
        public ShadowInfo Shadow { get; set; }

        public string Form { get; set; }
        public IList<string> Forms { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public Stats BaseStats { get; set; }

        public class Stats
        {
            public static Stats Convert(JToken token)
            {
                var p = new Stats();
                foreach(JProperty t in token.Children())
                {
                    if(t.Name == "baseStamina")
                    {
                        p.Stamina = int.Parse(t.Value.ToString());
                    }
                    else if (t.Name == "baseAttack")
                    {
                        p.Attack = int.Parse(t.Value.ToString());
                    }
                    else if (t.Name == "baseDefense")
                    {
                        p.Defense = int.Parse(t.Value.ToString());
                    }
                }
                return p;
            }

            public int Stamina { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
        }

        public class ShadowInfo
        {
            public static ShadowInfo Convert(JToken token)
            {
                var s = new ShadowInfo()
                {
                    Purification = Cost.Convert(token, "purificationStardustNeeded", "purificationCandyNeeded"),
                    ShadowChargeMove = token["shadowChargeMove"].ToString(),
                    PurifiedChargeMove = token["purifiedChargeMove"].ToString()
                };
                return s;
            }

            public string PurifiedChargeMove { get; set; }
            public string ShadowChargeMove { get; set; }
            public Cost Purification { get; set; }
        }
    }
}