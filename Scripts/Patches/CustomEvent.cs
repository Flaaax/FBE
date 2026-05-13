using System.Reflection;
using FBE.Scripts;
using FBE.Scripts.Events;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(ModelDb))]
public static class CustomSharedEventsPatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ModelDb), "AllSharedEvents");
    }

    public static void Postfix(ref IEnumerable<EventModel> __result)
    {
        __result = __result.Concat(ICustomModel.Events);
    }
}

// [HarmonyPatch(typeof(ModelDb))]
// public static class RemoveVanillaEvent
// {
//     public static MethodBase TargetMethod()
//     {
//         return AccessTools.PropertyGetter(typeof(ModelDb), "AllEvents");
//     }
//
//     public static void Postfix(ref IEnumerable<EventModel> __result)
//     {
//         __result = __result.Concat(ICustomModel.Events);
//     }
// }

[HarmonyPatch(typeof(EventModel))]
public static class CustomEventPatch1
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(EventModel), "InitialPortraitPath");
    }

    public static bool Prefix(ref string __result, EventModel __instance)
    {
        if (__instance is not FBEEventModel { CustomInitialPortraitPath: not null } model) return true;
        __result = model.CustomInitialPortraitPath;
        return false;
    }
}

[HarmonyPatch(typeof(EventModel))]
public static class CustomEventPatch2
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(EventModel), "BackgroundScenePath");
    }

    public static bool Prefix(ref string __result, EventModel __instance)
    {
        if (__instance is not FBEEventModel { CustomBackgroundScenePath: not null } model) return true;
        __result = model.CustomBackgroundScenePath;
        return false;
    }
}

[HarmonyPatch(typeof(EventModel))]
public static class CustomEventPatch3
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(EventModel), "VfxPath");
    }

    public static bool Prefix(ref string __result, EventModel __instance)
    {
        if (__instance is not FBEEventModel { CustomVfxPath: not null } model) return true;
        __result = model.CustomVfxPath;
        return false;
    }
}