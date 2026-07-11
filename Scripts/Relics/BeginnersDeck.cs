using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace FBE.Scripts.Relics;

[Pool(typeof(SharedRelicPool))]
class BeginnersDeck : FBERelicModel
{
	private const int _clawBundleChancePercent = 1;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		var list = GenerateRandomBundles(base.Owner);
		List<IReadOnlyList<CardModel>> list2 = [];
		foreach (var item in list)
		{
			list2.Add(item.Select(c => Owner.RunState.CreateCard(c, Owner)).ToList());
		}

		foreach (var item2 in await CardSelectCmd.FromChooseABundleScreen(Owner, list2))
		{
			await CardPileCmd.Add(item2, PileType.Deck);
		}
	}

	/// <summary>
	/// Generates 2 random bundles for the player. Each bundle contains 2 commons and 1 uncommon.
	/// All 6 cards across both bundles are unique.
	/// For Defect, each bundle has a 1% chance to be 3x Claw instead.
	/// </summary>
	private static List<IReadOnlyList<CardModel>> GenerateRandomBundles(Player player)
	{
		var rewards = player.PlayerRng.Rewards;
		var cardPool = player.Character.CardPool;
		var options = CardCreationOptions
			.ForNonCombatWithUniformOdds([cardPool], c => c.Rarity == CardRarity.Common)
			.WithFlags(CardCreationFlags.NoRarityModification);
		options = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options);
		var options2 = CardCreationOptions
			.ForNonCombatWithUniformOdds([cardPool], c => c.Rarity == CardRarity.Uncommon)
			.WithFlags(CardCreationFlags.NoRarityModification);
		options2 = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options2);
		var source = options.GetPossibleCards(player).ToList();
		var source2 = options2.GetPossibleCards(player).ToList();
		var list = new List<IReadOnlyList<CardModel>>();
		var usedCardIds = new HashSet<ModelId>();
		for (var num = 0; num < 2; num++)
		{
			List<CardModel> list2 = [];
			var list3 = source.Where(c => !usedCardIds.Contains(c.Id)).ToList();
			for (var num2 = 0; num2 < 2; num2++)
			{
				var cardModel2 = rewards.NextItem(list3);
				list2.Add(cardModel2);
				usedCardIds.Add(cardModel2.Id);
				list3.Remove(cardModel2);
			}

			var items = source2.Where(c => !usedCardIds.Contains(c.Id)).ToList();
			var cardModel3 = rewards.NextItem(items);
			list2.Add(cardModel3);
			usedCardIds.Add(cardModel3.Id);
			list.Add(list2);
		}

		return list;
	}
}