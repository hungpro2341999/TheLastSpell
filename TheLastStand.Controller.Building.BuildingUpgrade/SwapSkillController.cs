using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Definition.Skill;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class SwapSkillController : BuildingUpgradeEffectController
{
	public SwapSkill SwapSkill => base.BuildingUpgradeEffect as SwapSkill;

	public SwapSkillController(SwapSkillDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new SwapSkill(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		if (!SkillDatabase.SkillDefinitions.TryGetValue(SwapSkill.SwapSkillDefinition.NewSkillId, out var value))
		{
			TPSingleton<SkillManager>.Instance.LogError("Skill " + SwapSkill.SwapSkillDefinition.NewSkillId + " not found!", CLogLevel.MAJOR);
		}
		TheLastStand.Model.Building.Building building = base.BuildingUpgradeEffect.BuildingUpgrade.Building;
		int overallUsesCount = -1;
		if (SwapSkill.SwapSkillDefinition.OverallUsesCount == 0)
		{
			TheLastStand.Model.Skill.Skill skill = null;
			if (building.BattleModule?.Skills != null)
			{
				skill = building.BattleModule.Skills.Find((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId);
			}
			if (building.BattleModule?.Goals != null)
			{
				using IEnumerator<Goal> enumerator = building.BattleModule.Goals.Where((Goal x) => x.Skill.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId).GetEnumerator();
				if (enumerator.MoveNext())
				{
					skill = enumerator.Current.Skill;
				}
			}
			if (skill != null)
			{
				overallUsesCount = skill.OverallUses;
			}
		}
		else
		{
			overallUsesCount = SwapSkill.SwapSkillDefinition.OverallUsesCount;
		}
		SkillDefinition skillDefinition = value;
		BattleModule battleModule = building.BattleModule;
		int usesPerTurnCount = value.UsesPerTurnCount;
		TheLastStand.Model.Skill.Skill skill2 = new SkillController(skillDefinition, battleModule, overallUsesCount, usesPerTurnCount).Skill;
		if (building.BattleModule?.Skills != null)
		{
			building.BattleModule.Skills = building.BattleModule.Skills.Select((TheLastStand.Model.Skill.Skill x) => (!(x.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId)) ? x : skill2).ToList();
		}
		if (building.BattleModule?.Goals != null)
		{
			foreach (Goal item in building.BattleModule.Goals.Where((Goal x) => x.Skill.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId))
			{
				item.Skill = skill2;
			}
		}
		if (!onLoad && building.BuildingView != null && building.BuildingView.HandledDefensesHUD != null)
		{
			building.BuildingView.HandledDefensesHUD.DisplayHandledDefensesUses(state: true);
		}
	}
}
