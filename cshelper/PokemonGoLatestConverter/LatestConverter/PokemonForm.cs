namespace ConsoleApp1
{
    public class PokemonForm
    {
        public string PokemonId { get; set; }

        public string Form { get; set; }

        public bool DisableTransferToPokemonHome { get; set; }

        public bool CanEvolve { get; set; }

        public bool OriginalFormCanTempEvolveButThisCannot { get; set; }

        public static PokemonForm None = new PokemonForm()
        {
            PokemonId = "<none>"
        };

        public bool IsNone()
        {
            return PokemonId == "<none>";
        }
    }
}
