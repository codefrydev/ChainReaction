using System.ComponentModel.DataAnnotations;

namespace ChainReaction.Model
{
    public class Config
    {
        public static bool IsCusTomDimention { get; set; } = false;
        public static int Rows { get; set; } = 5;
        public static int Column { get; set; } = 5;
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
            new Player()
            {
                Name = "Player Blue Cat",
                RColor = 70,
                GColor = 130,
                BColor = 180
            },
            new Player()
            {
                Name = "Player Brown Mouse",
                RColor = 205,
                GColor = 133,
                BColor = 63
            },
            new Player()
            {
                Name = "Player Orange",
                RColor = 255,
                GColor = 140,
                BColor = 0
            },
            new Player()
            {
                Name = "Player Yellow Cheese",
                RColor = 255,
                GColor = 215,
                BColor = 0
            },
            new Player()
            {
                Name = "Player Green",
                RColor = 60,
                GColor = 179,
                BColor = 113
            },
            new Player()
            {
                Name = "Player Purple",
                RColor = 147,
                GColor = 112,
                BColor = 219
            }

        ];
    }
}
