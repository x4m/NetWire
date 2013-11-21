namespace NetWireUltimate
{
    struct Tuple
    {
        public int Item1 { get; set; }
        public int Item2 { get; set; }
        public static Tuple Create(int i1, int i2)
        {
            return new Tuple { Item1 = i1, Item2 = i2 };
        }

        public bool Equals(Tuple other)
        {
            return Item1 == other.Item1 && Item2 == other.Item2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Tuple && Equals((Tuple)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Item1 * 397) ^ Item2;
            }
        }
    }
}