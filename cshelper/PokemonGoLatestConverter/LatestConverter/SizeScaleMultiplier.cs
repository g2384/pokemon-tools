namespace ConsoleApp1
{
    public class SizeScaleMultiplier
    {
        public decimal XXS { get; set; }

        public decimal XS { get; set; }

        public decimal XL { get; set; }

        public decimal XXL { get; set; }

        public static SizeScaleMultiplier None = new SizeScaleMultiplier();

        public bool IsNone()
        {
            return (XXS == 0 && XS == 0 && XL == 0 && XXL == 0);
        }
    }
}
