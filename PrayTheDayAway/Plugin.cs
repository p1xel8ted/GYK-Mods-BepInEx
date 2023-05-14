﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;

namespace PrayTheDayAway;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.praythedayaway";
    private const string PluginName = "Pray The Day Away!";
    private const string PluginVer = "0.2.8";

    private static ConfigEntry<bool> _debug;
    private static ManualLogSource Log { get; set; }
    private static Harmony Harmony { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }

    private static ConfigEntry<bool> EverydayIsSermonDay { get; set; }
    private static ConfigEntry<bool> SermonOverAndOver { get; set; }
    private static ConfigEntry<bool> NotifyOnPrayerLoss { get; set; }
    private static ConfigEntry<bool> AlternateMode { get; set; }
    private static ConfigEntry<bool> RandomlyUpgradeBasicPrayer { get; set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 606}));
        ModEnabled.SettingChanged += ApplyPatches;

        EverydayIsSermonDay = Config.Bind("1. General", "Everyday Is Sermon Day", true, new ConfigDescription("Allow sermons to be held every day.", null, new ConfigurationManagerAttributes {Order = 605}));

        SermonOverAndOver = Config.Bind("1. General", "Sermon Over And Over", false, new ConfigDescription("Allow sermons to be repeated without limitation.", null, new ConfigurationManagerAttributes {Order = 604}));

        AlternateMode = Config.Bind("2. Mode", "Alternate Mode", true, new ConfigDescription("Chance to lower item level instead of chance to lose it on prayer.", null, new ConfigurationManagerAttributes {Order = 603}));

        NotifyOnPrayerLoss = Config.Bind("3. Notifications", "Notify On Prayer Loss", true, new ConfigDescription("Display notifications when prayer items are lost.", null, new ConfigurationManagerAttributes {Order = 602}));

        RandomlyUpgradeBasicPrayer = Config.Bind("4. Upgrades", "Randomly Upgrade Basic Prayer", true, new ConfigDescription("Allow basic prayers to be randomly upgraded (to a known starred prayer).", null, new ConfigurationManagerAttributes {Order = 601}));

        _debug = Config.Bind("5. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 600}));
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