using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
        private static string _outputFolder = @"..\..\..\..\..\..\pokemon_go\latest";
        private static TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

        static void Main(string[] args)
        {
            var latestJson = @"..\..\..\..\..\..\pokemon_go\latest.json";
            var allLines = File.ReadAllText(latestJson);
            var allNodes = JsonNode.Parse(allLines)!;
            var objectSettings = new JsonArray();
            var others = new JsonArray();
            var avatars = new JsonArray();
            var badges = new JsonArray();
            var characters = new JsonArray();
            var combatLeague = new JsonArray();
            var combatV = new JsonArray();
            var stickers = new JsonArray();
            var animation = new JsonArray();
            var pokemons = new JsonArray();
            var moves = new JsonArray();
            var pokemonFamilies = new JsonArray();
            var ignoredSettingsRegex = new List<Regex>()
            {
                new Regex(@"COMBAT_RANKING_SETTINGS(_S\d+)?", RegexOptions.Compiled),
                new Regex(@"^INVASION_", RegexOptions.Compiled),
                new Regex(@"^PARTY_", RegexOptions.Compiled)
            };
            var familyRegex = new Regex(@"^V\d+_FAMILY_", RegexOptions.Compiled);
            var pokemonRegex = new Regex(@"^V\d+_POKEMON_", RegexOptions.Compiled);
            var moveRegex = new Regex(@"^V\d+_MOVE_", RegexOptions.Compiled);
            foreach (var o in allNodes.AsArray())
            {
                var oCopy = o.DeepClone();
                var obj = oCopy!.AsObject();
                var templateId = obj["templateId"]!.GetValue<string>();
                if (templateId.Contains("SETTINGS"))
                {
                    if (ignoredSettingsRegex.Any(e => e.IsMatch(templateId)))
                    {
                        continue;
                    }
                    objectSettings.Add(oCopy);
                }
                else if (templateId.StartsWith("pgorelease")
                    || templateId.StartsWith("general1.ticket")
                    || templateId.StartsWith("bundle"))
                {
                    continue;
                }
                else if (templateId.StartsWith("AVATAR_"))
                {
                    avatars.Add(oCopy);
                }
                else if (templateId.StartsWith("BADGE"))
                {
                    badges.Add(oCopy);
                }
                else if (templateId.StartsWith("CHARACTER_"))
                {
                    characters.Add(oCopy);
                }
                else if (templateId.StartsWith("COMBAT_LEAGUE_"))
                {
                    combatLeague.Add(oCopy);
                }
                else if (templateId.StartsWith("COMBAT_V"))
                {
                    combatV.Add(oCopy);
                }
                else if (templateId.StartsWith("STICKER_"))
                {
                    stickers.Add(oCopy);
                }
                else if (templateId.StartsWith("camera_") || templateId.StartsWith("sequence_"))
                {
                    animation.Add(oCopy);
                }
                else if (familyRegex.IsMatch(templateId))
                {
                    pokemonFamilies.Add(oCopy);
                }
                else if (pokemonRegex.IsMatch(templateId))
                {
                    if (obj["data"]!.AsObject().TryGetPropertyValue("pokemonSettings", out var pokemonSettings))
                    {
                        var pokemonSettingsObj = pokemonSettings!.AsObject();
                        pokemonSettingsObj.Remove("camera");
                        pokemonSettingsObj.Remove("animationTime");
                        pokemonSettingsObj.Remove("buddyOffsetMale");
                        pokemonSettingsObj.Remove("buddyOffsetFemale");
                        pokemonSettingsObj.Remove("buddyGroupNumber");
                        pokemonSettingsObj.Remove("buddyScale");
                        pokemonSettingsObj.Remove("buddyPortraitOffset");
                        pokemonSettingsObj.Remove("modelHeight");
                        pokemonSettingsObj.Remove("modelScale");
                        pokemonSettingsObj.Remove("modelScaleV2");
                        pokemonSettingsObj.Remove("candyToEvolve"); // reason: already in evolutionBranch
                        pokemonSettingsObj.Remove("raidBossDistanceOffset");

                         var type1 = ConvertPokemonType(pokemonSettingsObj["type"]);
                        if (pokemonSettingsObj.ContainsKey("type2"))
                        {
                            var type2 = ConvertPokemonType(pokemonSettingsObj["type2"]);
                            pokemonSettingsObj["types"] = new JsonArray(type1, type2);
                        }
                        else
                        {
                            pokemonSettingsObj["types"] = new JsonArray(type1);
                        }
                        pokemonSettingsObj.Remove("type");
                        pokemonSettingsObj.Remove("type2");

                        pokemonSettingsObj["pokemonId"] = ConvertToTitleCase(pokemonSettingsObj["pokemonId"]);
                        if (pokemonSettingsObj.TryGetPropertyValue("encounter", out var encounter))
                        {
                            var encounterObj = encounter!.AsObject();
                            encounterObj.Remove("collisionRadiusM");
                            encounterObj.Remove("collisionHeightM");
                            encounterObj.Remove("collisionHeadRadiusM");
                            encounterObj.Remove("movementTimerS");
                            encounterObj.Remove("jumpTimeS");
                            encounterObj.Remove("attackTimerS");
                            encounterObj.Remove("dodgeDurationS");
                            encounterObj.Remove("dodgeDistance");
                            encounterObj.Remove("minPokemonActionFrequencyS");
                            encounterObj.Remove("maxPokemonActionFrequencyS");
                        }
                    }
                    pokemons.Add(oCopy);
                }
                else if (moveRegex.IsMatch(templateId))
                {
                    moves.Add(oCopy);
                }
                else
                {
                    others.Add(oCopy);
                }
            }

            Save(objectSettings, "settings.json");
            Save(others, "others.json");
            Save(avatars, "avatars.json");
            Save(badges, "badges.json");
            Save(characters, "characters.json");
            Save(combatLeague, "combatLeague.json");
            Save(combatV, "combatMoves.json");
            Save(stickers, "stickers.json");
            Save(animation, "animation.json");
            Save(pokemons, "pokemons.json");
            Save(moves, "moves.json");
            Save(pokemonFamilies, "pokemonFamilies.json");
        }

        private static JsonNode? ConvertToTitleCase(JsonNode? jsonNode)
        {
            var str = jsonNode!.GetValue<string>().ToLowerInvariant();
            str = _textInfo.ToTitleCase(str);
            return str;
        }

        private static string ConvertPokemonType(JsonNode? jsonNode)
        {
            var str = jsonNode!.GetValue<string>();
            str = str.Replace("POKEMON_TYPE_", "").ToLowerInvariant();
            return str;
        }

        private static void Save(JsonArray objectSettings, string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var str = objectSettings.ToJsonString(options);
            var outputFolder = Path.Combine(_outputFolder, fileName);
            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
            }
            File.WriteAllText(Path.Combine(_outputFolder, fileName), str);
        }
    }
}
