using Newtonsoft.Json.Linq;

namespace PokemonGoParser
{
    public class WeatherBonusSettings
    {
        private readonly JToken t;
        public WeatherBonusSettings(JToken token)
        {
            t = token["data"]["weatherBonusSettings"];
        }

        public JToken Token => t;
    }
}