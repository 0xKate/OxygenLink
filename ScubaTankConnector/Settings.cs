using Nautilus.Json;
using Nautilus.Options.Attributes;
using System;

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

        [Toggle("Destroy on Death", Tooltip = "If enabled, the Oxygen Link will be destroyed when the player dies.")]
        public Boolean DestroyOnDeath = false;
    }
}