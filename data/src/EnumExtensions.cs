using Newtonsoft.Json;

namespace CrawlBulbapedia
{
    public static class EnumExtensions
    {
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : Enum
        {
            var jsonString = $"'{value.ToLower()}'";
            return JsonConvert.DeserializeObject<TEnum>(jsonString, Program.SEConverter);
        }

        public static bool EqualsTo<TEnum>(this string strA, TEnum enumB) where TEnum : Enum
        {
            TEnum enumA;
            try
            {
                enumA = strA.ToEnum<TEnum>();
            }
            catch
            {
                return false;
            }
            return enumA.Equals(enumB);
        }
    }
}