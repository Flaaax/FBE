using System.Reflection;
using FBE.Scripts.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(Vakuu))]
static class PatchVakuuPool1
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Vakuu), "Pool1");
    }

    static void Postfix(Vakuu __instance, ref IEnumerable<EventOption> __result)
    {
        __result = __result.Append(PatchVakuuRelicOptions.Create<Diplopia>(__instance));
    }
}

[HarmonyPatch(typeof(Vakuu))]
static class PatchVakuuPool3
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Vakuu), "Pool3");
    }

    static void Postfix(ref IEnumerable<EventOption> __result)
    {
        var lordsParasolId = ModelDb.Relic<LordsParasol>().Id;
        __result = __result.Where(option => option.Relic?.Id != lordsParasolId);
    }
}

static class PatchVakuuRelicOptions
{
    private static readonly MethodInfo RelicOptionMethod =
        AccessTools.Method(typeof(AncientEventModel), "RelicOption",
            [typeof(RelicModel), typeof(string), typeof(string)])
        ?? throw new MissingMethodException(typeof(AncientEventModel).FullName, "RelicOption");

    public static EventOption Create<T>(AncientEventModel ancient) where T : RelicModel
    {
        var relic = ModelDb.Relic<T>().ToMutable();
        var option = RelicOptionMethod.Invoke(ancient, [relic, "INITIAL", null]);
        return (EventOption?)option
               ?? throw new MissingMethodException(typeof(AncientEventModel).FullName, "RelicOption");
    }
}
