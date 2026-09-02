using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class EliteEnemyUnitTemplateDefinition : EnemyUnitTemplateDefinition
{
	public string EliteId { get; private set; }

	public override string SpecificAssetsId => EliteId;

	public Dictionary<UnitStatDefinition.E_Stat, StatModifierDefinition> ModifiedStats { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, StatModifierDefinition>();

	public EliteEnemyUnitTemplateDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		EliteId = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("Base");
		if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(xAttribute2.Value, out var value))
		{
			CLoggerManager.Log("Couldn't find base enemy '" + xAttribute2.Value + "' in database for elite enemy '" + EliteId + "'", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
			return;
		}
		CopyValuesFromBaseDefinition(value);
		foreach (XElement item in xElement.Elements("ModifiedStat"))
		{
			XAttribute xAttribute3 = item.Attribute("Id");
			StatModifierDefinition value2 = new StatModifierDefinition(item);
			if (Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute3.Value, out var result))
			{
				ModifiedStats.Add(result, value2);
				continue;
			}
			CLoggerManager.Log("Couldn't parse \"" + xAttribute3.Value + "\" into a suitablefor elite enemy '" + EliteId + "'", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
			break;
		}
	}

	private void CopyValuesFromBaseDefinition(EnemyUnitTemplateDefinition eutd)
	{
		base.Id = eutd.Id;
		base.DamagedParticlesId = eutd.DamagedParticlesId;
		base.InjuryDefinitions = eutd.InjuryDefinitions;
		base.MoveMethod = eutd.MoveMethod;
		base.OriginX = eutd.OriginX;
		base.OriginY = eutd.OriginY;
		base.Tiles = eutd.Tiles;
		base.UnitType = eutd.UnitType;
		base.Accuracy = eutd.Accuracy;
		base.AffixDefinitions = eutd.AffixDefinitions;
		base.ArmorTotal = eutd.ArmorTotal;
		base.IsTargetableByAI = eutd.IsTargetableByAI;
		base.Behavior = eutd.Behavior;
		base.Block = eutd.Block;
		base.Critical = eutd.Critical;
		base.CriticalPower = eutd.CriticalPower;
		base.Dodge = eutd.Dodge;
		base.ExperienceGain = eutd.ExperienceGain;
		base.DamageSkillId = eutd.DamageSkillId;
		base.DamnedSoulsEarned = eutd.DamnedSoulsEarned;
		base.DeathSoundFolderName = eutd.DeathSoundFolderName;
		base.HealthGaugeSize = eutd.HealthGaugeSize;
		base.HealthRegen = eutd.HealthRegen;
		base.HealthTotal = eutd.HealthTotal;
		base.HideInNightReport = eutd.HideInNightReport;
		base.IsInvulnerable = eutd.IsInvulnerable;
		base.LockedOrientation = eutd.LockedOrientation;
		base.MagicalDamage = eutd.MagicalDamage;
		base.MovePointsTotal = eutd.MovePointsTotal;
		base.MoveSpeed = eutd.MoveSpeed;
		base.MoveSoundFolderName = eutd.MoveSoundFolderName;
		base.OnDeathBehavior = eutd.OnDeathBehavior;
		base.Panic = eutd.Panic;
		base.PhysicalDamage = eutd.PhysicalDamage;
		base.StatProgressions = eutd.StatProgressions;
		base.SkillProgressions = eutd.SkillProgressions;
		base.RangedDamage = eutd.RangedDamage;
		base.Reliability = eutd.Reliability;
		base.Resistance = eutd.Resistance;
		base.SkillsToDisplayIds = eutd.SkillsToDisplayIds;
		base.SortingOrderOverride = eutd.SortingOrderOverride;
		base.StunResistance = eutd.StunResistance;
		base.Tier = eutd.Tier;
		base.VisualEvolutions = eutd.VisualEvolutions;
		base.VisualOffset = eutd.VisualOffset;
		base.Weight = eutd.Weight;
	}
}
