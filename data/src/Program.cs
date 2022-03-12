using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Text.RegularExpressions;

namespace CrawlBulbapedia
{
    public class Item
    {
        public string Name { get; set; }
        public IDictionary<Language, ForeignName> OtherNames { get; set; }
    }

    public class Pokemon
    {
        public string Name { get; set; }
        public IDictionary<Language, ForeignName> OtherNames { get; set; }
        public IList<HeldItemTable> HeldItems { get; set; } = new List<HeldItemTable>();
        public int No { get; set; }
        public AnnotatedText CatchRate { get; set; }
        public string Category { get; set; } // pokemon type
        public IDictionary<string, PokemonType[]> Types { get; set; }
        public IDictionary<string, Stats> Stats { get; set; } = new Dictionary<string, Stats>();
        public TypeEffectiveness TypeEffectiveness { get;set;}
    }

    public class TypeEffectiveness
    {
        public List<string> E1 { get; set; } // damaged normally by
        public List<string> E2 { get; set; } // weak to
        public List<string> E0 { get; set; } // immune to
        public List<string> E0_5 { get; set; } // resistant to
    }

    public class Stats
    {
        public PokemonStat BaseStats { get; set; }
        public PokemonStat MaxStatsAt50 { get; set; }
        public PokemonStat MinStatsAt50 { get; set; }
        public PokemonStat MaxStatsAt100 { get; set; }
        public PokemonStat MinStatsAt100 { get; set; }
    }

