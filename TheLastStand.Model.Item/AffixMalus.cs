using System.Collections.Generic;
using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Item;

public class AffixMalus : IAffix
{
	public AffixMalusController AffixMalusController { get; private set; }

	public AffixMalusDefinition AffixMalusDefinition { get; private set; }

	public AffixMalusDefinition.E_MalusLevel MalusLevel { get; set; }

	public AffixMalus(SerializedAffixMalus container, AffixMalusController affixMalusController)
	{
		AffixMalusController = affixMalusController;
		Deserialize(container);
	}

	public AffixMalus(AffixMalusDefinition affixMalusDefinition, AffixMalusController affixMalusController)
	{
		AffixMalusDefinition = affixMalusDefinition;
		AffixMalusController = affixMalusController;
	}

	public void Deserialize(ISerializedData container = null)
	{
		SerializedAffixMalus serializedAffixMalus = container as SerializedAffixMalus;
		MalusLevel = serializedAffixMalus.MalusLevel;
		AffixMalusDefinition = ItemDatabase.AffixMalusDefinitions[serializedAffixMalus.Stat];
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers()
	{
		return new Dictionary<UnitStatDefinition.E_Stat, float> { 
		{
			AffixMalusDefinition.Stat,
			AffixMalusDefinition.MalusPerLevel[MalusLevel]
		} };
	}

	public ISerializedData Serialize()
	{
		return new SerializedAffixMalus
		{
			MalusLevel = MalusLevel,
			Stat = AffixMalusDefinition.Stat
		};
	}
}
