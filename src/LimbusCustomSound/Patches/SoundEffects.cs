using HarmonyLib;
using System.Collections.Generic;
using FMODUnity;

using SoundInstance = LimbusCustomSound.Features.SoundManager.SoundInstance;
using ModLogger = LimbusCustomSound.Utils.Logger;

namespace LimbusCustomSound.Patches;

public static class SoundEffects
{
    private static readonly Dictionary<FMOD.Studio.EventInstance, SoundInstance> PatchedEvents = new();
    private static readonly HashSet<FMOD.Studio.EventInstance> MainVoiceEvents = new();
    private static SoundInstance _mainVoice;

    public static void Patch(Harmony harmony)
    {
        ModLogger.Debug("Patching SoundEffects");
        harmony.PatchAll(typeof(SoundEffects));
        ModLogger.Debug("SoundEffects patched");
    }

    [HarmonyPatch(typeof(RuntimeManager), nameof(RuntimeManager.CreateInstance), new System.Type[] { typeof(FMOD.GUID) })]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PatchCreateInstance(FMOD.GUID guid, ref FMOD.Studio.EventInstance __result)
    {
        RuntimeManager.StudioSystem.lookupPath(guid, out var eventPath);

        if (!Features.SoundManager.HasReplacement(eventPath))
        {
            ModLogger.Debug($"[FMOD] Created event instance for {eventPath} (No replacement)");
            return;
        }

        var sound = Features.SoundManager.CreateSound(eventPath);
        StretchToMatch(__result, sound);
        PatchedEvents[__result] = sound;

        ModLogger.Debug($"[FMOD] Created event instance for {eventPath} (Replaced)");
    }

    [HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.SetMainVoice))]
    [HarmonyPrefix]
    private static void PatchSetMainVoice(FMOD.Studio.EventInstance instance, int charid)
    {
        if (!PatchedEvents.ContainsKey(instance))
        {
            return;
        }

        MainVoiceEvents.Add(instance);
    }

    [HarmonyPatch(typeof(FMOD.Studio.EventInstance), nameof(FMOD.Studio.EventInstance.start))]
    [HarmonyPrefix]
    private static void PatchSoundEventStart(FMOD.Studio.EventInstance __instance)
    {
        ModLogger.Debug($"[FMOD] start called for instance: {__instance}");

        if (!PatchedEvents.TryGetValue(__instance, out var sound))
        {
            ModLogger.Debug($"[FMOD] No replacement found, proceeding with original event.");
            return;
        }

        if (MainVoiceEvents.Contains(__instance))
        {
            ModLogger.Debug($"[FMOD] Detected main voice event. Replacing _mainVoice.");
            _mainVoice?.Release();
            _mainVoice = sound;
            MainVoiceEvents.Remove(__instance);
        }

        ModLogger.Debug($"[FMOD] Muting original event instance.");
        __instance.setVolume(0);

        ModLogger.Debug($"[FMOD] Starting replacement sound: {sound}");
        sound.Start();
    }

    [HarmonyPatch(typeof(FMOD.Studio.EventInstance), nameof(FMOD.Studio.EventInstance.setVolume))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool PatchSetVolume(FMOD.Studio.EventInstance __instance, float volume)
    {
        if (!PatchedEvents.ContainsKey(__instance))
        {
            return true;
        }

        if (volume == 0)
        {
            return true;
        }

        __instance.setVolume(0);
        return false;
    }

    [HarmonyPatch(typeof(FMOD.Studio.EventInstance), nameof(FMOD.Studio.EventInstance.setPaused))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool PatchSetPaused(FMOD.Studio.EventInstance __instance, bool paused)
    {
        if (!PatchedEvents.TryGetValue(__instance, out var sound))
        {
            return true;
        }

        sound.SetPaused(paused);
        return false;
    }

    [HarmonyPatch(typeof(FMOD.Studio.EventInstance), nameof(FMOD.Studio.EventInstance.stop))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void PatchSoundEventStop(FMOD.Studio.EventInstance __instance)
    {
        if (!PatchedEvents.TryGetValue(__instance, out var sound))
        {
            return;
        }

        sound.Release();
        PatchedEvents.Remove(__instance);
    }

    [HarmonyPatch(typeof(FMOD.Studio.EventInstance), nameof(FMOD.Studio.EventInstance.release))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void PatchSoundEventRelease(FMOD.Studio.EventInstance __instance)
    {
        if (!PatchedEvents.TryGetValue(__instance, out var sound))
        {
            return;
        }

        sound.Release();
        PatchedEvents.Remove(__instance);
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.GetEventLength))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PatchGetEventLength(FMOD.Studio.EventInstance soundEvent, ref float __result)
    {
        if (!PatchedEvents.TryGetValue(soundEvent, out var sound))
        {
            
            return;
        }

        ModLogger.Debug($"[FMOD] Got sound duration: {sound.Duration}");
        __result = sound.Duration / 1000f;
    }

    private static void StretchToMatch(FMOD.Studio.EventInstance soundEvent, SoundInstance sound)
    {
        soundEvent.getDescription(out var eventDescription);
        eventDescription.getLength(out var duration);

        ModLogger.Debug($"[FMOD] Got event duration: {duration}ms");

        if (duration <= 0)
        {
            ModLogger.Debug("[FMOD] Skipping StretchToMatch because event duration is invalid.");
            // Apply default behavior for sounds without a valid duration
            soundEvent.setPitch(1);  // Set pitch to normal
            return;
        }

        float newPitch = (float)sound.Duration / duration;
        ModLogger.Debug($"[FMOD] Adjusting pitch: {newPitch}");
        soundEvent.setPitch(1);
    }
}
