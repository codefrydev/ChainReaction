namespace ChainReaction.Pages
{
    public class Player
    { 
        public string Name { get; set; } 
        public int RColor { get; set; }
        public int GColor { get; set; }
        public int BColor { get; set; }

        public string ColorFormed()
        {
            return $"rgb({RColor},{GColor},{BColor})";
        }
        public string HoverColorFormed()
        {
            return $"rgb({RColor},{GColor},{BColor},.2)";
        }

    }
}
