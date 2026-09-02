using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Database;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class TutorialSequenceView : CutsceneView
{
	public static class Constants
	{
		public const string SequenceNameFormat = "TutorialSequence_Step{0}";
	}

	private int cutsceneStep;

	public override IEnumerator PlayCutscene(Action callback = null)
	{
		cutsceneStep++;
		string text = $"TutorialSequence_Step{cutsceneStep}";
		CutsceneDefinition valueOrDefault = GameDatabase.CutsceneDefinitions.GetValueOrDefault(text);
		if (valueOrDefault == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play " + text + " but it was not found in the database.", this, CLogLevel.MAJOR, forcePrintInUnity: true, printStackTrace: false);
			yield break;
		}
		base.IsPlaying = true;
		Game.E_State previousState = TPSingleton<GameManager>.Instance.Game.State;
		GameController.SetState(Game.E_State.CutscenePlaying);
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			o.UnitView.UnitHUD.DisplayHealthIfNeeded();
		});
		TPSingleton<BuildingManager>.Instance.Buildings.ForEach(delegate(TheLastStand.Model.Building.Building o)
		{
			o.BuildingView.BuildingHUD.DisplayIfNeeded();
		});
		Vector3 previousCameraPosition = ProCamera2D.Instance.CameraTargets[0].TargetTransform.position;
		bool cameraWasZoomedIn = ACameraView.IsZoomedIn;
		yield return PlayCutsceneDefinition(valueOrDefault, base.CutsceneData);
		ACameraView.MoveTo(previousCameraPosition);
		ACameraView.Zoom(cameraWasZoomedIn);
		GameController.SetState(previousState);
		base.IsPlaying = false;
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			o.UnitView.UnitHUD.DisplayHealthIfNeeded();
		});
		TPSingleton<BuildingManager>.Instance.Buildings.ForEach(delegate(TheLastStand.Model.Building.Building o)
		{
			o.BuildingView.BuildingHUD.DisplayIfNeeded();
		});
		callback?.Invoke();
	}
}
