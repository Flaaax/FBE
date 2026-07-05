using System.Globalization;
using System.Reflection;
using System.Text.Json;
using MegaCrit.Sts2.Core.Modding;

namespace FBE.Scripts.Config;

public static class RitsuLibSettingsInterop
{
    public const string EnablePlaceholderOptionKey = "enablePlaceholderOption";
    public const string PlaceholderValueKey = "placeholderValue";

    private const string ResetActionKey = "resetConfig";

    public static object CreateRitsuLibSettingsSchema()
    {
        return new Dictionary<string, object?>
        {
            ["modId"] = Entry.ModId,
            ["modDisplayName"] = "FBE",
            ["pages"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["pageId"] = "main",
                    ["title"] = "FBE",
                    ["description"] =
                        "Optional settings exposed through RitsuLib. Values are stored in user://FBE/config.json.",
                    ["sections"] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["id"] = "general",
                            ["title"] = "General",
                            ["entries"] = new object[]
                            {
                                new Dictionary<string, object?>
                                {
                                    ["id"] = "enabled",
                                    ["type"] = "toggle",
                                    ["key"] = EnablePlaceholderOptionKey,
                                    ["label"] = "Enable placeholder option",
                                    ["description"] =
                                        "Placeholder setting for validating the optional RitsuLib configuration page.",
                                    ["defaultValue"] = true,
                                    ["scope"] = "global"
                                },
                                new Dictionary<string, object?>
                                {
                                    ["id"] = "value",
                                    ["type"] = "int-slider",
                                    ["key"] = PlaceholderValueKey,
                                    ["label"] = "Placeholder value",
                                    ["description"] = "This value is currently not connected to gameplay.",
                                    ["min"] = 0,
                                    ["max"] = 100,
                                    ["step"] = 5,
                                    ["defaultValue"] = 50,
                                    ["scope"] = "global"
                                },
                                new Dictionary<string, object?>
                                {
                                    ["id"] = "configPath",
                                    ["type"] = "info-card",
                                    ["label"] = "Config file",
                                    ["body"] = FBEConfig.ConfigFilePath
                                },
                                new Dictionary<string, object?>
                                {
                                    ["id"] = "reset",
                                    ["type"] = "button",
                                    ["key"] = ResetActionKey,
                                    ["label"] = "Reset config",
                                    ["buttonText"] = "Reset",
                                    ["tone"] = "accent"
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public static object? GetRitsuLibSettingValue(string key) => FBEConfig.GetValue(key);

    public static void SetRitsuLibSettingValue(string key, object? value) => FBEConfig.SetValue(key, value);

    public static bool GetRitsuLibSettingBool(string key)
    {
        return FBEConfig.GetValue(key) switch
        {
            bool value => value,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            var value => Convert.ToBoolean(value, CultureInfo.InvariantCulture)
        };
    }

    public static void SetRitsuLibSettingBool(string key, bool value) => FBEConfig.SetValue(key, value);

    public static int GetRitsuLibSettingInt(string key)
    {
        return FBEConfig.GetValue(key) switch
        {
            int value => value,
            JsonElement element when element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value) =>
                value,
            var value => Convert.ToInt32(value, CultureInfo.InvariantCulture)
        };
    }

    public static void SetRitsuLibSettingInt(string key, int value) => FBEConfig.SetValue(key, value);

    public static void SaveRitsuLibSettings() => FBEConfig.Save();

    public static void InvokeRitsuLibSettingAction(string key)
    {
        if (key == ResetActionKey)
            FBEConfig.Reset();
    }
}

internal static class OptionalRitsuLibIntegration
{
    private const string RitsuLibModId = "STS2-RitsuLib";
    private const string MinSupportedRitsuLibVersion = "0.4.51";

    private static bool _initialized;
    private static bool _loggedAvailable;
    private static bool _loggedMissing;
    private static bool _settingsRegistered;

    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        LogCurrentState();
        ModManager.OnModDetected += OnModDetected;
    }

    private static void OnModDetected(Mod mod)
    {
        if (IsRitsuLib(mod))
            LogRitsuLibState(mod);
    }

    private static void LogCurrentState()
    {
        var mod = ModManager.GetLoadedMods().FirstOrDefault(IsRitsuLib);
        if (mod != null)
        {
            LogRitsuLibState(mod);
            return;
        }

        if (_loggedMissing)
            return;

        _loggedMissing = true;
        Entry.Log.Info(
            $"Optional RitsuLib settings page is disabled because {RitsuLibModId} is not loaded. FBE config path: {FBEConfig.ConfigFilePath}");
    }

    private static void LogRitsuLibState(Mod mod)
    {
        var version = mod.manifest?.version;
        if (!IsVersionAtLeast(version, MinSupportedRitsuLibVersion))
        {
            Entry.Log.Warn(
                $"Detected {RitsuLibModId} {version ?? "<unknown>"}, but FBE settings require >= {MinSupportedRitsuLibVersion}.");
            return;
        }

        if (_loggedAvailable)
            return;

        _loggedAvailable = true;
        TryRegisterSettingsPage(mod);
    }

    private static void TryRegisterSettingsPage(Mod mod)
    {
        if (_settingsRegistered)
            return;

        var interopType = mod.assemblies
            .Select(assembly => assembly.GetType("STS2RitsuLib.Settings.ModSettingsRuntimeReflectionInteropMirror",
                false))
            .FirstOrDefault(type => type != null);

        if (interopType == null)
        {
            Entry.Log.Warn(
                $"Detected supported {RitsuLibModId}, but its runtime settings interop entry point was not found.");
            return;
        }

        var register = interopType.GetMethod(
            "RegisterProviderTypeAndTryRegister",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string), typeof(string)]);

        if (register == null)
        {
            Entry.Log.Warn(
                $"Detected supported {RitsuLibModId}, but its runtime settings interop registration method was not found.");
            return;
        }

        try
        {
            var providerType = typeof(RitsuLibSettingsInterop);
            var registered = Convert.ToInt32(register.Invoke(null,
                [providerType.FullName, providerType.Assembly.GetName().Name]), CultureInfo.InvariantCulture);

            _settingsRegistered = registered > 0;
            Entry.Log.Info(_settingsRegistered
                ? $"Detected {RitsuLibModId} {mod.manifest?.version}; registered FBE settings page through RitsuLib runtime interop."
                : $"Detected {RitsuLibModId} {mod.manifest?.version}; FBE settings page was already registered or not accepted by RitsuLib.");
        }
        catch (Exception ex)
        {
            Entry.Log.Warn(
                $"Failed to register FBE settings page through {RitsuLibModId}. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool IsRitsuLib(Mod mod)
    {
        return string.Equals(mod.manifest?.id, RitsuLibModId, StringComparison.OrdinalIgnoreCase)
               && mod.state == ModLoadState.Loaded;
    }

    private static bool IsVersionAtLeast(string? actual, string minimum)
    {
        return TryParseVersion(actual, out var actualVersion)
               && TryParseVersion(minimum, out var minimumVersion)
               && CompareVersions(actualVersion, minimumVersion) >= 0;
    }

    private static bool TryParseVersion(string? value, out int[] version)
    {
        version = [0, 0, 0];
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
            normalized = normalized[1..];

        var suffixIndex = normalized.IndexOfAny(['-', '+']);
        if (suffixIndex >= 0)
            normalized = normalized[..suffixIndex];

        var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        for (var i = 0; i < Math.Min(parts.Length, version.Length); i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out version[i]))
                return false;
        }

        return true;
    }

    private static int CompareVersions(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
        for (var i = 0; i < Math.Min(left.Count, right.Count); i++)
        {
            var comparison = left[i].CompareTo(right[i]);
            if (comparison != 0)
                return comparison;
        }

        return left.Count.CompareTo(right.Count);
    }
}
