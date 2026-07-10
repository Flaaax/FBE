using FBE.Scripts.Monsters;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace FBE.Scripts.Relics;

[Pool(typeof(EventRelicPool))]
class DarkBum : FBERelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		if (CombatManager.Instance.IsInProgress)
		{
			await SummonPet();
		}
	}
	
	public override async Task BeforeCombatStart()
	{
		await SummonPet();
	}

	private async Task SummonPet()
	{
		await PlayerCmd.AddPet<DarkBumMonster>(Owner);
	}
}