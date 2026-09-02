using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class GenerateNewItemsRosterController : BuildingPassiveEffectController
{
	public List<CreateRosterItemController> CreateRosterItemControllers { get; } = new List<CreateRosterItemController>();

	public GenerateNewItemsRoster GenerateNewItemsRoster => base.BuildingPassiveEffect as GenerateNewItemsRoster;

	public GenerateNewItemsRosterController(PassivesModule buildingPassivesModule, GenerateNewItemsRosterDefinition buildingPassiveDefinition)
	{
		buildingPassivesModule.BuildingParent.TryCreateEmptyProductionModule();
		base.BuildingPassiveEffect = new GenerateNewItemsRoster(buildingPassivesModule, buildingPassiveDefinition, this);
		foreach (CreateRosterItemDefinition createItemRosterDefinition in buildingPassiveDefinition.CreateItemRosterDefinitions)
		{
			CreateRosterItemControllers.Add(new CreateRosterItemController(buildingPassivesModule.BuildingParent.ProductionModule, createItemRosterDefinition));
		}
	}

	public override void Apply()
	{
		GenerateItems();
	}

	private void GenerateItems()
	{
		TPSingleton<ItemManager>.Instance.Log("#" + GetType().Name + ".#About to generate items for building " + base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id + " !");
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.ClearItems();
		int i = 0;
		for (int count = CreateRosterItemControllers.Count; i < count; i++)
		{
			Node count2 = CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.Count;
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<CreateItemModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int j = 0; j < effects.Length; j++)
				{
					if (CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.HasID && effects[j].CreateItemId == CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.Id)
					{
						count2 = effects[j].Count;
						break;
					}
				}
			}
			int num = count2.EvalToInt(new ItemInterpreterContext());
			for (int k = 0; k < num; k++)
			{
				int num2 = CreateRosterItemControllers[i].CreateRosterItem.GenerationProbabilitiesTree.GenerateLevel();
				TPSingleton<ItemManager>.Instance.Log($"#{GetType().Name}.#Generating one item of index {k} and level {num2}");
				ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.Shop, CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition, num2);
			}
		}
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.SortItems();
	}
}
