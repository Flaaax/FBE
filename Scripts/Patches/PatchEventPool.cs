using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(EventModel), nameof(EventModel.IsAllowed))]
internal static class PatchLostWispEvent
{
    private static bool Prefix(EventModel __instance, ref bool __result)
    {
        if (__instance is not LostWisp)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
