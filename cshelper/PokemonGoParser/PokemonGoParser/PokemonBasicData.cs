using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PokemonGoParser
{
    public interface ICustomEqual<T>
    {
        bool IsEqual(T obj);
    }
    public class PokemonBasicData : ICustomEqual<PokemonBasicData>
    {
        public static PokemonBasicData Convert(JToken token, JToken spawnToken)
        {
            var data = token["data"];
            var settings = data["pokemonSettings"];
            var p = new PokemonBasicData();
            p.ID = int.Parse(data["templateId"].ToString().Split("_")[0].Replace("V", ""));
            p.Name = settings["pokemonId"].ToString();
            p.UniqueName = nameRegex.Replace(data["templateId"].ToString(), "");
            p.Form = settings["form"]?.ToString();
            p.Types = new List<string>();
            if (settings["type"] != null)
            {
                p.Types.Add(settings["type"].ToString().Replace("POKEMON_TYPE_", ""));
            }
            if (settings["type2"] != null)
            {
                p.Types.Add(settings["type2"].ToString().Replace("POKEMON_TYPE_", ""));
            }
            p.Height = double.Parse(settings.TryGetString("pokedexHeightM", "0"));
            p.Weight = double.Parse(settings.TryGetString("pokedexWeightKg", "0"));
            p.HeightStdDev = double.Parse(settings.TryGetString("heightStdDev", "0"));
            p.WeightStdDev = double.Parse(settings.TryGetString("weightStdDev", "0"));
            p.BuddyDistance = double.Parse(settings.TryGetString("kmBuddyDistance", "0"));
            p.IsTradable = settings.ToValue("isTradable", false);
            p.IsTransferable = settings.ToValue("isTransferable", false);
            p.IsDeployable = settings.ToValue("isDeployable", false);
            p.IsReleased = JsonConvert.SerializeObject(settings["camera"]).Length > 3;
            ToGender(spawnToken, p);
            return p;
        }

        private static void ToGender(JToken spawnToken, PokemonBasicData p)
        {
            var gender = spawnToken["data"]["genderSettings"]["gender"];
            if(gender != null)
            {
                p.GenderRatio = new Dictionary<string, double>();
                foreach (JProperty k in gender.Children())
                {
                    if(k.Name == "pokemon")
                    {
                        continue;
                    }
                    if(k.Name == "malePercent")
                    {
                        var m = double.Parse(k.Value.ToString());
                        if (m > 0)
                        {
                            p.GenderRatio.Add("M", m);
                        }
                        continue;
                    }
                    if (k.Name == "femalePercent")
                    {
                        var m = double.Parse(k.Value.ToString());
                        if (m > 0)
                        {
                            p.GenderRatio.Add("F", m);
                        }
                        continue;
                    }
                    if (k.Name == "genderlessPercent")
                    {
                        var m = double.Parse(k.Value.ToString());
                        if (m > 0)
                        {
                            p.GenderRatio.Add("N", m);
                        }
                        continue;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }

        static Regex nameRegex = new Regex(@"V[\d]+_POKEMON_", RegexOptions.Compiled);

        public int ID { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string Form { get; set; }
        public IList<string> Forms { get; set; }
        public IList<string> Types { get; set; }
        public IDictionary<string, double> GenderRatio { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public double HeightStdDev { get; set; }
        public double WeightStdDev { get; set; }
        public double BuddyDistance { get; set; }
        public string Color { get; set; }
        public List<string> Evos { get; set; }
        public bool IsReleased { get; set; }
        public bool IsDeployable { get; set; }
        public bool IsTradable { get; set; }
        public bool IsTransferable { get; set; }
        public string EvoPre { get; set; }

        static JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public bool IsEqual(PokemonBasicData pokemon)
        {
            var from = JsonConvert.SerializeObject(this, jSettings);
            var fromP = JsonConvert.DeserializeObject<PokemonBasicData>(from);
            SetToNull(fromP);
            from = JsonConvert.SerializeObject(fromP);
            var pokemonJson = JsonConvert.SerializeObject(pokemon, jSettings);
            var pokemonJsonP = JsonConvert.DeserializeObject<PokemonBasicData>(pokemonJson);
            SetToNull(pokemonJsonP);
            pokemonJson = JsonConvert.SerializeObject(pokemonJsonP);
            return from == pokemonJson;
        }

        private static void SetToNull(PokemonBasicData fromP)
        {
            fromP.Form = null;
            fromP.Forms = null;
            fromP.UniqueName = null;
        }
    }
}