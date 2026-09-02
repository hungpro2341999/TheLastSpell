using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Unit.Perk;

public class PerksDescriptionTooltipDisplay : MonoBehaviour
{
	public static class Constants
	{
		public const string PerksDescriptionLocaKey = "CharacterSheet_PerkPointsDescription";
	}

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	private void Awake()
	{
		genericTooltipDisplayer.LocaKey = "CharacterSheet_PerkPointsDescription";
		genericTooltipDisplayer.LocalizationArguments = new object[1];
		genericTooltipDisplayer.LocalizationArguments[0] = GetLastPerkPointLevel();
	}

	private int GetLastPerkPointLevel()
	{
		int num = 0;
		foreach (KeyValuePair<int, int> item in PlayableUnitDatabase.PerksPointsPerLevel)
		{
			if (item.Key > num)
			{
				num = item.Key;
			}
		}
		return num;
	}
}
