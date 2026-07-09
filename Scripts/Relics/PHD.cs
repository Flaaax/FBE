using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace FBE.Scripts.Relics;

[Pool(typeof(SharedRelicPool))]
class PHD : FBERelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];


	public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
	{
		if (potion.Owner == Owner && CombatManager.Instance.IsInProgress)
		{
			Flash();
			await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
		}
	}
}