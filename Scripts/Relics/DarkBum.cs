using FBE.Scripts.Monsters;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Utils;

namespace FBE.Scripts.Relics;

[Pool(typeof(EventRelicPool))]
class DarkBum : FBERelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new BlockVar(6m, ValueProp.Unpowered),
		new EnergyVar(1),
		new PowerVar<StrengthPower>(1),
		new PowerVar<DexterityPower>(1),
		new HealVar(5),
		new GoldVar(30)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	private bool _usedThisTurn;

	private bool UsedThisTurn
	{
		get => _usedThisTurn;
		set
		{
			AssertMutable();
			_usedThisTurn = value;
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner.Creature))
		{
			return Task.CompletedTask;
		}

		UsedThisTurn = false;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		UsedThisTurn = false;
		return Task.CompletedTask;
	}

	private WeightedList<Func<PlayerChoiceContext, Task>> Effects => new()
	{
		{ _ => PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner), 15 },
		{ _ => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null), 2 },
		{
			_ => PotionCmd.TryToProcure(
				PotionFactory.CreateRandomPotionInCombat(Owner, Owner.RunState.Rng.CombatPotionGeneration)
					.ToMutable(), Owner),
			Owner.PotionSlots.Any(p => p is null) ? 1 : 0
		},
		{ context => CardPileCmd.Draw(context, Owner), 2 },
		{
			context => PowerCmd.Apply<StrengthPower>(context, Owner.Creature,
				DynamicVars.Strength.BaseValue, Owner.Creature, null),
			2
		},
		{
			context => PowerCmd.Apply<DexterityPower>(context, Owner.Creature,
				DynamicVars.Dexterity.BaseValue, Owner.Creature, null),
			2
		},
		{
			_ => CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue),
			Owner.Creature.CurrentHp == Owner.Creature.MaxHp ? 0 : 1
		},
		{ _ => PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner), 1 }
	};

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

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card,
		bool isAutoPlay,
		ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		if (card.Owner != Owner || UsedThisTurn)
		{
			return (pileType, position);
		}

		return (PileType.Exhaust, position);
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != Owner || UsedThisTurn)
		{
			return;
		}

		UsedThisTurn = true;

		var effect = Effects.GetRandom(Owner.RunState.Rng.CombatOrbGeneration);

		Flash();

		await Cmd.Wait(0.25f);
		await effect(choiceContext);
	}
}