using System;
using LimbusCustomSound.Core;

namespace LimbusCustomSound.Utils;

public enum LogLevel
{
    Debug = 0,
    Warning = 1,
    Error = 2,
}

public static class Logger
{
    private static BepInEx.Logging.ManualLogSource _logSource;
    private static LogLevel _logLevel = LogLevel.Error;

    public static void Initialize(string logSourceName = null)
    {
        if (Config.EnableDebugLogging.Value)
        {
            _logLevel = LogLevel.Debug;
        }
        
        _logSource = BepInEx.Logging.Logger.CreateLogSource(logSourceName ?? PluginInfo.Name);
    }

    public static void Debug(string message)
    {
        if (_logLevel <= LogLevel.Debug) {
            _logSource.LogInfo(message);
        }
    }

    public static void Warning(string message)
    {   
        if (_logLevel <= LogLevel.Warning) {
            _logSource.LogWarning(message);
        }
    }

    public static void Error(string message)
    {
        if (_logLevel <= LogLevel.Error) {
            _logSource.LogError(message);
        }
    }

    public static void Message(string message)
    {
        _logSource.LogInfo(message);
    }

    public static void Exception(Exception exception, string message)
    {
        if (_logLevel <= LogLevel.Error) {
            // TODO: Prettier exception message
            _logSource.LogError($"{message}\nException: {exception}");
        }
    }
}
