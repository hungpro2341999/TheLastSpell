using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class CreateItemDisplay : AppearingEffectDisplay
{
	[SerializeField]
	private Image itemDisplay;

	public void Init(ItemDefinition itemDefinition)
	{
		base.Init();
		itemDisplay.sprite = ItemView.GetUiSprite(itemDefinition.ArtId);
	}

	public void Init(ProductionItems productionItem)
	{
		base.Init();
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_" + productionItem.ProductionBuildingDefinition.Id);
		if (sprite != null)
		{
			itemDisplay.sprite = sprite;
		}
	}
}
