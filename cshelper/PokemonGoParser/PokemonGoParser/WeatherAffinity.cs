
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace PokemonGoParser
{
    public class WeatherAffinity
    {
        public WeatherAffinity(JToken token)
        {
            var a = token["data"]["weatherAffinities"];
            Name = a["weatherCondition"].ToString();
            PokemonTypes = a["pokemonType"].ToArray<string>().Select(e => e.Replace("POKEMON_TYPE_", "")).ToArray();
        }
        public string Name { get; set; }
        public string[] PokemonTypes { get; set; }
    }
}