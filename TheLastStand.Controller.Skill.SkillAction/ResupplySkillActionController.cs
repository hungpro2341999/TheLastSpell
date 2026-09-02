using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction;

public class ResupplySkillActionController : SkillActionController
{
	public ResupplySkillAction ResupplySkillAction => base.SkillAction as ResupplySkillAction;

	public ResupplySkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new ResupplySkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new ResupplySkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
		ResupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionDefinition = ResupplySkillAction.ResupplySkillActionDefinition;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		if (targetTile.Building != null && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster) && targetTile.Building.DamageableModule != null && !targetTile.Building.DamageableModule.IsDead)
		{
			return targetTile.Building.BattleModule != null;
		}
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		if (targetTile.Unit != null && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster))
		{
			return !targetTile.Unit.IsDead;
		}
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return ApplyActionOnTile(targetTile, caster);
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		bool num = IsBuildingAffected(targetTile);
		bool flag = IsUnitAffected(targetTile);
		TheLastStand.Model.Building.Building building = targetTile.Building;
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		if (num)
		{
			ApplyResupplySkillBuildingsEffect(skillActionResultDatas, building);
		}
		if (flag)
		{
			ApplyResupplySkillsSkillEffect(skillActionResultDatas, unit);
		}
		return skillActionResultDatas;
	}

	private void ApplyResupplySkillBuildingsEffect(SkillActionResultDatas resultDatas, TheLastStand.Model.Building.Building targetBuilding)
	{
		if (base.SkillAction.TryGetEffects("ResupplyOverallUses", out List<SkillEffectDefinition> effects, onlyNative: false))
		{
			foreach (SkillEffectDefinition item in effects)
			{
				ResupplyOverallUsesSkillEffectDefinition resupplyOverallUsesSkillEffectDefinition = item as ResupplyOverallUsesSkillEffectDefinition;
				if (!resupplyOverallUsesSkillEffectDefinition.TargetIds.Contains(targetBuilding.Id))
				{
					continue;
				}
				bool flag = targetBuilding.IsHandledDefense && targetBuilding.BattleModule.HasDisabledStateAndZeroRemainingCharges;
				foreach (TheLastStand.Model.Skill.Skill skill in targetBuilding.BattleModule.Skills)
				{
					if (skill != null && skill.OverallUses != 0 && skill.OverallUsesRemaining < skill.OverallUses)
					{
						skill.OverallUsesRemaining = Mathf.Min(skill.OverallUsesRemaining + resupplyOverallUsesSkillEffectDefinition.Amount, skill.ComputeTotalUses());
						GainOverallUsesDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainOverallUsesDisplay", ResourcePooler.LoadOnce<GainOverallUsesDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainOverallUsesDisplay"), EffectManager.EffectDisplaysParent);
						pooledComponent.Init(resupplyOverallUsesSkillEffectDefinition.Amount);
						targetBuilding.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
						targetBuilding.BuildingController.BlueprintModuleController.DisplayEffects();
					}
				}
				if (flag)
				{
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(targetBuilding, targetBuilding.OriginTile);
				}
			}
			resultDatas.AddAffectedBuilding(targetBuilding);
		}
		if (!base.SkillAction.TryGetEffects("ResupplyCharges", out List<SkillEffectDefinition> effects2, onlyNative: false))
		{
			return;
		}
		foreach (SkillEffectDefinition item2 in effects2)
		{
			ResupplyChargesSkillEffectDefinition resupplyChargesSkillEffectDefinition = item2 as ResupplyChargesSkillEffectDefinition;
			if (resupplyChargesSkillEffectDefinition.TargetIds.Contains(targetBuilding.Id))
			{
				TrapDamageableModule trapDamageableModule = targetBuilding.DamageableModule as TrapDamageableModule;
				if (targetBuilding.BattleModule.RemainingTrapCharges < targetBuilding.BuildingDefinition.BattleModuleDefinition.MaximumTrapCharges)
				{
					trapDamageableModule.TrapDamageableModuleController.Repair(resupplyChargesSkillEffectDefinition.Amount);
					GainUsesDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainUsesDisplay", ResourcePooler.LoadOnce<GainUsesDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainUsesDisplay"), EffectManager.EffectDisplaysParent);
					pooledComponent2.Init(resupplyChargesSkillEffectDefinition.Amount);
					targetBuilding.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
					targetBuilding.BuildingController.BlueprintModuleController.DisplayEffects();
				}
			}
		}
		resultDatas.AddAffectedBuilding(targetBuilding);
	}

	private void ApplyResupplySkillsSkillEffect(SkillActionResultDatas resultDatas, TheLastStand.Model.Unit.Unit targetUnit)
	{
		if (!base.SkillAction.TryGetEffects("ResupplySkills", out List<SkillEffectDefinition> effects, onlyNative: false))
		{
			return;
		}
		foreach (SkillEffectDefinition item in effects)
		{
			ResupplySkillsSkillEffectDefinition resupplySkillsSkillEffectDefinition = item as ResupplySkillsSkillEffectDefinition;
			if (!(targetUnit is PlayableUnit playableUnit))
			{
				continue;
			}
			List<TheLastStand.Model.Skill.Skill> list = playableUnit.PlayableUnitController.GetSkillsFromSlotType(ItemSlotDefinition.E_ItemSlotId.WeaponSlot, getBaseAndReplacementSkills: true).FindAll((TheLastStand.Model.Skill.Skill x) => x.UsesPerTurnRemaining < x.UsesPerTurn);
			if (list.Count != 0)
			{
				for (int num = 0; num < list.Count; num++)
				{
					if (list[num] != base.SkillAction.Skill)
					{
						list[num].SetUsesPerTurnRemaining(Mathf.Min(list[num].UsesPerTurnRemaining + resupplySkillsSkillEffectDefinition.Amount, list[num].UsesPerTurn));
					}
				}
			}
			resultDatas.AddAffectedUnit(targetUnit);
		}
	}
}
