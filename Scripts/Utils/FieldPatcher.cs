using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;

namespace FBE.Scripts.Utils;

public static class FieldPatcher
{
    public static void Set<T, TValue>(T instance, string propertyName, TValue value)
    {
        var backingField = $"<{propertyName}>k__BackingField";

        var field = AccessTools.Field(typeof(T), backingField);
        if (field == null)
        {
            Log.Warn($"Failed to set property {propertyName} of type {typeof(T)}");
            return;
        }

        field.SetValue(instance, value);
    }
    
    public static TValue? Get<T, TValue>(T instance, string propertyName)
    {
        var backingField = $"<{propertyName}>k__BackingField";

        var field = AccessTools.Field(typeof(T), backingField);
        if (field == null)
            throw new MissingFieldException(typeof(T).FullName, backingField);

        return (TValue?)field.GetValue(instance);
    }
}