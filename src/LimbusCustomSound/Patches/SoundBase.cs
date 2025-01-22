using HarmonyLib;
using MainUI;

using ModLogger = LimbusCustomSound.Utils.Logger;

namespace LimbusCustomSound.Patches;

public static class SoundBase
{
    public static void Patch(Harmony harmony)
    {
        ModLogger.Debug("Patching SoundBase");
        harmony.PatchAll(typeof(SoundBase));
        ModLogger.Debug("SoundBase patched");
    }

    [HarmonyPatch(typeof(SettingsPanelSounds), nameof(SettingsPanelSounds.RefreshVolumes))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PatchRefreshVolumes(SettingsPanelSounds __instance)
    {
        var voiceVolume = __instance._voiceVolume * __instance._masterVolume;
        var effectVolume = __instance._vfxVolume * __instance._masterVolume;
        var musicVolume = __instance._bgmVolume * __instance._masterVolume;
        var masterVolume = __instance._masterVolume;

        UpdateVolumes(voiceVolume, effectVolume, musicVolume, masterVolume);
    }

    [HarmonyPatch(typeof(GlobalGameManager), nameof(GlobalGameManager.Start))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PatchStart(GlobalGameManager __instance)
    {
        Features.SoundManager.InitializeGroups();

        var soundManager = Singleton<SoundManager>.Instance;
        var voiceVolume = soundManager.Volume_Voice;
        var effectVolume = soundManager.Volume_SFX;
        var musicVolume = soundManager.Volume_BGM;
        var masterVolume = soundManager.Volume_Master;

        UpdateVolumes(voiceVolume, effectVolume, musicVolume, masterVolume);
    }

    private static void UpdateVolumes(float voiceVolume, float effectVolume, float musicVolume, float masterVolume)
    {
        Features.SoundManager.SetVolume(Features.SoundType.Voice, voiceVolume);
        Features.SoundManager.SetVolume(Features.SoundType.Effect, effectVolume);
        Features.SoundManager.SetVolume(Features.SoundType.Music, musicVolume);
        Features.SoundManager.SetVolume(Features.SoundType.Unknown, masterVolume * 0.5f);

        ModLogger.Debug($"Volumes updated: V = {voiceVolume}, E = {effectVolume}, M = {musicVolume}, U = {masterVolume * 0.5f}");
    }
}
