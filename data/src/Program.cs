using HtmlAgilityPack;
using Newtonsoft.Json;
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
        public List<HeldItemTable> HeldItems { get; set; } = new List<HeldItemTable>();

    }

    public static class Program
    {

        public static void Main()
        {
            /*
         * get urls: https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9mon_by_National_Pok%C3%A9dex_number
         
         var a = [...document.querySelectorAll("table td:nth-child(4) a")]
        b = a.map(function(e){return e.href})
        b.join("\n")

        */
            //DownloadRawWebpages();

            var processedPath = "../../../../processedHTML";
            //RemoveRedundantTags(processedPath);

            //TODO
            // Remove <script>
            // add to git repo
            // get items page
            ExtractInfo(processedPath);
        }

        private static void RemoveRedundantTags(string targetPath)
        {
            var files = GetAllFiles(Directory.GetCurrentDirectory(), "*.html").ToArray();
            var data = new List<Pokemon>();
            var itemLinks = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var pokemon = new Pokemon();
                var doc = new HtmlDocument();
                doc.Load(file);
                var node = doc.DocumentNode;
                var html = node.GetChildNode("html");
                if(html == null)
                {
                    throw new Exception();
                }

                var body = html.GetChildNode("body");
                if (body == null)
                {
                    throw new Exception();
                }
                var div = body.GetChildNodeById("div", "globalWrapper");
                if(div == null)
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
                foreach(var l in lastTwo)
                {
                    content2.RemoveChild(l);
                }
                var p = Path.Combine(targetPath, file.Split("\\").Last());
                var str = content2.OuterHtml;
                File.WriteAllText(p, str);
                var fi = new FileInfo(file).Length;
                var f2 = new FileInfo(p).Length;
                Console.WriteLine($"{fi} -> {f2} ({Math.Round((decimal)f2 / fi, 3)})");
            }
        }

        private static HtmlNode? GetChildNodeById(this HtmlNode node, string childTag, string id)
        {
            return node.ChildNodes.Where(e => e.Name == childTag && e.Id == id).FirstOrDefault();
        }

        private static HtmlNode[] GetChildNodes(this HtmlNode node, string childTag)
        {
            return node.ChildNodes.Where(e => e.Name == childTag).ToArray();
        }

        private static HtmlNode? GetChildNode(this HtmlNode node, string childTag)
        {
            return node.ChildNodes.Where(e => e.Name == childTag).FirstOrDefault();
        }

        private static HtmlNode? GetChildNode(this HtmlNode node, string childTag, string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return node.ChildNodes.Where(e => e.Name == childTag && !e.GetClasses().Any()).FirstOrDefault();
            }
            return node.ChildNodes.Where(e => e.Name == childTag && e.HasClass(className)).FirstOrDefault();
        }

        private static void ExtractInfo(string path)
        {
            var files = GetAllFiles(path, "*.html").ToArray();
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
                pokemon.Name = name;
                var heldItems = node.SelectSingleNode("//*[@id=\"Held_items\"]");
                if (heldItems != null)
                {
                    var heldItemsTable = GetNextTable(heldItems);
                    if (heldItemsTable == null)
                    {
                        continue;
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
                    pokemon.HeldItems = itemTables;
                }
                else
                {
                    Console.Write(": No Held Item");
                }

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
            File.WriteAllText("itemLinks.txt", string.Join("\n", itemLinks.Values.OrderBy(e => e)));
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data3.json", json);
        }

        private static HtmlNode? GetNextTable(HtmlNode heldItems)
        {
            var heldItemsTable2 = heldItems.ParentNode;
            if (heldItemsTable2.Name == "h3")
            {
                var h3 = heldItemsTable2.NextSibling;
                while (h3.Name == "#text")
                {
                    h3 = h3.NextSibling;
                }
                if (h3.Name == "table")
                {
                    return h3;
                }
            }
            return null;
        }

        public static IEnumerable<string> GetAllFiles(string path, string mask, Func<FileInfo, bool>? checkFile = null)
        {
            if (string.IsNullOrEmpty(mask))
                mask = "*.*";
            var files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (checkFile == null || checkFile(new FileInfo(file)))
                    yield return file;
            }
        }

        private static void DownloadRawWebpages()
        {
            var file = File.ReadAllLines("urls.txt");
            foreach (var f in file)
            {
                if (string.IsNullOrEmpty(f))
                {
                    continue;
                }

                DownloadWebpage(f);
            }
        }

        public static void DownloadWebpage(string url)
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

            File.WriteAllText(fileName + ".html", source);
        }
    }
}