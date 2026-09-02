using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class EnemyUnitTemplateDefinition : UnitTemplateDefinition
{
	public static class Consts
	{
		public static class Ids
		{
			public const string Clawer = "Clawer";

			public const string ClawerElite = "ClawerElite";

			public const string SpeedyClawer = "SpeedyClawer";

			public const string Boomer = "Boomer";

			public const string SpawnerCocoon = "SpawnerCocoon";

			public const string Ghost = "Ghost";

			public const string Bloody = "Bloody";
		}

		public const string BaseVariantId = "01";
	}

	public class Stat
	{
		public UnitStatDefinition.E_Stat Id { get; private set; }

		public float Max { get; set; }

		public float Min { get; set; }

		public int Odd { get; set; }

		public Stat(UnitStatDefinition.E_Stat id)
		{
			Id = id;
		}
	}

	public class StatProgression
	{
		public UnitStatDefinition.E_Stat Id { get; }

		public byte Delay { get; set; }

		public byte IncreaseEveryXDay { get; set; } = 1;

		public float Value { get; set; }

		public int MaxIncreases { get; set; } = int.MaxValue;

		public StatProgression(UnitStatDefinition.E_Stat id)
		{
			Id = id;
		}
	}

	public class VisualVariant : StringWeightedDefinition
	{
		protected override string XMLValueAttributeName => "Id";

		public VisualVariant(XContainer container, Dictionary<string, string> tokenVariables = null)
			: base(container, tokenVariables)
		{
		}
	}

	public List<EnemyAffixDefinition> AffixDefinitions { get; protected set; }

	public float AppearanceDelay { get; private set; }

	public string AssetsId
	{
		get
		{
			if (!UseTemplateAssets)
			{
				return Id;
			}
			return TemplateId;
		}
	}

	public virtual string SpecificAssetsId
	{
		get
		{
			if (!UseTemplateAssets)
			{
				return Id;
			}
			return TemplateId;
		}
	}

	public float CastSpawnSkillDelay { get; private set; }

	public Stat Reliability { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Reliability);

	public Stat Accuracy { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Accuracy);

	public Stat ArmorTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.ActionPointsTotal);

	public BehaviorDefinition Behavior { get; protected set; }

	public Stat Block { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Block);

	public Stat Critical { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Critical);

	public Stat CriticalPower { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.CriticalPower);

	public Stat Dodge { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Dodge);

	public Stat ExperienceGain { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.ExperienceGain);

	public string DamageSkillId { get; protected set; }

	public Stat DamnedSoulsEarned { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.DamnedSoulsEarned);

	public string DeathSoundFolderName { get; protected set; } = string.Empty;

	public UnitView.E_GaugeSize HealthGaugeSize { get; protected set; }

	public Stat HealthRegen { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.HealthRegen);

	public Stat HealthTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.HealthTotal);

	public bool HideInNightReport { get; protected set; }

	public string Id { get; protected set; }

	public bool IsInvulnerable { get; protected set; }

	public bool IsTargetableByAI { get; protected set; }

	public GameDefinition.E_Direction LockedOrientation { get; protected set; }

	public Stat MagicalDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.MagicalDamage);

	public Stat MovePointsTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.MovePointsTotal);

	public float MoveSpeed { get; protected set; }

	public string MoveSoundFolderName { get; protected set; } = string.Empty;

	public BehaviorDefinition OnDeathBehavior { get; protected set; }

	public Stat Panic { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Panic);

	public Stat PhysicalDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.PhysicalDamage);

	public List<StatProgression> StatProgressions { get; protected set; } = new List<StatProgression>();

	public List<SkillProgression> SkillProgressions { get; protected set; } = new List<SkillProgression>();

	public Stat RangedDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.RangedDamage);

	public Stat Resistance { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Resistance);

	public SkillDefinition ZoneControlSkill { get; private set; }

	public List<string> SkillsToDisplayIds { get; protected set; } = new List<string>();

	public int? SortingOrderOverride { get; protected set; }

	public Stat StunResistance { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.StunResistance);

	public string TemplateId { get; protected set; }

	public int Tier { get; protected set; } = 1;

	public override Tile.E_UnitAccess UnitAccessNeeded => Tile.E_UnitAccess.Enemy;

	public override bool UpdateAnimatorOnOrientationChange { get; protected set; }

	public bool UseTemplateAssets { get; protected set; }

	public string SpawnCutsceneId { get; private set; }

	public List<string> VisualEvolutions { get; protected set; }

	public Vector2 VisualOffset { get; protected set; }

	public GroupWeightedDefinition<string> VisualVariants { get; protected set; }

	public int Weight { get; protected set; } = 1;

	public EnemyUnitTemplateDefinition(XContainer container)
		: base(container)
	{
		base.UnitType = DamageableType.Enemy;
	}

	protected override bool CanStopOnSingleTile(Tile tile, TheLastStand.Model.Unit.Unit unit = null)
	{
		if (!base.CanStopOnSingleTile(tile, unit))
		{
			return false;
		}
		TheLastStand.Model.Unit.Unit unit2 = tile.Unit;
		if (unit2 != null && !unit2.IsDead && tile.Unit != unit)
		{
			if (tile.Unit is PlayableUnit)
			{
				return false;
			}
			if (tile.Unit is EnemyUnit enemyUnit && (enemyUnit.TargetTile == tile || enemyUnit.TargetTile == null))
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanTravelThrough(Tile tile, E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (!base.CanTravelThrough(tile, moveMethod, ignoreUnits, ignoreBuildings))
		{
			return false;
		}
		switch (moveMethod)
		{
		case E_MoveMethod.Walking:
			if (!ignoreUnits && tile.Unit is PlayableUnit && !tile.Unit.IsDead)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		EnemyUnitTemplateDefinition value = null;
		Id = xElement.Attribute("Id").Value;
		XAttribute xAttribute = xElement.Attribute("TemplateId");
		if (xAttribute != null)
		{
			TemplateId = xAttribute.Value;
			if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(TemplateId, out value))
			{
				BossUnitTemplateDefinition value2 = null;
				Dictionary<string, BossUnitTemplateDefinition> bossUnitTemplateDefinitions = BossUnitDatabase.BossUnitTemplateDefinitions;
				if (bossUnitTemplateDefinitions == null || !bossUnitTemplateDefinitions.TryGetValue(TemplateId, out value2))
				{
					CLoggerManager.Log("EnemyUnit " + Id + " could not find template with Id " + TemplateId + "!", LogType.Error);
					return;
				}
				value = value2;
			}
			XAttribute xAttribute2 = xElement.Attribute("UseTemplateAssets");
			if (xAttribute2 != null)
			{
				if (!bool.TryParse(xAttribute2.Value, out var result))
				{
					CLoggerManager.Log("Could not parse UseTemplateAssets attribute value " + xAttribute2.Value + " to a valid bool!", LogType.Error);
					return;
				}
				UseTemplateAssets = result;
			}
		}
		DeserializeAffixes(xElement.Element("Affixes"));
		XElement xElement2 = xElement.Element("Tier");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result2))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition must have a valid Tier!", LogType.Error);
				return;
			}
			Tier = result2;
		}
		else
		{
			if (value == null)
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition (" + Id + ") must have a Tier element!", LogType.Error);
				return;
			}
			Tier = value.Tier;
		}
		XElement xElement3 = xElement.Element("Weight");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out var result3))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition must have a valid Weight!", LogType.Error);
				return;
			}
			Weight = result3;
		}
		else
		{
			if (value == null)
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition must have a Weight element!", LogType.Error);
				return;
			}
			Weight = value.Weight;
		}
		XElement xElement4 = xElement.Element("VisualVariants");
		if (xElement4 != null)
		{
			VisualVariants = new GroupWeightedDefinition<string>(xElement4);
			foreach (XElement item2 in xElement.Element("VisualVariants").Elements("VisualVariant"))
			{
				if (item2.Attribute("Id").IsNullOrEmpty())
				{
					CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " VisualVariant must have a valid Id", LogType.Error);
					continue;
				}
				VisualVariant item = new VisualVariant(item2);
				VisualVariants.Params.Add(item);
			}
		}
		else if (value != null)
		{
			VisualVariants = value.VisualVariants;
		}
		if (xElement.Element("UpdateAnimatorOnOrientationChange") != null)
		{
			UpdateAnimatorOnOrientationChange = true;
		}
		else if (value != null)
		{
			UpdateAnimatorOnOrientationChange = value.UpdateAnimatorOnOrientationChange;
		}
		XElement xElement5 = xElement.Element("DeathSoundFolderName");
		if (xElement5 != null)
		{
			XAttribute xAttribute3 = xElement5.Attribute("Value");
			if (xAttribute3.IsNullOrEmpty())
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " has an Element DeathSoundFolderName but without a valid Value attribute", LogType.Error);
				return;
			}
			DeathSoundFolderName = xAttribute3.Value;
		}
		else if (value != null)
		{
			DeathSoundFolderName = value.DeathSoundFolderName;
		}
		XElement xElement6 = xElement.Element("MoveSoundFolderName");
		if (xElement6 != null)
		{
			XAttribute xAttribute4 = xElement6.Attribute("Value");
			if (xAttribute4.IsNullOrEmpty())
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " has an Element MoveSoundFolderName but without a valid Value attribute", LogType.Error);
				return;
			}
			MoveSoundFolderName = xAttribute4.Value;
		}
		else if (value != null)
		{
			MoveSoundFolderName = value.MoveSoundFolderName;
		}
		XElement xElement7 = xElement.Element("AppearanceDelay");
		if (xElement7 != null)
		{
			if (!float.TryParse(xElement7.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				CLoggerManager.Log("AppearanceDelay of Boss Unit : " + Id + " has an invalid value !", LogType.Error);
			}
			AppearanceDelay = result4;
		}
		else if (value != null)
		{
			AppearanceDelay = value.AppearanceDelay;
		}
		else
		{
			AppearanceDelay = 0f;
		}
		XElement xElement8 = xElement.Element("CastSpawnSkillDelay");
		if (xElement8 != null)
		{
			if (!float.TryParse(xElement8.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5))
			{
				CLoggerManager.Log("CastSpawnSkillDelay of Boss Unit : " + Id + " has an invalid value !", LogType.Error);
			}
			CastSpawnSkillDelay = result5;
		}
		else if (value != null)
		{
			CastSpawnSkillDelay = value.CastSpawnSkillDelay;
		}
		else
		{
			CastSpawnSkillDelay = -1f;
		}
		XElement xElement9 = xElement.Element("MoveMethod");
		if (xElement9 != null)
		{
			XAttribute xAttribute5 = xElement9.Attribute("Method");
			base.MoveMethod = (E_MoveMethod)Enum.Parse(typeof(E_MoveMethod), xAttribute5.Value);
		}
		else if (value != null)
		{
			base.MoveMethod = value.MoveMethod;
		}
		else
		{
			base.MoveMethod = E_MoveMethod.Walking;
		}
		if (xElement.Element("IsInvulnerable") != null)
		{
			IsInvulnerable = true;
		}
		else if (value != null)
		{
			IsInvulnerable = value.IsInvulnerable;
		}
		else
		{
			IsInvulnerable = false;
		}
		XElement xElement10 = xElement.Element("AvoidAutoTargeting");
		if (xElement10 != null)
		{
			XAttribute xAttribute6 = xElement10.Attribute("Value");
			IsTargetableByAI = !bool.Parse(xAttribute6.Value);
		}
		else if (value != null)
		{
			IsTargetableByAI = value.IsTargetableByAI;
		}
		else
		{
			IsTargetableByAI = true;
		}
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.HealthTotal.ToString()), HealthTotal, value?.HealthTotal);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.ArmorTotal.ToString()), ArmorTotal, value?.ArmorTotal);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Accuracy.ToString()), Accuracy, value?.Accuracy);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Block.ToString()), Block, value?.Block);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Dodge.ToString()), Dodge, value?.Dodge);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.MovePointsTotal.ToString()), MovePointsTotal, value?.MovePointsTotal);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Resistance.ToString()), Resistance, value?.Resistance);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.PhysicalDamage.ToString()), PhysicalDamage, value?.PhysicalDamage);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.RangedDamage.ToString()), RangedDamage, value?.RangedDamage);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.MagicalDamage.ToString()), MagicalDamage, value?.MagicalDamage);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Reliability.ToString()), Reliability, value?.Reliability);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.HealthRegen.ToString()), HealthRegen, value?.HealthRegen);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.Critical.ToString()), Critical, value?.Critical);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.CriticalPower.ToString()), CriticalPower, value?.CriticalPower);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.StunResistance.ToString()), StunResistance, value?.StunResistance);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.DamnedSoulsEarned.ToString()), DamnedSoulsEarned, value?.DamnedSoulsEarned);
		FillUnitTemplateStat(xElement.Element(UnitStatDefinition.E_Stat.ExperienceGain.ToString()), ExperienceGain, value?.ExperienceGain);
		XElement xElement11 = xElement.Element("DamageSkillId");
		if (xElement11 != null)
		{
			DamageSkillId = xElement11.Value;
		}
		else if (value != null)
		{
			DamageSkillId = value.DamageSkillId;
		}
		XElement xElement12 = xElement.Element("MoveSpeed");
		if (xElement12 != null)
		{
			if (!float.TryParse(xElement12.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result6))
			{
				CLoggerManager.Log("Invalid MoveSpeed!", LogType.Error);
				result6 = EnemyUnitDatabase.DefaultUnitMoveSpeed;
			}
			MoveSpeed = result6;
		}
		else if (value != null)
		{
			MoveSpeed = value.MoveSpeed;
		}
		else
		{
			MoveSpeed = EnemyUnitDatabase.DefaultUnitMoveSpeed;
		}
		XElement xElement13 = xElement.Element("ZoneControlSkill");
		if (xElement13 != null)
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(xElement13.Value, out var value3))
			{
				ZoneControlSkill = value3;
			}
		}
		else if (value != null)
		{
			ZoneControlSkill = value.ZoneControlSkill;
		}
		XElement xElement14 = xElement.Element("SkillsToDisplay");
		if (xElement14 != null)
		{
			foreach (XElement item3 in xElement14.Elements("SkillToDisplay"))
			{
				SkillsToDisplayIds.Add(item3.Value);
			}
		}
		else if (value != null)
		{
			SkillsToDisplayIds = value.SkillsToDisplayIds;
		}
		XElement xElement15 = xElement.Element("HealthGaugeSize");
		if (xElement15 != null)
		{
			HealthGaugeSize = (UnitView.E_GaugeSize)Enum.Parse(typeof(UnitView.E_GaugeSize), xElement15.Value);
		}
		else if (value != null)
		{
			HealthGaugeSize = value.HealthGaugeSize;
		}
		XElement xElement16 = xElement.Element("Panic");
		if (xElement16 != null)
		{
			XAttribute xAttribute7 = xElement16.Attribute("Value");
			if (!float.TryParse(xAttribute7.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result7))
			{
				CLoggerManager.Log("Invalid Panic " + xAttribute7.Value, LogType.Error);
			}
			XAttribute xAttribute8 = xElement16.Attribute("Min");
			Panic.Min = ((xAttribute8 != null) ? int.Parse(xAttribute8.Value) : ((int)result7));
			XAttribute xAttribute9 = xElement16.Attribute("Max");
			Panic.Max = ((xAttribute9 != null) ? ((float)int.Parse(xAttribute9.Value)) : Panic.Min);
			XAttribute xAttribute10 = xElement16.Attribute("Odd");
			Panic.Odd = ((xAttribute10 == null) ? 1 : int.Parse(xAttribute10.Value));
		}
		else if (value != null)
		{
			Panic = value.Panic;
		}
		XElement xElement17 = xElement.Element("Behavior");
		if (xElement17.IsNullOrEmpty() && value == null)
		{
			CLoggerManager.Log("Enemy " + Id + " must have a valid Behavior element or a template to take it from!", LogType.Error);
			return;
		}
		Behavior = ((xElement17 != null) ? new BehaviorDefinition(xElement17) : value.Behavior);
		XElement xElement18 = xElement.Element("OnDeathBehavior");
		if (xElement18 != null)
		{
			OnDeathBehavior = new BehaviorDefinition(xElement18);
		}
		else if (value != null)
		{
			OnDeathBehavior = value.OnDeathBehavior;
		}
		XElement xElement19 = xElement.Element("StatsProgressions");
		if (xElement19 != null)
		{
			IEnumerable<XElement> enumerable = xElement19?.Elements("StatProgression");
			if (enumerable != null)
			{
				foreach (XElement item4 in enumerable)
				{
					if (Enum.TryParse<UnitStatDefinition.E_Stat>(item4.Attribute("Id").Value, out var result8))
					{
						try
						{
							string text = item4.Attribute("MaxIncreases")?.Value;
							StatProgressions.Add(new StatProgression(result8)
							{
								Delay = byte.Parse(item4.Attribute("Delay")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
								IncreaseEveryXDay = byte.Parse(item4.Attribute("IncreaseEveryXDay")?.Value ?? "1", NumberStyles.Any, CultureInfo.InvariantCulture),
								Value = float.Parse(item4.Value, NumberStyles.Any, CultureInfo.InvariantCulture),
								MaxIncreases = ((text != null) ? int.Parse(text) : int.MaxValue)
							});
						}
						catch (FormatException ex)
						{
							CLoggerManager.Log("Invalid Stat progression supplied for enemy " + Id + " and stat " + result8.ToString() + ":\n" + ex, LogType.Error);
						}
					}
				}
			}
		}
		else if (value != null)
		{
			StatProgressions = value.StatProgressions;
		}
		IEnumerable<XElement> enumerable2 = xElement.Element("SkillProgressions")?.Elements("SkillProgression");
		if (enumerable2 != null)
		{
			foreach (XElement item5 in enumerable2)
			{
				SkillProgressions.Add(SkillProgression.Deserialize(item5));
			}
		}
		else if (value != null)
		{
			SkillProgressions = value.SkillProgressions;
		}
		if (xElement.Element("HideInNightReport") != null)
		{
			HideInNightReport = true;
		}
		else if (value != null)
		{
			HideInNightReport = value.HideInNightReport;
		}
		else
		{
			HideInNightReport = false;
		}
		if (xElement.Element("VisualEvolutions") != null)
		{
			VisualEvolutions = new List<string>();
			foreach (XElement item6 in xElement.Element("VisualEvolutions").Elements("VisualId"))
			{
				XAttribute xAttribute11 = item6.Attribute("Id");
				if (xAttribute11.IsNullOrEmpty())
				{
					CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " VisualId must have a valid Id", LogType.Error);
				}
				else
				{
					VisualEvolutions.Add(xAttribute11.Value);
				}
			}
		}
		else if (value != null)
		{
			VisualEvolutions = value.VisualEvolutions;
		}
		if (xElement.Element("Injuries") != null)
		{
			DeserializeInjuries(xElement.Element("Injuries"));
		}
		else if (value != null)
		{
			base.InjuryDefinitions = value.InjuryDefinitions;
		}
		XElement xElement20 = xElement.Element("VisualOffset");
		if (xElement20 != null)
		{
			XAttribute xAttribute12 = xElement20.Attribute("X");
			XAttribute xAttribute13 = xElement20.Attribute("Y");
			if (!float.TryParse(xAttribute12.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result9))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " VisualOffset X attribute value " + xAttribute12.Value + " could not be parsed to a valid float!", LogType.Error);
				result9 = 0f;
			}
			if (!float.TryParse(xAttribute13.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " VisualOffset Y attribute value " + xAttribute13.Value + " could not be parsed to a valid float!", LogType.Error);
				result10 = 0f;
			}
			VisualOffset = new Vector2(result9, result10);
		}
		else if (value != null)
		{
			VisualOffset = value.VisualOffset;
		}
		XElement xElement21 = xElement.Element("SortingOrderOverride");
		if (xElement21 != null)
		{
			if (!int.TryParse(xElement21.Value, out var result11))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " SortingOrderOverride value " + xElement21.Value + " could not be parsed to a valid int!", LogType.Error);
				SortingOrderOverride = null;
			}
			else
			{
				SortingOrderOverride = result11;
			}
		}
		else if (value != null)
		{
			SortingOrderOverride = value.SortingOrderOverride;
		}
		XElement xElement22 = xElement.Element("LockedOrientation");
		if (xElement22 != null)
		{
			if (!Enum.TryParse<GameDefinition.E_Direction>(xElement22.Value, out var result12))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition " + Id + " LockedOrientation value " + xElement22.Value + " could not be parsed to a valid direction!", LogType.Error);
				LockedOrientation = GameDefinition.E_Direction.None;
			}
			else
			{
				LockedOrientation = result12;
			}
		}
		else if (value != null)
		{
			LockedOrientation = value.LockedOrientation;
		}
		else
		{
			LockedOrientation = GameDefinition.E_Direction.None;
		}
		XElement xElement23 = xElement.Element("DamagedParticles");
		if (xElement23 != null)
		{
			XAttribute xAttribute14 = xElement23.Attribute("Id");
			base.DamagedParticlesId = xAttribute14.Value;
		}
		else if (value != null)
		{
			base.DamagedParticlesId = value.DamagedParticlesId;
		}
		XElement xElement24 = xElement.Element("SpawnCutscene");
		if (xElement24 != null)
		{
			XAttribute xAttribute15 = xElement24.Attribute("Id");
			SpawnCutsceneId = xAttribute15.Value;
		}
		else if (value != null)
		{
			SpawnCutsceneId = value.SpawnCutsceneId;
		}
	}

	public override void DeserializeInjuries(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		base.InjuryDefinitions = new List<InjuryDefinition>();
		foreach (XElement item2 in xElement.Elements("Injury"))
		{
			InjuryDefinition item = new InjuryDefinition(item2, HealthTotal.Min);
			base.InjuryDefinitions.Add(item);
		}
	}

	private void DeserializeAffixes(XElement xAffixes)
	{
		AffixDefinitions = new List<EnemyAffixDefinition>();
		if (xAffixes == null)
		{
			return;
		}
		foreach (XElement item in xAffixes.Elements("Affix"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			AffixDefinitions.Add(EnemyUnitDatabase.EnemyAffixDefinitions[xAttribute.Value]);
		}
	}

	private void FillUnitTemplateStat(XElement xStat, Stat stat, Stat templateStat = null)
	{
		if (xStat != null)
		{
			XAttribute xAttribute = xStat.Attribute("Min");
			stat.Min = ((xAttribute != null) ? int.Parse(xAttribute.Value) : int.Parse(xStat.Value));
			XAttribute xAttribute2 = xStat.Attribute("Max");
			stat.Max = ((xAttribute2 != null) ? ((float)int.Parse(xAttribute2.Value)) : stat.Min);
			XAttribute xAttribute3 = xStat.Attribute("Odd");
			stat.Odd = ((xAttribute3 == null) ? 1 : int.Parse(xAttribute3.Value));
		}
		else if (templateStat != null)
		{
			stat.Min = templateStat.Min;
			stat.Max = templateStat.Max;
			stat.Odd = templateStat.Odd;
		}
	}
}
