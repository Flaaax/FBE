using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Unlocks;

namespace FBE.Scripts.Patches;

[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
public static class PatchRemoveCardsFromPools
{
    private static readonly Dictionary<Type, HashSet<Type>> RemovedCards = new()
    {
        { typeof(NecrobinderCardPool), [typeof(Afterlife), typeof(SentryMode)] },
        { typeof(RegentCardPool), [typeof(Genesis)] }
    };

    static void Postfix(CardPoolModel __instance, ref IEnumerable<CardModel> __result)
    {
        if (!RemovedCards.TryGetValue(__instance.GetType(), out var removed))
            return;

        __result = __result.Where(card => !removed.Contains(card.GetType())).ToList();
    }
}