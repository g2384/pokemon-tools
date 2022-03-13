using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace CrawlBulbapedia
{
    public enum LanguageCode
    {
        [EnumMember(Value="ja-Hrkt")]
        jaHrkt,
        roomaji,
        ko,
        [EnumMember(Value = "zh-Hant")]
        zhHant,
        fr,
        de,
        es,
        it,
        en,
        cs,
        ja,
        [EnumMember(Value = "zh-Hans")]
        zhHans,
        br,
        [EnumMember(Value = "pt-BR")]
        ptBR
    }
    public static class PokeApiParser
    {
        internal static void ConvertToData(string path, string targetPath)
        {
            var dPokemon = ReadCsvFile(path, "pokemon.csv", 0);
            // id:identifier,species_id,height,weight,base_experience,order,is_default

            var dPokemonSpeciesNames = ReadCsvFile(path, "pokemon_species_names.csv", 0);
            // pokemon_species_id:local_language_id,name,genus

            var dLanguages = ReadCsvFile(path, "languages.csv", 0);
            //id:iso639,iso3166,identifier,official,order
            var language = dLanguages.Where(e => e.Value[0][3] == "1").ToDictionary(e => e.Key, e => e.Value[0][2].ToEnum<LanguageCode>());//id:identifier

            var dPokemonItems = ReadCsvFile(path, "pokemon_items.csv", 0);
            //pokemon_id:version_id,item_id,rarity

            var pokemons = new List<Pokemon2>();
            foreach (var item in dPokemon)
            {
                var pokemon = new Pokemon2();
                pokemons.Add(pokemon);
                var speciesId = item.Value[0][1];
                pokemon.No = int.Parse(speciesId); // species_id
                pokemon.Id = int.Parse(item.Key);
                var nameLanguages = dPokemonSpeciesNames[speciesId];
                var names = ConvertToForeignName(nameLanguages, language);
                pokemon.Name = item.Value[0][0]; // identifier
                pokemon.OtherNames = names;


            }

            var ordered = pokemons.OrderBy(e => e.Id);
            var json = JsonConvert.SerializeObject(ordered, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var output = Path.Combine(targetPath, "data3.json");
            File.WriteAllText(output, json);
        }

        private static IDictionary<LanguageCode, string> ConvertToForeignName(string[][] names, IDictionary<string, LanguageCode> langDict)
        {
            var fn = new Dictionary<LanguageCode, string>();
            foreach (var name in names)
            {
                if(langDict.TryGetValue(name[0], out var lan))
                {
                    fn[lan] = name[1];
                }
            }
            return fn;
        }

        private static IDictionary<string, string[][]> ReadCsvFile(string path, string fileName, int key)
        {
            var f = Path.Combine(path, fileName);
            var t = File.ReadAllLines(f);
            foreach (var line in t.Skip(1))
            {
                if (line.Contains("\""))
                {
                    throw new Exception();
                }
            }
            var temp1 = t.Skip(1).Select(x => x.Split(",")).GroupBy(e => e[key]);
            var temp2 = temp1.ToDictionary(e => e.Key, e => e.Select(x => x.Skip(1).ToArray()).ToArray());
            return temp2;
        }
    }
}