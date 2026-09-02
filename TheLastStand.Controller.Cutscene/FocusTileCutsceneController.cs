using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class FocusTileCutsceneController : CutsceneController
{
	public FocusTileCutsceneDefinition FocusTileCutsceneDefinition => base.CutsceneDefinition as FocusTileCutsceneDefinition;

	public FocusTileCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		Transform transform = TileMapManager.GetTile(FocusTileCutsceneDefinition.PosX, FocusTileCutsceneDefinition.PosY)?.TileView.transform;
		if (transform != null)
		{
			ACameraView.MoveTo(transform);
		}
		else
		{
			TPSingleton<CutsceneManager>.Instance.LogError(string.Format("{0} : Tile {1}:{2} does not exist!", "FocusTileCutsceneController", FocusTileCutsceneDefinition.PosX, FocusTileCutsceneDefinition.PosY), CLogLevel.MAJOR);
		}
		yield break;
	}
}
