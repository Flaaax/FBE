using System.Globalization;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace FBE.Scripts.Config;

public sealed class FBEConfigData
{
	public bool EnablePlaceholderOption { get; set; } = true;
	public int PlaceholderValue { get; set; } = 50;
}

public static class FBEConfig
{
	private const string DataKey = "settings";
	private const string ConfigFileName = "config.json";
	private const string SettingsTextTable = "settings_ui";

	private static bool _registered;

	private static readonly ModSettingsValueBinding<FBEConfigData, bool> EnablePlaceholderOptionBinding = new(
		Entry.ModId,
		DataKey,
		SaveScope.Global,
		static settings => settings.EnablePlaceholderOption,
		static (settings, value) => settings.EnablePlaceholderOption = value);

	private static readonly ModSettingsValueBinding<FBEConfigData, int> PlaceholderValueBinding = new(
		Entry.ModId,
		DataKey,
		SaveScope.Global,
		static settings => settings.PlaceholderValue,
		static (settings, value) => settings.PlaceholderValue = Math.Clamp(value, 0, 100));

	public static void RegisterSettingsPage()
	{
		if (_registered)
			return;

		ModDataStore.For(Entry.ModId).Register(
			key: DataKey,
			fileName: ConfigFileName,
			scope: SaveScope.Global,
			defaultFactory: static () => new FBEConfigData(),
			autoCreateIfMissing: true);

		RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
			.WithTitle(Text("FBE_SETTINGS_PAGE.title", "FBE 设置"))
			.WithModDisplayName(Text("FBE_SETTINGS_MOD_DISPLAY_NAME.title", "FBE"))
			.WithVisibleOnHostSurfaces(ModSettingsHostSurface.MainMenu | ModSettingsHostSurface.RunPause)
			.AddSection("placeholder", section => section
				.WithTitle(Text("FBE_SETTINGS_PLACEHOLDER_SECTION.title", "占位配置"))
				.AddToggle(
					"enablePlaceholderOption",
					Text("FBE_SETTINGS_ENABLE_PLACEHOLDER_OPTION.title", "启用占位选项"),
					EnablePlaceholderOptionBinding)
				.AddIntSlider(
					"placeholderValue",
					Text("FBE_SETTINGS_PLACEHOLDER_VALUE.title", "占位数值"),
					PlaceholderValueBinding,
					minValue: 0,
					maxValue: 100,
					step: 5,
					valueFormatter: static value => value.ToString(CultureInfo.InvariantCulture))));

		_registered = true;
	}

	private static ModSettingsText Text(string key, string fallback) =>
		ModSettingsText.LocString(SettingsTextTable, key, fallback);
}
