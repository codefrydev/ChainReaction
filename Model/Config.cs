using System.ComponentModel.DataAnnotations;

namespace ChainReaction.Model
{
    public class Config
    {
        public static bool Kampan {get;set;} = false;
        public static bool Dhwani {get;set;} = false;
        public static bool Icon { get; set; } = true;
        [Range(10, 1000)]
        public static int DelayTimeInMilliSecond { get; set; } = 20;

        [Range(1, 5)]
        public static int CellCapacity { get; set; } = 3;
        public static int CellHeight { get; set; } = 64;

        [MinLength(2)]
        public static int NumberOfPlayer { get; set; } = 2;

        public static string CurrentUserColor { get; set; } = "rgb(255,0,0)";
        public static string HoverColor { get; set; } = string.Empty;

        public static List<Player> PlayerList { get; set; } =
        [
            new Player()
            {
                Name = "Player One",
                RColor = 255,
                GColor = 0,
                BColor = 0
            },
            new Player()
            {
                Name = "Player Two",
                RColor = 0,
                GColor = 255,
                BColor = 0
            },
            new Player()
            {
                Name = "Player Three",
                RColor = 0,
                GColor = 0,
                BColor = 0
            },
            new Player()
            {
                Name = "Player Four",
                RColor = 0,
                GColor = 0,
                BColor = 255
            },
            new Player()
            {
                Name = "Player Six",
                RColor = 0,
                GColor = 255,
                BColor = 255
            },
            new Player()
            {
                Name = "Player Five",
                RColor = 109,
                GColor = 197,
                BColor = 209
            }

        ];
    }
}
