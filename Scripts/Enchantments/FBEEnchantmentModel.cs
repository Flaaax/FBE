using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Enchantments;

// ReSharper disable once InconsistentNaming
public abstract class FBEEnchantmentModel : EnchantmentModel, ICustomModel
{
    public virtual string? CustomIconPath => $"res://FBE/images/enchantments/{GetType().Name}.png";
}