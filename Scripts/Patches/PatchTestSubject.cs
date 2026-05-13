using System.Reflection;
using FBE.Scripts.Powers;
using FBE.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(EnragePower), nameof(EnragePower.AfterCardPlayed))]
public static class PatchEnragePower1
{
    public static bool Prefix(EnragePower __instance, ref Task __result, PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        __result = PatchHelper.WrapAsync(Task);
        return false;

        async Task Task()
        {
            if (cardPlay.Card.Type == CardType.Skill)
            {
                await Cmd.Wait(0.5f);
                var playerCount = __instance.CombatState.PlayerCreatures.Count;
                var p = 1.0 / playerCount;
                if (__instance.Owner.Monster!.Rng.NextFloat() <= p)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, __instance.Owner, __instance.Amount,
                        __instance.Owner, null);
                }
            }
        }
    }
}


[HarmonyPatch(typeof(EnragePower))]
static class PatchEnragePower2
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(EnragePower), "CanonicalVars");
    }

    static bool Prefix(ref IEnumerable<DynamicVar> __result, EnragePower __instance)
    {
        var playerCount = RunManager.Instance.DebugOnlyGetState()!.Players.Count;
        var fraction = $"1/{playerCount}";

        __result =
        [
            new StringVar("Prob", fraction)
        ];
        return false;
    }
}