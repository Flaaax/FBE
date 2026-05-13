using System.Reflection;
using FBE.Scripts.Enchantments;
using FBE.Scripts.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(PowerModel))]
public static class PatchIconPath1
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(PowerModel), "PackedIconPath");
    }

    public static bool Prefix(ref string __result, PowerModel __instance)
    {
        if (__instance is not FBEPowerModel power)
        {
            return true;
        }

        if (power.CustomIconPath == null) return true;
        __result = power.CustomIconPath;
        return false;

    }
}

[HarmonyPatch(typeof(PowerModel))]
public static class PatchIconPath2
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(PowerModel), "BigIconPath");
    }

    public static bool Prefix(ref string __result, PowerModel __instance)
    {
        if (__instance is not FBEPowerModel power)
        {
            return true;
        }

        if (power.CustomIconPath == null) return true;
        __result = power.CustomIconPath;
        return false;

    }
}

[HarmonyPatch(typeof(EnchantmentModel))]
public static class PatchIconPath3
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(EnchantmentModel), "IntendedIconPath");
    }
    
    public static bool Prefix(ref string __result, EnchantmentModel __instance)
    {
        if (__instance is not FBEEnchantmentModel enchantment)
        {
            return true;
        }

        if (enchantment.CustomIconPath == null) return true;
        __result = enchantment.CustomIconPath;
        return false;

    }
}