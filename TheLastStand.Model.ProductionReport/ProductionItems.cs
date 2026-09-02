using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;

namespace TheLastStand.Model.ProductionReport;

public class ProductionItems : ProductionObject, ISerializable, IDeserializable
{
	private int productionLevel;

	private string buildingActionId;

	private int createItemIndex = -1;

	public TheLastStand.Model.Item.Item ChosenItem { get; set; }

	public CreateItemDefinition CreateItemDefinition { get; private set; }

	public bool IsNightProduction { get; set; }

	public List<TheLastStand.Model.Item.Item> Items { get; } = new List<TheLastStand.Model.Item.Item>();

	public LevelProbabilitiesTreeController LevelProbabilitiesTreeController { get; private set; }

	public ProductionItems(ISerializedData container, ProductionItemController productionObjectController)
		: base(productionObjectController)
	{
		Deserialize(container);
		InitProduction();
	}

	public ProductionItems(ProductionItemController productionObjectController, BuildingDefinition productionBuilding, int productionLevel = -1, string buildingActionId = "", int createItemIndex = -1)
		: base(productionObjectController, productionBuilding)
	{
		this.productionLevel = productionLevel;
		this.buildingActionId = buildingActionId;
		this.createItemIndex = createItemIndex;
		InitProduction();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedProductionItems serializedProductionItems = container as SerializedProductionItems;
		if (string.IsNullOrEmpty(serializedProductionItems.Id))
		{
			TPSingleton<ItemManager>.Instance.LogWarning("SerializedProductionItems ID is null or empty: could not deserialize production item. This should be a save version compatibility issue.", CLogLevel.DETAILED);
			return;
		}
		base.ProductionBuildingDefinition = ((serializedProductionItems.Id != "NightRewardItems") ? BuildingDatabase.BuildingDefinitions[serializedProductionItems.Id] : null);
		foreach (SerializedItem item2 in serializedProductionItems.Items)
		{
			TheLastStand.Model.Item.Item item = new ItemController(item2, null).Item;
			Items.Add(item);
		}
		IsNightProduction = serializedProductionItems.IsNightProduction;
		productionLevel = serializedProductionItems.ProductionLevel;
		buildingActionId = serializedProductionItems.BuildingActionId;
		createItemIndex = serializedProductionItems.CreateItemIndex;
	}

	public ISerializedData Serialize()
	{
		SerializedProductionItems serializedProductionItems = new SerializedProductionItems
		{
			Id = ((base.ProductionBuildingDefinition != null) ? base.ProductionBuildingDefinition.Id : "NightRewardItems"),
			IsNightProduction = IsNightProduction,
			ProductionLevel = productionLevel,
			BuildingActionId = buildingActionId,
			CreateItemIndex = createItemIndex
		};
		for (int i = 0; i < Items.Count; i++)
		{
			serializedProductionItems.Items.Add(Items[i].Serialize() as SerializedItem);
		}
		return serializedProductionItems;
	}

	private void InitProduction()
	{
		if (base.ProductionBuildingDefinition != null)
		{
			RetrieveCreateItemDefinition();
			if (ItemDatabase.ItemGenerationModifierListDefinitions.TryGetValue(CreateItemDefinition.LevelModifierListId, out var value))
			{
				LevelProbabilitiesTreeController = new LevelProbabilitiesTreeController(productionLevel, value);
			}
			else
			{
				TPSingleton<ItemManager>.Instance.LogError("ProbabilityTreeEntriesDefinition not found with " + CreateItemDefinition.LevelModifierListId + " !");
			}
		}
	}

	private void RetrieveCreateItemDefinition()
	{
		if (base.ProductionBuildingDefinition.ProductionModuleDefinition.BuildingGaugeEffectDefinition is CreateItemGaugeEffectDefinition createItemGaugeEffectDefinition)
		{
			CreateItemDefinition = createItemGaugeEffectDefinition.CreateItemDefinition;
		}
		else
		{
			if (string.IsNullOrEmpty(buildingActionId))
			{
				return;
			}
			BuildingActionDefinition buildingActionDefinition = base.ProductionBuildingDefinition.ProductionModuleDefinition.BuildingActionDefinitions.FirstOrDefault((BuildingActionDefinition o) => o.Id == buildingActionId);
			if (buildingActionDefinition == null)
			{
				return;
			}
			foreach (BuildingActionEffectDefinition item in buildingActionDefinition.BuildingActionEffectDefinition)
			{
				if (item is ScavengeBuildingActionEffectDefinition scavengeBuildingActionEffectDefinition && scavengeBuildingActionEffectDefinition.CreateItemDefinitions.Count != 0)
				{
					CreateItemDefinition = scavengeBuildingActionEffectDefinition.CreateItemDefinitions[(createItemIndex > -1) ? createItemIndex : 0];
					break;
				}
			}
		}
	}
}
