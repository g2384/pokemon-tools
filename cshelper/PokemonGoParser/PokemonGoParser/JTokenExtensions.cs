using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static T[] ToArray<T>(this JToken token)
        {
            return JsonConvert.DeserializeObject<T[]>(JsonConvert.SerializeObject(token));
        }
    }
}