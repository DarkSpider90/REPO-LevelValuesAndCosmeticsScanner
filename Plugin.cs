using BepInEx;
using BepInEx.Logging;
using LevelValuesAndCosmeticsScanner.Runtime;
using LevelValuesAndCosmeticsScanner.Settings;

namespace LevelValuesAndCosmeticsScanner
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public sealed class ModEntryPoint : BaseUnityPlugin
    {
        private const string PLUGIN_GUID = "DarkSpider90.LevelValuesAndCosmeticsScanner";
        private const string PLUGIN_NAME = "LevelValuesAndCosmeticsScanner";
        private const string PLUGIN_VERSION = "1.0.0";

        public static ManualLogSource Log { get; private set; }

        public void Awake()
        {
            Log = base.Logger;

            ModSettings.Bind(Config);
            gameObject.AddComponent<LevelScannerRuntime>();
            Log.LogInfo($"{PLUGIN_NAME} {PLUGIN_VERSION} loaded.");
        }
    }
}
