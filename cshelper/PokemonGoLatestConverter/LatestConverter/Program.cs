﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
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
                        pokemonSettingsObj.Remove("buddyScale");
                        pokemonSettingsObj.Remove("buddyPortraitOffset");
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

            Save(objectSettings, "Settings.json");
            Save(others, "Others.json");
            Save(avatars, "Avatars.json");
            Save(badges, "Badges.json");
            Save(characters, "Characters.json");
            Save(combatLeague, "CombatLeague.json");
            Save(combatV, "CombatMoves.json");
            Save(stickers, "Stickers.json");
            Save(animation, "Animation.json");
            Save(pokemons, "Pokemons.json");
            Save(moves, "Moves.json");
            Save(pokemonFamilies, "PokemonFamilies.json");
        }

        private static void Save(JsonArray objectSettings, string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var str = objectSettings.ToJsonString(options);
            File.WriteAllText(fileName, str);
        }
    }
}