using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace FBE.Scripts;

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
[ModInitializer("Init")]
public class Entry
{
	public static Logger Log { get; } = new("FBE", LogType.Generic);
	public static bool EnableSyncDebugTracePatches { get; private set; }

	private static Harmony? _harmony;

	// 初始化函数
	public static void Init()
	{
		//允许Debug日志（会造成日志膨胀）
		EnableSyncDebugTracePatches = false;

		// 打patch（即修改游戏代码的功能）用
		// 传入参数随意，只要不和其他人撞车即可
		_harmony = new Harmony("STS2.FBE");
		_harmony.PatchAll();

		RegisterSavedPropertyModels();
		// 使得tscn可以加载自定义脚本
		//ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
		Log.Info("Mod initialized!");
	}
	
	private static void RegisterSavedPropertyModels()
	{
		const BindingFlags flags =
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic;

		foreach (var type in typeof(Entry).Assembly.GetTypes())
		{
			if (!type.IsClass || type.IsAbstract)
				continue;

			if (!typeof(ICustomModel).IsAssignableFrom(type))
				continue;

			var hasSavedProperty = type
				.GetProperties(flags)
				.Any(p => p.GetCustomAttribute<SavedPropertyAttribute>() != null);

			if (!hasSavedProperty)
				continue;

			SavedPropertiesTypeCache.InjectTypeIntoCache(type);
			Log.Info($"Registered SavedProperty model: {type.FullName}");
		}
	}
}

[HarmonyPatch(typeof(NPlayerHand), "SelectCardInSimpleMode")]
[HarmonyPatch([typeof(NHandCardHolder)])]
static class PatchSelectCardInSimpleMode //单选时跳过确认
{
	static void Postfix(NPlayerHand __instance)
	{
		var prefs = Traverse.Create(__instance).Field("_prefs").GetValue<CardSelectorPrefs>();

		if (prefs.MinSelect == 1 && prefs.MaxSelect == 1)
		{
			Traverse.Create(__instance).Method("CheckIfSelectionComplete").GetValue();
		}
	}
}

[HarmonyPatch(typeof(NPlayerHand), "SelectCardInUpgradeMode")]
[HarmonyPatch([typeof(NHandCardHolder)])]
static class PatchSelectCardInUpgradeMode //同上
{
	static void Postfix(NPlayerHand __instance)
	{
		var prefs = Traverse.Create(__instance).Field("_prefs").GetValue<CardSelectorPrefs>();

		if (prefs.MinSelect == 1 && prefs.MaxSelect == 1)
		{
			Traverse.Create(__instance).Method("CheckIfSelectionComplete").GetValue();
		}
	}
}
