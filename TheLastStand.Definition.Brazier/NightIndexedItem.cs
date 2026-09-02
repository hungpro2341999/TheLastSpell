using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Brazier;

public abstract class NightIndexedItem
{
	public int NightIndex;

	public static T GetLastNightIndexedItem<T>(List<T> nightIndexedItems) where T : NightIndexedItem
	{
		int i = 0;
		T result = nightIndexedItems[0];
		for (; i < nightIndexedItems.Count && TPSingleton<GameManager>.Instance.Game.DayNumber >= nightIndexedItems[i].NightIndex; i++)
		{
			result = nightIndexedItems[i];
		}
		return result;
	}

	public virtual void Init(int nightIndex, XElement xElement)
	{
		NightIndex = nightIndex;
	}
}
