using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;

namespace TheLastStand.View.Skill.SkillAction.SkillActionExecution;

public class ResupplySkillActionExecutionView : SkillActionExecutionView
{
	public List<TheLastStand.Model.Building.Building> BuildingHUDDisplayed = new List<TheLastStand.Model.Building.Building>();

	public ResupplySkillActionExecution ResupplySkillActionExecution => base.SkillExecution as ResupplySkillActionExecution;

	public void DisplayTargetingBuildingsHUD()
	{
		BuildingHUDDisplayed.Clear();
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>();
		if (ResupplySkillActionExecution.ResupplySkillActionDefinition.TryGetAllEffects("ResupplyCharges", out List<SkillEffectDefinition> effects) && effects.Count != 0)
		{
			foreach (string item in (effects[0] as ResupplyChargesSkillEffectDefinition).TargetIds)
			{
				foreach (TheLastStand.Model.Building.Building item3 in TPSingleton<BuildingManager>.Instance.Buildings.FindAll((TheLastStand.Model.Building.Building x) => x.Id == item))
				{
					if (item3.BattleModule != null && item3.BattleModule.RemainingTrapCharges < item3.BuildingDefinition.BattleModuleDefinition.MaximumTrapCharges)
					{
						list.Add(item3);
					}
				}
			}
		}
		if (ResupplySkillActionExecution.ResupplySkillActionDefinition.TryGetAllEffects("ResupplyOverallUses", out List<SkillEffectDefinition> effects2) && effects2.Count != 0)
		{
			foreach (string item2 in (effects2[0] as ResupplyOverallUsesSkillEffectDefinition).TargetIds)
			{
				foreach (TheLastStand.Model.Building.Building item4 in TPSingleton<BuildingManager>.Instance.Buildings.FindAll((TheLastStand.Model.Building.Building x) => x.Id == item2))
				{
					if (item4.BattleModule == null)
					{
						continue;
					}
					foreach (TheLastStand.Model.Skill.Skill skill in item4.BattleModule.Skills)
					{
						if (skill != null && skill.OverallUses != 0 && skill.OverallUsesRemaining < skill.OverallUses)
						{
							list.Add(item4);
							break;
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		TPSingleton<EnemyUnitManager>.Instance.ForceHideHUD();
		foreach (TheLastStand.Model.Building.Building item5 in list)
		{
			item5.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
			BuildingHUDDisplayed.Add(item5);
		}
	}

	public void HideDisplayedHUD()
	{
		foreach (TheLastStand.Model.Building.Building item in BuildingHUDDisplayed)
		{
			item.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: false);
		}
		BuildingHUDDisplayed.Clear();
		TPSingleton<EnemyUnitManager>.Instance.ForceRefreshHUD();
	}

	protected override void DisplayAreaOfEffectTileFeedback(Tile affectedTile, bool isSurroundingEffect)
	{
	}
}
