﻿namespace ChainReaction.Model
{
    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public int RColor { get; set; }
        public int GColor { get; set; }
        public int BColor { get; set; }

        public string ColorFormed()
            => $"rgb({RColor},{GColor},{BColor})";
        public string HoverColorFormed()
            => $"rgb({RColor},{GColor},{BColor},.2)";

    }
}
