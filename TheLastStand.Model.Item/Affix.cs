using System.Collections.Generic;
using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Item;

public class Affix : IAffix
{
	public AffixController AffixController { get; private set; }

	public AffixDefinition AffixDefinition { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, float> EpicStatModifiers
	{
		get
		{
			if (!IsEpic)
			{
				return null;
			}
			return AffixDefinition.EpicStatModifiers;
		}
	}

	public bool IsEpic { get; set; }

	public int Level { get; set; } = 1;

	public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers => AffixDefinition.LevelDefinitions[Level].StatModifiers;

	public Affix(SerializedAffix container, AffixController affixController)
	{
		AffixController = affixController;
		Deserialize(container);
	}

	public Affix(AffixDefinition affixDefinition, AffixController affixController)
	{
		AffixController = affixController;
		AffixDefinition = affixDefinition;
	}

	public void Deserialize(ISerializedData container = null)
	{
		SerializedAffix serializedAffix = container as SerializedAffix;
		AffixDefinition = ItemDatabase.AffixDefinitions[serializedAffix.Id];
		IsEpic = serializedAffix.IsEpic;
		Level = serializedAffix.Level;
	}

	public Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers()
	{
		if (!IsEpic)
		{
			return StatModifiers;
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>(StatModifiers);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> epicStatModifier in EpicStatModifiers)
		{
			if (dictionary.ContainsKey(epicStatModifier.Key))
			{
				dictionary[epicStatModifier.Key] += epicStatModifier.Value;
			}
		}
		return dictionary;
	}

	public ISerializedData Serialize()
	{
		return new SerializedAffix
		{
			Id = AffixDefinition.Id,
			IsEpic = IsEpic,
			Level = Level
		};
	}
}
