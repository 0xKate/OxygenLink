using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using System.Reflection;

namespace OxygenLink
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public const string
            GUID = $"{AUTHOR}.{NAME}",
            NAME = "OxygenLink",
            VERSION = "1.4.2",
            AUTHOR = "0xKate",
            NEXUS = "https://www.nexusmods.com/subnautica/mods/1397";

        public new static ManualLogSource Logger { get; private set; }
        public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public static string PluginFolder { get; } = System.IO.Path.GetDirectoryName(Assembly.Location);
        public static string AssetFolder { get; } = System.IO.Path.Combine(Plugin.PluginFolder, "Assets");

        public static Settings Settings { get; private set; }
        public void Awake()
        {
            Logger = base.Logger;
            Settings = OptionsPanelHandler.RegisterModOptions<Settings>();
            InitializePrefabs();
            Harmony.CreateAndPatchAll(Assembly, $"{GUID}");
            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }

        private void InitializePrefabs()
        {
            OxygenLinkPrefab.Register();
        }
    }
}