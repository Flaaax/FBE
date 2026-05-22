using System.Reflection;
using FBE.Scripts;
using FBE.Scripts.Events;
using FBE.Scripts.Relics;
using FBE.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(SfxCmd), nameof(SfxCmd.Play))]
[HarmonyPatch([typeof(string), typeof(float)])]
public static class CustomSfxPatch1
{
    public static bool Prefix(string sfx, float volume)
    {
        if (NonInteractiveMode.IsActive || CombatManager.Instance.IsEnding)
            return true;
        if (!sfx.StartsWith("res://")) return true;
        AudioHelper.Play(sfx, volume);
        return false;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
public static class CustomSfxPatch2
{
    public static void Postfix(IRunState runState, ICombatState? combatState, Creature creature,
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.IsPlayer)
        {
            AudioHelper.PlayRandom("res://FBE/audio/isaac dies new0.wav");
        }
    }
}

[HarmonyPatch(typeof(TheBombPower), nameof(TheBombPower.BeforeSideTurnEnd))]
public static class TheBombPowerSfxPatch
{
    public static void Prefix(TheBombPower __instance, PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (__instance.Applier?.Player == null || !__instance.Applier.Player.Relics.Any(r => r is TopHat))
            return;

        if (!participants.Contains(__instance.Owner) || __instance.Amount > 1)
            return;

        TaskHelper.RunSafely(PlayDelayed());
    }

    private static async Task PlayDelayed()
    {
        await Cmd.CustomScaledWait(0.4f, 0.8f);
        AudioHelper.PlayRandom("res://FBE/audio/boss explosions 0.wav", 0.8f);
    }
}