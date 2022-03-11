using System.Diagnostics.CodeAnalysis;

namespace CrawlBulbapedia
{
    public class HeldItemTable : IEqualityComparer<HeldItemTable>, IEquatable<HeldItemTable>
    {
        public string Name { get; set; }
        public string Probability { get; set; }
        public ISet<GameVersion> Games { get; set; } = new HashSet<GameVersion>();
        public Dictionary<GameVersion, List<string>> Notes { get; set; } = new Dictionary<GameVersion, List<string>>();
        public List<string> ItemNotes { get; set; } = new List<string>();
        public string Image { get; set; }

        public bool Equals(HeldItemTable? x, HeldItemTable? y)
        {
            if (x == null && y == x)
            {
                return true;
            }
            if (y == null)
            {
                return false;
            }
            if (x.Name == y.Name && x.Probability == y.Probability)
            {
                return true;
            }
            return false;
        }

        public bool Equals(HeldItemTable? other)
        {
            return Equals(this, other);
        }

        public int GetHashCode([DisallowNull] HeldItemTable obj)
        {
            return obj.Name.GetHashCode() ^ obj.Probability.GetHashCode();
        }
    }
}