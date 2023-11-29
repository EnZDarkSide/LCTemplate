using BepInEx.Logging;

namespace LockDoorsMod;
public static class Logger
{
    public static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
}