    public class PokemonStat
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }
    }

    public class ForeignName
    {
        public AnnotatedText Name { get; set; }
        public IList<AnnotatedText> OtherNames { get; set; } = new List<AnnotatedText>();
    }

    public class AnnotatedText
    {
        public string Text { get; set; }
        public string Pronunciation { get; set; }
        public string Explanation { get; set; }
        public string Meaning { get; set; }
    }

    public static class Program
    {
        public static StringEnumConverter SEConverter = new StringEnumConverter { CamelCaseText = true };
        public static void Main()
        {
            //convert Enums to Strings (instead of Integer) globally
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(SEConverter);
                return settings;
            });

            /*
         * get urls: https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9mon_by_National_Pok%C3%A9dex_number
         
         var a = [...document.querySelectorAll("table td:nth-child(4) a")]
        b = a.map(function(e){return e.href})
        b.join("\n")

        */
            var dataPath = "../../../..";
            //DownloadRawWebpages(dataPath, "pokemon");

            var processedPath = "../../../../processedHTML";
            //RemoveTag.Remove(processedPath, "pokemon");

            ExtractInfo(processedPath, dataPath, "pokemon");

            //DownloadRawWebpages(dataPath, "item");
            //RemoveTag.Remove(processedPath, "item");
            //ExtractItemInfo(processedPath, dataPath, "item");
        }

        private static void ExtractInfo(string sourcePath, string targetPath, string folder)
        {
            sourcePath = Path.Combine(sourcePath, folder);
            var files = FileHelper.GetAllFiles(sourcePath, "*.html").ToArray();
            var data = new List<Pokemon>();
            var itemLinks = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var pokemon = new Pokemon();
                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode;
                var heading = node.SelectSingleNode("//*[@id=\"firstHeading\"]");
                var name = heading.InnerText.Replace(" (Pokémon)", "");
                Console.Write(Environment.NewLine);
                Console.Write(name);
                pokemon.OtherNames = GetOtherNames(node);
                pokemon.Name = name;
                pokemon.HeldItems = GetHeldItemTables(node, itemLinks);
                var stats = GetBaseStats(node);
                foreach (var s in stats)
                {
                    pokemon.Stats[s.Item1] = new Stats()
                    {
                        BaseStats = s.Item2,
                        MaxStatsAt50 = s.Item3,
                        MinStatsAt50 = s.Item4,
                        MaxStatsAt100 = s.Item5,
                        MinStatsAt100 = s.Item6
                    };
                }
                pokemon.TypeEffectiveness = GetEffectiveness(node);

                GetPokeInfo(node, pokemon);

                data.Add(pokemon);
                continue;
                var evo = node.SelectSingleNode("#Evolution");
                if (evo != null)
                {
                    var evoTable = evo.NextSibling;
                    if (evoTable == null)
                    {

                    }
                }
                else
                {

                }
            }
            var outLinks = Path.Combine(targetPath, "item_urls.txt");
            File.WriteAllText(outLinks, string.Join("\n", itemLinks.Values.OrderBy(e => e)));
            var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var output = Path.Combine(targetPath, "data.json");
            File.WriteAllText(output, json);
            var jsonTE = JsonConvert.SerializeObject(TypeEffectivenessNotes, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(Path.Combine(targetPath, "type-effective-notes.json"), jsonTE);
        }

        private static ISet<string> TypeEffectivenessNotes = new HashSet<string>();

        private static TypeEffectiveness GetEffectiveness(HtmlNode node)
        {
            var cc = node.SelectSingleNode("//div[@id=\"mw-content-text\"]//*[@id=\"Type_effectiveness\"]//parent::h3");
            if (cc == null)
            {
                throw new Exception();
            }
            var nt = GetNextTable(cc, false, false);
            var ths = nt.GetNearestNodes("tr");
            var ef = new TypeEffectiveness();
            foreach(var th in ths)
            {
                var inT = th.InnerText.Trim();
                if(inT.Contains("Under normal battle conditions in Generation VIII"))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(inT))
                {
                    continue;
                }
                var tds = th.RecursiveGetChildNode("tr");
                var tds2 = tds.GetNearestNodes("span");
                var tds3 = RemoveHiddenNodes(tds2);
                var keyw = inT.Replace(" ", "");
                if(keyw.Contains("Damagednormallyby"))
                {
                    ef.E1 = GetEffectivenessTypes(tds3);
                }
                else if (keyw.Contains("Weakto"))
                {
                    ef.E2 = GetEffectivenessTypes(tds3);
                }
                else if (keyw.Contains("Immuneto"))
                {
                    ef.E0 = GetEffectivenessTypes(tds3);
                }
                else if (keyw.Contains("Resistantto"))
                {
                    ef.E0_5 = GetEffectivenessTypes(tds3);
                }
                else
                {
                    var tr2 = th.GetNearestNodes("tr");
                    foreach (var tt in tr2)
                    {
                        if (tt.InnerText.Contains("Notes:"))
                        {
                            continue;
                        }
                        else
                        {
                            TypeEffectivenessNotes.Add(tt.InnerHtml.Replace("\n", ""));
                        }
                    }
                }
            }
            return ef;
        }

        private static List<string> GetEffectivenessTypes(HtmlNode[] sp)
        {
            var types = new List<string>();
            foreach(var s in sp)
            {
                var s2 = newLines.Split(s.InnerText.Trim());
                if (s2.Length == 1)
                {
                    types.Add(s2[0]);
                }
                else
                {
                    types.Add(s2[0] + ":" + s2[1]);
                }
            }

            return types;
        }

        private static Regex newLines = new Regex(@"\n+", RegexOptions.Compiled);

        private static List<(string, PokemonStat, PokemonStat, PokemonStat, PokemonStat, PokemonStat)> GetBaseStats(HtmlNode node)
        {
            var cc = node.SelectSingleNode("//div[@id=\"mw-content-text\"]//*[@id=\"Base_stats\"]//parent::h4");
            if (cc == null)
            {
                //throw new Exception();
                //TODO fix this;
                return new List<(string, PokemonStat, PokemonStat, PokemonStat, PokemonStat, PokemonStat)>();
            }
            var l = new List<(string, PokemonStat, PokemonStat, PokemonStat, PokemonStat, PokemonStat)>();
            var nt = cc;
            while (true)
            {
                nt = GetNextTable(nt, out var soStr, "h5");
                if (string.IsNullOrEmpty(soStr))
                {
                    soStr = "";
                }
                if (nt == null)
                {
                    break;
                }
                var (baseStat, maxAt50, minAt50, maxAt100, minAt100) = GetStat(nt);
                l.Add((soStr, baseStat, maxAt50, minAt50, maxAt100, minAt100));
            }
            return l;
        }

        private static (PokemonStat, PokemonStat, PokemonStat, PokemonStat, PokemonStat) GetStat(HtmlNode? nt)
        {
            var baseStat = new PokemonStat();
            var maxAt50 = new PokemonStat();
            var maxAt100 = new PokemonStat();
            var minAt50 = new PokemonStat();
            var minAt100 = new PokemonStat();
            var ths = nt.GetNearestNodes("tr");
            foreach (var th in ths)
            {
                var t = th.InnerText.Trim();
                if (t.Contains("Stat"))
                {
                    continue;
                }
                if (t.Contains("At Lv. 50"))
                {
                    continue;
                }
                if (t.StartsWith("Tota"))
                {
                    continue;
                }
                if (t.Contains("Minimum stats are"))
                {
                    continue;
                }
                var sp = newLines.Split(t);
                if (sp.Length != 3)
                {
                    continue;
                }
                if (sp[0].StartsWith("HP"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.HP = v1;
                    minAt50.HP = v2;
                    maxAt50.HP = v3;
                    minAt100.HP = v4;
                    maxAt100.HP = v5;
                }
                else if (sp[0].StartsWith("Attack"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.Attack = v1;
                    minAt50.Attack = v2;
                    maxAt50.Attack = v3;
                    minAt100.Attack = v4;
                    maxAt100.Attack = v5;
                }
                else if (sp[0].StartsWith("Defense"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.Defense = v1;
                    minAt50.Defense = v2;
                    maxAt50.Defense = v3;
                    minAt100.Defense = v4;
                    maxAt100.Defense = v5;
                }
                else if (sp[0].StartsWith("Sp. Atk"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.SpecialAttack = v1;
                    minAt50.SpecialAttack = v2;
                    maxAt50.SpecialAttack = v3;
                    minAt100.SpecialAttack = v4;
                    maxAt100.SpecialAttack = v5;
                }
                else if (sp[0].StartsWith("Sp. Def"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.SpecialDefense = v1;
                    minAt50.SpecialDefense = v2;
                    maxAt50.SpecialDefense = v3;
                    minAt100.SpecialDefense = v4;
                    maxAt100.SpecialDefense = v5;
                }
                else if (sp[0].StartsWith("Speed"))
                {
                    var (v1, v2, v3, v4, v5) = GetStatNumbers(sp);
                    baseStat.Speed = v1;
                    minAt50.Speed = v2;
                    maxAt50.Speed = v3;
                    minAt100.Speed = v4;
                    maxAt100.Speed = v5;
                }
                else
                {
                    throw new Exception();
                }
            }
            return (baseStat, maxAt50, minAt50, maxAt100, minAt100);
        }

        private static (int, int, int, int, int) GetStatNumbers(string[] sp)
        {
            var hp = sp[0].Split(":")[1];
            var hpv = int.Parse(hp);
            var h2 = sp[1].Split("-");
            var h3 = sp[2].Split("-");
            var minv = h2[0];
            var mv = int.Parse(minv);
            var maxv = h2[1];
            var mv2 = int.Parse(maxv);
            var minv2 = h3[0];
            var mv3 = int.Parse(minv2);
            var maxv2 = h3[1];
            var mv4 = int.Parse(maxv2);
            return (hpv, mv, mv2, mv3, mv4);
        }

        static public string StripControlChars(this string s)
        {
            return Regex.Replace(s, @"[^\x20-\x7F]", "");
        }

        private static IDictionary<int, string> _uniqueIdCheck = new Dictionary<int, string>();

        private static void GetPokeInfo(HtmlNode node, Pokemon pokemon)
        {
            var cc = node.SelectSingleNode("//div[@id=\"mw-content-text\"]/div/table[2]");

            var tds = cc.GetNearestNodes("td", "table");
            foreach (var td in tds)
            {
                var tdStyle = td.GetAttributeValue("style", "").Replace(" ", "");
                if (tdStyle.Contains("display:none"))
                {
                    continue;
                }

                if (GetBasicInfo(td, pokemon))
                {
                    continue;
                }


                td.GetChildNodeContains("a", "td", out var typeDiv, "Type");
                if (typeDiv != null)
                {
                    var tds2 = typeDiv.GetNearestNodes("td", "a");
                    var f = RemoveHiddenNodes(tds2);
                    var types = new Dictionary<string, PokemonType[]>();
                    if (f.Length > 1)
                    {

                    }
                    foreach (var f5 in f)
                    {
                        var t = new PokemonType();
                        var ts = f5.GetNearestNodes("small", "table");
                        var name = ts.FirstOrDefault()?.InnerText;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = "";
                        }
                        var ft = f5.GetNearestNodes("td", "a");
                        var f3 = RemoveHiddenNodes(ft);
                        var typesStr = string.Join("\n", f3.Select(e => e.InnerText));
                        var types2 = typesStr.Split("\n").Where(e => e.Trim().Length > 0).ToList();
                        if (types2.Contains("Unknown"))
                        {
                            throw new Exception();
                        }
                        var t2 = types2.Select(e => e.ToEnum<PokemonType>()).ToArray();
                        types.Add(name, t2);
                    }
                    pokemon.Types = types;
                    if (!types.Any())
                    {
                        throw new Exception();
                    }
                    continue;
                }

                td.GetChildNodeContains("a", "td", out var crDiv, "Catch rate");
                if (crDiv != null)
                {
                    var tds2 = crDiv.GetNearestNodes("td", "a");
                    var f = RemoveHiddenNodes(tds2);
                    if (f.Length > 1)
                    {
                        throw new Exception();
                    }
                    var f1 = f.Single();
                    var typesStr = f1.InnerText.Trim();
                    var cr2 = new AnnotatedText();
                    cr2.Text = typesStr;
                    var catchRateExp = f1.RecursiveGetChildNode("span");
                    if (catchRateExp != null)
                    {
                        cr2.Explanation = catchRateExp.GetAttributeValue("title", null);
                    }
                    pokemon.CatchRate = cr2;
                    continue;
                }
            }
        }

        private static bool GetBasicInfo(HtmlNode td, Pokemon pokemon)
        {
            var result = false;
            var category = td.GetNearestNodesByTitle("a", "Pokémon category");
            if (category != null)
            {
                var ctr = category.InnerText.Trim();
                pokemon.Category = ctr;
                result = true;
            }

            var no = td.GetNearestNodesByTitle("a", "List of Pokémon by National Pokédex number");
            if (no != null)
            {
                var ctr = no.InnerText.Trim();
                ctr = ctr.Replace("#", "");
                if (int.TryParse(ctr, out var num))
                {
                    if (_uniqueIdCheck.ContainsKey(num))
                    {
                        throw new Exception();
                    }
                    _uniqueIdCheck.Add(num, "");
                    pokemon.No = num;
                    result = true;
                }
                else
                {
                    throw new Exception();
                }
            }
            return result;
        }

        private static HtmlNode[] RemoveHiddenNodes(HtmlNode[] nodes)
        {
            var r = new List<HtmlNode>();
            foreach (var td in nodes)
            {
                var tdStyle = td.GetAttributeValue("style", "").Replace(" ", "");
                if (!tdStyle.Contains("display:none"))
                {
                    r.Add(td);
                }
            }
            return r.ToArray();
        }

        private static bool GetChildNodeContains(this HtmlNode node, string childTag, string returnTag, out HtmlNode? found, params string[] title)
        {
            found = null;
            if (node == null)
            {
                return false;
            }
            foreach (var c in node.ChildNodes)
            {
                var t = c.GetAttributeValue("title", "");
                if (c.Name == childTag && title.Contains(t))
                {
                    found = c;
                    if (found?.Name != returnTag)
                    {
                        found = node;
                    }
                    return true;
                }
                else
                {
                    var result = c.GetChildNodeContains(childTag, returnTag, out found, title);
                    if (result)
                    {
                        if (found?.Name != returnTag)
                        {
                            found = c;
                        }
                        if (found?.Name != returnTag)
                        {
                            found = node;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static HtmlNode? GetNearestNodesByTitle(this HtmlNode node, string childTag, params string[] title)
        {
            if (node == null)
            {
                return null;
            }
            foreach (var c in node.ChildNodes)
            {
                var t = c.GetAttributeValue("title", "");
                if (c.Name == childTag && title.Contains(t))
                {
                    return c;
                }
                else
                {
                    var result = c.GetNearestNodesByTitle(childTag, title);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private static void ExtractItemInfo(string sourcePath, string targetPath, string folder)
        {
            sourcePath = Path.Combine(sourcePath, folder);
            var files = FileHelper.GetAllFiles(sourcePath, "*.html").ToArray();
            var data = new List<Item>();
            foreach (var file in files)
            {
                var pokemon = new Item();
                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode;
                var heading = node.SelectSingleNode("//*[@id=\"firstHeading\"]");
                var name = heading.InnerText.Replace(" (item)", "", StringComparison.InvariantCultureIgnoreCase);
                Console.Write(Environment.NewLine);
                Console.Write(name);
                pokemon.OtherNames = GetOtherNames(node);
                pokemon.Name = name;


                data.Add(pokemon);
                continue;
                var evo = node.SelectSingleNode("#Evolution");
                if (evo != null)
                {
                    var evoTable = evo.NextSibling;
                    if (evoTable == null)
                    {

                    }
                }
                else
                {

                }
            }
            var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var output = Path.Combine(targetPath, "items.json");
            File.WriteAllText(output, json);
        }

        private static IList<HeldItemTable> GetHeldItemTables(HtmlNode node, IDictionary<string, string> itemLinks)
        {
            var heldItems = node.SelectSingleNode("//*[@id=\"Held_items\"]");
            if (heldItems != null)
            {
                var heldItemsTable = GetNextTable(heldItems);
                if (heldItemsTable == null)
                {
                    Console.Write(": No Held Item");
                    return null;
                }
                var itemTables = new List<HeldItemTable>();
                var rt = heldItemsTable.GetRegularisedTable(out var indices);
                foreach (var r in rt)
                {
                    var template = new HeldItemTable();
                    foreach (var i in indices)
                    {
                        if (i.Key.Contains("Game", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var id in i.Value)
                            {
                                var a = r[id].GetChildNode("a");
                                var g = a?.InnerText.Trim();
                                if (g == null)
                                {
                                    throw new Exception();
                                }
                                var g2 = g.ToEnum<GameVersion>();
                                template.Games.Add(g2);

                                var exp = r[id].GetChildNode("span", "explain");
                                if (exp != null)
                                {
                                    var v = exp.GetAttributeValue("title", "");
                                    if (!template.Notes.TryAdd(g2, new List<string>() { v }))
                                    {
                                        if (!template.Notes[g2].Contains(v))
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                            }
                        }
                        else if (i.Key.Contains("Held Item", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var id in i.Value)
                            {
                                var (name, prob, image, note, itemLink) = GetItemInfo(r[id]);
                                var newt = new HeldItemTable()
                                {
                                    Name = name,
                                    Probability = prob
                                };
                                var oldT = itemTables.FirstOrDefault(e => e.Equals(newt));
                                var found = true;
                                if (oldT == null)
                                {
                                    found = false;
                                    oldT = newt;
                                }
                                foreach (var item in template.Games)
                                {
                                    oldT.Games.Add(item);
                                }
                                foreach (var n in template.Notes)
                                {
                                    if (!oldT.Notes.TryAdd(n.Key, n.Value) && !oldT.Notes[n.Key].SequenceEqual(n.Value))
                                    {
                                        throw new Exception();
                                    }
                                }
                                if (!string.IsNullOrEmpty(note))
                                {
                                    //oldT.ItemNotes.Add(note);
                                    foreach (var g in oldT.Games)
                                    {
                                        if (oldT.Notes.ContainsKey(g))
                                        {
                                            if (oldT.Notes[g]?.Any() == true)
                                            {
                                                if (oldT.Notes[g].Contains(note))
                                                {
                                                    //throw new Exception();
                                                }
                                                else
                                                {
                                                    oldT.Notes[g].Add(note);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            oldT.Notes.Add(g, new List<string>() { note });
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(oldT.Image) && oldT.Image != image)
                                {
                                    throw new Exception();
                                }
                                oldT.Image = image;

                                if (!found)
                                {
                                    itemTables.Add(oldT);
                                }

                                AddItemLink(itemLinks, itemLink, name);
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

                foreach (var i in itemTables)
                {
                    if (!i.Games.Any())
                    {
                        throw new Exception();
                    }
                    if (!i.Notes.Any())
                    {
                        i.Notes = null;
                    }
                    if (!i.ItemNotes.Any())
                    {
                        i.ItemNotes = null;
                    }
                }

                return itemTables;
            }
            else
            {
                Console.Write(": No Held Item");
                return null;
            }
        }

        private static (string, string, string, string, string) GetItemInfo(HtmlNode r)
        {
            var image = r.GetChildNode("a", "image")?.GetChildNode("img").GetAttributeValue("src", "");

            var a = r.GetChildNode2("a", "image");
            var prob = GetItemProbText(r);
            var itemLink = a.GetAttributeValue("href", "");
            var name = a?.InnerText.Trim();
            if (name == null)
            {
                throw new Exception();
            }
            name = CorrectName(name);

            //var prob = r.GetDirectInnerText().Trim();

            var exp = r.GetChildNode("span", "explain");
            var itemNote = exp?.GetAttributeValue("title", "");
            return (name, prob, image, itemNote, itemLink);
        }

        private static string GetItemProbText(HtmlNode r)
        {
            var text = "";
            foreach (var c in r.ChildNodes)
            {
                if (c.HasClass("image"))
                {
                    continue;
                }
                var title = c.GetAttributeValue("title", "");
                var inner = c.InnerHtml.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    title = title.Replace("&#39;", "'");
                    var src = c.GetAttributeValue("href", "");
                    if (!src.Contains("(Pok%C3%A9walker)"))
                    {
                        title = title.StripControlChars();
                        inner = inner.StripControlChars();
                        if (title == inner || title.Contains(inner))
                        {
                            continue;
                        }
                        var inner2 = inner.Replace(" ", "");
                        var src2 = src.Replace("_", "");
                        if (!src.StartsWith("/wiki/Pok"))
                        {
                            if (src.Contains(inner) || src2.Contains(inner2))
                            {
                                continue;
                            }
                        }
                    }
                }
                if (c.HasClass("explain"))
                {
                    continue;
                }
                text += c.InnerText;
            }
            return text.Trim();
        }

        private static ISet<string> KK = new HashSet<string>();

        private static string CorrectName(string name)
        {
            var d = new Dictionary<string, string>()
            {
                { "NeverMeltIce", "Never-Melt Ice" },
                { "TwistedSpoon", "Twisted Spoon" },
                { "TinyMushroom", "Tiny Mushroom" },
                { "BalmMushroom", "Balm Mushroom" },
                { "BrightPowder", "Bright Powder" },
                { "DeepSeaTooth", "Deep Sea Tooth" },
                { "DeepSeaScale", "Deep Sea Scale" },
                { "SilverPowder", "Silver Powder" },
                { "BlackGlasses", "Black Glasses" },
                { "MysteryBerry", "Mystery Berry" }
            };
            if (d.ContainsKey(name))
            {
                return d[name];
            }
            else
            {
                name = name.StripControlChars();
                if (KK.Add(name))
                {

                }
            }
            return name;
        }

        private static void AddItemLink(IDictionary<string, string> itemLinks, string itemLink, string? itemName)
        {
            if (!itemLink.Contains("(Pok%C3%A9walker)"))
            {
                if (itemLink.Contains("Black_Belt"))
                {
                    itemLink = "/wiki/Black_Belt_(item)";
                }
                else if (itemLink.Contains("wiki/Metronome"))
                {
                    itemLink = "/wiki/Metronome_(item)";
                }

                if (itemLinks.TryGetValue(itemName, out var link))
                {
                    if (link != itemLink)
                    {
                        if (link.StartsWith("/wiki/NeverMeltIce")
                            || link.StartsWith("/wiki/TwistedSpoon")
                            || link.StartsWith("/wiki/TinyMushroom")
                            || link.StartsWith("/wiki/BalmMushroom")
                            || link.StartsWith("/wiki/BrightPowder")
                            || link.StartsWith("/wiki/DeepSeaTooth")
                            || link.StartsWith("/wiki/DeepSeaScale")
                            || link.StartsWith("/wiki/SilverPowder")
                            || link.StartsWith("/wiki/BlackGlasses"))
                        {
                            itemLinks[itemName] = itemLink;
                        }
                        else if (!itemLink.StartsWith("/wiki/Berry#")
                            && !itemLink.StartsWith("/wiki/Gold_Bottle_Cap#")
                            && !itemLink.StartsWith("/wiki/Gem#")
                            && !itemLink.StartsWith("/wiki/Potion#")
                            && !link.StartsWith("/wiki/Valuable_item#Pearl")
                            && !itemLink.StartsWith("/wiki/Type-enhancing_item#")
                            && !itemLink.StartsWith("/wiki/NeverMeltIce")
                            && !itemLink.StartsWith("/wiki/TwistedSpoon")
                            && !itemLink.StartsWith("/wiki/TinyMushroom")
                            && !itemLink.StartsWith("/wiki/BalmMushroom")
                            && !itemLink.StartsWith("/wiki/BrightPowder")
                            && !itemLink.StartsWith("/wiki/DeepSeaTooth")
                            && !itemLink.StartsWith("/wiki/DeepSeaScale")
                            && !itemLink.StartsWith("/wiki/SilverPowder")
                            && !itemLink.StartsWith("/wiki/BlackGlasses"))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
                else
                {
                    itemLinks[itemName] = itemLink;
                }
            }
        }

        // get translated names
        private static IDictionary<Language, ForeignName> GetOtherNames(HtmlNode node)
        {
            var dict = new Dictionary<Language, ForeignName>();
            var h2 = node.SelectSingleNode("//*[@id=\"In_other_languages\"]");
            if (h2 == null)
            {
                h2 = node.SelectSingleNode("//*[@id=\"Names\"]");
            }
            if (h2 == null)
            {
                Console.Write(": no translation");
                return dict;
            }
            var table = GetNextTable(h2);
            if (table == null)
            {
                Console.Write(": no translation");
            }
            var trs = table.ChildNodes["tbody"].GetChildNodes("tr");
            trs = RegulariseTable(trs);
            var trTitle = trs[0];
            var tdTitle = trTitle.GetChildNodes("th");
            int lanIndex = -1;
            int titleIndex = -1;
            int meaningIndex = -1;
            var count = 0;
            foreach (var t in tdTitle)
            {
                var text = t.InnerText.Trim();
                var colSpan = t.GetAttributeValue("colspan", 1);
                if (text == "Language")
                {
                    lanIndex = count;
                }
                else if (text == "Title" || text == "Name")
                {
                    titleIndex = count;
                }
                else if (text == "Meaning" || text == "Origin")
                {
                    meaningIndex = count;
                }
                count += colSpan;
            }
            foreach (var tr in trs.Skip(1))
            {
                var tds = tr.GetChildNodes("td");
                if (tds.Length > 1)
                {
                    var names = new ForeignName();
                    string meaning = null;
                    if (meaningIndex >= 0)
                    {
                        meaning = tds[meaningIndex].InnerHtml.Trim();
                    }
                    var lan = tds[lanIndex].InnerText.Trim();
                    var lanSpan = tds[lanIndex].GetAttributeValue("colspan", 1);
                    if (titleIndex == 2 && lanIndex == 0 && lanSpan == 1)
                    {
                        lan = lan + " " + tds[lanIndex + 1].InnerText.Trim();
                    }
                    var curTitleIndex = titleIndex - lanSpan + 1;
                    var key = ConvertToLan(lan);
                    var name = tds[curTitleIndex].InnerHtml.Trim();
                    if (name.Contains("<i>"))
                    {
                        var lines = name.Split("<br>");
                        var firstLine = lines[0].Trim();
                        var (name1, pinyin1, exp) = GetName(firstLine);
                        var n1 = new AnnotatedText()
                        {
                            Text = name1,
                            Pronunciation = pinyin1,
                            Explanation = exp,
                            Meaning = meaning
                        };
                        names.Name = n1;
                        foreach (var line in lines.Skip(1))
                        {
                            var (name2, pinyin2, exp2) = GetName(line.Trim());
                            var n2 = new AnnotatedText()
                            {
                                Text = name2,
                                Pronunciation = pinyin2,
                                Explanation = exp2
                            };
                            names.OtherNames.Add(n2);
                        }
                    }
                    else
                    {
                        names.Name = new AnnotatedText()
                        {
                            Text = name,
                            Meaning = meaning
                        };
                    }
                    if (!names.OtherNames.Any())
                    {
                        names.OtherNames = null;
                    }
                    if (dict.ContainsKey(key))
                    {
                        if (dict[key].OtherNames == null)
                        {
                            dict[key].OtherNames = new List<AnnotatedText>()
                            {
                                names.Name
                            };
                        }
                        else
                        {
                            dict[key].OtherNames.Add(names.Name);
                        }
                    }
                    else
                    {
                        dict.Add(key, names);
                    }
                }
                else
                {
                    //throw new Exception();
                }
            }
            return dict;
        }

        private static HtmlNode[] RegulariseTable(HtmlNode[] trs)
        {
            for (int i = 1; i < trs.Length; i++)
            {
                var tr = trs[i];
                var tds = tr.GetChildNodes("td");
                for (int j = 0; j < tds.Length; j++)
                {
                    var rowspan = tds[j].GetAttributeValue("rowspan", 1);
                    if (rowspan > 1)
                    {
                        var tdsC = tds[j];
                        tdsC.SetAttributeValue("rowspan", "1");
                        for (var k = 1; k < rowspan; k++)
                        {
                            if (i + k >= trs.Length)
                            {
                                continue;
                            }
                            trs[i + k].ChildNodes.Insert(j, tds[j]);
                        }
                    }
                }
            }
            return trs;
        }

        private static Language ConvertToLan(string lan)
        {
            var ll = lan.ToLowerInvariant();
            if (ll == "brazil portuguese")
            {
                ll = "Brazilian Portuguese";
            }
            else if (ll == "portuguese brazil")
            {
                ll = "Brazilian Portuguese";
            }
            else if (ll == "portuguese portugal")
            {
                ll = "Portugal Portuguese";
            }
            else if (ll.Contains("mandarin"))
            {
                ll = "mandarin chinese";
            }
            else if (ll.Contains("cantonese"))
            {
                ll = "cantonese chinese";
            }
            else if (ll == "spanish latin america")
            {
                ll = "Latin America Spanish";
            }
            else if (ll == "spanish spain")
            {
                ll = "Spain Spanish";
            }
            else if (ll == "french europe")
            {
                ll = "french";
            }
            else if (ll == "portuguese" || ll == "european portuguese")
            {
                ll = "Portugal Portuguese";
            }
            return ll.ToEnum<Language>();
        }

        private static (string, string, string) GetName(string text)
        {
            var pinyinNode = new HtmlDocument();
            pinyinNode.LoadHtml(text);
            var p = pinyinNode.DocumentNode;
            var p2 = p.RecursiveGetChildNode("i")?.InnerText.Trim();
            var expSpan = p.RecursiveGetChildNode("span");
            var exp = default(string);
            if (expSpan != null)
            {
                exp = expSpan.GetAttributeValue("title", null);
                var inner = expSpan.InnerText.Trim();
                if (exp == "*" || string.IsNullOrEmpty(inner))
                {
                    exp = null;
                }
            }
            var p3 = p.GetDirectInnerText().Trim();
            return (p3, p2, exp);
        }

        private static HtmlNode? GetNextTable(HtmlNode heldItems, out string stepOverTagstr, string stepOverTag = "")
        {
            stepOverTagstr = null;
            var h3 = heldItems.NextSibling;
            while (h3.Name == "#text" || h3.Name=="p" || h3.Name == stepOverTag)
            {
                h3 = h3.NextSibling;
                if (h3.Name == stepOverTag)
                {
                    stepOverTagstr = h3.InnerText.Trim();
                }
            }
            if (h3.Name == "table")
            {
                var t2 = h3.ChildNodes["tbody"];
                var t3 = t2?.ChildNodes["tr"];
                var t4 = t3?.ChildNodes["td"];
                var t5 = t4?.ChildNodes["table"];
                if (t5 != null)
                {
                    return t5;
                }
                return h3;
            }
            return null;
        }

        private static HtmlNode? GetNextTable(HtmlNode heldItems, bool gotoParent = true, bool getInnerTable = true)
        {
            var heldItemsTable2 = heldItems;
            if (gotoParent)
            {
                heldItemsTable2 = heldItems.ParentNode;
            }
            if (heldItemsTable2.Name == "h3" || heldItemsTable2.Name == "h2" || heldItemsTable2.Name == "h4")
            {
                var h3 = heldItemsTable2.NextSibling;
                while (h3.Name == "#text")
                {
                    h3 = h3.NextSibling;
                }
                if (h3.Name == "table")
                {
                    if (getInnerTable)
                    {
                        var t2 = h3.ChildNodes["tbody"];
                        var t3 = t2?.ChildNodes["tr"];
                        var t4 = t3?.ChildNodes["td"];
                        var t5 = t4?.ChildNodes["table"];
                        if (t5 != null)
                        {
                            return t5;
                        }
                    }
                    return h3;
                }
            }
            return null;
        }

        private static void DownloadRawWebpages(string dataPath, string folder)
        {
            var path = Path.Combine(dataPath, folder + "_urls.txt");
            var file = File.ReadAllLines(path);
            foreach (var f in file)
            {
                if (string.IsNullOrEmpty(f))
                {
                    continue;
                }
                var f2 = f;
                if (f2.StartsWith("/wiki"))
                {
                    f2 = "https://bulbapedia.bulbagarden.net" + f;
                }

                DownloadWebpage(f2, folder);
            }
        }

        public static void DownloadWebpage(string url, string folder)
        {
            var fileName = url.Split('/').Last();
            var fullPath = folder + "/" + fileName + ".html";
            if (File.Exists(fullPath))
            {
                return;
            }
            WebRequest req = HttpWebRequest.Create(url);

            req.Method = "GET";

            string source;
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }

            File.WriteAllText(fullPath, source);
        }
    }
}