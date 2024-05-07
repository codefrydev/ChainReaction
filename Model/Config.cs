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
    }
}
