using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Building;
using TheLastStand.Model.Unit;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public class VictorySequenceView : CutsceneView
{
	[SerializeField]
	private AnimatedSceneSkipText skipText;

	[SerializeField]
	[Min(0f)]
	private float delayBeforeBlackFade = 0.5f;

	[SerializeField]
	[Min(0f)]
	private float delayAfterBlackFade = 0.5f;

	[SerializeField]
	[Min(0f)]
	private float blackFadeDuration = 2f;

	[SerializeField]
	private Ease blackFadeCurve = Ease.InQuart;

	[SerializeField]
	private float cameraYOffsetFromCircle = 2.5f;

	[SerializeField]
	[Range(0f, 10f)]
	private float victorySfxDelay = 1.7f;

	[SerializeField]
	private AudioSource audioSource;

	public static int barkerIndex;

	public string debugCityIdOverride;

	private bool finalFadeRunning;

	private bool firstCityVictory;

	public float CameraYOffsetFromCircle => cameraYOffsetFromCircle;

	public float VictorySfxDelay => victorySfxDelay;

	public AudioSource AudioSource => audioSource;

	public override bool CanBeSkipped()
	{
		if (base.CanBeSkipped() && !finalFadeRunning)
		{
			return !firstCityVictory;
		}
		return false;
	}

	public override IEnumerator PlayCutscene(Action callback = null)
	{
		base.IsPlaying = true;
		Callback = callback;
		PrepareView();
		barkerIndex = 0;
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		firstCityVictory = !selectedCity.HaveWon;
		ToggleSkipText(CanBeSkipped());
		string text = (string.IsNullOrEmpty(debugCityIdOverride) ? selectedCity.CityDefinition.Id : debugCityIdOverride);
		CutsceneDefinition cutsceneDefinition = GameDatabase.CutsceneDefinitions.GetValueOrDefault(text);
		if (cutsceneDefinition == null)
		{
			KeyValuePair<string, CutsceneDefinition> keyValuePair = GameDatabase.CutsceneDefinitions.First();
			TPSingleton<CutsceneManager>.Instance.LogWarning("CutsceneDefinition missing for Id " + text + ", using first one in database (" + keyValuePair.Key + ").");
			cutsceneDefinition = keyValuePair.Value;
		}
		CutsceneData cutsceneData = new CutsceneData
		{
			City = selectedCity
		};
		yield return PlayCutsceneDefinition(cutsceneDefinition, cutsceneData);
		ToggleSkipText(state: false);
		yield return SharedYields.WaitForSeconds(delayBeforeBlackFade);
		finalFadeRunning = true;
		yield return StartCoroutine(FadeToBlack(blackFadeDuration));
		finalFadeRunning = false;
		base.IsPlaying = false;
		callback?.Invoke();
	}

	public override IEnumerator Skip()
	{
		ToggleSkipText(state: false);
		yield return StartCoroutine(FadeToBlack(0.5f));
		yield return base.Skip();
	}

	public void ResetPillarVisualEffects()
	{
		BuildingManager.MagicCircle.MagicCircleView.StopShakeCoroutine();
		BuildingManager.MagicCircle.MagicCircleView.SealView.OnPillarsCutsceneStart();
		CameraView.CameraLutView.TogglePillarLut(state: false);
		CameraView.CameraVisualEffects.ToggleChromaticAberration(state: false);
		ACameraView.Zoom(zoomIn: true, instant: true);
	}

	private void PrepareView()
	{
		TPSingleton<SoundManager>.Instance.StopMusic();
		StartCoroutine(TPSingleton<UIManager>.Instance.ToggleUICoroutine());
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			o.UnitView.UnitHUD.DisplayHealthIfNeeded();
		});
		TPSingleton<BuildingManager>.Instance.Buildings.ForEach(delegate(TheLastStand.Model.Building.Building o)
		{
			o.BuildingView.BuildingHUD.DisplayIfNeeded();
		});
		PlayableUnitManager.GatherUnitsForVictorySequence();
		SpawnWaveManager.SpawnWaveView.Refresh();
		if (!string.IsNullOrEmpty(debugCityIdOverride))
		{
			BuildingManager.MagicCircle.MagicCircleView.SealView.InitAnimations();
		}
	}

	private void ToggleSkipText(bool state)
	{
		if (!(skipText == null))
		{
			skipText.gameObject.SetActive(state);
		}
	}

	private IEnumerator FadeToBlack(float duration)
	{
		CanvasFadeManager.FadeIn(duration, 99, blackFadeCurve);
		yield return new WaitUntil(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver);
		yield return SharedYields.WaitForSeconds(delayAfterBlackFade);
	}
}
