using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class LookAtCircleCutsceneController : CutsceneController
{
	public LookAtCircleCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			o.UnitController.LookAt(BuildingManager.MagicCircle.OriginTile);
		});
		yield break;
	}
}
