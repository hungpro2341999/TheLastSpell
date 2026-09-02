using TheLastStand.Definition.Unit.Race;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public interface IBarker
{
	bool HasBark { get; set; }

	Tile OriginTile { get; }

	RaceDefinition BarkerRaceDefinition { get; }

	Transform BarkViewFollowTarget { get; }
}
