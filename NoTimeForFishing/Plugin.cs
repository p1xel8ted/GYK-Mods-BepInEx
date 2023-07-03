﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace NoTimeForFishing;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.notimeforfishing";
    private const string PluginName = "No Time For Fishing!";
    private const string PluginVer = "3.2.3";
    private static ManualLogSource Log { get; set; }
    private static Harmony Harmony { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        ModEnabled = Config.Bind("General", "Enabled", true, $"Toggle {PluginName}");
        ModEnabled.SettingChanged += ApplyPatches;

        ApplyPatches(this, null);
    }

    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }
}