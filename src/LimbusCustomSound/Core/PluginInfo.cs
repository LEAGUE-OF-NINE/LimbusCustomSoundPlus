using System.IO;
using System.Reflection;
using UnityEngine;

namespace LimbusCustomSound.Core;

public static class PluginInfo {
    public const string Guid = "com.kimght.LimbusCustomSound";
    public const string Name = "LimbusCustomSound";
    public const string Version = "1.0.0";
    public const string Authors = "Bamboo-hatted Kim (kimght), Disaer";

    public static string ModPath { get; }
    public static string GamePath { get; }
    public static string AssemblyPath { get; }

    static PluginInfo()
    {
        AssemblyPath = Assembly.GetExecutingAssembly().Location;
        ModPath = Path.GetDirectoryName(AssemblyPath);
        GamePath = new DirectoryInfo(Application.dataPath!).Parent?.FullName;
    }
}
