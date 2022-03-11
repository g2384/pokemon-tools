using System.Runtime.Serialization;

namespace CrawlBulbapedia
{
    public enum PokemonType
    {
        Normal, 
        Fire,
        Fighting,
        Water,
        Flying,
        Grass,
        Poison, 
        Electric,
        Ground,
        Psychic,
        Rock,
        Ice,
        Bug, 
        Dragon,
        Ghost, 
        Dark,
        Steel, 
        Fairy,
        [EnumMember(Value = "???")]
        Unknown
    }
}