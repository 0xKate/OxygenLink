using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;

namespace OxygenLink
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Menu("Oxygen Link")]
    public class Settings : ConfigFile
    {
        public static Settings Current => Plugin.Settings;

        [Choice("Recipe Difficulty (Requires Restart!)", Tooltip = "Determines how far into the game you must get to make an oxygen link. (Requires Restart!)")]
        public Difficulty RecipeDifficulty = Difficulty.Easy;
    }
}