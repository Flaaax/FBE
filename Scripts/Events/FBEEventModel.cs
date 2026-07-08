using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace FBE.Scripts.Events;

public abstract class FBEEventModel : EventModel, IFBEModel
{
    protected FBEEventModel()
    {
        IFBEModel.AddEvent(this);
    }
    
    protected bool IsLocalOwner()
    {
        return LocalContext.IsMe(Owner);
    }
    
    protected void SetLocalPortrait(string portraitPath)
    {
        if (!IsLocalOwner())
        {
            return;
        }

        NEventRoom.Instance?.SetPortrait(PreloadManager.Cache.GetTexture2D(portraitPath));
    }
    
    public virtual string? CustomInitialPortraitPath => null;
    public virtual string? CustomBackgroundScenePath => null;
    public virtual string? CustomVfxPath => null;

    protected EventOption Option(Func<Task> onChosen, IEnumerable<IHoverTip>? tips = null, string pageKey = "INITIAL")
    {
        var method = onChosen.Method;
        if (method.IsSpecialName)
            Log.Warn(
                "Method passed as delegate to CustomEventModel.Option has special name; recommended to directly pass declared method or provide an explicit title and description LocString.");
        var txt = method.Name;

        return new EventOption(this, onChosen,
            $"{Id.Entry}.pages.{pageKey}.options.{StringHelper.Slugify(txt)}", tips ?? []);
    }
    
    protected LocString PageDescription(string pageKey)
    {
        return L10NLookup($"{Id.Entry}.pages.{pageKey}.description");
    }
}