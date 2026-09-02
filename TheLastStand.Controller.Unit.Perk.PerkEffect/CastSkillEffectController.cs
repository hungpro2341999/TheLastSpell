using System.Collections.Generic;
using Sirenix.Utilities;
using TPLib;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class CastSkillEffectController : APerkEffectController
{
	public CastSkillEffect CastSkillEffect => base.PerkEffect as CastSkillEffect;

	public CastSkillEffectController(CastSkillEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new CastSkillEffect(aPerkEffectDefinition as CastSkillEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		if (data != null && data.IsTriggeredByPerk && !base.PerkEffect.APerkEffectDefinition.CanBeTriggeredByPerk)
		{
			return;
		}
		base.Trigger(data);
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
		CastSkillEffect.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(base.PerkEffect.APerkModule.Perk.Owner);
		HashSet<Tile> hashSet = new HashSet<Tile>();
		List<Tile> allValidTargetTiles = CastSkillEffect.Skill.SkillAction.SkillActionExecution.GetAllValidTargetTiles();
		hashSet.AddRange(CastSkillEffect.PerkTargeting.GetTargetTiles(data, base.PerkEffect.APerkModule.Perk, allValidTargetTiles));
		if (hashSet.Count > 0)
		{
			foreach (Tile item in hashSet)
			{
				TileObjectSelectionManager.E_Orientation orientation = CastSkillEffect.Skill.TileDependantOrientation(item);
				CastSkillEffect.Skill.SkillAction.SkillActionExecution.TargetTiles.Clear();
				CastSkillEffect.Skill.SkillAction.SkillActionExecution.TargetTiles.Add(new SkillTargetedTileInfo(item, orientation));
				CastSkillEffect.Skill.SkillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
			}
			foreach (Tile item2 in hashSet)
			{
				if (item2.Unit != null)
				{
					if (!item2.Unit.UnitView.UnitHUD.IsAnimating)
					{
						item2.Unit.UnitView.RefreshHealth();
						item2.Unit.UnitView.RefreshArmor();
						item2.Unit.UnitView.RefreshStatus();
						item2.Unit.UnitView.RefreshInjuryStage();
						item2.Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
					}
					else
					{
						item2.Unit.UnitView.UnitHUD.AnimatedDisplayFinishEvent += item2.Unit.UnitController.OnUnitHUDAnimatedDisplayFinished;
					}
				}
			}
		}
		CastSkillEffect.Skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
	}
}
