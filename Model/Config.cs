using System.ComponentModel.DataAnnotations;

namespace ChainReaction.Model
{
    public class Config
    {
        public static int Width { get; set; } = 10;
        public static int Height { get; set; } = 10;

        [MinLength(2)]
        public static int NumberOfPlayer { get; set; } = 2;

        public static string CurrentUserColor { get; set; } = string.Empty;
        public static string HoverColor { get; set; } = string.Empty;
    }
}
