using System.Reflection;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace FBE.Scripts;

public interface ICustomModel
{
    private static readonly HashSet<Type> RegisteredTypes = [];
    private static bool RegisterType(Type t) => RegisteredTypes.Add(t);
    public static readonly List<EventModel> Events = [];

    public static void AddModel(Type modelType)
    {
        if (!RegisterType(modelType))
            return;
        var poolAttribute = modelType.GetCustomAttribute<PoolAttribute>() ?? throw new Exception(
            $"Model {modelType.FullName} must be marked with a PoolAttribute to determine which pool to add it to.");
        // if (!CustomContentDictionary.IsValidPool(modelType, poolAttribute.PoolType))
        //     throw new Exception($"Model {modelType.FullName} is assigned to incorrect type of pool {poolAttribute.PoolType.FullName}.");
        //Does not check validity
        ModHelper.AddModelToPool(poolAttribute.PoolType, modelType);
    }

    public static void AddEvent(EventModel customEvent)
    {
        if (!RegisterType(customEvent.GetType()))
            return;
        Events.Add(customEvent);
    }

    public void Log(string msg)
    {
        var id = ((AbstractModel)this).Id.Entry;
        MegaCrit.Sts2.Core.Logging.Log.Info($"[{id}] {msg}");
    }
}