using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Stat;

public abstract class UnitStatsController
{
	private bool childStatsLinked;

	private bool parentStatsLinked;

	public UnitStats UnitStats { get; set; }

	public UnitStatsController(TheLastStand.Model.Unit.Unit unit)
	{
		CreateModel(unit);
		Init();
	}

	public abstract void Init();

	public virtual void InitStat(UnitStatDefinition.E_Stat stat, float value)
	{
		if (!UnitStats.Stats.ContainsKey(stat))
		{
			UnitStats.Stats.Add(stat, new UnitStat(this, stat, UnitDatabase.UnitStatDefinitions[stat].Boundaries[UnitStats.UnitType]));
		}
		SetBaseStat(stat, value);
	}

	public virtual void InitStat(ISerializedData container)
	{
		SerializedUnitStat serializedUnitStat = container as SerializedUnitStat;
		InitStat(serializedUnitStat.StatId, serializedUnitStat.Base);
	}

	public UnitStat GetStat(UnitStatDefinition.E_Stat stat)
	{
		return UnitStats.Stats[stat];
	}

	public float ClampToBoundaries(float value, UnitStatDefinition.E_Stat stat)
	{
		return value.Clamp(GetStat(stat).Boundaries);
	}

	public virtual float ComputeStatBonus(UnitStatDefinition.E_Stat stat, bool withoutStatus = false)
	{
		float num = (withoutStatus ? 0f : GetStat(stat).Status);
		if (stat != UnitStatDefinition.E_Stat.Health && stat != UnitStatDefinition.E_Stat.HealthTotal)
		{
			num += GetStat(stat).Injuries;
		}
		return num;
	}

	public virtual float IncreaseBaseStat(UnitStatDefinition.E_Stat stat, float value, bool includeChildStat, bool refreshHud = true)
	{
		float num = GetStat(stat).Base;
		SetBaseStat(stat, GetStat(stat).Base + value, includeChildStat, refreshHud);
		return GetStat(stat).Base - num;
	}

	public virtual float DecreaseBaseStat(UnitStatDefinition.E_Stat stat, float value, bool includeChildStat, bool refreshHud = true)
	{
		float num = GetStat(stat).Base;
		SetBaseStat(stat, num - value, includeChildStat, refreshHud);
		return num - GetStat(stat).Base;
	}

	public void SetBaseStat(UnitStatDefinition.E_Stat statType, float value, bool updateChildStat = false, bool refreshHud = true)
	{
		UnitStat stat = GetStat(statType);
		Vector2 baseBoundaries = stat.BaseBoundaries;
		value = value.Clamp(baseBoundaries);
		bool num = updateChildStat && UnitDatabase.UnitStatDefinitions[statType].ChildStatId != UnitStatDefinition.E_Stat.Undefined;
		float num2 = 0f;
		if (num)
		{
			num2 = stat.FinalClamped;
		}
		stat.Base = value;
		if (num)
		{
			float num3 = stat.FinalClamped - num2;
			UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[statType].ChildStatId;
			UnitStat stat2 = GetStat(childStatId);
			Vector2 baseBoundaries2 = stat2.BaseBoundaries;
			stat2.Base += num3;
			stat2.Base = stat2.Base.Clamp(baseBoundaries2);
		}
		if (refreshHud && UnitStats.Unit.UnitView != null)
		{
			UnitStats.Unit.UnitView.RefreshHud(statType);
		}
	}

	public void SnapBaseStatTo(UnitStatDefinition.E_Stat target, UnitStatDefinition.E_Stat source)
	{
		SetBaseStat(target, GetStat(source).FinalClamped);
	}

	public virtual void OnStatusAdded(TheLastStand.Model.Status.Status status)
	{
		if (status is StatModifierStatus statModifierStatus)
		{
			if (statModifierStatus.IsFromInjury)
			{
				GetStat(statModifierStatus.Stat).InjuriesStatuses += statModifierStatus.ModifierValue;
			}
			else
			{
				GetStat(statModifierStatus.Stat).Status += statModifierStatus.ModifierValue;
			}
		}
	}

	public virtual void OnStatusRemoved(TheLastStand.Model.Status.Status status)
	{
		if (status is StatModifierStatus statModifierStatus)
		{
			GetStat(statModifierStatus.Stat).Status -= statModifierStatus.ModifierValue;
		}
	}

