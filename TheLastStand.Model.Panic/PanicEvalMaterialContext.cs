using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.Model.Panic;

public class PanicEvalMaterialContext
{
	public int RewardValue
	{
		get
		{
			int num = TPSingleton<GameManager>.Instance.Game.DayNumber + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.PanicRewardResourcesOffset;
			if (num <= TPSingleton<PanicManager>.Instance.MaterialValues.Count)
			{
				return TPSingleton<PanicManager>.Instance.MaterialValues[Mathf.Max(0, num - 1)];
			}
			TPSingleton<PanicManager>.Instance.LogWarning($"Trying to get value for the panic reward (material) that is above the last index ! Index : {num}", CLogLevel.MAJOR);
			return TPSingleton<PanicManager>.Instance.MaterialValues[^1];
		}
	}
}
