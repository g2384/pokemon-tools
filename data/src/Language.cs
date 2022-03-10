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
        French
    }
}