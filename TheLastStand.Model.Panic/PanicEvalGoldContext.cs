using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.Model.Panic;

public class PanicEvalGoldContext
{
	public int RewardValue
	{
		get
		{
			int num = TPSingleton<GameManager>.Instance.Game.DayNumber + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.PanicRewardResourcesOffset;
			if (num <= TPSingleton<PanicManager>.Instance.GoldValues.Count)
			{
				return TPSingleton<PanicManager>.Instance.GoldValues[Mathf.Max(0, num - 1)];
			}
			TPSingleton<PanicManager>.Instance.LogWarning($"Trying to get value for the panic reward (gold) that is above the last index ! Index : {num}", CLogLevel.MAJOR);
			return TPSingleton<PanicManager>.Instance.GoldValues[^1];
		}
	}
}
