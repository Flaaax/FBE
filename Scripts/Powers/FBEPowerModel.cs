using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Powers;

public abstract class FBEPowerModel : PowerModel, IFBEModel
{
    public virtual string? CustomIconPath => $"res://FBE/images/powers/{GetType().Name}.png";
}