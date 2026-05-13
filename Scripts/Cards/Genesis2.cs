using FBE.Scripts.Powers;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace FBE.Scripts.Cards;

[Pool(typeof(RegentCardPool))]
public class Genesis2() : FBECardModel(2, CardType.Power, CardRarity.Rare, TargetType.None)
{
    public override string PortraitPath => ImageHelper.GetImagePath($"atlases/card_atlas.sprites/regent/genesis.tres");
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StarsPerTurn", 6)
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<GenesisPower2>(choiceContext, Owner.Creature, DynamicVars["StarsPerTurn"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
    
}