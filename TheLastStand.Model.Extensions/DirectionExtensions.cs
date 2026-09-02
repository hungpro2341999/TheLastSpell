using System.Collections.Generic;
using TheLastStand.Definition;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.View.Unit;

namespace TheLastStand.Model.Extensions;

public static class DirectionExtensions
{
	public static SpawnDirectionsDefinition.E_Direction ToSpawnDirection(this GameDefinition.E_Direction direction)
	{
		return direction switch
		{
			GameDefinition.E_Direction.North => SpawnDirectionsDefinition.E_Direction.Top, 
			GameDefinition.E_Direction.East => SpawnDirectionsDefinition.E_Direction.Right, 
			GameDefinition.E_Direction.South => SpawnDirectionsDefinition.E_Direction.Bottom, 
			GameDefinition.E_Direction.West => SpawnDirectionsDefinition.E_Direction.Left, 
			_ => SpawnDirectionsDefinition.E_Direction.None, 
		};
	}

	public static bool IsOppositeTo(this SpawnDirectionsDefinition.E_Direction direction, SpawnDirectionsDefinition.E_Direction otherDirection)
	{
		return direction switch
		{
			SpawnDirectionsDefinition.E_Direction.Top => otherDirection == SpawnDirectionsDefinition.E_Direction.Bottom, 
			SpawnDirectionsDefinition.E_Direction.Right => otherDirection == SpawnDirectionsDefinition.E_Direction.Left, 
			SpawnDirectionsDefinition.E_Direction.Bottom => otherDirection == SpawnDirectionsDefinition.E_Direction.Top, 
			SpawnDirectionsDefinition.E_Direction.Left => otherDirection == SpawnDirectionsDefinition.E_Direction.Right, 
			_ => false, 
		};
	}

	public static bool IsAdjacentTo(this SpawnDirectionsDefinition.E_Direction first, SpawnDirectionsDefinition.E_Direction second)
	{
		return first switch
		{
			SpawnDirectionsDefinition.E_Direction.Top => second != SpawnDirectionsDefinition.E_Direction.Bottom, 
			SpawnDirectionsDefinition.E_Direction.Bottom => second != SpawnDirectionsDefinition.E_Direction.Top, 
			SpawnDirectionsDefinition.E_Direction.Left => second != SpawnDirectionsDefinition.E_Direction.Right, 
			SpawnDirectionsDefinition.E_Direction.Right => second != SpawnDirectionsDefinition.E_Direction.Left, 
			_ => false, 
		};
	}

	public static bool IsCentralZone(this SpawnWaveView.E_SpawnWaveDetailedZone detailedZone)
	{
		switch (detailedZone)
		{
		case SpawnWaveView.E_SpawnWaveDetailedZone.None:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W:
			return true;
		default:
			return false;
		}
	}

	public static bool IsDetailedZone(this SpawnWaveView.E_SpawnWaveDetailedZone detailedZone)
	{
		return !detailedZone.IsCentralZone();
	}

	public static TileFlagDefinition.E_TileFlagTag ToTileFlag(this SpawnWaveView.E_SpawnWaveDetailedZone detailedZone)
	{
		return detailedZone switch
		{
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N => TileFlagDefinition.E_TileFlagTag.Zone_N, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N_NE => TileFlagDefinition.E_TileFlagTag.Zone_N_NE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_NE => TileFlagDefinition.E_TileFlagTag.Zone_NE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E_NE => TileFlagDefinition.E_TileFlagTag.Zone_E_NE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E => TileFlagDefinition.E_TileFlagTag.Zone_E, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E_SE => TileFlagDefinition.E_TileFlagTag.Zone_E_SE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_SE => TileFlagDefinition.E_TileFlagTag.Zone_SE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S_SE => TileFlagDefinition.E_TileFlagTag.Zone_S_SE, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S => TileFlagDefinition.E_TileFlagTag.Zone_S, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S_SW => TileFlagDefinition.E_TileFlagTag.Zone_S_SW, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_SW => TileFlagDefinition.E_TileFlagTag.Zone_SW, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W_SW => TileFlagDefinition.E_TileFlagTag.Zone_W_SW, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W => TileFlagDefinition.E_TileFlagTag.Zone_W, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W_NW => TileFlagDefinition.E_TileFlagTag.Zone_W_NW, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_NW => TileFlagDefinition.E_TileFlagTag.Zone_NW, 
			SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N_NW => TileFlagDefinition.E_TileFlagTag.Zone_N_NW, 
			_ => TileFlagDefinition.E_TileFlagTag.None, 
		};
	}

	public static List<SpawnDirectionsDefinition.E_Direction> ToSpawnDirections(this SpawnWaveView.E_SpawnWaveDetailedZone detailedZone)
	{
		switch (detailedZone)
		{
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N_NE:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_N_NW:
			return new List<SpawnDirectionsDefinition.E_Direction> { SpawnDirectionsDefinition.E_Direction.Top };
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E_NE:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_E_SE:
			return new List<SpawnDirectionsDefinition.E_Direction> { SpawnDirectionsDefinition.E_Direction.Right };
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S_SE:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_S_SW:
			return new List<SpawnDirectionsDefinition.E_Direction> { SpawnDirectionsDefinition.E_Direction.Bottom };
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W_SW:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W:
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_W_NW:
			return new List<SpawnDirectionsDefinition.E_Direction> { SpawnDirectionsDefinition.E_Direction.Left };
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_NE:
			return new List<SpawnDirectionsDefinition.E_Direction>
			{
				SpawnDirectionsDefinition.E_Direction.Top,
				SpawnDirectionsDefinition.E_Direction.Right
			};
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_SE:
			return new List<SpawnDirectionsDefinition.E_Direction>
			{
				SpawnDirectionsDefinition.E_Direction.Bottom,
				SpawnDirectionsDefinition.E_Direction.Right
			};
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_NW:
			return new List<SpawnDirectionsDefinition.E_Direction>
			{
				SpawnDirectionsDefinition.E_Direction.Top,
				SpawnDirectionsDefinition.E_Direction.Left
			};
		case SpawnWaveView.E_SpawnWaveDetailedZone.Zone_SW:
			return new List<SpawnDirectionsDefinition.E_Direction>
			{
				SpawnDirectionsDefinition.E_Direction.Bottom,
				SpawnDirectionsDefinition.E_Direction.Left
			};
		default:
			return new List<SpawnDirectionsDefinition.E_Direction>();
		}
	}

	public static bool TryTranslateToSpawnDirections(this SpawnWaveView.E_SpawnWaveDetailedZone detailedZone, out List<SpawnDirectionsDefinition.E_Direction> directions)
	{
		directions = detailedZone.ToSpawnDirections();
		return directions.Count > 0;
	}
}
