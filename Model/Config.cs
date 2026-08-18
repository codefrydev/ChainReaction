using System.ComponentModel.DataAnnotations;

namespace ChainReaction.Model
{
    public class Config
    {
        public static bool IsCusTomDimention { get; set; } = false;
        public static string BoardPreset { get; set; } = "Auto"; // Auto, Quick, Classic, Large, Custom
        public static int Rows { get; set; } = 9;
        public static int Column { get; set; } = 6;
        public static bool Kampan { get; set; } = true;
        public static bool Dhwani { get; set; } = true;
        public static bool Icon { get; set; } = true;

        public static string GameSpeed { get; set; } = "Normal"; // Fast, Normal, Dramatic
        
        public static int DelayTimeInMilliSecond
        {
            get => GameSpeed switch
            {
                "Fast" => 20,
                "Dramatic" => 70,
                _ => 36 // Normal
            };
        }

        [Range(1, 5)]
        public static int CellCapacity { get; set; } = 3;
        public static int CellHeight { get; set; } = 54;

        [Range(2, 8)]
        public static int NumberOfPlayer { get; set; } = 2;

        public static string CurrentUserColor { get; set; } = "rgb(239, 68, 68)";
        public static string HoverColor { get; set; } = string.Empty;

        // Curated harmonious game palette (Crimson, Cyan, Emerald, Amber, Violet, Pink, Lime, Orange)
        public static readonly List<(string Name, int R, int G, int B)> PresetColors = new()
        {
            ("Crimson Red", 239, 68, 68),       // #ef4444
            ("Cyan Azure", 6, 182, 212),        // #06b6d4
            ("Emerald Green", 16, 185, 129),    // #10b981
            ("Golden Amber", 245, 158, 11),     // #f59e0b
            ("Electric Purple", 139, 92, 246),  // #8b5cf6
            ("Hot Pink", 236, 72, 153),         // #ec4899
            ("Lime Shock", 132, 204, 22),       // #84cc16
            ("Coral Orange", 249, 115, 22)      // #f97316
        };

        public static List<T> SuffeledArray<T>(List<T> array)
        {
            var rand = new Random();
            for (int i = 0; i < array.Count; i++)
            {
                var randIndex = rand.Next(i, array.Count);
                (array[i], array[randIndex]) = (array[randIndex], array[i]);
            }
            return array;
        }

        public static List<Player> PlayerList { get; set; } =
        [
            new Player { Name = "Player 1", RColor = 239, GColor = 68, BColor = 68 },   // Crimson Red
            new Player { Name = "Player 2", RColor = 6, GColor = 182, BColor = 212 },   // Cyan Azure
            new Player { Name = "Player 3", RColor = 16, GColor = 185, BColor = 129 },  // Emerald Green
            new Player { Name = "Player 4", RColor = 245, GColor = 158, BColor = 11 },  // Golden Amber
            new Player { Name = "Player 5", RColor = 139, GColor = 92, BColor = 246 },  // Electric Purple
            new Player { Name = "Player 6", RColor = 236, GColor = 72, BColor = 153 },  // Hot Pink
            new Player { Name = "Player 7", RColor = 132, GColor = 204, BColor = 22 },   // Lime Shock
            new Player { Name = "Player 8", RColor = 249, GColor = 115, BColor = 22 }   // Coral Orange
        ];
    }
}
