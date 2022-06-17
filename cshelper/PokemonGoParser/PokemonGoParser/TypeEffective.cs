using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PokemonGoParser
{
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
}