// using System.Reflection;
// using FBE.Scripts;
// using HarmonyLib;
// using MegaCrit.Sts2.Core.Models;
//
// namespace FBE.Scripts.Patches;
//
// [HarmonyPatch(typeof(ModelDb))]
// public static class PatchModelDb
// {
// 	public static MethodBase TargetMethod()
// 	{
// 		return AccessTools.PropertyGetter(typeof(ModelDb), "AllSharedAncients");
// 	}
//
// 	public static void Postfix(ref IEnumerable<AncientEventModel> __result)
// 	{
// 		__result = __result.Concat(IFBEModel.Ancients);
// 	}
// }
