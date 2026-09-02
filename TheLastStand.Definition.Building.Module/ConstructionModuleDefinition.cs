using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class ConstructionModuleDefinition : BuildingModuleDefinition
{
	public int NativeBuildLimit { get; private set; } = -1;

	public string BuildLimitGroupId { get; private set; } = string.Empty;

	public int ConstructionAnimationFrameRate { get; private set; }

	public int ConstructionAnimationShockwaveFrame { get; private set; }

	public BuildingDefinition.E_ConstructionAnimationType ConstructionAnimationType { get; private set; } = BuildingDefinition.E_ConstructionAnimationType.Instantaneous;

	public BuildingDefinition.E_ConstructionAnimationType DestructionAnimationType { get; private set; } = BuildingDefinition.E_ConstructionAnimationType.Instantaneous;

	public List<GroundDefinition.E_GroundCategory> GroundCategories { get; private set; }

	public bool IsBuyable { get; private set; }

	public bool IsDemolishable { get; private set; }

	public bool IsRepairable { get; private set; }

	public int NativeGoldCost { get; private set; }

	public int NativeMaterialsCost { get; private set; }

	public BuildingDefinition.E_OccupationVolumeType OccupationVolumeType { get; private set; } = BuildingDefinition.E_OccupationVolumeType.Adjacent;

	public bool ShouldDisplayConstructionTileFeedback { get; private set; }

	public bool PlayConstructionSound { get; private set; }

	public bool PlayDestructionSound { get; private set; }

	public ConstructionModuleDefinition(BuildingDefinition buildingDefinition, XContainer constructionDefinition)
		: base(buildingDefinition, constructionDefinition)
	{
	}

	public bool IsUnlimited(bool useDefault = false)
	{
		return GetBuildLimit(useDefault) < 0;
	}

	public int GetBuildLimit(bool useDefault = false)
	{
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(BuildLimitGroupId, out var value))
		{
			return value.GetBuildLimit(useDefault);
		}
		if (useDefault)
		{
			return NativeBuildLimit;
		}
		int num = 0;
		if (NativeBuildLimit != -1)
		{
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num2 = effects.Length - 1; num2 >= 0; num2--)
				{
					if (effects[num2].BuildingId == BuildingDefinition.Id && effects[num2].MaxCityInstancesBonus != -1)
					{
						num += effects[num2].MaxCityInstancesBonus;
					}
				}
			}
			num += TPSingleton<GlyphManager>.Instance.BuildLimitModifiers.GetValueOrDefault(BuildingDefinition.Id);
		}
		return NativeBuildLimit + num;
	}

	public string GetLocalizedBuildLimit(bool useDefaultValues = false)
	{
		if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(BuildLimitGroupId, out var value))
		{
			string text = string.Empty;
			foreach (string buildingId in value.BuildingIds)
			{
				if (BuildingDatabase.BuildingDefinitions.TryGetValue(buildingId, out var value2))
				{
					text += ((text == string.Empty) ? value2.Name : (Localizer.Get("Generic_EnumerableSeparator") + value2.Name));
				}
			}
			return Localizer.Format("ConstructionPanel_BuildLimitGroupTooltip", GetBuildLimit(useDefaultValues), (!useDefaultValues) ? TPSingleton<ConstructionManager>.Instance.GetBuildingCount(this) : 0, text);
		}
		return Localizer.Format("ConstructionPanel_BuildLimitTooltip", GetBuildLimit(useDefaultValues), (!useDefaultValues) ? TPSingleton<ConstructionManager>.Instance.GetBuildingCount(this) : 0);
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("ConstructionAnimationTypes");
		XElement xElement3 = xElement2?.Element("ConstructionAnimationType");
		if (xElement3 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_ConstructionAnimationType>(xElement3.Attribute("Value").Value, out var result))
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + " must have a valid ConstructionAnimationType!", LogType.Error);
				return;
			}
			ConstructionAnimationType = result;
			if (ConstructionAnimationType == BuildingDefinition.E_ConstructionAnimationType.Animated)
			{
				XElement xElement4 = xElement2.Element("ConstructionAnimationFrameRate");
				if (xElement4 != null)
				{
					XAttribute xAttribute = xElement4.Attribute("Value");
					if (xAttribute != null)
					{
						if (int.TryParse(xAttribute.Value, out var result2))
						{
							ConstructionAnimationFrameRate = Mathf.Clamp(result2, 1, int.MaxValue);
						}
						else
						{
							CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " ConstructionAnimationFrameRate has an invalid value, setting it to 20.", LogType.Error);
							ConstructionAnimationFrameRate = 20;
						}
					}
				}
				else
				{
					CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " is set to animated but ConstructionAnimationFrameRate is not defined, setting it to 20.");
					ConstructionAnimationFrameRate = 20;
				}
				XElement xElement5 = xElement2.Element("ConstructionAnimationShockwaveFrame");
				if (xElement5 != null)
				{
					XAttribute xAttribute2 = xElement5.Attribute("Value");
					if (xAttribute2 != null)
					{
						if (int.TryParse(xAttribute2.Value, out var result3))
						{
							ConstructionAnimationShockwaveFrame = Mathf.Clamp(result3, 0, int.MaxValue);
						}
						else
						{
							CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " ConstructionAnimationShockwaveFrame has an invalid value, setting it to -1 (no shockwave).", LogType.Error);
							ConstructionAnimationShockwaveFrame = -1;
						}
					}
				}
				else
				{
					ConstructionAnimationShockwaveFrame = -1;
				}
			}
		}
		XElement xElement6 = xElement.Element("DestructionAnimationTypes")?.Element("DestructionAnimationType");
		if (xElement6 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_ConstructionAnimationType>(xElement6.Attribute("Value").Value, out var result4))
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + " must have a valid DestructionAnimationType!", LogType.Error);
				return;
			}
			DestructionAnimationType = result4;
		}
		XElement xElement7 = xElement.Element("OccupationVolumeType");
		if (xElement7 != null)
		{
			if (!Enum.TryParse<BuildingDefinition.E_OccupationVolumeType>(xElement7.Value, out var result5))
			{
				CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " must have a valid OccupationVolumeType!", LogType.Error);
				return;
			}
			OccupationVolumeType = result5;
		}
		GroundCategories = new List<GroundDefinition.E_GroundCategory>();
		foreach (XElement item in xElement.Element("GroundCategories").Elements("GroundCategory"))
		{
			if (Enum.TryParse<GroundDefinition.E_GroundCategory>(item.Value, out var result6))
			{
				GroundCategories.Add(result6);
				continue;
			}
			CLoggerManager.Log("Error while parsing GroundCategory for " + BuildingDefinition.Id + " : " + item.Value + " is not a valid GroundType", LogType.Error, CLogLevel.NORMAL, forcePrintInUnity: true, GetType().Name);
		}
		ShouldDisplayConstructionTileFeedback = OccupationVolumeType != BuildingDefinition.E_OccupationVolumeType.Ignore || (GroundCategories.Count == 1 && GroundCategories[0] == GroundDefinition.E_GroundCategory.City);
		XElement xElement8 = xElement.Element("MaterialsCost");
		if (xElement8 != null)
		{
			if (int.TryParse(xElement8.Value, out var result7))
			{
				NativeMaterialsCost = result7;
			}
			else
			{
				CLoggerManager.Log("Could not parse the MaterialsCost element in " + BuildingDefinition.Id + " into an int : " + xElement8.Value + ".", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else
		{
			NativeMaterialsCost = 0;
		}
		XElement xElement9 = xElement.Element("GoldCost");
		if (xElement9 != null)
		{
			if (int.TryParse(xElement9.Value, out var result8))
			{
				NativeGoldCost = result8;
			}
			else
			{
				CLoggerManager.Log("Could not parse the GoldCost element in " + BuildingDefinition.Id + " into an int : " + xElement9.Value + ".", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else
		{
			NativeGoldCost = 0;
		}
		XElement xElement10 = xElement.Element("IsBuyable");
		IsBuyable = ((xElement10 != null) ? bool.Parse(xElement10.Value) : (NativeGoldCost > 0 || NativeMaterialsCost > 0));
		XElement xElement11 = xElement.Element("IsRepairable");
		IsRepairable = ((xElement11 != null) ? bool.Parse(xElement11.Value) : (NativeGoldCost > 0 || NativeMaterialsCost > 0));
		XElement xElement12 = xElement.Element("IsDemolishable");
		IsDemolishable = ((xElement12 != null) ? bool.Parse(xElement12.Value) : IsBuyable);
		XElement xElement13 = xElement.Element("BuildLimit");
		if (!xElement13.IsNullOrEmpty())
		{
			BuildingLimitGroupDefinition value;
			if (sbyte.TryParse(xElement13.Value, out var result9))
			{
				if (result9 == 0)
				{
					CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " has an invalid BuildLimit (must be in the range -1 to 127, excluding 0)!", LogType.Error);
				}
				else
				{
					NativeBuildLimit = result9;
				}
			}
			else if (BuildingDatabase.BuildingLimitGroupDefinitions.TryGetValue(xElement13.Value, out value))
			{
				BuildLimitGroupId = value.Id;
				value.BuildingIds.Add(BuildingDefinition.Id);
			}
			else
			{
				CLoggerManager.Log("BuildingDefinition " + BuildingDefinition.Id + " has an invalid BuildLimit (must be in the range -1 to 127 excluding 0 OR an existing BuildingLimitGroup Id)!", LogType.Error);
			}
		}
		PlayConstructionSound = xElement.Element("MuteConstructionSound") == null;
		PlayDestructionSound = xElement.Element("MuteDestructionSound") == null;
	}
}
