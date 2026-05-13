using System.Diagnostics;
using System.Reflection;
using FBE.Scripts.Relics;
using FBE.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

// ReSharper disable InconsistentNaming

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(SunkenStatue))]
static class PatchSunkenStatueCanonicalVars
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(SunkenStatue), "CanonicalVars");
    }

    static bool Prefix(ref IEnumerable<DynamicVar> __result)
    {
        __result =
        [
            new StringVar("Relic", ModelDb.Relic<SwordOfStoneMk2>().Title.GetFormattedText()),
            new GoldVar(111),
            new DynamicVar("HpLoss", 7m)
        ];
        return false;
    }
}

[HarmonyPatch(typeof(SunkenStatue), "GenerateInitialOptions")]
static class PatchSunkenStatueGenerateInitialOptions
{
    static bool Prefix(SunkenStatue __instance, ref IReadOnlyList<EventOption> __result)
    {
        var grabSword = AccessTools.MethodDelegate<Func<Task>>(
            AccessTools.Method(typeof(SunkenStatue), "GrabSword"),
            __instance
        );

        var diveIntoWater = AccessTools.MethodDelegate<Func<Task>>(
            AccessTools.Method(typeof(SunkenStatue), "DiveIntoWater"),
            __instance
        );

        __result =
        [
            new EventOption(
                __instance,
                grabSword,
                "SUNKEN_STATUE.pages.INITIAL.options.GRAB_SWORD",
                HoverTipFactory.FromRelic<SwordOfStoneMk2>()
            ),
            new EventOption(
                __instance,
                diveIntoWater,
                "SUNKEN_STATUE.pages.INITIAL.options.DIVE_INTO_WATER"
            ).ThatDoesDamage(__instance.DynamicVars["HpLoss"].BaseValue)
        ];

        return false;
    }
}

[HarmonyPatch(typeof(SunkenStatue), "GrabSword")]
static class PatchSunkenStatueGrabSword
{
    // ReSharper disable once RedundantAssignment
    static bool Prefix(SunkenStatue __instance, ref Task __result)
    {
        __result = PatchHelper.WrapAsync(Task);
        return false;

        async Task Task()
        {
            Debug.Assert(__instance.Owner != null, "__instance.Owner != null");
            await RelicCmd.Obtain<SwordOfStoneMk2>(__instance.Owner);

            var type = typeof(SunkenStatue);

            // 调用 protected string L10NLookup(string key)
            var l10NLookupMethod = type.GetMethod("L10NLookup", BindingFlags.Instance | BindingFlags.NonPublic);

            if (l10NLookupMethod == null) throw new MissingMethodException(type.FullName, "L10NLookup");

            var _description =
                (LocString?)l10NLookupMethod.Invoke(__instance, ["SUNKEN_STATUE.pages.GRAB_SWORD.description"]);
            var description = _description ?? throw new MissingMethodException(type.FullName, "L10NLookup");

            // 调用 protected void SetEventFinished(string text)
            var setEventFinishedMethod =
                type.GetMethod("SetEventFinished", BindingFlags.Instance | BindingFlags.NonPublic);

            if (setEventFinishedMethod == null) throw new MissingMethodException(type.FullName, "SetEventFinished");

            setEventFinishedMethod.Invoke(__instance, [description]);
        }
    }
}