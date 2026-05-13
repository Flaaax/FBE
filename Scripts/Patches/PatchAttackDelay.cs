using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Cards;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.WithAttackerAnim))]
public class PatchAttackDelay
{
    private static readonly FieldInfo HitCount = typeof(AttackCommand).GetField(
        "_hitCount",
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private static readonly FieldInfo Delay = typeof(AttackCommand).GetField(
        "_attackerAnimDelay",
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    public static void Postfix(AttackCommand __instance, string? animName, float delay, Creature? visualAttacker = null)
    {
        if (__instance.ModelSource is SovereignBlade && (int)HitCount.GetValue(__instance)! > 1)
        {
            Delay.SetValue(__instance, delay / 2.0f);
        }
    }
}