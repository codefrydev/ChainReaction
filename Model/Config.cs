using System.ComponentModel.DataAnnotations;

namespace ChainReaction.Model
{
    public class Config
    {
        public static bool icon = true;
        public static int CellHeight { get; set; } = 64;

        [MinLength(2)]
        public static int NumberOfPlayer { get; set; } = 2;

        public static string CurrentUserColor { get; set; } = string.Empty;
        public static string HoverColor { get; set; } = string.Empty;

        public static List<Player> PlayerList { get; set; } =
        [
            new Player()
            {
                Name="Player One",
                RColor=255,
                BColor=0,
                GColor=0
            },
            new Player()
            {
                Name="Player Two",
                RColor=0,
                BColor=0,
                GColor=255
            },new Player()
            {
                Name="Player Three",
                RColor=255,
                BColor=0,
                GColor=255
            },
            new Player()
            {
                Name="Player Four",
                RColor=0,
                BColor=255,
                GColor=0
            },new Player()
            {
                Name="Player Five",
                RColor=0,
                BColor=0,
                GColor=0
            },
            new Player()
            {
                Name="Player Six",
                RColor=0,
                BColor=255,
                GColor=255
            }
        ];
    }
}
