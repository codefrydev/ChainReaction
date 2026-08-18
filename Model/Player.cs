namespace ChainReaction.Model
{
    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public int RColor { get; set; }
        public int GColor { get; set; }
        public int BColor { get; set; }

        public int CellCount { get; set; } = 0;
        public int OrbCount { get; set; } = 0;
        public bool IsEliminated { get; set; } = false;
        public bool IsBot { get; set; } = false;
        public string BotDifficulty { get; set; } = "Medium"; // Easy, Medium, Hard

        public string ColorFormed()
            => $"rgb({RColor},{GColor},{BColor})";

        public string HoverColorFormed()
            => $"rgba({RColor},{GColor},{BColor},0.25)";

        public string GlowColorFormed()
            => $"rgba({RColor},{GColor},{BColor},0.65)";

        public string HexColor
        {
            get => $"#{RColor:X2}{GColor:X2}{BColor:X2}";
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                var hex = value.TrimStart('#');
                if (hex.Length == 6)
                {
                    try
                    {
                        RColor = Convert.ToInt32(hex.Substring(0, 2), 16);
                        GColor = Convert.ToInt32(hex.Substring(2, 2), 16);
                        BColor = Convert.ToInt32(hex.Substring(4, 2), 16);
                    }
                    catch
                    {
                        // Ignore parse errors
                    }
                }
            }
        }

        // Computes optimal text color (white or dark slate) based on perceived luminance
        public string ContrastTextColor
        {
            get
            {
                double luminance = (0.299 * RColor + 0.587 * GColor + 0.114 * BColor);
                return luminance > 165 ? "#1e293b" : "#ffffff";
            }
        }

        // Returns a darker shade suitable for borders and rings
        public string DarkerBorderColor
        {
            get
            {
                int r = Math.Max(0, (int)(RColor * 0.7));
                int g = Math.Max(0, (int)(GColor * 0.7));
                int b = Math.Max(0, (int)(BColor * 0.7));
                return $"rgb({r},{g},{b})";
            }
        }

        public Player Clone()
        {
            return new Player
            {
                Name = this.Name,
                RColor = this.RColor,
                GColor = this.GColor,
                BColor = this.BColor,
                CellCount = this.CellCount,
                OrbCount = this.OrbCount,
                IsEliminated = this.IsEliminated,
                IsBot = this.IsBot,
                BotDifficulty = this.BotDifficulty
            };
        }
    }
}
