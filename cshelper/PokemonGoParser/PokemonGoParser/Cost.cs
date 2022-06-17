using Newtonsoft.Json.Linq;

namespace PokemonGoParser
{
    public class Cost
    {
        public static Cost Convert(JToken token, string stardust, string candy)
        {
            var cost = new Cost()
            {
                Stardust = int.Parse(token.TryGetString(stardust, "0")),
                Candy = int.Parse(token.TryGetString(candy, "0"))
            };
            return cost;
        }

        public int Stardust { get; set; }
        public int Candy { get; set; }
    }
}