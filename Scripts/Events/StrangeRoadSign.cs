using System.Diagnostics;
using FBE.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace FBE.Scripts.Events;

public sealed class StrangeRoadSign : FBEEventModel
{
    // 背景图位置
    public override string CustomInitialPortraitPath => "res://FBE/images/events/WierdGuidepost.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(0)
    ];

    public override void CalculateVars()
    {
        DynamicVars.Gold.BaseValue = Rng.NextInt(235, 265);
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(p => p.Deck.Cards.Count(IsValid) >= 2);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(GoAttack),
        Option(GoDefend, GetCardHoverTips()),
        Option(RemoveSign)
    ];

    private static IEnumerable<IHoverTip> GetCardHoverTips()
    {
        var h1 = HoverTipFactory.FromCardWithCardHoverTips<Barricade>();
        var h2 = HoverTipFactory.FromCardWithCardHoverTips<Entrench>();
        var h3 = HoverTipFactory.FromCardWithCardHoverTips<BodySlam>();
        return h1.Concat(h2).Concat(h3);
    }

    private async Task GoAttack()
    {
        Debug.Assert(Owner != null, nameof(Owner) + " != null");
        var cardsToRemove = (await CardSelectCmd.FromDeckForRemoval(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2), player: Owner,
            filter: IsValid)).ToList();

        var pools = ModelDb.AllCharacterCardPools.ToList();
        pools.Add(ModelDb.CardPool<ColorlessCardPool>());

        var options = CardCreationOptions.ForNonCombatWithUniformOdds(pools, c => c.Tags.Contains(CardTag.Strike));
        var cards = CardFactory.CreateForReward(Owner, 3, options).ToList();
        foreach (var item in await CardSelectCmd.FromSimpleGridForRewards(
                     prefs: new CardSelectorPrefs(L10NLookup("FBE-STRANGE_ROAD_SIGN.pages.GO_ATTACK.selectionScreenPrompt"),
                         1), context: new BlockingPlayerChoiceContext(), cards: cards, player: Owner))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
        }
        
        await CardPileCmd.RemoveFromDeck(cardsToRemove);

        // var options =
        //     CardCreationOptions.ForNonCombatWithDefaultOdds(pools, c => c.Type == CardType.Attack);
        // var cardModel = CardFactory.CreateForReward(Owner, 1, options).FirstOrDefault()?.Card;
        // if (cardModel != null)
        // {
        //     CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardModel, PileType.Deck), 1.2f,
        //         CardPreviewStyle.EventLayout);
        // }

        SetEventFinished(PageDescription("GO_ATTACK_CHOSEN"));
    }

    private async Task GoDefend()
    {
        CardModel[] cards =
        [
            Owner!.RunState.CreateCard<Barricade>(Owner),
            Owner.RunState.CreateCard<Entrench>(Owner),
            Owner.RunState.CreateCard<BodySlam>(Owner)
        ];

        foreach (var card in cards)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        }

        SetEventFinished(PageDescription("GO_DEFEND_CHOSEN"));
    }

    private async Task RemoveSign()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.IntValue, Owner!);
        await RelicCmd.Obtain<RemovedRoadSign>(Owner!);

        SetEventFinished(PageDescription("DESTROY_SIGN_CHOSEN"));
    }

    private static bool IsValid(CardModel card)
    {
        if (!card.GainsBlock) return false;
        return card is { Rarity: CardRarity.Basic, IsRemovable: true };
    }
}