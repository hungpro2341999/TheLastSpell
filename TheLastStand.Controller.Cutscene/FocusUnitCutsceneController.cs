using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class FocusUnitCutsceneController : CutsceneController
{
	public FocusUnitCutsceneDefinition FocusUnitCutsceneDefinition => base.CutsceneDefinition as FocusUnitCutsceneDefinition;

	public FocusUnitCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Unit == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a FocusUnitCutsceneController with a null unit.");
			yield break;
		}
		if (FocusUnitCutsceneDefinition.ZoomIn)
		{
			ACameraView.Zoom(zoomIn: true);
		}
		ACameraView.MoveTo(cutsceneData.Unit.OriginTile.TileView.transform.position, CameraView.AnimationMoveSpeed);
		yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
	}
}
