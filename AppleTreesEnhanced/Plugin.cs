﻿using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GYKHelper;
using HarmonyLib;
using UnityEngine;

namespace AppleTreesEnhanced
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("p1xel8ted.gyk.gykhelper")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.gyk.appletreesenhanced";
        private const string PluginName = "Apple Tree's Enhanced!";
        private const string PluginVer = "2.7.4";
        private static ManualLogSource Log { get; set; }
        private static Harmony _harmony;

        private static ConfigEntry<bool> _debug;
        private static ConfigEntry<bool> _modEnabled;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            InitConfiguration();

            ApplyPatches(this, null);
        }

        private void InitConfiguration()
        {
            _modEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Toggle {PluginName}", null, new ConfigurationManagerAttributes {Order = 10}));
            _modEnabled.SettingChanged += ApplyPatches;

            IncludeGardenBerryBushes = Config.Bind("2. Player Garden", "Include Garden Berry Bushes", true, new ConfigDescription("Enable enhancements for player garden berry bushes", null, new ConfigurationManagerAttributes {Order = 9}));
            IncludeGardenTrees = Config.Bind("2. Player Garden", "Include Garden Trees", true, new ConfigDescription("Enable enhancements for player garden trees", null, new ConfigurationManagerAttributes {Order = 8}));
            IncludeGardenBeeHives = Config.Bind("2. Player Garden", "Include Garden Bee Hives", false, new ConfigDescription("Enable enhancements for player garden bee hives", null, new ConfigurationManagerAttributes {Order = 7}));

            RealisticHarvest = Config.Bind("3. Harvesting", "Realistic Harvest", true, new ConfigDescription("Enable randomization of harvest amounts and drop time", null, new ConfigurationManagerAttributes {Order = 6}));
            ShowHarvestReadyMessages = Config.Bind("3. Harvesting", "Show Harvest Ready Messages", true, new ConfigDescription("Display messages when harvest is ready", null, new ConfigurationManagerAttributes {Order = 5}));

            IncludeWorldBerryBushes = Config.Bind("4. World Environment", "Include World Berry Bushes", false, new ConfigDescription("Enable enhancements for world berry bushes (not recommended without Wheres Ma Storage)", null, new ConfigurationManagerAttributes {Order = 4}));

            BeeKeeperBuyback = Config.Bind("5. Economy", "Bee Keeper Buyback", false, new ConfigDescription("Allow beekeeper to buy back bees", null, new ConfigurationManagerAttributes {Order = 3}));

            _debug = Config.Bind("6. Advanced", "Debug Logging", false, new ConfigDescription("Toggle debug logging on or off", null, new ConfigurationManagerAttributes {IsAdvanced = true, Order = 2}));
        }


        private static void ApplyPatches(object sender, EventArgs eventArgs)
        {
            if (_modEnabled.Value)
            {
                Log.LogInfo($"Applying patches for {PluginName}");
                Actions.GameStartedPlaying += CleanUpTrees;
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                Log.LogInfo($"Removing patches for {PluginName}");
                Actions.GameStartedPlaying -= CleanUpTrees;
                _harmony.UnpatchSelf();
            }
        }

        private static void CleanUpTrees(MainGame mainGame)
        {
            if (!MainGame.game_started) return;
            Log.LogInfo($"Running CleanUpTrees as Player has spawned in.");
            ProcessDudBees();
            ProcessDudTrees();
            ProcessDudBushes();
            ProcessReadyObjects();
        }

        private static void ProcessDudBees()
        {
            var dudBees = FindObjectsOfType<WorldGameObject>(true)
                .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.BeeHouse).Where(b => b.progress <= 0)
                .Where(Helpers.IsPlayerBeeHive);

            var dudBeesCount = 0;
            foreach (var dudBee in dudBees)
            {
                dudBeesCount++;
                Helpers.ProcessBeeRespawn(dudBee);

                if (_debug.Value)
                {
                    Log.LogMessage($"Fixed DudBee {dudBeesCount}");
                }
            }
        }

        private static void ProcessDudTrees()
        {
            var dudTrees = FindObjectsOfType<WorldGameObject>(true)
                .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.GardenAppleTree).Where(b => b.progress <= 0);

            var dudTreeCount = 0;
            foreach (var dudTree in dudTrees)
            {
                dudTreeCount++;
                Helpers.ProcessRespawn(dudTree, Helpers.Constants.HarvestGrowing.GardenAppleTree,
                    Helpers.Constants.HarvestSpawner.GardenAppleTree);

                if (_debug.Value)
                {
                    Log.LogMessage($"Fixed DudGardenTree {dudTreeCount}");
                }
            }
        }

        private static void ProcessDudBushes()
        {
            var dudBushes = FindObjectsOfType<WorldGameObject>(true)
                .Where(a => a.obj_id == Helpers.Constants.HarvestGrowing.GardenBerryBush).Where(b => b.progress <= 0);

            var dudBushCount = 0;
            foreach (var dudBush in dudBushes)
            {
                dudBushCount++;
                Helpers.ProcessRespawn(dudBush, Helpers.Constants.HarvestGrowing.GardenBerryBush,
                    Helpers.Constants.HarvestSpawner.GardenBerryBush);

                if (_debug.Value)
                {
                    Log.LogMessage($"Fixed DudGardenBush {dudBushCount}");
                }
            }
        }

        private static void ProcessReadyObjects()
        {
            var readyBees = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.BeeHouse)
                .Where(Helpers.IsPlayerBeeHive);
            var readyGardenTrees = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.GardenAppleTree);
            var readyGardenBushes = FindObjectsOfType<WorldGameObject>(true).Where(a => a.obj_id == Helpers.Constants.HarvestReady.GardenBerryBush);
            var readyWorldBushes = FindObjectsOfType<WorldGameObject>(true).Where(a => Helpers.WorldReadyHarvests.Contains(a.obj_id));

            foreach (var item in readyBees)
            {
                Helpers.ProcessGardenBeeHive(item);
            }

            foreach (var item in readyGardenTrees)
            {
                Helpers.ProcessGardenAppleTree(item);
            }

            foreach (var item in readyGardenBushes)
            {
                Helpers.ProcessGardenBerryBush(item);
            }

            foreach (var item in readyWorldBushes)
            {
                switch (item.obj_id)
                {
                    case Helpers.Constants.HarvestReady.WorldBerryBush1:
                        Helpers.ProcessBerryBush1(item);
                        break;

                    case Helpers.Constants.HarvestReady.WorldBerryBush2:
                        Helpers.ProcessBerryBush2(item);
                        break;

                    case Helpers.Constants.HarvestReady.WorldBerryBush3:
                        Helpers.ProcessBerryBush3(item);
                        break;
                }
            }
        }
    }
}