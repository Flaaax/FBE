using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.GetEntry))]
public static class PatchGetEntry
{
    public static void Postfix(ref string __result, Type type)
    {
        if (type.IsAssignableTo(typeof(ICustomModel)))
        {
            __result = "FBE-" + __result;
        }
    }
}