using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Utils;

public class CardPileHelper
{
    public static async Task AutoPlayFromHand(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        await CardPileCmd.Add(card, PileType.Play);

        if (!card.Owner.Creature.IsDead)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
}