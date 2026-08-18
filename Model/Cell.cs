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

        // Is this cell 1 orb away from exploding?
        public bool IsCritical => CurrentCount > 0 && CurrentCount == Capacity;

        public bool IsAvailableFor(string playerName) =>
            string.IsNullOrEmpty(Name) || Name == playerName;

        public string CellTypeName => Capacity switch
        {
            1 => "Corner",
            2 => "Edge",
            _ => "Center"
        };
    }
}
