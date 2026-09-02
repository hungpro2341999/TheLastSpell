using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Unit;

namespace TheLastStand.Model.Unit.Enemy;

public class EliteEnemyUnit : EnemyUnit
{
	public class StringToEliteEnemyUnitTemplateIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.Keys);
	}

	public class StringToEliteAffixIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(EnemyUnitDatabase.EnemyAffixDefinitions.Keys);
	}

	public override string Description
	{
		get
		{
			if (Localizer.TryGet("EliteEnemyDescription_" + EliteEnemyUnitTemplateDefinition.EliteId, out var value))
			{
				return value;
			}
			return Localizer.Get("EnemyDescription_" + EliteEnemyUnitTemplateDefinition.Id);
		}
	}

	public EliteEnemyUnitController EliteEnemyUnitController => base.UnitController as EliteEnemyUnitController;

	public EliteEnemyUnitStatsController EliteEnemyUnitStatsController => base.UnitStatsController as EliteEnemyUnitStatsController;

	public EliteEnemyUnitTemplateDefinition EliteEnemyUnitTemplateDefinition { get; private set; }

	public override string Name
	{
		get
		{
			if (Localizer.TryGet("EliteEnemyName_" + EliteEnemyUnitTemplateDefinition.EliteId, out var value))
			{
				return value;
			}
			return Localizer.Get("EnemyName_" + EliteEnemyUnitTemplateDefinition.Id);
		}
	}

	public override string SpecificId => EliteEnemyUnitTemplateDefinition.EliteId;

	public EliteEnemyUnit(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, EliteEnemyUnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, EnemyAffixDefinition enemyAffixDefinition = null)
		: base(eliteEnemyUnitTemplateDefinition, unitController, unitView, unitCreationSettings)
	{
		EliteEnemyUnitTemplateDefinition = eliteEnemyUnitTemplateDefinition;
		if (enemyAffixDefinition == null)
		{
			PickRandomAffix();
		}
		else
		{
			base.Affixes.Insert(0, CreateAffix(enemyAffixDefinition));
		}
	}

	public EliteEnemyUnit(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition, SerializedEliteEnemyUnit serializedEliteEnemyUnit, EliteEnemyUnitController unitController, UnitView unitView, UnitCreationSettings unitCreationSettings, int saveVersion)
		: base(eliteEnemyUnitTemplateDefinition, unitController, unitView, unitCreationSettings)
	{
		EliteEnemyUnitTemplateDefinition = eliteEnemyUnitTemplateDefinition;
		Deserialize(serializedEliteEnemyUnit, saveVersion);
	}

	private void PickRandomAffix()
	{
		if (!EnemyUnitDatabase.EliteToAffixDefinitions.TryGetValue(SpecificId, out var value))
		{
			return;
		}
		List<EnemyAffixDefinition> list = new List<EnemyAffixDefinition>();
		foreach (EnemyAffixDefinition item in value)
		{
			bool flag = true;
			if (!string.IsNullOrEmpty(item.LockedByApocalypseFlag))
			{
				flag = !ApocalypseManager.CurrentApocalypse.AffixesFlags.Contains(item.LockedByApocalypseFlag);
			}
			if (!string.IsNullOrEmpty(item.UnlockedByApocalypseFlag))
			{
				flag = ApocalypseManager.CurrentApocalypse.AffixesFlags.Contains(item.UnlockedByApocalypseFlag);
			}
			if (flag)
			{
				list.Add(item);
			}
		}
		float totalWeight = 0f;
		list.ForEach(delegate(EnemyAffixDefinition filteredAffix)
		{
			totalWeight += filteredAffix.Weight;
		});
		float randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0f, totalWeight);
		for (int num = 0; num < list.Count; num++)
		{
			totalWeight -= list[num].Weight;
			if (totalWeight <= randomRange)
			{
				base.Affixes.Insert(0, CreateAffix(list[num]));
				return;
			}
		}
		if (list.Count > 0)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogWarning("Something went wrong with the affix picking algorithm, ask Matthieu H to fix this.");
			base.Affixes.Insert(0, CreateAffix(list[0]));
		}
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEliteEnemyUnit serializedEliteEnemyUnit = container as SerializedEliteEnemyUnit;
		if (EnemyUnitDatabase.EliteToAffixDefinitions.TryGetValue(SpecificId, out var value) && value.TryFind((EnemyAffixDefinition x) => x.Id == serializedEliteEnemyUnit.AffixId, out var value2))
		{
			base.Affixes.Insert(0, CreateAffix(value2));
		}
		else
		{
			PickRandomAffix();
		}
		ADeserialize(serializedEliteEnemyUnit, saveVersion);
	}

	public override void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
		DeserializeStats((container as SerializedEliteEnemyUnit)?.EliteEnemyUnitStats, saveVersion);
	}

	public override void DeserializeStats(ISerializedData serializedUnitStats, int saveVersion)
	{
		SerializedEliteEnemyUnitStats serializedUnitStats2 = serializedUnitStats as SerializedEliteEnemyUnitStats;
		base.UnitStatsController = new EliteEnemyUnitStatsController(serializedUnitStats2, this, saveVersion);
	}

	public override ISerializedData Serialize()
	{
		return new SerializedEliteEnemyUnit(SerializeUnit())
		{
			Id = EliteEnemyUnitTemplateDefinition.EliteId,
			BossPhaseActorId = base.BossPhaseActorId,
			OverrideVariantId = base.CurrentVariantIndex,
			LinkedBuilding = LinkedBuilding?.RandomId,
			IsGuardian = base.IsGuardian,
			IgnoreFromEnemyUnitsCount = base.IgnoreFromEnemyUnitsCount,
			LastHourInFog = base.LastHourInFog,
			LastHourInAnyFog = base.LastHourInAnyFog,
			SerializedBehavior = new SerializedBehavior(this),
			AffixId = base.Affixes[0].EnemyAffixDefinition.Id,
			EliteEnemyUnitStats = (EliteEnemyUnitStatsController.EliteEnemyUnitStats.Serialize() as SerializedEliteEnemyUnitStats)
		};
	}
}
