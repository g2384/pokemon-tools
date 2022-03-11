using HtmlAgilityPack;

namespace CrawlBulbapedia
{
    public static class RemoveTag
    {
        public static void Remove(string targetPath, string folder)
        {
            var path = Directory.GetCurrentDirectory() + "\\" + folder;
            var p2 = Path.Combine(targetPath, folder);
            if (!Directory.Exists(p2))
            {
                Directory.CreateDirectory(p2);
            }
            var files = FileHelper.GetAllFiles(path, "*.html").ToArray();
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
    }
}