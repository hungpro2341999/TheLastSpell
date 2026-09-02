using TheLastStand.Controller.SpawnFx;
using TheLastStand.Definition.SpawnFx;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.SpawnFx;

public class SpawnFx
{
	public SpawnFxController SpawnFxController { get; private set; }

	public SpawnFxDefinition SpawnFxDefinition { get; private set; }

	public Tile SourceTile { get; set; }

	public SpawnFx(SpawnFxDefinition definition, SpawnFxController controller)
	{
		SpawnFxDefinition = definition;
		SpawnFxController = controller;
	}
}
