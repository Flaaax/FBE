using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace FBE.Scripts.Relics;

[Pool(typeof(EventRelicPool))]
class SwordOfStoneMk2 : FBERelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override string CustomIconPath => "res://FBE/images/relics/sword_of_stone.png";
    public override int DisplayAmount => Math.Max(DynamicVars["HealthToLose"].IntValue - HealthLost, 0);
    public bool Complete => DisplayAmount == 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("HealthToLose", 120m)];

    private int _healthLost;

    [SavedProperty]
    public int HealthLost
    {
        get => _healthLost;
        private set
        {
            AssertMutable();
            _healthLost = value;
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature || Complete || result.UnblockedDamage == 0)
        {
            return;
        }

        Flash();
        HealthLost += result.UnblockedDamage;
        Status = Complete ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();

        if (!CombatManager.Instance.IsInProgress && Complete)
        {
            await DoReplace();
        }
    }


    private async Task DoReplace()
    {
        Flash();
        await RelicCmd.Replace(this, ModelDb.Relic<SwordOfJade>().ToMutable());
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Complete)
        {
            await DoReplace();
        }
    }
}