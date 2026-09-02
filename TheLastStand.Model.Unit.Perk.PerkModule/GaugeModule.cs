using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Serialization.Perk;
using UnityEngine;

namespace TheLastStand.Model.Unit.Perk.PerkModule;

public class GaugeModule : BufferModule
{
	private int valueOffset;

	public bool HasBeenUsed;

	public int ValueOffset
	{
		get
		{
			return Mathf.Clamp(valueOffset, 0, GaugeMaxValue);
		}
		set
		{
			valueOffset = Mathf.Clamp(value, 0, GaugeMaxValue);
		}
	}

	public int GaugeValue => GaugeMaxValue - ValueOffset;

	public int GaugeMaxValue => GaugeModuleDefinition.GaugeValue.EvalToInt(base.Perk);

	public GaugeModuleDefinition GaugeModuleDefinition => base.PerkModuleDefinition as GaugeModuleDefinition;

	public UnitStatDefinition.E_Stat GaugeStat => GaugeModuleDefinition.GaugeStat;

	public GaugeModule(BufferModuleDefinition perkModuleDefinition, Perk perk)
		: base(perkModuleDefinition, perk)
	{
	}

	public override void OnUnlock(bool onLoad)
	{
		List<GaugeModule> perkGaugeModules = base.Perk.Owner.PlayableUnitStatsController.GetStat(GaugeStat).PerkGaugeModules;
		if (!perkGaugeModules.Contains(this))
		{
			perkGaugeModules.Add(this);
		}
		UnitStatDefinition.E_Stat parentStatIfExists = UnitDatabase.UnitStatDefinitions[GaugeStat].GetParentStatIfExists();
		if (parentStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			perkGaugeModules = base.Perk.Owner.PlayableUnitStatsController.GetStat(parentStatIfExists).PerkGaugeModules;
			if (!perkGaugeModules.Contains(this))
			{
				perkGaugeModules.Add(this);
			}
		}
	}

	public override void Lock(bool onLoad)
	{
		List<GaugeModule> perkGaugeModules = base.Perk.Owner.PlayableUnitStatsController.GetStat(GaugeStat).PerkGaugeModules;
		if (perkGaugeModules.Contains(this))
		{
			perkGaugeModules.Remove(this);
		}
		UnitStatDefinition.E_Stat parentStatIfExists = UnitDatabase.UnitStatDefinitions[GaugeStat].GetParentStatIfExists();
		if (parentStatIfExists != UnitStatDefinition.E_Stat.Undefined)
		{
			perkGaugeModules = base.Perk.Owner.PlayableUnitStatsController.GetStat(parentStatIfExists).PerkGaugeModules;
			if (perkGaugeModules.Contains(this))
			{
				perkGaugeModules.Remove(this);
			}
		}
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		base.Deserialize(container, saveVersion);
		if (container is SerializedGaugeModule serializedGaugeModule)
		{
			valueOffset = serializedGaugeModule.ValueOffest;
		}
	}

	public override ISerializedData Serialize()
	{
		return new SerializedGaugeModule
		{
			Buffer = base.Buffer,
			Buffer2 = base.Buffer2,
			Buffer3 = base.Buffer3,
			Buffer4 = base.Buffer4,
			Buffer5 = base.Buffer5,
			Buffer6 = base.Buffer6,
			Buffer7 = base.Buffer7,
			Buffer8 = base.Buffer8,
			Buffer9 = base.Buffer9,
			ValueOffest = valueOffset
		};
	}
}