	public virtual void AddInjury(InjuryDefinition injuryDefinition)
	{
		UnitStats.InjuryStage++;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> statModifier in injuryDefinition.StatModifiers)
		{
			GetStat(statModifier.Key).Injuries += statModifier.Value;
			UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[statModifier.Key].ChildStatId;
			if (childStatId != UnitStatDefinition.E_Stat.Undefined)
			{
				if (statModifier.Value > 0f)
				{
					GetStat(childStatId).Base += statModifier.Value;
				}
				else
				{
					SetBaseStat(childStatId, GetStat(childStatId).Base, updateChildStat: false, refreshHud: false);
				}
			}
		}
		if (UnitStats.Unit is EnemyUnit && ApocalypseManager.CurrentApocalypse.EnemiesInjuryStageInjuryStatModifiers.ContainsKey(UnitStats.InjuryStage))
		{
			foreach (ModifyEnemiesInjuryStageApocalypseEffectDefinition.ApocalypseInjuryStatModifier item in ApocalypseManager.CurrentApocalypse.EnemiesInjuryStageInjuryStatModifiers[UnitStats.InjuryStage])
			{
				if (item.OverrideValue)
				{
					GetStat(item.StatType).Injuries = item.Value;
				}
				else
				{
					GetStat(item.StatType).Injuries += item.Value;
				}
			}
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, InjuryDefinition.E_ValueMultiplier> statMultiplier in injuryDefinition.StatMultipliers)
		{
			GetStat(statMultiplier.Key).InjuriesValueMultiplier = statMultiplier.Value;
			UnitStatDefinition.E_Stat childStatId2 = UnitDatabase.UnitStatDefinitions[statMultiplier.Key].ChildStatId;
			if (childStatId2 != UnitStatDefinition.E_Stat.Undefined)
			{
				SetBaseStat(childStatId2, GetStat(childStatId2).Base, updateChildStat: false, refreshHud: false);
			}
		}
		UnitStats.Unit.PreventedSkillsIds.AddRange(injuryDefinition.PreventedSkillsIds);
	}

	public void ComputePreventedSkillsByGroupId()
	{
		foreach (string preventedSkillsId in UnitStats.Unit.PreventedSkillsIds)
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(preventedSkillsId, out var value))
			{
				if (!UnitStats.Unit.PreventedSkillsByGroupId.ContainsKey(value.GroupId))
				{
					UnitStats.Unit.PreventedSkillsByGroupId.Add(value.GroupId, new List<SkillDefinition> { value });
				}
				else
				{
					UnitStats.Unit.PreventedSkillsByGroupId[value.GroupId].Add(value);
				}
			}
		}
	}

	public virtual void ResetInjuries()
	{
		UnitStats.InjuryStage = 0;
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStat> stat in UnitStats.Stats)
		{
			stat.Value.Injuries = 0f;
			stat.Value.InjuriesValueMultiplier = InjuryDefinition.E_ValueMultiplier.None;
		}
		UnitStats.Unit.PreventedSkillsIds.Clear();
		UnitStats.Unit.PreventedSkillsByGroupId.Clear();
	}

	protected abstract void CreateModel(TheLastStand.Model.Unit.Unit unit);

	protected void LinkChildStats()
	{
		if (childStatsLinked)
		{
			return;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStat> stat in UnitStats.Stats)
		{
			UnitStatDefinition.E_Stat childStatIfExists = UnitDatabase.UnitStatDefinitions[stat.Key].GetChildStatIfExists();
			if (stat.Key != childStatIfExists)
			{
				stat.Value.ChildStat = UnitStats.Stats[childStatIfExists];
			}
		}
		childStatsLinked = true;
	}

	protected void LinkParentStats()
	{
		if (parentStatsLinked)
		{
			return;
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStat> stat in UnitStats.Stats)
		{
			UnitStatDefinition.E_Stat parentStatIfExists = UnitDatabase.UnitStatDefinitions[stat.Key].GetParentStatIfExists();
			if (stat.Key != parentStatIfExists)
			{
				stat.Value.ParentStat = UnitStats.Stats[parentStatIfExists];
			}
		}
		parentStatsLinked = true;
	}

	protected abstract void InitStats();
}
