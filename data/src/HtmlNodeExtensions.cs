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
    }
}