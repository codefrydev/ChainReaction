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

        public static string GameMode { get; set; } = "PvBot"; // PvBot (Vs Computer), PvP (Pass & Play), Custom
        public static string DefaultBotDifficulty { get; set; } = "Medium"; // Easy, Medium, Hard

        public static readonly List<string> PresetBotNames = new()
        {
            "AlphaBot",
            "NovaBot",
            "CyberBot",
            "TitanBot",
            "QuantumBot",
            "NexusBot",
            "VortexBot",
            "ZenBot"
        };

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
            new Player { Name = "You (P1)", RColor = 239, GColor = 68, BColor = 68, IsBot = false },
            new Player { Name = "AlphaBot", RColor = 6, GColor = 182, BColor = 212, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "NovaBot", RColor = 16, GColor = 185, BColor = 129, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "CyberBot", RColor = 245, GColor = 158, BColor = 11, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "TitanBot", RColor = 139, GColor = 92, BColor = 246, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "QuantumBot", RColor = 236, GColor = 72, BColor = 153, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "NexusBot", RColor = 132, GColor = 204, BColor = 22, IsBot = true, BotDifficulty = "Medium" },
            new Player { Name = "ZenBot", RColor = 249, GColor = 115, BColor = 22, IsBot = true, BotDifficulty = "Medium" }
        ];

        public static void ApplyGameMode(string mode)
        {
            GameMode = mode;
            if (mode == "PvBot")
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    if (i == 0)
                    {
                        PlayerList[i].IsBot = false;
                        if (string.IsNullOrEmpty(PlayerList[i].Name) || PlayerList[i].Name.StartsWith("Player 1") || PlayerList[i].Name.StartsWith("AlphaBot"))
                            PlayerList[i].Name = "You (P1)";
                    }
                    else
                    {
                        PlayerList[i].IsBot = true;
                        PlayerList[i].BotDifficulty = DefaultBotDifficulty;
                        if (string.IsNullOrEmpty(PlayerList[i].Name) || PlayerList[i].Name.StartsWith("Player "))
                        {
                            var botName = i - 1 < PresetBotNames.Count ? PresetBotNames[i - 1] : $"Bot {i + 1}";
                            PlayerList[i].Name = botName;
                        }
                    }
                }
            }
            else if (mode == "PvP")
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    PlayerList[i].IsBot = false;
                    if (PlayerList[i].Name == "You (P1)" || PresetBotNames.Contains(PlayerList[i].Name))
                    {
                        PlayerList[i].Name = $"Player {i + 1}";
                    }
                }
            }
        }
    }
}
