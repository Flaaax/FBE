using FBE.Scripts.Nodes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace FBE.Scripts.Utils;

internal static class CardSelectHelper
{
	public static async Task<IReadOnlyList<IReadOnlyList<CardModel>>> FromChooseBundlesScreen(
		Player player,
		IReadOnlyList<IReadOnlyList<CardModel>> bundles,
		int amount)
	{
		if (CombatManager.Instance.IsEnding || bundles.Count == 0)
			return [];

		if (amount <= 0 || amount > bundles.Count)
			throw new ArgumentOutOfRangeException(nameof(amount));

		var choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
		IReadOnlyList<int> indexes;

		if (TestMode.IsOn)
		{
			indexes = Enumerable.Range(0, amount).ToList();
		}
		else if (ShouldSelectLocally(player))
		{
			var screen = NChooseBundlesSelectionScreen.ShowScreen(bundles, amount);
			indexes = await screen.BundlesSelected();
			RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(
				player,
				choiceId,
				PlayerChoiceResult.FromIndexes(indexes.ToList()));
		}
		else
		{
			indexes = (await RunManager.Instance.PlayerChoiceSynchronizer
				.WaitForRemoteChoice(player, choiceId))
				.AsIndexes();
		}

		ValidateIndexes(indexes, bundles.Count, amount);
		return indexes.Select(index => bundles[index]).ToList();
	}

	private static bool ShouldSelectLocally(Player player)
	{
		return LocalContext.IsMe(player) &&
		       RunManager.Instance.NetService.Type != NetGameType.Replay;
	}

	private static void ValidateIndexes(IReadOnlyList<int> indexes, int bundleCount, int amount)
	{
		if (indexes.Count != amount ||
		    indexes.Distinct().Count() != amount ||
		    indexes.Any(index => index < 0 || index >= bundleCount))
		{
			throw new InvalidOperationException("Received an invalid card bundle selection.");
		}
	}
}
