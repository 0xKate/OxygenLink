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
        private const string
            GUID = "0x4B.",
            NAME = "OxygenLink",
            VERSION = "0.1.0.0",
            AUTHOR = "0x4B",
            NEXUS = "Placeholder";

        public const string MOD_FOLDER_LOCATION = $"./BepInEx/plugins/{NAME}/";
        public const string ASSETS_FOLDER_LOCATION = $"./BepInEx/plugins/{NAME}/Assets/";

        public new static ManualLogSource Logger { get; private set; }

        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        private void Awake()
        {
            // set project-scoped logger instance
            Logger = base.Logger;

            // Initialize custom prefabs
            InitializePrefabs();

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void InitializePrefabs()
        {
            OxygenLinkPrefab.Register();
        }
    }
}