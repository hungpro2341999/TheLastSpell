using System.Collections.Generic;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Model;

public class ComputedGoal
{
	public Goal Goal { get; }

	public HashSet<PlayableUnit> TargetPlayableUnits { get; }

	public SkillTargetedTileInfo TargetTileInfo { get; }

	public bool CanBeMerged => !(Goal.Owner is BossUnit);

	public Tile DesiredReachedTile { get; }

	public SkillCasterAttackGroup.E_Target TargetType { get; }

	public SkillCasterAttackGroup.E_Target TargetTileType { get; } = SkillCasterAttackGroup.E_Target.OTHER;

	public ComputedGoal(Goal goal, SkillTargetedTileInfo targetTileInfo)
	{
		Goal = goal;
		TargetTileInfo = targetTileInfo;
		DesiredReachedTile = (goal.GoalController.IsOrientationImportant ? TileObjectSelectionManager.GetTileFromTileToOrientation(targetTileInfo.Tile, targetTileInfo.Orientation, -1) : null);
		SkillAction skillAction = goal.Skill.SkillAction;
		if (!(skillAction is BuildSkillAction) && !(skillAction is GenericSkillAction))
		{
			if (!(skillAction is SpawnSkillAction))
			{
				if (skillAction is AttackSkillAction)
				{
					TargetType = SkillCasterAttackGroup.E_Target.OTHER;
					List<Tile> affectedTiles = goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(targetTileInfo.Tile, alwaysReturnFullPattern: false, targetTileInfo.Orientation);
					TargetPlayableUnits = new HashSet<PlayableUnit>();
					for (int i = 0; i < affectedTiles.Count; i++)
					{
						Tile tile = affectedTiles[i];
						SkillCasterAttackGroup.E_Target e_Target = SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
						if (goal.Skill.SkillAction.SkillActionController.IsUnitAffected(tile))
						{
							PlayableUnit playableUnit = tile.Unit as PlayableUnit;
							e_Target = ((playableUnit != null) ? SkillCasterAttackGroup.E_Target.ATTACK_HERO : SkillCasterAttackGroup.E_Target.ATTACK_ENEMY);
							if (playableUnit != null)
							{
								TargetPlayableUnits.Add(playableUnit);
							}
						}
						else if (goal.Skill.SkillAction.SkillActionController.IsBuildingAffected(tile))
						{
							e_Target = ((!(tile.Building is MagicCircle)) ? SkillCasterAttackGroup.E_Target.ATTACK_BUILDING : SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE);
						}
						if ((int)TargetType > (int)e_Target)
						{
							TargetType = e_Target;
						}
						if (tile == targetTileInfo.Tile)
						{
							TargetTileType = e_Target;
						}
					}
				}
				else
				{
					TargetType = SkillCasterAttackGroup.E_Target.OTHER;
					TargetTileType = SkillCasterAttackGroup.E_Target.OTHER;
				}
			}
			else
			{
				TargetType = SkillCasterAttackGroup.E_Target.SPAWN;
				TargetTileType = SkillCasterAttackGroup.E_Target.SPAWN;
			}
		}
		else
		{
			TargetType = SkillCasterAttackGroup.E_Target.GENERIC;
			TargetTileType = SkillCasterAttackGroup.E_Target.GENERIC;
		}
		if (!goal.Skill.HasManeuver || !(goal.Owner is EnemyUnit { GoalComputingStep: IBehaviorModel.E_GoalComputingStep.AfterMoving } enemyUnit))
		{
			return;
		}
		foreach (Tile occupiedTile in goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetManeuverTile(targetTileInfo.Tile, targetTileInfo.Orientation).GetOccupiedTiles(enemyUnit.UnitTemplateDefinition))
		{
			occupiedTile.WillBeReachedBy = goal.Owner.RandomId;
		}
	}
}
