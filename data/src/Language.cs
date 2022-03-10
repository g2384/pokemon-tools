using System.Runtime.Serialization;

namespace CrawlBulbapedia
{
    public enum Language
    {
        [EnumMember(Value = "Japanese")]
        Japanese,
        [EnumMember(Value = "Mandarin Chinese")]
        ChineseM,
        Italian,
        Korean,
        Thai,
        [EnumMember(Value = "Cantonese Chinese")]
        ChineseC,
        German,
        Spanish,
        French,
        [EnumMember(Value = "French Canada")]
        CanadaFr,
        [EnumMember(Value = "Brazilian Portuguese")]
        PortugueseBr,
        [EnumMember(Value = "Portugal Portuguese")]
        PortuguesePo,
        Polish,
        Russian,
        Swedish,
        Finnish,
        [EnumMember(Value = "Latin America Spanish")]
        SpanishLantinAmerica,
        [EnumMember(Value = "Spain Spanish")]
        SpanishSp,
        Vietnamese,
        Danish,
        Dutch,
        Norwegian,
        English,
        Hebrew,
        Greek,
        Hindi,
        Indonesian,
        Serbian,
        Turkish,
        Czech,
        Romanian,
        Hungarian,
        Arabic,
        Bulgarian,
        Valencian,
        Ukrainian
    }
}