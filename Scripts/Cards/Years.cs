using FBE.Scripts.Powers;
using FBE.Scripts.Utils;
using FBE.Scripts.VFX;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace FBE.Scripts.Cards;

[Utils.Pool(typeof(CurseCardPool))]
public class Years() : FBECardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.Self)
{
    //public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    public override int MaxUpgradeLevel => 0;

    protected override bool IsPlayable => _enabled;

    private bool HasBeenPlayedLastOrThisTurn =>
        CombatState != null &&
        CombatManager.Instance.History.CardPlaysFinished.Any(e =>
            e.CardPlay.Card is Years &&
            e.CardPlay.Card.Owner == Owner &&
            (e.HappenedThisTurn(CombatState) || e.HappenedLastPlayerTurn(Owner)));

    private bool _enabled;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ExtraTurnPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        PlayerCmd.EndTurn(Owner, canBackOut: false);

        AudioHelper.Play("res://FBE/audio/SOTE_SFX_EndTurn_v2.ogg");
        AudioHelper.Play("res://FBE/audio/STS_SFX_TimeWarp_v2.ogg");
        TimeWarpTurnEndVfx.Play();

        await Cmd.Wait(0.25f);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
            return;

        var c = CardPile.GetCards(player, PileType.Hand).FirstOrDefault(c => c == this);
        if (HasBeenPlayedLastOrThisTurn || c == null)
        {
            _enabled = false;
            return;
        }

        await Cmd.Wait(0.25f);
        _enabled = true;
        await CardCmd.AutoPlay(choiceContext, this, null);
        //await CardPileHelper.AutoPlayFromHand(choiceContext, this);
        _enabled = false;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        return card != this || _enabled;
    }
}
