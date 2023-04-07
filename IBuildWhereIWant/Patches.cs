﻿using System.Linq;
using GYKHelper;
using HarmonyLib;
using IBuildWhereIWant.lang;

namespace IBuildWhereIWant;

[HarmonyPatch]
public partial class Plugin
{
    private const string RefugeeZoneId = "refugee";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildGrid), nameof(BuildGrid.ShowBuildGrid))]
    public static void BuildGrid_ShowBuildGrid(ref bool show)
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        show = _disableGrid.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildGrid), nameof(BuildGrid.ClearPreviousTotemRadius))]
    public static void BuildGrid_ClearPreviousTotemRadius(ref bool apply_colors)
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        apply_colors = _disableGrid.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.EnterRemoveMode))]
    public static void BuildModeLogics_EnterRemoveMode(ref BuildModeLogics __instance)
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        __instance._remove_grey_spr.SetActive(_disableGreyRemoveOverlay.Value);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.CancelCurrentMode))]
    public static void BuildModeLogics_CancelCurrentMode()
    {
        if (!CrossModFields.CraftAnywhere) return;
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        OpenCraftAnywhere();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.CanBuild))]
    private static void BuildModeLogics_CanBuild(ref BuildModeLogics __instance)
    {
        __instance._multi_inventory = MainGame.me.player.GetMultiInventoryForInteraction();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.DoPlace))]
    private static void BuildModeLogics_DoPlace(ref BuildModeLogics __instance)
    {
        __instance._multi_inventory = MainGame.me.player.GetMultiInventoryForInteraction();
        if (CrossModFields.CraftAnywhere && MainGame.me.player.cur_zone.Length <= 0)
        {
            if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
            BuildGrid.ShowBuildGrid(false);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.FocusCameraOnBuildZone))]
    private static void BuildModeLogics_FocusCameraOnBuildZone(ref string zone_id)
    {
        if (!CrossModFields.CraftAnywhere) return;
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        zone_id = string.Empty;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.GetObjectRemoveCraftDefinition))]
    private static void BuildModeLogics_GetObjectRemoveCraftDefinition(string obj_id, ref ObjectCraftDefinition __result)
    {
        if (!CrossModFields.CraftAnywhere || MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;

        WriteLog($"[Remove]{obj_id}", true);

        __result = GameBalance.me.craft_obj_data
            .FirstOrDefault(objectCraftDefinition =>
                objectCraftDefinition.out_obj == obj_id &&
                objectCraftDefinition.build_type == ObjectCraftDefinition.BuildType.Remove);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildModeLogics), nameof(BuildModeLogics.OnBuildCraftSelected))]
    private static void BuildModeLogics_OnBuildCraftSelected(ref BuildModeLogics __instance)
    {
        if (!CrossModFields.CraftAnywhere) return;
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        BuildModeLogics.last_build_desk = _buildDeskClone;
        __instance._cur_build_zone_id = Zone;
        __instance._cur_build_zone = WorldZone.GetZoneByID(Zone);
        __instance._cur_build_zone_bounds = __instance._cur_build_zone.GetBounds();
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(FloatingWorldGameObject), nameof(FloatingWorldGameObject.RecalculateAvailability))]
    public static void FloatingWorldGameObject_RecalculateAvailability()
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;

        FloatingWorldGameObject.can_be_built = _disableBuildingCollision.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FlowGridCell), nameof(FlowGridCell.IsInsideWorldZone))]
    public static void FlowGridCell_IsInsideWorldZone(ref bool __result)
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FlowGridCell), nameof(FlowGridCell.IsPlaceAvailable))]
    public static void FlowGridCell_IsPlaceAvailable(ref bool __result)
    {
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        __result = true;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.GetUniversalObjectInfo))]
    public static void WorldGameObject_GetUniversalObjectInfo(ref WorldGameObject __instance, ref UniversalObjectInfo __result)
    {
        if (_buildDeskClone == null) return;
        if (__instance != _buildDeskClone) return;
        if (MainGame.me.player.GetMyWorldZoneId().Contains(RefugeeZoneId)) return;
        __result.header = GetLocalizedString(strings.Header);
        __result.descr = GetLocalizedString(strings.Description);
    }
}