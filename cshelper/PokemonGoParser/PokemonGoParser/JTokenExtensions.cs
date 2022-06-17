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

        public static T ToValue<T>(this JToken token, string key, T defaultValue)
        {
            if (token[key] == null)
            {
                return defaultValue;
            }
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(token[key]));
        }
    }
}