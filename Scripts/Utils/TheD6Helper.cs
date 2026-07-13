using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

namespace FBE.Scripts.Utils;

class TheD6Helper
{
	private static readonly MethodInfo ModelDbPowerMethod = typeof(ModelDb)
		.GetMethods(BindingFlags.Public | BindingFlags.Static)
		.Single(method => method.Name == nameof(ModelDb.Power)
		                  && method.IsGenericMethodDefinition
		                  && method.GetParameters().Length == 0);

	public static bool TryGetPowerVar(DynamicVar obj, out Type? t)
	{
		t = null;

		for (var type = obj.GetType(); type != null; type = type.BaseType)
		{
			switch (type.IsGenericType)
			{
				case true when
					type.GetGenericTypeDefinition() == typeof(PowerVar<>):
					t = type.GetGenericArguments()[0];
					return true;
			}
		}

		return false;
	}

	public static PowerModel GetPower(Type powerType)
	{
		return (PowerModel)ModelDbPowerMethod
			.MakeGenericMethod(powerType)
			.Invoke(null, null)!;
	}
}
