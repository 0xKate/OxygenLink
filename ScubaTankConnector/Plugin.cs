using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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
            VERSION = "1.0.5",
            AUTHOR = "KateMods",
            NEXUS = "Placeholder";

        public new static ManualLogSource Logger { get; private set; }
        public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public static string PluginFolder = Assembly.Location.Replace($"{Assembly.GetName().Name}.dll", "");
        public static string AssetFolder = $"{PluginFolder}\\Assets\\";

        private void Awake()
        {
            Logger = base.Logger;
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