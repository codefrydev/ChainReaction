namespace ChainReaction.Model
{
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Capacity { get; set; }
        public int CurrentCount { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

    }
}
