using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

public class SkillDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Phase
	{
		None = 0,
		Production = 1,
		Deployment = 2,
		Night = 4,
		Day = 3,
		All = 7
	}

	public enum E_InvalidCastDisplayBehaviour
	{
		None,
		Hidden,
		DisplayedUnavailable
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string SkipTurn = "SkipTurn";

			public const string GargoyleSkipTurn = "GargoyleSkipTurn2";
		}

		public const char AreaOfEffectSymbol = 'X';

		public const char ManeuverEffectSymbol = 'M';

		public const char SurroundingEffectSymbol = 'e';

		public const char EmptyEffectSymbol = '_';
	}

	public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnits = AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.All;

	public int ActionPointsCost { get; private set; }

	public int AffectedTilesCount { get; private set; }

	public E_Phase AllowDuringPhase { get; private set; }

	public bool AllowFriendlyFire { get; private set; }

	public AreaOfEffectDefinition AreaOfEffectDefinition { get; private set; }

	public string ArtId { get; private set; }

	public bool CanRotate { get; private set; }

	public bool CanFlip { get; private set; }

	public bool LockAutoOrientation { get; private set; }

	public bool CardinalDirectionOnly { get; private set; }

	public List<SkillConditionDefinition> ContextualConditions { get; private set; } = new List<SkillConditionDefinition>();

	public E_Phase DisplayDuringPhase { get; private set; }

	public int HealthCost { get; private set; }

	public bool InfiniteRange { get; private set; }

	public E_InvalidCastDisplayBehaviour InvalidCastDisplayBehaviour { get; private set; }

	public string Id { get; private set; }

	public string GroupId { get; private set; }

	public bool IsContextual { get; private set; }

	public bool IsLockedByPerk { get; private set; }

	public bool IsBrazierSpecific { get; private set; }

	public int Level { get; private set; }

	public string LocalizationId { get; private set; }

	public int ManaCost { get; private set; }

	public int MovePointsCost { get; private set; }

	public SkillCastFxDefinition PreSkillCastFxDefinition { get; private set; }

	public bool RangeModifiable { get; private set; }

	public Vector2Int Range { get; private set; }

	public SkillActionDefinition SkillActionDefinition { get; private set; }

	public SkillCastFxDefinition SkillCastFxDefinition { get; private set; }

	public string SoundId { get; private set; }

	public int SurroundingEffectTilesCount { get; private set; }

	public int TotalAreaOfEffectTilesCount => AffectedTilesCount + SurroundingEffectTilesCount;

	public ValidTargets ValidTargets { get; private set; }

	public int UsesPerTurnCount { get; private set; } = -1;

	public SkillDefinition(XContainer container)
		: base(container)
	{
	}

	public bool CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect type, bool isSurroundingEffect)
	{
		if (isSurroundingEffect)
		{
			if (SkillActionDefinition.SkillEffectDefinitions != null && SkillActionDefinition.SkillEffectDefinitions.TryGetValue("SurroundingEffect", out var value))
			{
				foreach (SkillEffectDefinition item in value)
				{
					if (item is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
			return false;
		}
		if (SkillActionDefinition.SkillEffectDefinitions != null)
		{
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in SkillActionDefinition.SkillEffectDefinitions)
			{
				if (skillEffectDefinition.Key == "SurroundingEffect")
				{
					continue;
				}
				foreach (SkillEffectDefinition item2 in skillEffectDefinition.Value)
				{
					if (item2 is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition2 && affectingUnitSkillEffectDefinition2.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
		}
		return AffectedUnits.HasFlag(type);
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		SkillDefinition skillDefinition = null;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute == null)
		{
			CLoggerManager.Log("The skill has no ID !", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			if (SkillDatabase.SkillDefinitions.ContainsKey(xAttribute2.Value))
			{
				skillDefinition = SkillDatabase.SkillDefinitions[xAttribute2.Value];
			}
			else
			{
				CLoggerManager.Log("Error while parsing Template parameter of skill " + Id + "! The skill with ID " + xAttribute2.Value + " does'nt exist.", LogType.Error);
			}
		}
		AllowFriendlyFire = xElement.Element("AllowFriendlyFire") != null || (skillDefinition?.AllowFriendlyFire ?? false);
		GroupId = xElement.Attribute("GroupId")?.Value ?? skillDefinition?.GroupId ?? Id;
		XAttribute xAttribute3 = xElement.Attribute("IsContextual");
		IsContextual = xAttribute3 != null && bool.Parse(xAttribute3.Value);
		XAttribute xAttribute4 = xElement.Attribute("IsLockedByPerk");
		IsLockedByPerk = xAttribute4 != null && bool.Parse(xAttribute4.Value);
		XAttribute xAttribute5 = xElement.Attribute("IsBrazierSpecific");
		IsBrazierSpecific = xAttribute5 != null && bool.Parse(xAttribute5.Value);
		LocalizationId = xElement.Element("OverrideLocalizationId")?.Value ?? skillDefinition?.LocalizationId ?? Id;
		ArtId = xElement.Element("OverrideArtId")?.Value ?? skillDefinition?.ArtId ?? Id;
		SoundId = xElement.Element("OverrideSoundId")?.Value ?? skillDefinition?.SoundId ?? GroupId;
		XElement xElement2 = xElement.Element("ActionPointsCost");
		int result;
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing ActionPointsCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			ActionPointsCost = result;
		}
		else if (skillDefinition != null)
		{
			ActionPointsCost = skillDefinition.ActionPointsCost;
		}
		XElement xElement3 = xElement.Element("MovePointsCost");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing MovePointsCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			MovePointsCost = result;
		}
		else if (skillDefinition != null)
		{
			MovePointsCost = skillDefinition.MovePointsCost;
		}
		XElement xElement4 = xElement.Element("ManaCost");
		if (xElement4 != null)
		{
			if (!int.TryParse(xElement4.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing ManaCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			ManaCost = result;
		}
		else if (skillDefinition != null)
		{
			ManaCost = skillDefinition.ManaCost;
		}
		XElement xElement5 = xElement.Element("HealthCost");
		if (xElement5 != null)
		{
			if (!int.TryParse(xElement5.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing HealthCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			HealthCost = result;
		}
		else if (skillDefinition != null)
		{
			HealthCost = skillDefinition.HealthCost;
		}
		XElement xElement6 = xElement.Element("UsesPerTurnCount");
		if (xElement6 != null)
		{
			if (!int.TryParse(xElement6.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing UsesPerTurnCount parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			UsesPerTurnCount = result;
		}
		else if (skillDefinition != null)
		{
			UsesPerTurnCount = skillDefinition.UsesPerTurnCount;
		}
		XElement xElement7 = xElement.Element("Range");
		if (xElement7 != null)
		{
			Range = new Vector2Int(int.Parse(xElement7.Attribute("Min").Value), int.Parse(xElement7.Attribute("Max").Value));
			XAttribute xAttribute6 = xElement7.Attribute("CardinalDirectionOnly");
			if (xAttribute6 != null)
			{
				if (!bool.TryParse(xAttribute6.Value, out var result2))
				{
					CLoggerManager.Log($"The level {Level} of skill {Id} has an invalid CardinalDirectionOnly!", LogType.Error);
					return;
				}
				CardinalDirectionOnly = result2;
			}
			XAttribute xAttribute7 = xElement7.Attribute("Modifiable");
			if (xAttribute7 != null)
			{
				if (!bool.TryParse(xAttribute7.Value, out var result3))
				{
					CLoggerManager.Log($"The level {Level} of skill {Id} has an invalid Modifiable!", LogType.Error);
					return;
				}
				RangeModifiable = result3;
			}
		}
		else if (skillDefinition != null)
		{
			Range = skillDefinition.Range;
			CardinalDirectionOnly = skillDefinition.CardinalDirectionOnly;
			RangeModifiable = skillDefinition.RangeModifiable;
		}
		InfiniteRange = xElement.Element("InfiniteRange") != null || (skillDefinition?.InfiniteRange ?? false);
		int num = 0;
		XElement xElement8 = xElement.Element("AreaOfEffect");
		if (xElement8 != null)
		{
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Origin = new Vector2Int(int.Parse(xElement8.Attribute("OriginX").Value), int.Parse(xElement8.Attribute("OriginY").Value)),
				Pattern = new List<List<char>>(),
				IsSingleTarget = false
			};
			string[] array = xElement8.Value.Split('\n');
			for (int num2 = array.Length - 1; num2 >= 0; num2--)
			{
				array[num2] = array[num2].RemoveWhitespace();
				if (array[num2] != string.Empty)
				{
					AreaOfEffectDefinition.Pattern.Add(new List<char>(array[num2].Length));
					for (int i = 0; i < array[num2].Length; i++)
					{
						AreaOfEffectDefinition.Pattern[AreaOfEffectDefinition.Pattern.Count - 1].Add(array[num2][i]);
						if (array[num2][i] == 'X')
						{
							AffectedTilesCount++;
						}
						else if (array[num2][i] == 'e')
						{
							SurroundingEffectTilesCount++;
						}
						else if (array[num2][i] == 'M')
						{
							num++;
						}
					}
				}
			}
			AreaOfEffectDefinition.IsSingleTarget |= AffectedTilesCount == 1;
			if (num > 1)
			{
				CLoggerManager.Log("Skill " + Id + " must have 0 or 1 maneuver tile in area of effect!", LogType.Error);
				return;
			}
		}
		else if (skillDefinition != null)
		{
			AreaOfEffectDefinition = skillDefinition.AreaOfEffectDefinition;
			AffectedTilesCount = skillDefinition.AffectedTilesCount;
			SurroundingEffectTilesCount = skillDefinition.SurroundingEffectTilesCount;
		}
		CanRotate = xElement.Element("CanRotate") != null || (skillDefinition?.CanRotate ?? false);
		CanFlip = xElement.Element("CanFlip") != null || (skillDefinition?.CanFlip ?? false);
		LockAutoOrientation = xElement.Element("LockAutoOrientation") != null || (skillDefinition?.LockAutoOrientation ?? false);
		if (CanRotate && LockAutoOrientation)
		{
			CLoggerManager.Log("Both CanRotate and LockAutoOrientation are set to true in the skill " + Id + ", something is probably wrong here.", LogType.Error, CLogLevel.MAJOR);
		}
		XElement xElement9 = xElement.Element("AffectedUnits");
		if (xElement9 != null)
		{
			AffectedUnits.Deserialize(xElement9);
		}
		XElement xElement10 = xElement.Element("SkillAction");
		if (xElement10 != null)
		{
			foreach (XElement item in xElement10.Elements())
			{
				if (SkillActionDefinition != null)
				{
					CLoggerManager.Log("Skill " + Id + " already has a skill action!", LogType.Error);
					break;
				}
				switch (item.Name.LocalName)
				{
				case "Attack":
					SkillActionDefinition = new AttackSkillActionDefinition(xElement10);
					continue;
				case "Generic":
					SkillActionDefinition = new GenericSkillActionDefinition(xElement10);
					continue;
				case "GoIntoWatchtower":
					SkillActionDefinition = new GoIntoWatchtowerSkillActionDefinition(xElement10);
					continue;
				case "SkipTurn":
					SkillActionDefinition = new SkipTurnSkillActionDefinition(xElement10);
					continue;
				case "QuitWatchtower":
					SkillActionDefinition = new QuitWatchtowerSkillActionDefinition(xElement10);
					continue;
				case "Spawn":
					SkillActionDefinition = new SpawnSkillActionDefinition(xElement10);
					continue;
				case "Build":
					SkillActionDefinition = new BuildSkillActionDefinition(xElement10);
					continue;
				case "Resupply":
					SkillActionDefinition = new ResupplySkillActionDefinition(xElement10);
					continue;
				}
				CLoggerManager.Log("Unknown skill effect type: " + item.Name.LocalName + " on skill " + Id + ".", LogType.Error);
			}
		}
		else
		{
			SkillActionDefinition = skillDefinition.SkillActionDefinition;
		}
		if (SkillActionDefinition.HasEffect("Maneuver"))
		{
			if (num == 0 && skillDefinition == null)
			{
				CLoggerManager.Log("Skill " + Id + " has the maneuver skill effect but no maneuver tile in area of effect!", LogType.Error);
				return;
			}
		}
		else if (num > 0)
		{
			CLoggerManager.Log("Skill " + Id + " has a maneuver tile in area of effect but not the maneuver skill effect!", LogType.Error);
			return;
		}
		XElement xElement11 = xElement.Element("ValidTargets");
		if (xElement11 != null)
		{
			ValidTargets = new ValidTargets
			{
				Buildings = new Dictionary<string, ValidTargets.Constraints>()
			};
			foreach (XElement item2 in xElement11.Elements("Building"))
			{
				XAttribute xAttribute8 = item2.Attribute("Id");
				if (xAttribute8.IsNullOrEmpty())
				{
					CLoggerManager.Log("ValidTargets' building of skill " + Id + " must have a valid Id", LogType.Error);
					continue;
				}
				XAttribute xAttribute9 = item2.Attribute("MustBeEmpty");
				bool result4 = false;
				if (xAttribute9 != null && !bool.TryParse(xAttribute9.Value, out result4))
				{
					CLoggerManager.Log("Invalid MustBeEmptyAttribute", LogType.Error);
					continue;
				}
				XAttribute xAttribute10 = item2.Attribute("NeedRepair");
				bool result5 = false;
				if (xAttribute10 != null && !bool.TryParse(xAttribute10.Value, out result5))
				{
					CLoggerManager.Log("Invalid NeedRepairAttribute", LogType.Error);
				}
				else
				{
					ValidTargets.Buildings.Add(xAttribute8.Value, new ValidTargets.Constraints(result4, result5));
				}
			}
			foreach (XElement item3 in xElement11.Elements("BuildingsList"))
			{
				XAttribute xAttribute11 = item3.Attribute("Id");
				if (xAttribute11.IsNullOrEmpty())
				{
					CLoggerManager.Log("ValidTargets' buildings list of skill " + Id + " must have a valid Id", LogType.Error);
					continue;
				}
				XAttribute xAttribute12 = item3.Attribute("NeedRepair");
				bool result6 = false;
				if (xAttribute12 != null && !bool.TryParse(xAttribute12.Value, out result6))
				{
					CLoggerManager.Log("Invalid NeedRepairAttribute", LogType.Error);
					continue;
				}
				foreach (string id in GenericDatabase.IdsListDefinitions[xAttribute11.Value].Ids)
				{
					if (!ValidTargets.Buildings.ContainsKey(id))
					{
						ValidTargets.Buildings.Add(id, new ValidTargets.Constraints(mustBeEmpty: false, result6));
					}
				}
			}
			foreach (XElement item4 in xElement11.Elements("BuildingCategory"))
			{
				XAttribute xAttribute13 = item4.Attribute("Category");
				if (xAttribute13.IsNullOrEmpty() || !Enum.TryParse<BuildingDefinition.E_BuildingCategory>(xAttribute13.Value, out var result7))
				{
					CLoggerManager.Log("ValidTargets' building category of skill " + Id + " must have a valid category : \"" + xAttribute13?.Value + "\"", LogType.Error);
					continue;
				}
				foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
				{
					if (buildingDefinition.Value.BlueprintModuleDefinition.Category.HasFlag(result7))
					{
						ValidTargets.Buildings.Add(buildingDefinition.Key, new ValidTargets.Constraints(mustBeEmpty: false, needRepair: false));
					}
				}
			}
			ValidTargets.PlayableUnits = xElement11.Element("PlayableUnits") != null;
			ValidTargets.EnemyUnits = xElement11.Element("EnemyUnits") != null;
			ValidTargets.EmptyTiles = xElement11.Element("EmptyTiles") != null;
			ValidTargets.WalkableCityTiles = xElement11.Element("WalkableCityTiles") != null;
			ValidTargets.WalkableTiles = xElement11.Element("WalkableTiles") != null;
			ValidTargets.UncrossableGrounds = xElement11.Element("UncrossableGrounds") != null;
		}
		else if (skillDefinition?.ValidTargets != null)
		{
			ValidTargets = skillDefinition.ValidTargets;
		}
		XElement xElement12 = xElement.Element("AllowDuringPhases");
		if (xElement12 != null)
		{
			foreach (XElement item5 in xElement12.Elements())
			{
				if (!Enum.TryParse<E_Phase>(item5.Name.LocalName, out var result8))
				{
					CLoggerManager.Log("Could not parse " + item5.Name.LocalName + " to a valid E_Phase.", LogType.Error);
					return;
				}
				AllowDuringPhase |= result8;
			}
		}
		else if (skillDefinition != null)
		{
			AllowDuringPhase = skillDefinition.AllowDuringPhase;
		}
		else
		{
			AllowDuringPhase = E_Phase.Night;
		}
		XElement xElement13 = xElement.Element("DisplayDuringPhases");
		if (xElement13 != null)
		{
			foreach (XElement item6 in xElement13.Elements())
			{
				if (!Enum.TryParse<E_Phase>(item6.Name.LocalName, out var result9))
				{
					CLoggerManager.Log("Could not parse " + item6.Name.LocalName + " to a valid E_Phase.", LogType.Error);
					return;
				}
				DisplayDuringPhase |= result9;
			}
		}
		else if (skillDefinition != null)
		{
			DisplayDuringPhase = skillDefinition.DisplayDuringPhase;
		}
		else
		{
			DisplayDuringPhase = E_Phase.All;
		}
		XElement xElement14 = xElement.Element("ContextualConditions");
		if (xElement14 != null)
		{
			ContextualConditions = new List<SkillConditionDefinition>();
			foreach (XElement item7 in xElement14.Elements())
			{
				switch (item7.Name.ToString())
				{
				case "InPlayableUnitRange":
					ContextualConditions.Add(new InPlayableUnitRangConditionDefinition(item7));
					break;
				case "InWatchtower":
					ContextualConditions.Add(new InWatchtowerConditionDefinition(item7));
					break;
				case "OnlyDuringPhase":
					ContextualConditions.Add(new OnlyDuringPhaseConditionDefinition(item7));
					break;
				case "MaxTargetHealthLeft":
					ContextualConditions.Add(new MaxTargetHealthLeftConditionDefinition(item7));
					break;
				case "NotInBuilding":
					ContextualConditions.Add(new NotInBuildingConditionDefinition(item7));
					break;
				case "NextToBuilding":
					ContextualConditions.Add(new NextToBuildingConditionDefinition(item7));
					break;
				case "OntoBuilding":
					ContextualConditions.Add(new OntoBuildingConditionDefinition(item7));
					break;
				case "MinTargetInjuryStage":
					ContextualConditions.Add(new MinTargetInjuryStageConditionDefinition(item7));
					break;
				}
			}
		}
		else if (skillDefinition != null)
		{
			ContextualConditions = skillDefinition.ContextualConditions;
		}
		XElement xElement15 = xElement.Element("InvalidCastDisplayBehaviour");
		if (xElement15 != null)
		{
			if (!Enum.TryParse<E_InvalidCastDisplayBehaviour>(xElement15.Value, out var result10))
			{
				CLoggerManager.Log("Could not parse " + xElement15.Value + " to a valid E_InvalidCastDisplayBehaviour.", LogType.Error);
				return;
			}
			InvalidCastDisplayBehaviour = result10;
		}
		else if (skillDefinition != null)
		{
			InvalidCastDisplayBehaviour = skillDefinition.InvalidCastDisplayBehaviour;
		}
		else
		{
			InvalidCastDisplayBehaviour = E_InvalidCastDisplayBehaviour.Hidden;
		}
		XElement xElement16 = xElement.Element("CastFXs");
		SkillCastFxDefinition = ((xElement16 != null) ? new SkillCastFxDefinition(xElement16) : skillDefinition?.SkillCastFxDefinition);
		XElement xElement17 = xElement.Element("PreCastFXs");
		PreSkillCastFxDefinition = ((xElement17 != null) ? new SkillCastFxDefinition(xElement17) : skillDefinition?.PreSkillCastFxDefinition);
		if (AreaOfEffectDefinition == null)
		{
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Pattern = new List<List<char>>
				{
					new List<char> { 'X' }
				},
				IsSingleTarget = true
			};
		}
	}
}
