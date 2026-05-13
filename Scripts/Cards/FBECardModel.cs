using System.Reflection;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts.Cards;

// ReSharper disable once InconsistentNaming
public abstract class FBECardModel : CardModel, ICustomModel
{
    protected FBECardModel(int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary = true) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        ICustomModel.AddModel(GetType());
    }

    public override string PortraitPath => $"res://FBE/images/cards/{GetType().Name}.png";
    
}