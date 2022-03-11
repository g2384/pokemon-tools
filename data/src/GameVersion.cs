using System.Runtime.Serialization;

namespace CrawlBulbapedia
{
    public enum GameVersion
    {
        Ruby,
        Sapphire,
        Emerald,
        Diamond,
        Pearl,
        Sword,
        Shield,
        Events,
        Platinum,
        Gold,
        Silver,
        Crystal,
        HeartGold,
        SoulSilver,
        Red,
        Blue,
        Yellow,
        Sun,
        Moon,
        FireRed,
        LeafGreen,
        X,
        Y,
        Black,
        White,
        [EnumMember(Value = "Black 2")]
        Black2,
        [EnumMember(Value = "White 2")]
        White2,
        [EnumMember(Value = "Omega Ruby")]
        OmegaRuby,
        [EnumMember(Value = "Alpha Sapphire")]
        AlphaSapphire,
        [EnumMember(Value = "Pokémon XD")]
        PokemonXD,
        [EnumMember(Value = "Ultra Sun")]
        UltraSun,
        [EnumMember(Value = "Ultra Moon")]
        UltraMoon,
        Ranch,
        Colosseum,
        [EnumMember(Value = "Pokéwalker")]
        Pokewalker,
        Stadium,
        [EnumMember(Value = "Battle Revolution")]
        BattleRevolution,
        Channel
    }
}