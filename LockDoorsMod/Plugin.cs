using BepInEx;
using HarmonyLib;

namespace LockDoorsMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LockingKeyModBase : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private static LockingKeyModBase instance;
        private void Awake()
        {
            instance ??= this;
            LockDoorsMod.Logger.logger.LogInfo("Lock Doors 1.0.0 :)");
            harmony.PatchAll();
        }
    }
}