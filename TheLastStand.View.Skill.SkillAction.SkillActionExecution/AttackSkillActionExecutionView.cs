using System.Linq;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.View.Skill.SkillAction.SkillActionExecution;

public class AttackSkillActionExecutionView : SkillActionExecutionView
{
	protected override void DisplayAreaOfEffectTileFeedback(Tile affectedTile, bool isSurroundingEffect)
	{
		bool flag = false;
		IDamageable damageable = null;
		if (base.SkillExecution.Skill.SkillAction.SkillActionController.IsUnitAffected(affectedTile) && base.SkillExecution.Skill.SkillDefinition.AffectedUnits.ShouldDamageableBeAffected(base.SkillExecution.Skill.SkillAction.SkillActionExecution.Caster, affectedTile.Unit, base.SkillExecution.Skill))
		{
			damageable = affectedTile.Unit;
		}
		else if (base.SkillExecution.Skill.SkillAction.SkillActionController.IsBuildingAffected(affectedTile))
		{
			if (base.SkillExecution.Skill.HasExtinguish)
			{
				TheLastStand.Model.Building.Building building = affectedTile.Building;
				if (building != null && building.IsBrazier)
				{
					goto IL_010d;
				}
			}
			if (base.SkillExecution.Skill.SkillDefinition.AffectedUnits.ShouldDamageableBeAffected(base.SkillExecution.Skill.SkillAction.SkillActionExecution.Caster, affectedTile.Building?.DamageableModule, base.SkillExecution.Skill))
			{
				goto IL_010d;
			}
		}
		goto IL_0122;
		IL_0122:
		if (damageable == null || base.SkillExecution.PreviewAffectedDamageables.Contains(damageable))
		{
			return;
		}
		base.SkillExecution.PreviewAffectedDamageables.Add(damageable);
		damageable.DamageableView.DamageableHUD.Highlight = true;
		bool healthDisplayed = true;
		if (flag)
		{
			healthDisplayed = base.SkillExecution is AttackSkillActionExecution && (!isSurroundingEffect || ((base.SkillExecution.Skill.SkillAction.SkillActionDefinition.TryGetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect", out var effect) && effect.SkillEffectDefinitions.Any((SkillEffectDefinition skillEffectDefinition) => skillEffectDefinition.Id == "Damage")) ? true : false));
		}
		damageable.DamageableView.DamageableHUD.HealthDisplayed = healthDisplayed;
		if (damageable is TheLastStand.Model.Unit.Unit unit)
		{
			if (damageable is EnemyUnit enemyUnit)
			{
				enemyUnit.EnemyUnitView.EnemyUnitHUD.DisplayIconFeedback(show: false);
			}
			unit.UnitView.UnitHUD.AttackEstimationDisplay.Display(base.SkillExecution, affectedTile, unit, isSurroundingEffect, casterEffect: false);
		}
		return;
		IL_010d:
		damageable = affectedTile.Building?.DamageableModule;
		flag = true;
		goto IL_0122;
	}
}
