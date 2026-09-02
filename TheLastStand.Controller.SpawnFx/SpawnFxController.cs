using TheLastStand.Definition.SpawnFx;
using TheLastStand.Model.SpawnFx;
using TheLastStand.View.SpawnFx;

namespace TheLastStand.Controller.SpawnFx;

public class SpawnFxController
{
	public TheLastStand.Model.SpawnFx.SpawnFx SpawnFx { get; private set; }

	public SpawnFxController(SpawnFxDefinition definition)
	{
		SpawnFx = new TheLastStand.Model.SpawnFx.SpawnFx(definition, this);
	}

	public void PlaySpawnFxs()
	{
		SpawnFxView.PlaySpawnFxs(SpawnFx);
	}
}
