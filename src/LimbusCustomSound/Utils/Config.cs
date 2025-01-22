using BepInEx.Configuration;

namespace LimbusCustomSound.Utils;

public static class Config
{
    private static ConfigFile _configFile;   
    public static ConfigEntry<bool> EnableDebugLogging { get; private set; }
    public static ConfigEntry<bool> EnableCustomSound { get; private set; }
    
    public static void Initialize(ConfigFile configFile)
    {
        _configFile = configFile;
        RegisterConfig();
    }
    
    private static void RegisterConfig()
    {
        EnableDebugLogging = _configFile.Bind("General", "EnableDebugLogging", true, "Enable debug logging (true | false)");
        EnableCustomSound = _configFile.Bind("General", "EnableCustomSound", true, "Enable custom sounds (true | false)");
    }
}