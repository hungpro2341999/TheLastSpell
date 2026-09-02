using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.DRM.Achievements;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Controller.Skill.SkillAction;

public class GenericSkillActionController : SkillActionController
{
	private GenericSkillAction GenericSkillAction => base.SkillAction as GenericSkillAction;

	public GenericSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new GenericSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new GenericSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		if (base.SkillAction.HasEffect("ExtinguishBrazier"))
		{
			return targetTile.Building?.BrazierModule != null;
		}
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		if (unit != null && unit.CanBeDamaged() && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster))
		{
			return !targetTile.Unit.IsDead;
		}
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		bool flag = IsUnitAffected(targetTile);
		bool hitsBuilding = IsBuildingAffected(targetTile);
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		skillActionResultDatas.AddAffectedUnit(unit);
		List<SkillEffectDefinition> list = null;
		Dictionary<string, List<SkillEffectDefinition>> allEffects = base.SkillAction.GetAllEffects();
		if (allEffects.Count > 0)
		{
			list = new List<SkillEffectDefinition>();
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> item2 in allEffects)
			{
				list.AddRange(item2.Value);
			}
			ApplySkillEffectsOnTile(targetTile, caster, skillActionResultDatas, list, flag, hitsBuilding);
		}
		if (flag)
		{
			TheLastStand.Model.Unit.Unit unit2 = targetTile.Unit;
			if (unit2 != null && unit2.Health > 0f && GenericSkillAction.TryGetFirstEffect<PropagationSkillEffectDefinition>("Propagation", out var effect))
			{
				Tile tile = targetTile;
				List<TheLastStand.Model.Unit.Unit> list2 = new List<TheLastStand.Model.Unit.Unit> { targetTile.Unit };
				int num = ((caster is TheLastStand.Model.Unit.Unit unit3) ? unit3.UnitController.GetModifiedPropagationsCount(effect.GetPropagationsCount(effect.GetPerkContext(unit3))) : effect.PropagationsCount);
				int num2 = 0;
				while (num2 < num)
				{
					List<Tile> enumerable = ((!(base.SkillAction.Skill.Owner is PlayableUnit playableUnit) || !playableUnit.AllowDiagonalPropagation(base.SkillAction.PerkDataContainer)) ? tile.GetAdjacentTiles() : tile.GetAdjacentTilesWithDiagonals());
					IEnumerable<Tile> enumerable2 = RandomManager.Shuffle(this, enumerable);
					bool flag2 = false;
					foreach (Tile item3 in enumerable2)
					{
						if (!item3.HasFog && item3.Unit != null && item3.Unit != caster && !list2.Contains(item3.Unit))
						{
							flag2 = true;
							base.SkillAction.SkillActionExecution.SkillExecutionController.AddPropagationAffectedUnit(base.SkillAction.SkillActionExecution.HitIndex, item3.Unit);
							list2.Add(item3.Unit);
							skillActionResultDatas.AddAffectedUnit(item3.Unit);
							tile = item3;
							num2++;
							if (list != null)
							{
								ApplySkillEffectsOnTile(item3, caster, skillActionResultDatas, list, flag, hitsBuilding);
							}
							break;
						}
					}
					if (!flag2)
					{
						if (list2.Count <= 1)
						{
							break;
						}
						list2.Clear();
						list2.Add(tile.Unit);
					}
				}
			}
			if (GenericSkillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item && item.ItemDefinition.Category == ItemDefinition.E_Category.Potion && targetTile.GetDamageable() is PlayableUnit)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PotionsUsed, 1.0);
				TPSingleton<AchievementManager>.Instance.SetAchievementProgression(StatContainer.STAT_POTION_USES_AMOUNT, (int)TPSingleton<MetaConditionManager>.Instance.CampaignContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.PotionsUsed), refreshAchievements: true, TPSingleton<AchievementManager>.Instance.GetAchievementProgression(StatContainer.STAT_POTION_USES_AMOUNT));
				TPSingleton<AchievementManager>.Instance.HandlePotionUsed(item, targetTile);
				if (targetTile.Unit != caster)
				{
					TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PotionsOnAllies, 1.0);
					TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_USE_POTION_ON_ALLY);
				}
			}
		}
		return skillActionResultDatas;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		if (!base.SkillAction.TryGetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect", out var effect))
		{
			TPSingleton<SkillManager>.Instance.LogError("No surrounding effect found!", CLogLevel.DETAILED);
			return skillActionResultDatas;
		}
		bool hitsUnit = IsUnitAffected(targetTile);
		bool hitsBuilding = IsBuildingAffected(targetTile);
		ApplySkillEffectsOnTile(targetTile, caster, skillActionResultDatas, effect.SkillEffectDefinitions, hitsUnit, hitsBuilding);
		return skillActionResultDatas;
	}

	protected override void ApplySkillEffectsOnTile(Tile targetTile, ISkillCaster caster, SkillActionResultDatas resultDatas, List<SkillEffectDefinition> skillEffectDefinitions, bool hitsUnit, bool hitsBuilding, bool forceApply = false)
	{
		base.ApplySkillEffectsOnTile(targetTile, caster, resultDatas, skillEffectDefinitions, hitsUnit, hitsBuilding, forceApply);
		EnsurePerkData(base.SkillAction.PerkDataContainer?.TargetTile ?? targetTile, targetTile.GetDamageable(), null, base.SkillAction.Skill.SkillContainer is Perk);
		targetTile.Unit?.Events.GetValueOrDefault(E_EffectTime.OnHitTaken)?.Invoke(base.SkillAction.PerkDataContainer);
	}
}
