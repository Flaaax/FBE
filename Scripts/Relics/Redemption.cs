using System.Diagnostics;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;

namespace FBE.Scripts.Relics;

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

[Pool(typeof(EventRelicPool))]
class Redemption : FBERelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(30m)
    ];
    
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType == RoomType.Boss)
        {
            Flash();
            await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
        }
    }
}