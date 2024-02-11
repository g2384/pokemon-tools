using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Template
    {
        public string TemplateId { get; set; }
        public IDictionary<string, dynamic> Data { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var latestJson = @"..\..\..\..\..\..\pokemon_go\latest.json";
            var allLines = File.ReadAllText(latestJson);
            var objects = JsonSerializer.Deserialize<Template[]>(allLines, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var objectSettings = new List<Template>();
            var others = new List<Template>();
            var avatars = new List<Template>();
            var badges = new List<Template>();
            var characters = new List<Template>();
            var combatLeague = new List<Template>();
            var combatV = new List<Template>();
            var stickers = new List<Template>();
            var animation = new List<Template>();
            var ignoredSettingsRegex = new List<Regex>()
            {
                new Regex(@"COMBAT_RANKING_SETTINGS(_S\d+)?"),
                new Regex(@"^INVASION_"),
                new Regex(@"^PARTY_")
            };
            foreach (var o in objects!)
            {
                if (o.TemplateId.Contains("SETTINGS"))
                {
                    if (ignoredSettingsRegex.Any(e => e.IsMatch(o.TemplateId)))
                    {
                        continue;
                    }
                    objectSettings.Add(o);
                }
                else if (o.TemplateId.StartsWith("pgorelease")
                    || o.TemplateId.StartsWith("general1.ticket")
                    || o.TemplateId.StartsWith("bundle"))
                {
                    continue;
                }
                else if (o.TemplateId.StartsWith("AVATAR_"))
                {
                    avatars.Add(o);
                }
                else if (o.TemplateId.StartsWith("BADGE"))
                {
                    badges.Add(o);
                }
                else if (o.TemplateId.StartsWith("CHARACTER_"))
                {
                    characters.Add(o);
                }
                else if (o.TemplateId.StartsWith("COMBAT_LEAGUE_"))
                {
                    combatLeague.Add(o);
                }
                else if (o.TemplateId.StartsWith("COMBAT_V"))
                {
                    combatV.Add(o);
                }
                else if (o.TemplateId.StartsWith("STICKER_"))
                {
                    stickers.Add(o);
                }
                else if (o.TemplateId.StartsWith("camera_") || o.TemplateId.StartsWith("sequence_"))
                {
                    animation.Add(o);
                }
                else
                {
                    others.Add(o);
                }
            }

            Save(objectSettings, "Settings.json");
            Save(others, "Others.json");
            Save(avatars, "Avatars.json");
            Save(badges, "Badges.json");
            Save(characters, "Characters.json");
            Save(combatLeague, "CombatLeague.json");
            Save(combatV, "CombatMoves.json");
            Save(stickers, "Stickers.json");
            Save(animation, "Animation.json");
        }

        private static void Save(List<Template> objectSettings, string fileName)
        {
            var str = JsonSerializer.Serialize(objectSettings, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            File.WriteAllText(fileName, str);
        }
    }
}
