using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Relics;

public abstract class FBERelicModel : RelicModel, IFBEModel
{
    public virtual string? CustomIconPath => $"res://FBE/images/relics/{GetType().Name}.png";
    
    public override string PackedIconPath => CustomIconPath ?? base.PackedIconPath;

    // 轮廓图标（原版85x85）
    protected override string PackedIconOutlinePath => CustomIconPath ?? base.PackedIconPath;

    // 大图标（原版256x256）
    protected override string BigIconPath => CustomIconPath ?? base.PackedIconPath;
    
    protected FBERelicModel()
    {
        IFBEModel.AddModel(GetType());
    }
}