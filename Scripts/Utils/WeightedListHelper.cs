using MegaCrit.Sts2.Core.Random;

namespace FBE.Scripts.Utils;

public static class WeightedListHelper
{
	public static T NextItem<T>(this List<(T Item, decimal Weight)> entries, Rng rng, bool remove = false)
	{
		ArgumentNullException.ThrowIfNull(entries);
		ArgumentNullException.ThrowIfNull(rng);

		var totalWeight = 0m;
		foreach (var entry in entries)
		{
			if (entry.Weight < 0m)
			{
				throw new ArgumentOutOfRangeException(nameof(entries), entry.Weight,
					"Weight must be a non-negative value.");
			}

			totalWeight += entry.Weight;
		}

		if (entries.Count == 0 || totalWeight <= 0m)
		{
			throw new InvalidOperationException("Cannot roll from an empty weighted list.");
		}

		var roll = (decimal)rng.NextDouble() * totalWeight;
		var cumulative = 0m;

		for (var i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];
			cumulative += entry.Weight;
			if (roll >= cumulative)
			{
				continue;
			}

			if (remove)
			{
				entries.RemoveAt(i);
			}

			return entry.Item;
		}

		throw new InvalidOperationException($"Weighted roll {roll} exceeded total weight {totalWeight}.");
	}
}
