using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Powers;

public class RainbowPower : FBEPowerModel
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Debuff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override string CustomIconPath => "res://FBE/images/powers/RainbowPower.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            Flash();
            OrbCmd.RemoveSlots(Owner.Player, 1);
            await PowerCmd.Decrement(this);
        }
        //Log.Warn("This is message is not expected!");
    }
    
}