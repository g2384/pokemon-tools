using HtmlAgilityPack;

namespace CrawlBulbapedia
{
    public static class HtmlNodeExtensions
    {
        public static HtmlNode? GetChildNodeById(this HtmlNode node, string childTag, string id)
        {
            return node.ChildNodes.Where(e => e.Name == childTag && e.Id == id).FirstOrDefault();
        }

        public static HtmlNode[] GetChildNodes(this HtmlNode node, string childTag)
        {
            return node.ChildNodes.Where(e => e.Name == childTag).ToArray();
        }

        public static HtmlNode? GetChildNode(this HtmlNode node, string childTag)
        {
            return node.ChildNodes.Where(e => e.Name == childTag).FirstOrDefault();
        }

        public static HtmlNode? GetChildNode(this HtmlNode node, string childTag, string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return node.ChildNodes.Where(e => e.Name == childTag && !e.GetClasses().Any()).FirstOrDefault();
            }
            return node.ChildNodes.Where(e => e.Name == childTag && e.HasClass(className)).FirstOrDefault();
        }

        public static HtmlNode? GetChildNode2(this HtmlNode node, string childTag, string excludedClassName)
        {
            return node.ChildNodes.Where(e => e.Name == childTag && !e.HasClass(excludedClassName)).LastOrDefault();
        }

        public static HtmlNode? RecursiveGetChildNode(this HtmlNode node, string childTag)
        {
            foreach(var c in node.ChildNodes)
            {
                if(c.Name == childTag)
                {
                    return c;
                }
                else
                {
                    var n = RecursiveGetChildNode(c, childTag);
                    if(n != null)
                    {
                        return n;
                    }
                }
            }
            return null;
        }

        public static List<HtmlNode[]> GetRegularisedTable(this HtmlNode table, out IDictionary<string, int[]> indices)
        {
            var trs = table.GetNearestNodes("tr", "a", "table");
            var trFirst = trs.First();
            var ths = trFirst.GetNearestNodes("th", "a", "table");
            indices = new Dictionary<string, int[]>();
            var count = 0;
            foreach(var th in ths)
            {
                var colspan = th.GetAttributeValue("colspan", 1);
                var ind = new List<int>();
                for(var i = 0; i < colspan; i++)
                {
                    ind.Add(count);
                    count++;
                }
                var head = th.InnerText.Trim();
                indices.Add(head, ind.ToArray());
            }
            
            var results = new List<HtmlNode[]>();
            for (int i = 0; i < trs.Length - 1; i++)
            {
                results.Add(new HtmlNode[count]);
            }
            var colI = 0;
            var rowI = 0;
            foreach (var tr in trs.Skip(1))
            {
                var ths2 = tr.GetNearestNodes("th", "a", "table");
                foreach(var th in ths2)
                {
                    AddToTable(count, results, ref colI, ref rowI, th);
                }
                var tds = tr.GetNearestNodes("td", "a", "table");
                foreach (var td in tds)
                {
                    AddToTable(count, results, ref colI, ref rowI, td);
                }
                rowI++;
                colI = 0;
                if (rowI < results.Count)
                {
                    foreach (var nn in results[rowI])
                    {
                        if (nn != null)
                        {
                            colI++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return results;
        }

        private static void AddToTable(int count, List<HtmlNode[]> results, ref int colI, ref int rowI, HtmlNode td)
        {
            var colspan = td.GetAttributeValue("colspan", 1);
            var rowspan = td.GetAttributeValue("rowspan", 1);
            for (int i = 0; i < colspan; i++)
            {
                results[rowI][colI] = td;
                for (int j = 0; j < rowspan; j++)
                {
                    if (rowI + j < results.Count)
                    {
                        results[rowI + j][colI] = td;
                    }
                }
                colI++;
            }
        }

        public static HtmlNode[] GetNearestNodes(this HtmlNode node, string childTag, params string[] stopAt)
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
    }
}