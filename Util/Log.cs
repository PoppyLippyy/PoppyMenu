using BepInEx.Logging;

namespace PoppyMenu
{
    internal static class Log
    {
        private static ManualLogSource _log;

        internal static void Init(ManualLogSource log) => _log = log;

        internal static void Info(object data) => _log?.LogInfo(data);
        internal static void Message(object data) => _log?.LogMessage(data);
        internal static void Warning(object data) => _log?.LogWarning(data);
        internal static void Error(object data) => _log?.LogError(data);
        internal static void Debug(object data) => _log?.LogDebug(data);
    }
}
