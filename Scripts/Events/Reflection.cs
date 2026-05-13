using System.Diagnostics;
using FBE.Scripts.Cards;
using FBE.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace FBE.Scripts.Events;

public sealed class Reflection : FBEEventModel
{
    // 背景图位置
    public override string CustomInitialPortraitPath => "res://FBE/images/events/Reflection.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("CardDeleteCount", 1m),
        new StringVar("RelicTitle", ModelDb.Relic<Insight>().Title.GetFormattedText())

    ];

    // 什么时候会遇到。这里的条件是所有玩家的金币都大于等于60
    public override bool IsAllowed(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(TryToRecall,
            HoverTipFactory.FromCardWithCardHoverTips<Years>()
                .Concat(HoverTipFactory.FromRelic<Insight>())),
        Option(StopThinking)
    ];

    private async Task TryToRecall()
    {
        await RelicCmd.Obtain<Insight>(Owner!);

        await CardPileCmd.AddCursesToDeck([ModelDb.Card<Years>()], Owner!);

        SetEventFinished(PageDescription("TRY_TO_RECALL_CHOSEN"));
    }

    private async Task StopThinking()
    {
        Debug.Assert(Owner != null, nameof(Owner) + " != null");
        var cards = (await CardSelectCmd.FromDeckForRemoval(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt,
                    DynamicVars["CardDeleteCount"].IntValue), player: Owner,
                filter: null))
            .ToList();
        await CardPileCmd.RemoveFromDeck(cards);
        SetEventFinished(PageDescription("STOP_THINKING_CHOSEN"));
    }
}