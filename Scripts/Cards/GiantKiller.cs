using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace FBE.Scripts.Cards;

[Pool(typeof(ColorlessCardPool))]
public class GiantKiller() : FBECardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(4m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcMultiplier),
        new IntVar("Multiplier", 8m),
        new IntVar("PlayerCount", GetPlayerCount())
    ];


    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(e => e.CurrentHp > ModifiedHealth) ?? false;

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    private int ModifiedHealth => IsMutable && Owner is not null ? Owner.Creature.CurrentHp * GetPlayerCount() : 0;

    private static int GetPlayerCount() => RunManager.Instance.DebugOnlyGetState()?.Players.Count ?? 1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        AudioHelper.Play("res://FBE/audio/giant_killer.ogg");
        var sfx = cardPlay.Target.CurrentHp > ModifiedHealth
            ? "res://FBE/audio/hit_crit.ogg"
            : null;
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", sfx, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(3m);
    }

    private static decimal CalcMultiplier(CardModel card, Creature? target)
    {
        return target != null && target.CurrentHp > ((GiantKiller)card).ModifiedHealth
            ? card.DynamicVars["Multiplier"].BaseValue - 1
            : 0;
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("ModifiedHealth", ModifiedHealth);
    }
}