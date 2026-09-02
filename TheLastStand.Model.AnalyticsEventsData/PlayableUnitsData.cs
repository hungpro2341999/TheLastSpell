using System;
using System.Collections.Generic;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.AnalyticsEventsData;

[Serializable]
public class PlayableUnitsData : List<DetailedPlayableUnitData>
{
	public PlayableUnitsData(List<PlayableUnit> playableUnits)
	{
		foreach (PlayableUnit playableUnit in playableUnits)
		{
			if (!playableUnit.IsDead)
			{
				Add(new DetailedPlayableUnitData(playableUnit));
			}
		}
	}
}
