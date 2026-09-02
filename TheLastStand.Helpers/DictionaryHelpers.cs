using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Helpers;

public static class DictionaryHelpers
{
	public static T GetRandomItemFromWeights<T>(Dictionary<T, int> dictionary, object caller)
	{
		int num = 0;
		foreach (KeyValuePair<T, int> item in dictionary)
		{
			num += item.Value;
		}
		int randomRange = RandomManager.GetRandomRange(caller, 0, num);
		foreach (KeyValuePair<T, int> item2 in dictionary)
		{
			num -= item2.Value;
			if (randomRange >= num)
			{
				return item2.Key;
			}
		}
		CLoggerManager.Log("Something went wrong with the function GetRandomItemFromWeights. The algorithm was unable to find an item, returning default value. This should never happen !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "DictionaryHelpers");
		return default(T);
	}

	public static T GetRandomItemFromWeights<T>(Dictionary<T, float> dictionary, object caller)
	{
		if (dictionary.Count == 0)
		{
			return default(T);
		}
		float num = 0f;
		foreach (KeyValuePair<T, float> item in dictionary)
		{
			num += item.Value;
		}
		float randomRange = RandomManager.GetRandomRange(caller, 0f, num);
		foreach (KeyValuePair<T, float> item2 in dictionary)
		{
			num -= item2.Value;
			if (randomRange >= num)
			{
				return item2.Key;
			}
		}
		CLoggerManager.Log("Something went wrong with the function GetRandomItemFromWeights. The algorithm was unable to find an item, returning default value. This should never happen !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "DictionaryHelpers");
		return default(T);
	}
}
