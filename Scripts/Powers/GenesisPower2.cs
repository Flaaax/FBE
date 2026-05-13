using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;

namespace FBE.Scripts.Powers;

public sealed class GenesisPower2 : FBEPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomIconPath => ImageHelper.GetImagePath($"powers/{"GENESIS_POWER".ToLowerInvariant()}.png");

    public override async Task AfterEnergyReset(Player player)
    {
        if (player == Owner.Player)
        {
            Flash();
            await PlayerCmd.GainStars(Amount, player);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        Flash();
        var player = Owner.Player!;
        var stars = player.PlayerCombatState!.Stars;
        await PlayerCmd.LoseStars(stars, player);
    }
}