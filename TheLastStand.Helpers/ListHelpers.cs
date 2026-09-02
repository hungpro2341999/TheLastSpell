using System;
using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Helpers;

public static class ListHelpers
{
	public static T GetRandomItemFromWeights<T>(List<Tuple<T, int>> list, object caller)
	{
		int num = 0;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			num += list[num2].Item2;
		}
		int randomRange = RandomManager.GetRandomRange(caller, 0, num);
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			num -= list[num3].Item2;
			if (randomRange >= num)
			{
				return list[num3].Item1;
			}
		}
		CLoggerManager.Log("Something went wrong with the function GetRandomItemFromWeights. The algorithm was unable to find an item, returning default value. This should never happen !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ListHelpers");
		return default(T);
	}

	public static T GetRandomItemFromWeights<T>(List<Tuple<T, float>> list, object caller)
	{
		float num = 0f;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			num += list[num2].Item2;
		}
		float randomRange = RandomManager.GetRandomRange(caller, 0f, num);
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			num -= list[num3].Item2;
			if (randomRange >= num)
			{
				return list[num3].Item1;
			}
		}
		CLoggerManager.Log("Something went wrong with the function GetRandomItemFromWeights. The algorithm was unable to find an item, returning default value. This should never happen !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ListHelpers");
		return default(T);
	}
}
