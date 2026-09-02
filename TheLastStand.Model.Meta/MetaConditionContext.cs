using System;
using System.Collections.Generic;

namespace TheLastStand.Model.Meta;

[Serializable]
public class MetaConditionContext
{
	public static readonly IReadOnlyList<Type> SupportedNumberTypes = new List<Type>
	{
		typeof(double),
		typeof(int),
		typeof(short),
		typeof(sbyte),
		typeof(uint),
		typeof(ushort),
		typeof(byte),
		typeof(float),
		typeof(decimal),
		typeof(decimal)
	};
}
