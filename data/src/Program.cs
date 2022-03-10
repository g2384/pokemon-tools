using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;

namespace CrawlBulbapedia
{
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
    }

    public class ForeignName
    {
        public string Name { get; set; }
        public List<string> OtherNames { get; set; } = new List<string>();
        public string Pronounciation { get; set; }
        public List<string> OtherPronounciations { get; set; } = new List<string>();
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

                var p = Path.Combine(p2, file.Split("\\").Last());
                var str = content2.OuterHtml;
                File.WriteAllText(p, str);
                var fi = new FileInfo(file).Length;
                var f2 = new FileInfo(p).Length;
                Console.WriteLine($"{fi} -> {f2} ({Math.Round((decimal)f2 / fi, 3)})");
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
                            var innerA = td.GetChildNode("a", null);
                            var itemName = innerA?.InnerText;
                            if (string.IsNullOrEmpty(itemName))
                            {
                                itemName = td.InnerText;
                            }
                            else
                            {
                                var itemLink = innerA.GetAttributeValue("href", "");
                                if (itemLinks.TryGetValue(itemName, out var link))
                                {
                                    if (link != itemLink)
                                    {
                                        if (!itemLink.StartsWith("/wiki/Berry#")
                                            && !itemLink.StartsWith("/wiki/Gold_Bottle_Cap#")
                                            && !itemLink.StartsWith("/wiki/Gem#")
                                            && !itemLink.StartsWith("/wiki/Potion#"))
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
                            var prob = td.GetDirectInnerText();
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
            var table = GetNextTable(h2);
            if (table == null)
            {
                Console.Write(": no translation");
            }
            var trs = table.ChildNodes["tbody"].GetChildNodes("tr");
            var trTitle = trs[0];
            var tdTitle = trTitle.GetChildNodes("th");
            int lanIndex = -1;
            int titleIndex = -1;
            int meaningIndex = -1;
            var count = 0;
            foreach (var t in tdTitle)
            {
                var text = t.InnerText.Trim();
                if (text == "Language")
                {
                    lanIndex = count;
                }
                if (text == "Title")
                {
                    titleIndex = count;
                }
                if (text == "Meaning")
                {
                    meaningIndex = count;
                }
                count++;
            }
            foreach (var tr in trs.Skip(1))
            {
                var tds = tr.GetChildNodes("td");
                if (tds.Length == 3)
                {
                    var names = new ForeignName();
                    var meaning = tds[meaningIndex].InnerHtml.Trim();
                    names.Meaning = meaning;
                    var lan = tds[lanIndex].InnerText.Trim();
                    var key = lan.ToEnum<Language>();
                    var name = tds[titleIndex].InnerHtml.Trim();
                    if (name.Contains("<i>"))
                    {
                        var lines = name.Split("\n");
                        var firstLine = lines[0];
                        var (name1, pinyin1) = GetName(firstLine);
                        names.Name = name1;
                        names.Pronounciation = pinyin1;
                        foreach (var line in lines.Skip(1))
                        {
                            var (name2, pinyin2) = GetName(firstLine);
                            names.OtherNames.Add(name2);
                            names.OtherPronounciations.Add(pinyin2);
                        }
                    }
                    else
                    {
                        names.Name = name;
                    }
                    if (!names.OtherNames.Any())
                    {
                        names.OtherNames = null;
                    }
                    if (!names.OtherPronounciations.Any())
                    {
                        names.OtherPronounciations = null;
                    }
                    dict.Add(key, names);
                }
                else
                {
                    //throw new Exception();
                }
            }
            return dict;
        }

        private static (string, string) GetName(string text)
        {
            var pinyinNode = new HtmlDocument();
            pinyinNode.LoadHtml(text);
            var p = pinyinNode.DocumentNode;
            var p2 = p.RecursiveGetChildNode("i")?.InnerText;
            var p3 = p.GetDirectInnerText();
            return (p3, p2);
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
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                return;
            }
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
            if (File.Exists(fileName))
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

            File.WriteAllText(folder + "/" + fileName + ".html", source);
        }
    }
}