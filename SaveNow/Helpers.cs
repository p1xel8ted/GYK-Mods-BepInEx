﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GYKHelper;
using SaveNow.lang;
using UnityEngine;

namespace SaveNow;

public partial class Plugin
{
    private static string GetLocalizedString(string content)
    {
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
        return content;
    }

    private static void WriteLog(string message, bool error = false)
    {
        if (error)
        {
            Log.LogError($"{message}");
        }
        else
        {
            if (Debug.Value)
            {
                Log.LogInfo($"{message}");
            }
        }
    }

    private static void WriteSavesToFile()
    {
        using var file = new StreamWriter(DataPath, false);
        foreach (var entry in SaveLocationsDictionary)
        {
            var result = entry.Value.ToString().Substring(1, entry.Value.ToString().Length - 2);
            result = result.Replace(" ", "");
            file.WriteLine("{0}={1}", entry.Key, result);
        }

        if (BackupSavesOnSave.Value)
        {
            MainGame.me.StartCoroutine(BackUpSaveDirectory());
        }
    }

    private static IEnumerator BackUpSaveDirectory()
    {
        try
        {
            foreach (var file in Directory.GetFiles(PlatformSpecific.GetSaveFolder()))
            {
                if (!File.Exists(Path.Combine(SavePath, Path.GetFileName(file))))
                {
                    File.Copy(file, Path.Combine(SavePath, Path.GetFileName(file)));
                }
            }
        }
        catch (Exception e)
        {
            WriteLog(e.Message, true);
        }

        yield return true;
    }

    private static void LoadSaveLocations()
    {
        if (!File.Exists(DataPath)) return;

        var lines = File.ReadAllLines(DataPath, Encoding.Default);
        foreach (var line in lines)
        {
            if (!line.Contains('=')) continue;
            var splitLine = line.Split('=');
            var saveName = splitLine[0];
            var tempVector = splitLine[1].Split(',');
            var vectorToAdd = new Vector3(float.Parse(tempVector[0].Trim(), CultureInfo.InvariantCulture),
                float.Parse(tempVector[1].Trim(), CultureInfo.InvariantCulture), float.Parse(tempVector[2].Trim(), CultureInfo.InvariantCulture));

            var found = SaveLocationsDictionary.TryGetValue(saveName, out _);

            if (!File.Exists(Path.Combine(PlatformSpecific.GetSaveFolder(), saveName + ".dat"))) continue;
            if (!found) SaveLocationsDictionary.Add(saveName, vectorToAdd);
        }
    }

    //reads co-ords from player, and saves to file
    private static bool SaveLocation(bool menuExit, string saveFile)
    {
        if (!Tools.TutorialDone()) return true;
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;

        Pos = MainGame.me.player.pos3;
        CurrentSave = MainGame.me.save_slot.filename_no_extension;

        var overwrite = SaveLocationsDictionary.TryGetValue(CurrentSave, out _);
        if (overwrite)
        {
            SaveLocationsDictionary.Remove(CurrentSave);
            SaveLocationsDictionary.Add(CurrentSave, Pos);
        }
        else
        {
            SaveLocationsDictionary.Add(CurrentSave, Pos);
        }

        WriteSavesToFile();

        if (menuExit) return true;
        if (SaveGameNotificationText.Value)
        {
            if (!saveFile.Equals(string.Empty))
            {
                if (NewFileOnAutoSave.Value)
                    Tools.ShowMessage(strings.AutoSave + ": " + saveFile, Vector3.zero);
                else
                    Tools.ShowMessage(strings.AutoSave + "!", Vector3.zero);
            }
            else
            {
                Tools.ShowMessage(strings.SaveMessage, Vector3.zero);
            }
        }

        return true;
    }

    private static void Resize<T>(List<T> list, int size)
    {
        var count = list.Count;
        if (size < count) list.RemoveRange(size, count - size);
    }

    //reads co-ords from file and teleports player there
    private static void RestoreLocation(MainGame mainGame)
    {
        LoadSaveLocations();
        Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;

        var homeVector = new Vector3(2841, -6396, -1332);
        var foundLocation =
            SaveLocationsDictionary.TryGetValue(mainGame.save_slot.filename_no_extension, out var posVector3);
        var pos = foundLocation ? posVector3 : homeVector;
        mainGame.player.PlaceAtPos(pos);
        if (TravelMessages.Value) Tools.ShowMessage(strings.Rush, pos);

        StartTimer();
    }

    private static void StartTimer()
    {
        if (AutoSaveConfig.Value)
        {
            GJTimer.AddTimer(SaveInterval.Value, AutoSave);
        }
    }

    private static void AutoSave()
    {
        if (!Tools.TutorialDone()) return;
        if (EnvironmentEngine.me.IsTimeStopped()) return;
        if (!Application.isFocused) return;
        if (!CanSave) return;
        if (!NewFileOnAutoSave.Value)
        {
            PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save,
                delegate { SaveLocation(false, MainGame.me.save_slot.filename_no_extension); });
        }
        else
        {
            GUIElements.me.ShowSavingStatus(true);
            var date = DateTime.Now.ToString("ddmmyyhhmmss");
            var newSlot = $"autosave.{date}".Trim();

            MainGame.me.save_slot.filename_no_extension = newSlot;
            PlatformSpecific.SaveGame(MainGame.me.save_slot, MainGame.me.save,
                delegate
                {
                    SaveLocation(false, newSlot);
                    GUIElements.me.ShowSavingStatus(false);
                });
        }

        StartTimer();
    }
}