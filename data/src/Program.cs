using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;

namespace CrawlBulbapedia
{
    public class Item
    {
        public string Name { get; set; }
        public IDictionary<Language, ForeignName> OtherNames { get; set; }
    }

    public class HeldItemTable
    {
        public List<string> Name { get; set; } = new List<string>();
        public List<string> Probability { get; set; } = new List<string>();
        public List<string> Gens { get; set; } = new List<string>();
        public List<string> Notes { get; set; } = new List<string>();
        public List<string> Images { get; set; } = new List<string>();
    }

    public class HeldItem
    {
        public string Name { get; set; }
        public string Probability { get; set; }
        public string Gen { get; set; }
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
            //RemoveRedundantTags(processedPath, "pokemon");

            ExtractInfo(processedPath, dataPath, "pokemon");

            //DownloadRawWebpages(dataPath, "item");
            //RemoveRedundantTags(processedPath, "item");
            //ExtractItemInfo(processedPath, dataPath, "item");
        }

        private static void RemoveRedundantTags(string targetPath, string folder)
        {
            var path = Directory.GetCurrentDirectory() + "\\" + folder;
            var p2 = Path.Combine(targetPath, folder);
            if (!Directory.Exists(p2))
            {
                Directory.CreateDirectory(p2);
            }
            var files = GetAllFiles(path, "*.html").ToArray();
            var data = new List<Pokemon>();
            var itemLinks = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var pokemon = new Pokemon();
                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode;
                var html = node.GetChildNode("html");
                if (html == null)
                {
                    throw new Exception();
                }

                var body = html.GetChildNode("body");
                if (body == null)
                {
                    throw new Exception();
                }
                var div = body.GetChildNodeById("div", "globalWrapper");
                if (div == null)
                {
                    throw new Exception();
                }
                var content = div.GetChildNodeById("div", "column-content");
                if (content == null)
                {
                    throw new Exception();
                }
                var content2 = content.GetChildNodeById("div", "content");
                if (content2 == null)
                {
                    throw new Exception();
                }
                var lastTwo = content2.ChildNodes.Where(e => e.Name != "h1" && e.Id != "bodyContent").ToArray();
                foreach (var l in lastTwo)
                {
                    content2.RemoveChild(l);
                }

                RemoveTableByCondition(content2, "tbody", "h1", "h2", "h3", "a", "span");

                var p = Path.Combine(p2, file.Split("\\").Last());
                var str = content2.OuterHtml;
                File.WriteAllText(p, str);
                var fi = new FileInfo(file).Length;
                var f2 = new FileInfo(p).Length;
                Console.WriteLine($"{fi} -> {f2} ({Math.Round((decimal)f2 / fi, 3)})");
            }
        }

        private static void RemoveTableByCondition(HtmlNode content2, params string[] stopAt)
        {
            var cs = content2.ChildNodes.ToArray();
            foreach (var c in cs)
            {
                if (c.Name == "table")
                {
                    var inn = c.InnerText;
                    if (inn.Contains("This section is incomplete.")
                        || inn.Contains("This template is incomplete."))
                    {
                        content2.RemoveChild(c);
                    }
                }
                else if (stopAt.Contains(c.Name))
                {
                    continue;
                }
                else
                {
                    RemoveTableByCondition(c, stopAt);
                }
            }
        }

        private static void ExtractInfo(string sourcePath, string targetPath, string folder)
        {
            sourcePath = Path.Combine(sourcePath, folder);
            var files = GetAllFiles(sourcePath, "*.html").ToArray();
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
        }

        private static IDictionary<int, string> _uniqueIdCheck = new Dictionary<int, string>();

        private static void GetPokeInfo(HtmlNode node, Pokemon pokemon)
        {
            var cc = node.SelectSingleNode("//div[@id=\"mw-content-text\"]/div/table[2]");
            var crw = cc.InnerText;

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

        private static HtmlNode[] GetNearestNodes(this HtmlNode node, string childTag, params string[] stopAt)
        {
            if (node == null)
            {
                return Array.Empty<HtmlNode>();
            }
            var found = new List<HtmlNode>();
            foreach (var c in node.ChildNodes)
            {
                if (stopAt.Contains(c.Name))
                {
                    continue;
                }
                if (c.Name == childTag)
                {
                    found.Add(c);
                }
                else
                {
                    var result = GetNearestNodes(c, childTag);
                    found.AddRange(result);
                }
            }
            return found.ToArray();
        }

        private static void ExtractItemInfo(string sourcePath, string targetPath, string folder)
        {
            sourcePath = Path.Combine(sourcePath, folder);
            var files = GetAllFiles(sourcePath, "*.html").ToArray();
            var data = new List<Item>();
            foreach (var file in files)
            {
                var pokemon = new Item();
                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode;
                var heading = node.SelectSingleNode("//*[@id=\"firstHeading\"]");
                var name = heading.InnerText.Replace(" (Item)", "");
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
                var trs = heldItemsTable.ChildNodes["tbody"].GetChildNodes("tr");
                var itemTables = new List<HeldItemTable>();
                foreach (var tr in trs.Skip(1))
                {
                    var ths = tr.GetChildNodes("th");
                    var hiTable = new HeldItemTable();
                    foreach (var th in ths)
                    {
                        var text = th.GetChildNode("a").InnerText;
                        var note = th.GetChildNode("span")?.GetAttributeValue("title", "");
                        hiTable.Gens.Add(text);
                        hiTable.Notes.Add(note);
                    }
                    var tds = tr.GetChildNodes("td");
                    if (tds == null || tds.Length == 0)
                    {
                        var last = itemTables.Last();
                        last.Gens.AddRange(hiTable.Gens);
                        last.Notes.AddRange(hiTable.Notes);
                    }
                    else
                    {
                        foreach (var td in tds)
                        {
                            var innerA = td.GetChildNode2("a", "image"); // not image
                            var itemName = innerA?.InnerText.Trim();
                            if (string.IsNullOrEmpty(itemName))
                            {
                                itemName = td.InnerText.Trim();
                            }
                            else
                            {
                                var itemLink = innerA.GetAttributeValue("href", "");
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
                                            if (!itemLink.StartsWith("/wiki/Berry#")
                                                && !itemLink.StartsWith("/wiki/Gold_Bottle_Cap#")
                                                && !itemLink.StartsWith("/wiki/Gem#")
                                                && !itemLink.StartsWith("/wiki/Potion#")
                                                && !link.StartsWith("/wiki/Valuable_item#Pearl")
                                                && !itemLink.StartsWith("/wiki/Type-enhancing_item#"))
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
                            var prob = td.GetDirectInnerText().Trim();
                            hiTable.Name.Add(itemName);
                            hiTable.Probability.Add(prob);


                            var image = td.GetChildNode("a", "image")?.GetChildNode("img").GetAttributeValue("src", "");
                            hiTable.Images.Add(image);
                        }
                        itemTables.Add(hiTable);
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
            var exp = p.RecursiveGetChildNode("span")?.GetAttributeValue("title", null);
            var p3 = p.GetDirectInnerText().Trim();
            return (p3, p2, exp);
        }

        private static HtmlNode? GetNextTable(HtmlNode heldItems)
        {
            var heldItemsTable2 = heldItems.ParentNode;
            if (heldItemsTable2.Name == "h3" || heldItemsTable2.Name == "h2")
            {
                var h3 = heldItemsTable2.NextSibling;
                while (h3.Name == "#text")
                {
                    h3 = h3.NextSibling;
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
            }
            return null;
        }

        public static IEnumerable<string> GetAllFiles(string path, string mask, Func<FileInfo, bool>? checkFile = null)
        {
            if (string.IsNullOrEmpty(mask))
                mask = "*.*";
            var files = Directory.GetFiles(path, mask, SearchOption.AllDirectories).OrderBy(e => e);
            foreach (var file in files)
            {
                if (checkFile == null || checkFile(new FileInfo(file)))
                    yield return file;
            }
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