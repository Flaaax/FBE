using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace FBE.Scripts.Cards;

[Pool(typeof(TokenCardPool))]
public class TheD6ChoiceCard() : FBECardModel(-1, CardType.None, CardRarity.Token, TargetType.None)
{
    private string? _runtimeTitle;
    private string? _runtimeProperty;
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;
    protected override Type PortraitOverride => typeof(TheD6);

    public int Index { get; private set; }
    private int _value;

    public void Init(string property, int value, int index)
    {
        AssertMutable();
        _runtimeTitle = property;
        _runtimeProperty = property;
        Index = index;
        _value = value;
    }

    public override string Title => _runtimeTitle ?? base.Title;

    protected override void AddExtraArgsToDescription(LocString description)
    {
        if (_runtimeProperty != null)
        {
            description.Add("property", _runtimeProperty);
            description.Add("value", _value);
        }
    }
}

[Pool(typeof(ColorlessCardPool))]
public class TheD6() : FBECardModel(1, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    private LocString? _propertySelectionPrompt;
    private LocString? _handSelectionPrompt;

    private LocString HandSelectionPrompt =>
        _handSelectionPrompt ??= new LocString("cards", Id.Entry + ".handSelectionPrompt");

    private LocString PropertySelectionPrompt =>
        _propertySelectionPrompt ??= new LocString("cards", Id.Entry + ".propertySelectionPrompt");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Selections", 1)
    ];

    // name, value, modifier
    private List<(LocString, int, Action<int>)> GetModifiers(CardModel card)
    {
        List<(LocString, int, Action<int>)> ret = [];
        if (!card.EnergyCost.CostsX)
        {
            ret.Add((new LocString("cards", Id.Entry + ".modifier.energy"),
                card.EnergyCost.GetResolved(), // Could this work well?
                i => card.EnergyCost.SetThisCombat(i)));
        }

        foreach (var dynVar in card.DynamicVars.Values.Where(dynVar => dynVar is not StringVar))
        {
            // "{name}"
            var str = new LocString("cards", Id.Entry + ".modifier.general");
            str.Add("name", dynVar.Name);
            ret.Add((str, dynVar.IntValue, i => dynVar.BaseValue = i));
        }

        return ret;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(HandSelectionPrompt, 1),
            context: choiceContext, player: Owner,
            filter: null,
            source: this)).FirstOrDefault();

        if (card == null)
        {
            return;
        }

        var modifiers = GetModifiers(card);

        // 只能选三个项，但这可能会改...
        Owner.RunState.Rng.CombatCardSelection.Shuffle(modifiers);

        var selectionCount = IsUpgraded ? 999 : DynamicVars["Selections"].IntValue;
        modifiers = modifiers.Take(selectionCount).ToList();

        List<CardModel> selections = [];
        for (var i = 0; i < modifiers.Count; i++)
        {
            var (name, value, _) = modifiers[i];
            var card1 = CombatState!.CreateCard<TheD6ChoiceCard>(Owner);
            card1.Init(name.GetFormattedText(), value, i);
            selections.Add(card1);
        }

        var selected = (TheD6ChoiceCard?)(await CardSelectCmd.FromSimpleGrid(choiceContext, selections,
            Owner, new CardSelectorPrefs(PropertySelectionPrompt, 1))).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        var action = modifiers[selected.Index].Item3;
        var rollValue = Owner.RunState.Rng.CombatCardSelection.NextInt(1, 6 + 1); // Maybe this range could change too?
        action(rollValue);
    }

    protected override void OnUpgrade()
    {
        //DynamicVars["StaticDischargePower"].UpgradeValueBy(1m);
    }
}