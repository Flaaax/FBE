using System.Globalization;
using System.Text.Json;
using Godot;
using GodotFileAccess = Godot.FileAccess;

namespace FBE.Scripts.Config;

public sealed class FBEConfigData
{
	public bool EnablePlaceholderOption { get; set; } = true;
	public int PlaceholderValue { get; set; } = 50;
}

public static class FBEConfig
{
	public const string ConfigDirectoryPath = "user://FBE";
	public const string ConfigFilePath = ConfigDirectoryPath + "/config.json";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		AllowTrailingCommas = true,
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true
	};

	private static bool _loaded;
	private static FBEConfigData _current = new();

	public static FBEConfigData Current
	{
		get
		{
			EnsureLoaded();
			return _current;
		}
	}

	public static void Load()
	{
		if (_loaded)
			return;

		_loaded = true;

		try
		{
			if (!GodotFileAccess.FileExists(ConfigFilePath))
			{
				_current = new FBEConfigData();
				Save();
				return;
			}

			using var file = GodotFileAccess.Open(ConfigFilePath, GodotFileAccess.ModeFlags.Read);
			if (file == null)
			{
				Entry.Log.Warn($"Could not open FBE config at {ConfigFilePath}; using defaults.");
				_current = new FBEConfigData();
				return;
			}

			var json = file.GetAsText();
			_current = JsonSerializer.Deserialize<FBEConfigData>(json, JsonOptions) ?? new FBEConfigData();
			Clamp();
		}
		catch (Exception ex)
		{
			_current = new FBEConfigData();
			Entry.Log.Warn($"Failed to load FBE config at {ConfigFilePath}; using defaults. {ex.GetType().Name}: {ex.Message}");
		}
	}

	public static void Save()
	{
		EnsureLoaded();
		Clamp();

		try
		{
			var error = DirAccess.MakeDirRecursiveAbsolute(ConfigDirectoryPath);
			if (error != Error.Ok && !DirAccess.DirExistsAbsolute(ConfigDirectoryPath))
			{
				Entry.Log.Warn($"Could not create FBE config directory {ConfigDirectoryPath}: {error}");
				return;
			}

			using var file = GodotFileAccess.Open(ConfigFilePath, GodotFileAccess.ModeFlags.Write);
			if (file == null)
			{
				Entry.Log.Warn($"Could not open FBE config at {ConfigFilePath} for writing.");
				return;
			}

			file.StoreString(JsonSerializer.Serialize(_current, JsonOptions));
		}
		catch (Exception ex)
		{
			Entry.Log.Warn($"Failed to save FBE config at {ConfigFilePath}. {ex.GetType().Name}: {ex.Message}");
		}
	}

	public static void Reset()
	{
		_current = new FBEConfigData();
		_loaded = true;
		Save();
	}

	public static object? GetValue(string key)
	{
		EnsureLoaded();
		return key switch
		{
			RitsuLibSettingsInterop.EnablePlaceholderOptionKey => _current.EnablePlaceholderOption,
			RitsuLibSettingsInterop.PlaceholderValueKey => _current.PlaceholderValue,
			_ => null
		};
	}

	public static void SetValue(string key, object? value)
	{
		EnsureLoaded();
		switch (key)
		{
			case RitsuLibSettingsInterop.EnablePlaceholderOptionKey:
				_current.EnablePlaceholderOption = CoerceBool(value, _current.EnablePlaceholderOption);
				break;
			case RitsuLibSettingsInterop.PlaceholderValueKey:
				_current.PlaceholderValue = CoerceInt(value, _current.PlaceholderValue);
				break;
			default:
				return;
		}

		Save();
	}

	private static void EnsureLoaded()
	{
		if (!_loaded)
			Load();
	}

	private static void Clamp()
	{
		_current.PlaceholderValue = Math.Clamp(_current.PlaceholderValue, 0, 100);
	}

	private static bool CoerceBool(object? value, bool fallback)
	{
		return value switch
		{
			bool b => b,
			JsonElement { ValueKind: JsonValueKind.True } => true,
			JsonElement { ValueKind: JsonValueKind.False } => false,
			string s when bool.TryParse(s, out var b) => b,
			string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i != 0,
			int i => i != 0,
			long i => i != 0,
			double d => Math.Abs(d) > double.Epsilon,
			_ => fallback
		};
	}

	private static int CoerceInt(object? value, int fallback)
	{
		return value switch
		{
			int i => i,
			long i => ClampToInt(i),
			double d => (int)Math.Round(d),
			float f => (int)Math.Round(f),
			decimal d => (int)Math.Round(d),
			JsonElement { ValueKind: JsonValueKind.Number } e when e.TryGetInt32(out var i) => i,
			JsonElement { ValueKind: JsonValueKind.Number } e when e.TryGetDouble(out var d) => (int)Math.Round(d),
			string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
			string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => (int)Math.Round(d),
			_ => fallback
		};
	}

	private static int ClampToInt(long value)
	{
		return value switch
		{
			> int.MaxValue => int.MaxValue,
			< int.MinValue => int.MinValue,
			_ => (int)value
		};
	}
}
