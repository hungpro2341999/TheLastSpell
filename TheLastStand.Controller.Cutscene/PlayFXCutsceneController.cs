using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Controller.SpawnFx;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.SpawnFx;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayFXCutsceneController : CutsceneController
{
	private readonly FormulaInterpreterContext formulaInterpreterContext = new FormulaInterpreterContext();

	public PlayFXCutsceneDefinition PlayFXCutsceneDefinition => base.CutsceneDefinition as PlayFXCutsceneDefinition;

	public PlayFXCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (PlayFXCutsceneDefinition.SpecifiedTilePosition)
		{
			cutsceneData.Tile = TileMapManager.GetTile(PlayFXCutsceneDefinition.TileX.Value, PlayFXCutsceneDefinition.TileY.Value);
		}
		if (cutsceneData.Tile == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a PlayFXCutsceneController with a null tile.");
			yield break;
		}
		formulaInterpreterContext.TargetObject = cutsceneData;
		if (PlayFXCutsceneDefinition.SpawnFxDefinition != null)
		{
			TheLastStand.Model.SpawnFx.SpawnFx spawnFx = new SpawnFxController(PlayFXCutsceneDefinition.SpawnFxDefinition).SpawnFx;
			spawnFx.SourceTile = cutsceneData.Tile;
			spawnFx.SpawnFxController.PlaySpawnFxs();
			if (PlayFXCutsceneDefinition.WaitForFXDuration)
			{
				yield return SharedYields.WaitForSeconds(spawnFx.SpawnFxDefinition.CastTotalDuration.EvalToFloat(formulaInterpreterContext));
			}
		}
	}
}
