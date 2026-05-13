using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models; // 如果你用了 Harmony 的 AccessTools

namespace FBE.Scripts.Utils;

public static class OrbHelper
{
    private static readonly MethodInfo EvokeMethod =
        AccessTools.Method(typeof(OrbCmd), "Evoke", [
            typeof(PlayerChoiceContext),
            typeof(Player),
            typeof(OrbModel),
            typeof(bool)
        ]);

    public static async Task EvokeAt(PlayerChoiceContext choiceContext, Player player, int index, bool dequeue = true)
    {
        var orbQueue = player.PlayerCombatState!.OrbQueue;
        if ((uint)index >= (uint)orbQueue.Orbs.Count)
        {
            return;
        }
        var orb = orbQueue.Orbs[index];
        choiceContext.PushModel(orb);
        await (Task)EvokeMethod.Invoke(null, [choiceContext, player, orb, dequeue])!;
        choiceContext.PopModel(orb);
    }
}