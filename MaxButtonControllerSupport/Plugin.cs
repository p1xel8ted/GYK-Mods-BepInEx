﻿using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using Rewired;

namespace MaxButtonControllerSupport;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[BepInDependency("p1xel8ted.gyk.gykhelper")]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.gyk.maxbuttoncontrollersupport";
    private const string PluginName = "Max Button Controller Support";
    private const string PluginVer = "1.3.3";

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
            Actions.WorldGameObjectInteractPrefix += WorldGameObject_Interact;
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Actions.WorldGameObjectInteractPrefix -= WorldGameObject_Interact;
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }

    private void Update()
    {
        if (!IsUpdateConditionsMet()) return;

        HandleGamepadInput();
    }

    private static bool IsUpdateConditionsMet()
    {
        return MainGame.game_started && !MainGame.me.player.is_dead && !MainGame.me.player.IsDisabled() && FloatingWorldGameObject.cur_floating == null;
    }

    private static void HandleGamepadInput()
    {
        if (!LazyInput.gamepad_active) return;
        
        var player = ReInput.players.GetPlayer(0);
        if (_itemCountGuiOpen)
        {
            HandleItemCountGuiInput(player);
        }
        else if (_craftGuiOpen && !_unsafeInteraction)
        {
            HandleCraftGuiInput(player);
        }
    }

    private static void HandleItemCountGuiInput(Player player)
    {
        if (player.GetButtonDown(19))
        {
            InvokeMaxButtonVendorMethod("SetMaxPrice", new object[] {_slider});
        }

        if (player.GetButtonDown(20))
        {
            InvokeMaxButtonVendorMethod("SetSliderValue", new object[] {_slider, 1});
        }
    }

    private static void HandleCraftGuiInput(Player player)
    {
        if (IsCraftGuiInputValid()) return;

        if (player.GetButtonDown(19))
        {
            InvokeMaxButtonCraftingMethod("SetMaximumAmount", new object[] {_craftItemGui, _crafteryWgo});
        }

        if (player.GetButtonDown(20))
        {
            InvokeMaxButtonCraftingMethod("SetMinimumAmount", new object[] {_craftItemGui});
        }
    }

    private static bool IsCraftGuiInputValid()
    {
        return _craftItemGui.current_craft.needs.Any(need => need.is_multiquality) || _craftItemGui.current_craft.one_time_craft;
    }

    private static void InvokeMaxButtonVendorMethod(string methodName, object[] parameters)
    {
        AccessTools.Method(typeof(MaxButtonVendor), methodName)?.Invoke(typeof(MaxButtonVendor), parameters);
    }

    private static void InvokeMaxButtonCraftingMethod(string methodName, object[] parameters)
    {
        AccessTools.Method(typeof(MaxButtonCrafting), methodName)?.Invoke(typeof(MaxButtonCrafting), parameters);
    }
}