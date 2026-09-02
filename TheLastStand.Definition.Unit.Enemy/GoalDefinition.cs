using System;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;
using TheLastStand.Definition.Unit.Enemy.PositioningMethod;
using TheLastStand.Definition.Unit.Enemy.TargetingMethod;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class GoalDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_InterruptionCondition
	{
		None = 0,
		AdjacentToPlayableUnit = 1,
		AdjacentToEnemyUnit = 2,
		AdjacentToUnit = 3,
		AdjacentToBuildingExceptWallAndBarricade = 4,
		AdjacentToWall = 8,
		AdjacentToBarricade = 0x10,
		AdjacentToBuilding = 0x1C
	}

	public int Cooldown { get; private set; }

	public GoalTargetTypeDefinition[] GoalTargetTypeDefinitions { get; private set; }

	public string Id { get; private set; }

	public E_InterruptionCondition InterruptionCondition { get; private set; }

	public IBehaviorModel.E_GoalComputingStep GoalComputingStep { get; private set; } = IBehaviorModel.E_GoalComputingStep.DuringTurn;

	public TheLastStand.Definition.Unit.Enemy.PositioningMethod.PositioningMethod PositioningMethod { get; private set; }

	public GoalConditionDefinition[][] PostconditionGroups { get; private set; }

	public GoalConditionDefinition[][] PreconditionGroups { get; private set; }

	public string SkillId { get; private set; }

	public TargetingMethodsContainerDefinition TargetingMethodsContainer { get; private set; }

	public GoalDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("GoalComputingStep");
		if (xAttribute2 != null && Enum.TryParse<IBehaviorModel.E_GoalComputingStep>(xAttribute2.Value, out var result))
		{
			GoalComputingStep = result;
		}
		XElement xElement2 = xElement.Element("SkillId");
		if (xElement2 == null)
		{
			Debug.LogError("GoalDefinition must have SkillId");
			return;
		}
		XAttribute xAttribute3 = xElement2.Attribute("Value");
		if (xAttribute3 == null)
		{
			Debug.LogError("SkillId must have Value");
			return;
		}
		SkillId = xAttribute3.Value;
		XElement xElement3 = xElement.Element("Cooldown");
		if (xElement3 != null)
		{
			XAttribute xAttribute4 = xElement3.Attribute("Value");
			Cooldown = int.Parse(xAttribute4.Value);
		}
		XElement xElement4 = xElement.Element("Preconditions");
		if (xElement4 != null)
		{
			int num = 0;
			foreach (XElement item in xElement4.Elements("ConditionsGroup"))
			{
				_ = item;
				num++;
			}
			PreconditionGroups = new GoalConditionDefinition[num][];
			int num2 = 0;
			foreach (XElement item2 in xElement4.Elements("ConditionsGroup"))
			{
				int num3 = 0;
				foreach (XElement item3 in item2.Elements())
				{
					_ = item3;
					num3++;
				}
				PreconditionGroups[num2] = new GoalConditionDefinition[num3];
				int num4 = 0;
				XElement xElement5 = item2.Element("CasterHasStatusCondition");
				if (xElement5 != null)
				{
					PreconditionGroups[num2][num4++] = new CasterHasStatusConditionDefinition(xElement5);
				}
				XElement xElement6 = item2.Element("CasterHealthCondition");
				if (xElement6 != null)
				{
					PreconditionGroups[num2][num4++] = new CasterHealthConditionDefinition(xElement6);
				}
				XElement xElement7 = item2.Element("InterpretedTurnCondition");
				if (xElement7 != null)
				{
					PreconditionGroups[num2][num4++] = new InterpretedTurnCondition(xElement7);
				}
				XElement xElement8 = item2.Element("PlayableUnitCloseToCasterCondition");
				if (xElement8 != null)
				{
					PreconditionGroups[num2][num4++] = new PlayableUnitCloseToCasterConditionDefinition(xElement8);
				}
				XElement xElement9 = item2.Element("NotInFogCondition");
				if (xElement9 != null)
				{
					PreconditionGroups[num2][num4++] = new NotInFogCondition(xElement9);
				}
				XElement xElement10 = item2.Element("NotInAnyFogCondition");
				if (xElement10 != null)
				{
					PreconditionGroups[num2][num4++] = new NotInAnyFogCondition(xElement10);
				}
				XElement xElement11 = item2.Element("DamageableAroundCondition");
				if (xElement11 != null)
				{
					PreconditionGroups[num2][num4++] = new DamageableAroundConditionDefinition(xElement11);
				}
				XElement xElement12 = item2.Element("SkillProgressionFlagIsToggledCondition");
				if (xElement12 != null)
				{
					PreconditionGroups[num2][num4++] = new SkillProgressionFlagIsToggledConditionDefinition(xElement12);
				}
				XElement xElement13 = item2.Element("ApocalypseSkillProgressionFlagIsToggledCondition");
				if (xElement13 != null)
				{
					PreconditionGroups[num2][num4++] = new ApocalypseSkillProgressionFlagIsToggledConditionDefinition(xElement13);
				}
				num2++;
			}
		}
		XElement xElement14 = xElement.Element("TargetTypes");
		int num5 = 0;
		foreach (XElement item4 in xElement14.Elements())
		{
			_ = item4;
			num5++;
		}
		GoalTargetTypeDefinitions = new GoalTargetTypeDefinition[num5];
		int num6 = 0;
		foreach (XElement item5 in xElement14.Elements())
		{
			GoalTargetTypeDefinitions[num6++] = new GoalTargetTypeDefinition(item5);
		}
		XElement xElement15 = xElement.Element("Postconditions");
		if (xElement15 != null)
		{
			int num7 = 0;
			foreach (XElement item6 in xElement15.Elements("ConditionsGroup"))
			{
				_ = item6;
				num7++;
			}
			PostconditionGroups = new GoalConditionDefinition[num7][];
			int num8 = 0;
			foreach (XElement item7 in xElement15.Elements("ConditionsGroup"))
			{
				int num9 = 0;
				foreach (XElement item8 in item7.Elements())
				{
					_ = item8;
					num9++;
				}
				PostconditionGroups[num8] = new GoalConditionDefinition[num9];
				int num10 = 0;
				XElement xElement16 = item7.Element("TargetsCountCondition");
				if (xElement16 != null)
				{
					PostconditionGroups[num8][num10++] = new TargetsCountCondition(xElement16);
				}
				num8++;
			}
		}
		TargetingMethodsContainer = new TargetingMethodsContainerDefinition(xElement.Element("TargetingMethod"));
		XElement xElement17 = xElement.Element("PositioningMethod");
		if (xElement17 != null)
		{
			if (xElement17.Element("ClosestTile") != null)
			{
				PositioningMethod = new ClosestTilePositioningMethod();
			}
			if (xElement17.Element("FarthestTile") != null)
			{
				PositioningMethod = new FarthestTilePositioningMethod();
			}
			if (xElement17.Element("Standard") != null)
			{
				PositioningMethod = new StandardPositioningMethod();
			}
		}
		XElement xElement18 = xElement.Element("InterruptionCondition");
		if (xElement18 != null)
		{
			if (xElement18.Element("AdjacentToPlayableUnit") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToPlayableUnit;
			}
			if (xElement18.Element("AdjacentToEnemyUnit") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToEnemyUnit;
			}
			if (xElement18.Element("AdjacentToUnit") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToUnit;
			}
			if (xElement18.Element("AdjacentToBuildingExceptWallAndBarricade") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBuildingExceptWallAndBarricade;
			}
			if (xElement18.Element("AdjacentToWall") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToWall;
			}
			if (xElement18.Element("AdjacentToBarricade") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBarricade;
			}
			if (xElement18.Element("AdjacentToBuilding") != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBuilding;
			}
		}
	}
}
