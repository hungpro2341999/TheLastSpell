using System.Collections.Generic;

namespace TheLastStand.Model.TileMap;

public class LineOfSightTiles
{
	public HashSet<Tile> Exclude;

	public HashSet<Tile> Obstacle;

	public Dictionary<Tile, bool> Range;

	public LineOfSightTiles()
	{
		Exclude = new HashSet<Tile>();
		Obstacle = new HashSet<Tile>();
		Range = new Dictionary<Tile, bool>();
	}

	public void Clear()
	{
		Exclude.Clear();
		Obstacle.Clear();
		Range.Clear();
	}
}
