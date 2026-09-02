using System;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class GoalTargetTypeDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_TargetType
	{
		Undefined,
		PlayableUnit,
		EnemyUnit,
		Building,
		Tile,
		TileFlag,
		Itself
	}

	public static class Constants
	{
		public const string Occupied = "Occupied";

		public const string Empty = "Empty";
	}

	public GoalConditionDefinition[][] ConditionGroups { get; private set; }

	public E_TargetType TargetType { get; private set; }

	public bool? IsTileContentAccepted { get; private set; }

	public GoalTargetTypeDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (!Enum.TryParse<E_TargetType>(xElement.Name.LocalName, out var result))
		{
			Debug.LogError("Unknown TargetType " + xElement.Name.LocalName);
			return;
		}
		TargetType = result;
		switch (xElement.Attribute("MustBe")?.Value)
		{
		case "Occupied":
			IsTileContentAccepted = true;
			break;
		case "Empty":
			IsTileContentAccepted = false;
			break;
		default:
			IsTileContentAccepted = null;
			break;
		}
		XElement xElement2 = xElement.Element("TargetConditions");
		if (xElement2 == null)
		{
			return;
		}
		int num = xElement2.Elements("ConditionsGroup").Count();
		ConditionGroups = new GoalConditionDefinition[num][];
		int num2 = 0;
		foreach (XElement item in xElement2.Elements("ConditionsGroup"))
		{
			int num3 = item.Elements().Count();
			ConditionGroups[num2] = new GoalConditionDefinition[num3];
			XElement[] array = item.Elements().ToArray();
			for (int i = 0; i < num3; i++)
			{
				switch (array[i].Name.ToString())
				{
				case "DamageableCountInAoeCondition":
					ConditionGroups[num2][i] = new DamageableCountInAoeConditionDefinition(array[i]);
					break;
				case "ExcludeDamageableTypeInAoeCondition":
					ConditionGroups[num2][i] = new ExcludeDamageableTypeInAoeConditionDefinition(array[i]);
					break;
				case "FlagTagCondition":
					ConditionGroups[num2][i] = new FlagTagConditionDefinition(array[i]);
					break;
				case "GroundCategoryCondition":
					ConditionGroups[num2][i] = new GroundCategoryConditionDefinition(array[i]);
					break;
				case "TargetIdCondition":
					ConditionGroups[num2][i] = new TargetIdConditionDefinition(array[i]);
					break;
				case "TargetHasBuildingIdCondition":
					ConditionGroups[num2][i] = new TargetHasBuildingIdConditionDefinition(array[i]);
					break;
				case "TargetInRangeCondition":
					ConditionGroups[num2][i] = new TargetInRangeConditionDefinition(array[i]);
					break;
				case "TargetIsNotEliteCondition":
					ConditionGroups[num2][i] = new TargetIsNotEliteConditionDefinition(array[i]);
					break;
				case "TargetHealthCondition":
					ConditionGroups[num2][i] = new TargetHealthConditionDefinition(array[i]);
					break;
				case "TileHasHazardCondition":
					ConditionGroups[num2][i] = new TileHasHazardConditionDefinition(array[i]);
					break;
				}
			}
			num2++;
		}
	}
}
