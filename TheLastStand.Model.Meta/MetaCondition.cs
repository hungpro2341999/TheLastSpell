using System;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Meta;

namespace TheLastStand.Model.Meta;

public class MetaCondition : ISerializable, IDeserializable
{
	public enum E_MetaConditionCategory
	{
		Unlock,
		Activation
	}

	public int OccurenceProgression;

	public bool HasBeenChecked;

	public MetaConditionController MetaConditionController { get; set; }

	public MetaConditionDefinition MetaConditionDefinition { get; set; }

	public string Id { get; set; }

	public int ConditionsGroupIndex => MetaConditionDefinition.ConditionsGroupIndex;

	public E_MetaConditionCategory Category { get; set; }

	public MetaConditionSpecificContext LocalContext { get; set; }

	public MetaUpgradeController MetaUpgradeController { get; }

	public MetaCondition(MetaUpgradeController upgradeController)
	{
		MetaUpgradeController = upgradeController;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedMetaCondition serializedMetaCondition)
		{
			Id = serializedMetaCondition.Id;
			LocalContext = serializedMetaCondition.LocalContext.Context;
			OccurenceProgression = serializedMetaCondition.OccurenceProgression;
			Category = serializedMetaCondition.Category;
			HasBeenChecked = serializedMetaCondition.HasBeenChecked;
			return;
		}
		throw new Exception("Attempted to deserialize a MetaCondition with no data: Should not happen! Please do not try to deserialize NULL into MetaConditions");
	}

	public ISerializedData Serialize()
	{
		return new SerializedMetaCondition
		{
			MetaUpgradeId = MetaUpgradeController.MetaUpgrade.MetaUpgradeDefinition.Id,
			Id = Id,
			LocalContext = new SerializedMetaConditionsContext
			{
				Context = LocalContext
			},
			OccurenceProgression = OccurenceProgression,
			HasBeenChecked = HasBeenChecked,
			Category = Category
		};
	}

	public override string ToString()
	{
		return $"[{Category}] {MetaConditionDefinition}";
	}
}
