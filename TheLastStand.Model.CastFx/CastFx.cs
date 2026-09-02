using System.Collections.Generic;
using TheLastStand.Controller.CastFx;
using TheLastStand.Definition.CastFx;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.CastFx;

public class CastFx
{
	public List<List<Tile>> AffectedTiles { get; } = new List<List<Tile>>();

	public CastFxController CastFxController { get; }

	public CastFxDefinition CastFxDefinition { get; }

	public CastFXInterpreterContext CastFXInterpreterContext { get; set; }

	public Tile SourceTile { get; set; }

	public Tile TargetTile { get; set; }

	public CastFx(CastFxDefinition definition, CastFxController controller)
	{
		CastFxDefinition = definition;
		CastFxController = controller;
	}
}
