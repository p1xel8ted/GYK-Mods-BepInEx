﻿using System.Linq;
using System.Threading;
using GYKHelper;

namespace QueueEverything;

public partial class Plugin
{
    // private static string GetLocalizedString(string content)
    // {
    //     Thread.CurrentThread.CurrentUICulture = CrossModFields.Culture;
    //     return content;
    // }
    
    private static bool IsUnsafeDefinition(CraftDefinition _craftDefinition)
    {
        var zombieCraft = _craftDefinition.craft_in.Any(craftIn => craftIn.Contains("zombie"));
        var refugeeCraft = _craftDefinition.craft_in.Any(craftIn => craftIn.Contains("refugee"));
        var unsafeOne = UnSafeCraftDefPartials.Any(_craftDefinition.id.Contains);
        var unsafeTwo = !_craftDefinition.icon.Contains("fire") && _craftDefinition.craft_in.Any(craftIn => UnSafeCraftObjects.Contains(craftIn));
        var unsafeThree = MultiOutCantQueue.Any(_craftDefinition.id.Contains);

        if (zombieCraft || refugeeCraft || unsafeOne || unsafeTwo || unsafeThree)
        {
            return true;
        }

        return false;
    }

    private static void WriteLog(string message, bool error = false)
    {
        if (error)
        {
            Log.LogError($"{message}");
        }
        else
        {
            if (_debug.Value)
            {
                Log.LogInfo($"{message}");
            }
        }
    }
}