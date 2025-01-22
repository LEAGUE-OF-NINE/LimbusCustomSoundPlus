using BepInEx;
using HarmonyLib;
using BepInEx.Unity.IL2CPP;

using ConfigManager = LimbusCustomSound.Utils.Config;
using ModLogger = LimbusCustomSound.Utils.Logger;

namespace LimbusCustomSound.Core;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BasePlugin 
{
    public override void Load()
    {
        InitializeUtils();

        if (!ConfigManager.EnableCustomSound.Value)
        {
            ModLogger.Message("Custom sound is disabled in config");
            return;
        }

        InitializeFeatures();
        ApplyPatches();
    }

    private void InitializeUtils()
    {
        ConfigManager.Initialize(Config);
        ModLogger.Initialize();
    }

    private static void InitializeFeatures()
    {
        Features.SoundManager.Initialize();
    }

    private static void ApplyPatches()
    {
        var harmony = new Harmony(PluginInfo.Name);

        Patches.SoundBase.Patch(harmony);
        Patches.SoundEffects.Patch(harmony);

        ModLogger.Message("Patches applied");
    }
}
