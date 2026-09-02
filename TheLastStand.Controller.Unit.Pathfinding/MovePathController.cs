using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.Unit.Pathfinding;

namespace TheLastStand.Controller.Unit.Pathfinding;

public class MovePathController
{
	public MovePath MovePath { get; }

	public MovePathController(MovePathView view)
	{
		MovePath = new MovePath(this, view);
		MovePath.MovePathView.MovePath = MovePath;
	}

	public void Clear(bool removeTeleportationMarker = true)
	{
		MovePath.MovePathView.Clear(removeTeleportationMarker);
		MovePath.Path.Clear();
		MovePath.MoveRange = 0;
	}

	public void ComputePath(PathfindingData pathfindingData)
	{
		Clear();
		MovePath.MoveRange = pathfindingData.MoveRange;
		MovePath.Path = PathfindingController.ComputePath(pathfindingData);
	}

	public void SetPath(IEnumerable<Tile> path)
	{
		Clear(removeTeleportationMarker: false);
		MovePath.MoveRange = -1;
		MovePath.Path = new List<Tile>(path);
	}

	public bool UpdateState(Tile currentTile)
	{
		MovePath.E_State state = MovePath.State;
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill)
		{
			if (PlayableUnitManager.SelectedSkill.HasManeuver && PlayableUnitManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(currentTile, PlayableUnitManager.SelectedSkill.CursorDependantOrientation))
			{
				MovePath.State = MovePath.E_State.Valid;
			}
			else
			{
				MovePath.State = MovePath.E_State.Invalid;
			}
		}
		else if (!TileObjectSelectionManager.HasPlayableUnitSelected || TileObjectSelectionManager.SelectedPlayableUnit.State != TheLastStand.Model.Unit.Unit.E_State.Ready || MovePath.Path.Count == 0)
		{
			MovePath.State = MovePath.E_State.Hidden;
		}
		else if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			MovePath.State = MovePath.E_State.Teleport;
		}
		else if (!PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(currentTile))
		{
			MovePath.State = MovePath.E_State.Away;
		}
		else if (MovePath.Path[^1].Unit != null)
		{
			if (MovePath.Path[^1].Unit is PlayableUnit)
			{
				MovePath.State = MovePath.E_State.Hidden;
			}
			else
			{
				MovePath.State = MovePath.E_State.Invalid;
			}
		}
		else if (MovePath.Path.Count == MovePath.MoveRange)
		{
			MovePath.State = MovePath.E_State.Max;
		}
		else
		{
			MovePath.State = MovePath.E_State.Valid;
		}
		return MovePath.State != state;
	}
}
