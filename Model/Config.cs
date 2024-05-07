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
                RColor=61,
                BColor=52,
                GColor=139 
            },
            new Player()
            {
                Name="Player Two",
                RColor=247,
                BColor=184,
                GColor=1
            },new Player()
            {
                Name="Player Three",
                RColor=243,
                BColor=91,
                GColor=4
            },
            new Player()
            {
                Name="Player Four",
                RColor=49,
                BColor=57,
                GColor=60
            },new Player()
            {
                Name="Player Five",
                RColor=13,
                BColor=118,
                GColor=255
            },
            new Player()
            {
                Name="Player Six",
                RColor=143,
                BColor=45,
                GColor=86
            }
        ];
    }
}
