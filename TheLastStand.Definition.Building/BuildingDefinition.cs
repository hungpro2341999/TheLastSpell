using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class BuildingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_ConstructionAnimationType
	{
		None,
		Instantaneous,
		Animated
	}

	public enum E_OccupationVolumeType
	{
		None,
		Adjacent,
		Ignore
	}

	[Flags]
	public enum E_BuildingCategory
	{
		None = 0,
		Obstacle = 1,
		Defensive = 2,
		Production = 4,
		LightFogSpawner = 9,
		LitBrazier = 0x11,
		UnlitBrazier = 0x21,
		Wall = 0x42,
		Watchtower = 0x82,
		Turret = 0x102,
		Trap = 0x202,
		HandledDefense = 0x402,
		Gate = 0x842,
		Barricade = 0x1002,
		BonePile = 0x3002,
		WalkableHandledDefense = 0x4402
	}

	[Flags]
	public enum E_ConstructionCategory
	{
		None = 0,
		Defensive = 1,
		Production = 2,
		All = 3
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string Catapult = "Catapult";
		}

		public const float DamagedSpriteThreshold = 0.5f;
	}

	public BattleModuleDefinition BattleModuleDefinition { get; private set; }

	public BlueprintModuleDefinition BlueprintModuleDefinition { get; private set; }

	public BrazierModuleDefinition BrazierModuleDefinition { get; private set; }

	public ConstructionModuleDefinition ConstructionModuleDefinition { get; private set; }

	public DamageableModuleDefinition DamageableModuleDefinition { get; private set; }

	public PassivesModuleDefinition PassivesModuleDefinition { get; private set; }

	public ProductionModuleDefinition ProductionModuleDefinition { get; private set; }

	public UpgradeModuleDefinition UpgradeModuleDefinition { get; private set; }

	public string Description => Localizer.Get("BuildingDescription_" + Id);

	public string Id { get; private set; }

	public string Name => Localizer.Get("BuildingName_" + Id);

	public List<string> IdListIds { get; }

	public BuildingDefinition(XContainer buildingDefinitionContainer)
		: base(buildingDefinitionContainer)
	{
		if (GenericDatabase.TryGetIdListIdsForEntity(Id, out var foundDefinitions))
		{
			IdListIds = foundDefinitions;
		}
	}

	public override void Deserialize(XContainer buildingDefinitionContainer)
	{
		XElement xElement = buildingDefinitionContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("Construction");
		if (xElement2 == null)
		{
			CLoggerManager.Log("The Construction element is missing in " + Id + ".", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		ConstructionModuleDefinition = new ConstructionModuleDefinition(this, xElement2);
		XElement xElement3 = xElement.Element("Blueprint");
		if (xElement3 == null)
		{
			CLoggerManager.Log("The Blueprint element is missing in " + Id + ".", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		BlueprintModuleDefinition = new BlueprintModuleDefinition(this, xElement3);
		XElement xElement4 = xElement.Element("Damageable");
		if (xElement4 != null)
		{
			DamageableModuleDefinition = new DamageableModuleDefinition(this, xElement4);
		}
		XElement xElement5 = xElement.Element("Brazier");
		if (xElement5 != null)
		{
			BrazierModuleDefinition = new BrazierModuleDefinition(this, xElement5);
		}
		XElement xElement6 = xElement.Element("Upgrade");
		if (xElement6 != null)
		{
			UpgradeModuleDefinition = new UpgradeModuleDefinition(this, xElement6);
		}
		XElement xElement7 = xElement.Element("Passives");
		if (xElement7 != null)
		{
			PassivesModuleDefinition = new PassivesModuleDefinition(this, xElement7);
		}
		XElement xElement8 = xElement.Element("Battle");
		if (xElement8 != null)
		{
			BattleModuleDefinition = new BattleModuleDefinition(this, xElement8);
		}
		XElement xElement9 = xElement.Element("Production");
		if (xElement9 != null)
		{
			ProductionModuleDefinition = new ProductionModuleDefinition(this, xElement9);
		}
	}
}
