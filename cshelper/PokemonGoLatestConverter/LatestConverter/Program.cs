using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
            var evolutionQuests = new JsonArray();
            var pokemonForms = new List<PokemonForm>();
            var pokemonExtended = new JsonArray();
            var ignoredSettingsRegex = new List<Regex>()
            {
                new Regex(@"COMBAT_RANKING_SETTINGS(_S\d+)?", RegexOptions.Compiled),
                new Regex(@"^INVASION_", RegexOptions.Compiled),
                new Regex(@"^PARTY_", RegexOptions.Compiled)
            };
            var familyRegex = new Regex(@"^V\d+_FAMILY_", RegexOptions.Compiled);
            var pokemonRegex = new Regex(@"^V\d+_POKEMON_", RegexOptions.Compiled);
            var moveRegex = new Regex(@"^V\d+_MOVE_", RegexOptions.Compiled);
            var allEvolutionRules = new HashSet<string>()
            {
                // known ones that are already handled in the webpage
                "evolution",
                "candyCost",
                "form",
                "candyCostPurified",
                "evolutionItemRequirement",
                "priority",
                "questDisplay",
                "noCandyCostViaTrade",
                "lureItemRequirement",
                "kmBuddyDistanceRequirement",
                "mustBeBuddy",
                "onlyDaytime",
                "onlyNighttime",
                "onlyFullMoon"
            };
            var sizeSettingsKeys = new HashSet<string>()
            {
                "xxsLowerBound",
                "xsLowerBound",
                "mLowerBound",
                "mUpperBound",
                "xlUpperBound",
                "xxlUpperBound",
                "xxsScaleMultiplier",
                "xsScaleMultiplier",
                "xlScaleMultiplier",
                "xxlScaleMultiplier"
            };
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
                else if (templateId.StartsWith("EXTENDED_POKEMON_") || templateId.StartsWith("EXTENDED_V"))
                {
                    if (obj["data"]!.AsObject().TryGetPropertyValue("pokemonExtendedSettings", out var pokemonSettings))
                    {
                        var pokemonSettingsObj = pokemonSettings!.AsObject();
                        var sizeSettings = pokemonSettingsObj["sizeSettings"]!.AsObject();
                        var sizeBounds = GetSizeBounds(sizeSettings);
                        var sizeScaleMultiplier = GetSizeScaleMultiplier(sizeSettings);

                        foreach (var e in sizeSettings)
                        {
                            if (sizeSettingsKeys.Add(e.Key))
                            {
                                Console.WriteLine($"Size Settings keys: template = '{templateId}', key = '{e.Key}'");
                            }
                        }

                        pokemonSettingsObj["sizeBounds"] = new JsonArray([sizeBounds.XXS, sizeBounds.XS, sizeBounds.MLower, sizeBounds.MUpper, sizeBounds.XL, sizeBounds.XXL]);
                        sizeSettings.Remove("xxsLowerBound");
                        sizeSettings.Remove("xsLowerBound");
                        sizeSettings.Remove("mLowerBound");
                        sizeSettings.Remove("mUpperBound");
                        sizeSettings.Remove("xlUpperBound");
                        sizeSettings.Remove("xxlUpperBound");

                        if (!sizeScaleMultiplier.IsNone())
                        {
                            pokemonSettingsObj["sizeScaleMultiplier"] = new JsonArray([sizeScaleMultiplier.XXS, sizeScaleMultiplier.XS, sizeScaleMultiplier.XL, sizeScaleMultiplier.XXL]);
                            sizeSettings.Remove("xxsScaleMultiplier");
                            sizeSettings.Remove("xsScaleMultiplier");
                            sizeSettings.Remove("xlScaleMultiplier");
                            sizeSettings.Remove("xxlScaleMultiplier");
                        }

                        if (sizeSettings.Count == 0)
                        {
                            pokemonSettingsObj.Remove("sizeSettings");
                        }
                    }

                    pokemonExtended.Add(oCopy);
                }
                else if (pokemonRegex.IsMatch(templateId))
                {
                    var hasForm = false;
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
                        pokemonSettingsObj.Remove("buddySize");

                        var stats = pokemonSettingsObj["stats"]!.AsObject();
                        if (stats.Count > 0)
                        {
                            var sValue = stats["baseStamina"]!.GetValue<int>();
                            var aValue = stats["baseAttack"]!.GetValue<int>();
                            var dValue = stats["baseDefense"]!.GetValue<int>();
                            pokemonSettingsObj["stats"] = new JsonArray([sValue, aValue, dValue]); // Stamina, Attack, Defense
                        }

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
                        if (pokemonSettingsObj.ContainsKey("pokemonClass"))
                        {
                            pokemonSettingsObj["pokemonClass"] = ConvertToPokemonClass(pokemonSettingsObj["pokemonClass"]);
                        }
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
                        if (pokemonSettingsObj.TryGetPropertyValue("evolutionBranch", out var evolutionBranch))
                        {
                            var evolutionBranchObj = evolutionBranch!.AsArray();
                            foreach (var evolution in evolutionBranchObj)
                            {
                                var evolutionObj = evolution!.AsObject();
                                if (evolutionObj.ContainsKey("lureItemRequirement"))
                                {
                                    var str2 = evolutionObj["lureItemRequirement"]!.GetValue<string>().ToLowerInvariant();
                                    str2 = str2.Replace("item_troy_disk_", string.Empty);
                                    evolutionObj["lureItemRequirement"] = str2;
                                }

                                foreach (var e in evolutionObj)
                                {
                                    if (allEvolutionRules.Add(e.Key))
                                    {
                                        Console.WriteLine($"Evolution keys: template = '{templateId}', key = '{e.Key}'");
                                    }
                                }
                            }
                        }

                        hasForm = pokemonSettingsObj.ContainsKey("form");
                    }

                    var added = false;
                    if (hasForm)
                    {
                        var form = PokemonAdded(pokemons, oCopy);
                        if (form.IsNone())
                        {

                        }
                        else
                        {
                            pokemonForms.Add(form);
                            added = true;
                        }
                    }

                    if (!added)
                    {
                        pokemons.Add(oCopy);
                    }
                }
                else if (moveRegex.IsMatch(templateId))
                {
                    if (obj["data"]!.AsObject().TryGetPropertyValue("moveSettings", out var moveSettings))
                    {
                        var moveSettingsObj = moveSettings!.AsObject();
                        moveSettingsObj["pokemonType"] = ConvertPokemonType(moveSettingsObj["pokemonType"]);
                    }
                    moves.Add(oCopy);
                }
                else if (templateId.Contains("_EVOLUTION_QUEST"))
                {
                    evolutionQuests.Add(oCopy);
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
            Save(evolutionQuests, "evolutionQuests.json");
            Save(pokemonFamilies, "pokemonFamilies.json");
            Save(pokemonExtended, "pokemonExtended.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
            var str = JsonSerializer.Serialize(pokemonForms, options);
            WriteToFile("pokemonForms.json", str);
        }

        private static SizeBounds GetSizeBounds(JsonObject sizeSettings)
        {
            var xxs = sizeSettings["xxsLowerBound"]!.GetValue<decimal>();
            var xs = sizeSettings["xsLowerBound"]!.GetValue<decimal>();
            var mL = sizeSettings["mLowerBound"]!.GetValue<decimal>();
            var mU = sizeSettings["mUpperBound"]!.GetValue<decimal>();
            var xl = sizeSettings["xlUpperBound"]!.GetValue<decimal>();
            var xxl = sizeSettings["xxlUpperBound"]!.GetValue<decimal>();

            return new SizeBounds
            {
                XXL = xxl,
                XL = xl,
                MLower = mL,
                MUpper = mU,
                XS = xs,
                XXS = xxs,
            };
        }

        private static SizeScaleMultiplier GetSizeScaleMultiplier(JsonObject sizeSettings)
        {
            var xxs = TryGetDecimal(sizeSettings, "xxsScaleMultiplier");
            var xs = TryGetDecimal(sizeSettings, "xsScaleMultiplier");
            var xl = TryGetDecimal(sizeSettings, "xlScaleMultiplier");
            var xxl = TryGetDecimal(sizeSettings, "xxlScaleMultiplier");

            if (xxs == null
                || xs == null
                || xl == null
                || xxl == null)
            {
                return SizeScaleMultiplier.None;
            }

            return new SizeScaleMultiplier
            {
                XXL = xxl.Value,
                XL = xl.Value,
                XS = xs.Value,
                XXS = xxs.Value,
            };
        }

        private static decimal? TryGetDecimal(JsonObject obj, string key)
        {
            if (obj.ContainsKey(key))
            {
                return obj[key]!.GetValue<decimal>();
            }

            return null;
        }

        private static PokemonForm PokemonAdded(JsonArray pokemons, JsonNode o)
        {
            var oCopy = o.DeepClone();
            var settingsObj = oCopy["data"]!["pokemonSettings"]!.AsObject();
            var nameO = settingsObj["pokemonId"]!.GetValue<string>();
            var name = nameO.Replace("_Female", "").Replace("_Male", "");
            var formO = settingsObj["form"]!.GetValue<string>();
            var form = formO.Replace(name.ToUpper(), "");

            if (formO.Contains("NIDORAN_NORMAL"))
            {

            }

            oCopy["templateId"] = oCopy["templateId"]!.GetValue<string>().Replace(form, "");
            oCopy["data"]!["templateId"] = oCopy["data"]!["templateId"]!.GetValue<string>().Replace(form, "");
            settingsObj.Remove("form");
            var canTemplEvolveO = settingsObj.ContainsKey("tempEvoOverrides");
            var templateId = oCopy["templateId"]!.GetValue<string>();
            var toHome = settingsObj["disableTransferToPokemonHome"]?.GetValue<bool>();
            if (toHome == true)
            {
                settingsObj.Remove("disableTransferToPokemonHome");
            }
            var canEvolveO = settingsObj["evolutionIds"] != null || settingsObj["evolutionBranch"] != null;

            var pokemonForm = new PokemonForm()
            {
                PokemonId = name,
                Form = form.Replace("_", " ").Trim(),
                DisableTransferToPokemonHome = toHome == true,
                CanEvolve = canEvolveO
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var oString = oCopy.ToJsonString(options);
            foreach (var p in pokemons)
            {
                if (p["templateId"]!.GetValue<string>() != templateId)
                {
                    continue;
                }

                // here's the original form

                var pCopy = p.DeepClone();
                var settingsObj2 = pCopy["data"]!["pokemonSettings"]!.AsObject();
                if (!canEvolveO)
                {
                    var canEvolve = settingsObj2["evolutionIds"] != null || settingsObj2["evolutionBranch"] != null;
                    if (canEvolve)
                    {
                        settingsObj2.Remove("evolutionIds");
                        settingsObj2.Remove("evolutionBranch");
                        settingsObj2.Remove("allowNoevolveEvolution");
                    }
                }

                if (!canTemplEvolveO)
                {
                    var canTempEvolve = settingsObj2.ContainsKey("tempEvoOverrides");
                    if (canTempEvolve)
                    {
                        pokemonForm.OriginalFormCanTempEvolveButThisCannot = true;
                        settingsObj2.Remove("tempEvoOverrides");
                    }
                }

                var original = pCopy.ToJsonString(options);
                if (original == oString)
                {
                    return pokemonForm;
                }
                else
                {

                }
            }

            return PokemonForm.None;
        }

        private static JsonNode? ConvertToTitleCase(JsonNode? jsonNode)
        {
            var str = jsonNode!.GetValue<string>().ToLowerInvariant();
            str = _textInfo.ToTitleCase(str);
            return str;
        }

        private static JsonNode? ConvertToPokemonClass(JsonNode? jsonNode)
        {
            var str = jsonNode!.GetValue<string>();
            str = str.Replace("POKEMON_CLASS_", "").ToLowerInvariant();
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
            WriteToFile(fileName, str);
        }

        private static void WriteToFile(string fileName, string str)
        {
            var outputFolder = Path.Combine(_outputFolder, fileName);
            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
            }
            File.WriteAllText(outputFolder, str);
        }
    }
}
