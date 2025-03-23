using System.IO;
using System.Collections.Generic;
using FMODUnity;

using ModLogger = LimbusCustomSound.Utils.Logger;
using System.Text.RegularExpressions;

namespace LimbusCustomSound.Features;

public enum SoundType
{
    Voice,
    Effect,
    Music,
    Unknown,
}

public static class SoundManager
{
    private const string SoundDirectory = "Sound";
    private const FMOD.MODE SoundLoadMode = FMOD.MODE.CREATESTREAM;
    private static readonly Dictionary<SoundType, FMOD.ChannelGroup> ChannelGroups = new();
    private static readonly HashSet<string> RegisteredReplacements = new();
    private static readonly HashSet<SoundInstance> ActiveSounds = new();

    private static SoundInstance ActiveMusicInstance;
    private static readonly Dictionary<string, SoundType> SoundTypeMappings = new()
        {
            { "BGM/", SoundType.Music },
            { "VOICE/", SoundType.Voice },
            { "SFX/", SoundType.Effect }
        };

    public static SoundType GetSoundType(string eventPath)
    {
        foreach (var mapping in SoundTypeMappings)
        {
            if (eventPath.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }

        return SoundType.Unknown;
    }

    public static void ReleaseCurrentlyActiveMusicInstance()
    {
        if (ActiveMusicInstance != null)
        {
            ActiveMusicInstance.Release();
            ActiveMusicInstance = null;
        }
    }

    public class SoundInstance
    {
        public readonly FMOD.Sound Sound;
        public readonly FMOD.Channel Channel;
        public readonly uint Duration;
        public readonly string EventPath;
        public bool Released { get; private set; }

        public SoundInstance(string eventPath, FMOD.ChannelGroup channelGroup, bool paused, FMOD.MODE soundLoadMode)
        {
            EventPath = eventPath;
            var system = RuntimeManager.CoreSystem;
            string filePath = GetFilePath(eventPath);

            if (!File.Exists(filePath))
            {
                ModLogger.Debug($"[FMOD] Sound file not found: {filePath}");
                return;
            }

            // Attempt to create the sound
            FMOD.RESULT result = system.createSound(filePath, soundLoadMode, out Sound);
            if (result != FMOD.RESULT.OK)
            {
                ModLogger.Debug($"[FMOD] Failed to load sound: {filePath}, Error: {result}");
                return;
            }

            // Validate sound format
            FMOD.SOUND_TYPE soundType;
            FMOD.OPENSTATE openState;
            int channels, bits;

            Sound.getFormat(out soundType, out _, out channels, out bits);
            Sound.getOpenState(out openState, out _, out _, out _);

            if (soundType != FMOD.SOUND_TYPE.WAV || openState != FMOD.OPENSTATE.READY || channels <= 0 || bits <= 0)
            {
                ModLogger.Debug($"[FMOD] Incompatible or corrupted sound file: {filePath}");
                Sound.release();
                return;
            }

            // Get sound duration
            Sound.getLength(out Duration, FMOD.TIMEUNIT.MS);
            system.playSound(Sound, channelGroup, paused, out Channel);
        }
        public bool Finished()
        {
            Channel.isPlaying(out bool isPlaying);
            return !isPlaying;
        }

        public void Start()
        {
            SetPaused(false);
        }

        public void Stop()
        {
            SetPaused(true);
        }

        public void SetPaused(bool paused)
        {
            Channel.setPaused(paused);
        }

        public void Release()
        {
            if (Released)
            {
                return;
            }

            Channel.stop();
            Sound.release();

            ActiveSounds.Remove(this);
            Released = true;
        }
    }

    public static void Initialize(string directory = null)
    {
        directory ??= Path.Combine(Core.PluginInfo.ModPath, SoundDirectory);

        // Check if the directory exists, and create it if it doesn't
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            ModLogger.Message($"Created missing sound directory: {directory}");
        }

        var directoryInfo = new DirectoryInfo(directory);
        var files = directoryInfo.GetFiles("*.wav", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            RegisteredReplacements.Add(
                Path.GetRelativePath(directory, file.FullName)
                    .Replace("\\", "/")
                    .Replace(".wav", "")
            );
        }

        ModLogger.Message($"Registered replacements for {RegisteredReplacements.Count} sound(s)");
    }

    public static void InitializeGroups()
    {
        var system = RuntimeManager.CoreSystem;

        foreach (var soundType in System.Enum.GetValues(typeof(SoundType)))
        {
            if (ChannelGroups.ContainsKey((SoundType)soundType))
            {
                continue;
            }

            var channelName = $"Custom Group {soundType.ToString()}";
            system.createChannelGroup(channelName, out var channelGroup);
            ChannelGroups.Add((SoundType)soundType, channelGroup);
        }
    }

    public static bool HasReplacement(string eventPath)
    {
        return RegisteredReplacements.Contains(eventPath.Replace("event:/", ""));
    }

    public static void SetVolume(SoundType soundType, float volume)
    {
        ChannelGroups[soundType].setVolume(volume);
    }

    public static void Release()
    {
        foreach (var channelGroup in ChannelGroups.Values)
        {
            channelGroup.release();
        }
    }

    public static SoundInstance CreateSound(string eventPath)
    {
        if (!HasReplacement(eventPath))
        {
            return null;
        }
        bool paused = false;
        FMOD.MODE soundLoadMode = FMOD.MODE.DEFAULT;

        var soundType = GetSoundType(eventPath);
        var channelGroup = ChannelGroups[soundType];

        ModLogger.Debug($"Got sound type: {soundType}");

        // If it's music and already playing, return the existing instance
        if (soundType == SoundType.Music && ActiveMusicInstance != null && !ActiveMusicInstance.Finished())
        {
            ModLogger.Debug($"Music {eventPath} is already playing, skipping creation.");
            return ActiveMusicInstance;
        }

        // Stop the currently playing music before playing a new one
        if (soundType == SoundType.Music && ActiveMusicInstance != null)
        {
            ReleaseCurrentlyActiveMusicInstance();
        }

        if (soundType == SoundType.Music)
        {
            soundLoadMode = FMOD.MODE.LOOP_NORMAL;
        }

        var newInstance = new SoundInstance(eventPath, channelGroup, paused, soundLoadMode);

        if (soundType == SoundType.Music)
        {
            ActiveMusicInstance = newInstance;
        }

        return newInstance;
    }

    private static string GetFilePath(string eventPath)
    {
        var fileName = eventPath.Replace("event:/", "").Replace("/", "\\") + ".wav";
        var filePath = Path.Combine(Core.PluginInfo.ModPath, SoundDirectory, fileName);

        return filePath;
    }
}
