﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;

namespace WheresMaStorage;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper", "3.0.1")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.wheresmastorage";
    private const string PluginName = "Where's Ma' Storage!";
    private const string PluginVer = "2.1.2";

    internal static ManualLogSource Log { get; private set; }
    private static Harmony Harmony { get; set; }

    private static ConfigEntry<bool> ModEnabled { get; set; }
    internal static ConfigEntry<bool> ModifyInventorySize { get; private set; }
    internal static ConfigEntry<bool> EnableGraveItemStacking { get; private set; }
    internal static ConfigEntry<bool> EnablePenPaperInkStacking { get; private set; }
    internal static ConfigEntry<bool> EnableChiselStacking { get; private set; }
    internal static ConfigEntry<bool> EnableToolAndPrayerStacking { get; private set; }
    internal static ConfigEntry<bool> AllowHandToolDestroy { get; private set; }
    internal static ConfigEntry<bool> ModifyStackSize { get; private set; }
    internal static ConfigEntry<bool> Debug { get; private set; }
    internal static ConfigEntry<bool> SharedInventory { get; private set; }
    internal static ConfigEntry<bool> DontShowEmptyRowsInInventory { get; private set; }
    internal static ConfigEntry<bool> ShowUsedSpaceInTitles { get; private set; }
    internal static ConfigEntry<bool> DisableInventoryDimming { get; private set; }
    internal static ConfigEntry<bool> ShowWorldZoneInTitles { get; private set; }
    internal static ConfigEntry<bool> HideInvalidSelections { get; private set; }
    internal static ConfigEntry<bool> RemoveGapsBetweenSections { get; private set; }
    internal static ConfigEntry<bool> RemoveGapsBetweenSectionsVendor { get; private set; }
    internal static ConfigEntry<bool> ShowOnlyPersonalInventory { get; private set; }
    internal static ConfigEntry<int> AdditionalInventorySpace { get; private set; }
    internal static ConfigEntry<int> StackSizeForStackables { get; private set; }
    internal static ConfigEntry<bool> HideStockpileWidgets { get; private set; }
    internal static ConfigEntry<bool> HideTavernWidgets { get; private set; }
    internal static ConfigEntry<bool> HideSoulWidgets { get; private set; }
    internal static ConfigEntry<bool> HideWarehouseShopWidgets { get; private set; }
    internal static ConfigEntry<bool> CollectDropsOnGameLoad { get; private set; }

    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 50}));
        ModEnabled.SettingChanged += ApplyPatches;
        Debug = Config.Bind("2. Advanced", "Debug Logging", false, new ConfigDescription("Enable or disable debug logging.", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 49}));
        SharedInventory = Config.Bind("3. Inventory", "Shared Inventory", true, new ConfigDescription("Enable or disable shared inventory when crafting.", null, new ConfigurationManagerAttributes {Order = 48}));
        ModifyInventorySize = Config.Bind("3. Inventory", "Modify Inventory Size", true, new ConfigDescription("Enable or disable modifying the inventory size.", null, new ConfigurationManagerAttributes {Order = 47}));
        AdditionalInventorySpace = Config.Bind("3. Inventory", "Additional Inventory Space", 20, new ConfigDescription("Set the number of additional inventory spaces.", null, new ConfigurationManagerAttributes {Order = 46}));
        ModifyStackSize = Config.Bind("4. Item Stacking", "Modify Stack Size", true, new ConfigDescription("Enable or disable modifying the stack size of items.", null, new ConfigurationManagerAttributes {Order = 45}));
        StackSizeForStackables = Config.Bind("4. Item Stacking", "Stack Size For Stackables", 999, new ConfigDescription("Set the maximum stack size for stackable items", new AcceptableValueRange<int>(1, 999), new ConfigurationManagerAttributes {Order = 44}));
        EnableGraveItemStacking = Config.Bind("4. Item Stacking", "Grave Item Stacking", false, new ConfigDescription("Allow grave items to stack", null, new ConfigurationManagerAttributes {Order = 43}));
        EnablePenPaperInkStacking = Config.Bind("4. Item Stacking", "Pen Paper Ink Stacking", false, new ConfigDescription("Allow pen, paper, and ink items to stack", null, new ConfigurationManagerAttributes {Order = 42}));
        EnableChiselStacking = Config.Bind("4. Item Stacking", "Chisel Stacking", false, new ConfigDescription("Allow chisel items to stack", null, new ConfigurationManagerAttributes {Order = 41}));
        EnableToolAndPrayerStacking = Config.Bind("4. Item Stacking", "Tool And Prayer Stacking", true, new ConfigDescription("Allow tool and prayer items to stack", null, new ConfigurationManagerAttributes {Order = 40}));
        DontShowEmptyRowsInInventory = Config.Bind("3. Inventory", "Dont Show Empty Rows In Inventory", true, new ConfigDescription("Enable or disable displaying empty rows in the inventory.", null, new ConfigurationManagerAttributes {Order = 39}));
        ShowUsedSpaceInTitles = Config.Bind("3. Inventory", "Show Used Space In Titles", true, new ConfigDescription("Enable or disable showing used space in inventory titles.", null, new ConfigurationManagerAttributes {Order = 38}));
        DisableInventoryDimming = Config.Bind("3. Inventory", "Inventory Dimming", true, new ConfigDescription("Enable or disable inventory dimming.", null, new ConfigurationManagerAttributes {Order = 37}));
        ShowWorldZoneInTitles = Config.Bind("3. Inventory", "Show World Zone In Titles", true, new ConfigDescription("Enable or disable showing world zone information in inventory titles.", null, new ConfigurationManagerAttributes {Order = 36}));
        HideInvalidSelections = Config.Bind("3. Inventory", "Hide Invalid Selections", true, new ConfigDescription("Enable or disable hiding invalid item selections in the inventory.", null, new ConfigurationManagerAttributes {Order = 35}));
        RemoveGapsBetweenSections = Config.Bind("3. Inventory", "Remove Gaps Between Sections", true, new ConfigDescription("Enable or disable removing gaps between inventory sections.", null, new ConfigurationManagerAttributes {Order = 34}));
        RemoveGapsBetweenSectionsVendor = Config.Bind("3. Inventory", "Remove Gaps Between Sections Vendor", true, new ConfigDescription("Enable or disable removing gaps between sections in the vendor inventory.", null, new ConfigurationManagerAttributes {Order = 33}));
        ShowOnlyPersonalInventory = Config.Bind("3. Inventory", "Show Only Personal Inventory", true, new ConfigDescription("Enable or disable showing only personal inventory.", null, new ConfigurationManagerAttributes {Order = 32}));
        AllowHandToolDestroy = Config.Bind("5. Gameplay", "Allow Hand Tool Destroy", true, new ConfigDescription("Enable or disable destroying objects with hand tools", null, new ConfigurationManagerAttributes {Order = 31}));
        CollectDropsOnGameLoad = Config.Bind("5. Gameplay", "Collect Drops On Game Load", true, new ConfigDescription("Enable or disable collecting drops on game load", null, new ConfigurationManagerAttributes {Order = 30}));
        HideStockpileWidgets = Config.Bind("6. UI", "Hide Stockpile Widgets", true, new ConfigDescription("Enable or disable hiding stockpile widgets", null, new ConfigurationManagerAttributes {Order = 29}));
        HideTavernWidgets = Config.Bind("6. UI", "Hide Tavern Widgets", true, new ConfigDescription("Enable or disable hiding tavern widgets", null, new ConfigurationManagerAttributes {Order = 28}));
        HideSoulWidgets = Config.Bind("6. UI", "Hide Soul Widgets", true, new ConfigDescription("Enable or disable hiding soul widgets", null, new ConfigurationManagerAttributes {Order = 27}));
        HideWarehouseShopWidgets = Config.Bind("6. UI", "Hide Warehouse Shop Widgets", true, new ConfigDescription("Enable or disable hiding warehouse shop widgets", null, new ConfigurationManagerAttributes {Order = 26}));
        Fields.GameBalanceAlreadyRun = false;
        Fields.InvSize = 20 + AdditionalInventorySpace.Value;
    }

    private static void ApplyPatches(object sender, EventArgs e)
    {
        if (ModEnabled.Value)
        {
            Actions.GameStartedPlaying += Helpers.RunWmsTasks;
            Actions.GameBalanceLoad += Helpers.GameBalanceLoad;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.GameStartedPlaying -= Helpers.RunWmsTasks;
            Actions.GameBalanceLoad -= Helpers.GameBalanceLoad;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }
}