using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace FBE.Scripts.Powers;

public class CoolantPower : FBEPowerModel
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string CustomIconPath => "res://FBE/images/powers/coolant_power.png";
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature) return;
        //await CreatureCmd.TriggerAnim(Owner, "Cast", player.Character.CastAnimDelay);
        for (var i = 0; i < Amount; i++)
        {
            await OrbCmd.Channel<FrostOrb>(choiceContext, player);
        }
    }
}