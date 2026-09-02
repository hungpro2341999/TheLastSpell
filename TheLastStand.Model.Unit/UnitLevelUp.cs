using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit;

public class UnitLevelUp : ISerializable, IDeserializable
{
	public enum E_StatLevelUpRarity
	{
		SmallRarity,
		MediumRarity,
		BigRarity
	}

	public struct SelectedStatToLevelUp
	{
		public UnitLevelUpStatDefinition Definition;

		public E_StatLevelUpRarity RarityLevel;

		public E_StatLevelUpRarity BonusIndex => RarityLevel;

		public SelectedStatToLevelUp(UnitLevelUpStatDefinition statDefinition, E_StatLevelUpRarity statLevelUpRarity)
		{
			Definition = statDefinition;
			RarityLevel = statLevelUpRarity;
		}

		public override bool Equals(object obj)
		{
			SelectedStatToLevelUp? selectedStatToLevelUp = obj as SelectedStatToLevelUp?;
			if (selectedStatToLevelUp.HasValue && Definition.Stat == selectedStatToLevelUp.Value.Definition.Stat)
			{
				return RarityLevel == selectedStatToLevelUp.Value.RarityLevel;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public static string SerializationElementName => "LevelUp";

	public List<SelectedStatToLevelUp> AvailableMainStats { get; set; } = new List<SelectedStatToLevelUp>();

	public List<SelectedStatToLevelUp> AvailableSecondaryStats { get; set; } = new List<SelectedStatToLevelUp>();

	public bool HasSelectedStat { get; set; }

	public int CommonNbReroll { get; set; }

	public int MainNbReroll { get; set; }

	public int SecondaryNbReroll { get; set; }

	public int SinkNbReroll { get; set; }

	public int DamnedSoulsRerollCost => TPSingleton<SinkManager>.Instance.AttributeSinkData.GetFinalPrice();

	public PlayableUnit PlayableUnit { get; set; }

	public SelectedStatToLevelUp SelectedStat { get; set; }

	public UnitLevelUpController UnitLevelUpController { get; private set; }

	public UnitLevelUpDefinition UnitLevelUpDefinition { get; private set; }

	public UnitLevelUp(UnitLevelUpDefinition definition, SerializedLevelUpBonuses container, UnitLevelUpController controller)
	{
		UnitLevelUpDefinition = definition;
		UnitLevelUpController = controller;
		Deserialize(container);
	}

	public UnitLevelUp(UnitLevelUpDefinition definition, UnitLevelUpController controller)
	{
		UnitLevelUpDefinition = definition;
		UnitLevelUpController = controller;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedLevelUpBonuses serializedLevelUpBonuses = container as SerializedLevelUpBonuses;
		CommonNbReroll = serializedLevelUpBonuses.CommonNbReroll;
		MainNbReroll = serializedLevelUpBonuses.MainNbReroll;
		SecondaryNbReroll = serializedLevelUpBonuses.SecondaryNbReroll;
		SinkNbReroll = serializedLevelUpBonuses.SinkNbReroll;
		AvailableMainStats = serializedLevelUpBonuses.AvailableMainBonuses.Select((SerializedLevelUpBonuses.SerializedLevelUpPotentialBonus o) => new SelectedStatToLevelUp(PlayableUnitDatabase.UnitLevelUpMainStatDefinitions[o.Stat], o.BonusIndex)).ToList();
		AvailableSecondaryStats = serializedLevelUpBonuses.AvailableSecondaryBonuses.Select((SerializedLevelUpBonuses.SerializedLevelUpPotentialBonus o) => new SelectedStatToLevelUp(PlayableUnitDatabase.UnitLevelUpSecondaryStatDefinitions[o.Stat], o.BonusIndex)).ToList();
	}

	public ISerializedData Serialize()
	{
		SerializedLevelUpBonuses serializedLevelUpBonuses = new SerializedLevelUpBonuses
		{
			CommonNbReroll = CommonNbReroll,
			MainNbReroll = MainNbReroll,
			SecondaryNbReroll = SecondaryNbReroll,
			SinkNbReroll = SinkNbReroll
		};
		for (int i = 0; i < AvailableMainStats.Count; i++)
		{
			serializedLevelUpBonuses.AvailableMainBonuses.Add(new SerializedLevelUpBonuses.SerializedLevelUpPotentialBonus
			{
				BonusIndex = AvailableMainStats[i].BonusIndex,
				Stat = AvailableMainStats[i].Definition.Stat
			});
		}
		for (int j = 0; j < AvailableSecondaryStats.Count; j++)
		{
			serializedLevelUpBonuses.AvailableSecondaryBonuses.Add(new SerializedLevelUpBonuses.SerializedLevelUpPotentialBonus
			{
				BonusIndex = AvailableSecondaryStats[j].BonusIndex,
				Stat = AvailableSecondaryStats[j].Definition.Stat
			});
		}
		return serializedLevelUpBonuses;
	}
}
