using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Database;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Manager;

public class CutsceneManager : Manager<CutsceneManager>
{
	public class StringToCutsceneIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => GameDatabase.CutsceneDefinitions.Keys.ToList();
	}

	[SerializeField]
	private VictorySequenceView victorySequenceView;

	[SerializeField]
	private PillarsCutsceneView pillarsCutsceneView;

	[SerializeField]
	private TutorialSequenceView tutorialSequenceView;

	[SerializeField]
	private GenericCutsceneView genericCutsceneViewPrefab;

	private readonly List<GenericCutsceneView> genericCutsceneViews = new List<GenericCutsceneView>();

	public static bool AnyCutscenePlaying
	{
		get
		{
			if (!TPSingleton<CutsceneManager>.Instance.VictorySequenceView.IsPlaying && !TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying && !TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.IsPlaying)
			{
				return TPSingleton<CutsceneManager>.Instance.genericCutsceneViews.Any((GenericCutsceneView x) => x.IsPlaying);
			}
			return true;
		}
	}

	public VictorySequenceView VictorySequenceView => victorySequenceView;

	public PillarsCutsceneView PillarsCutsceneView => pillarsCutsceneView;

	public TutorialSequenceView TutorialSequenceView => tutorialSequenceView;

	public bool VictorySequenceSkipped { get; private set; }

	public GenericCutsceneView GetGenericCutsceneView()
	{
		GenericCutsceneView genericCutsceneView = UnityEngine.Object.Instantiate(genericCutsceneViewPrefab, base.gameObject.transform);
		genericCutsceneViews.Add(genericCutsceneView);
		return genericCutsceneView;
	}

	public static void PlayCutscene(CutsceneView cutsceneView, Action callback = null)
	{
		TPSingleton<CutsceneManager>.Instance.StartCoroutine(cutsceneView.PlayCutscene(callback));
	}

	private IEnumerator SkipVictoryCutscene()
	{
		if (PillarsCutsceneView.CanBeSkipped())
		{
			yield return PillarsCutsceneView.Skip();
			yield return VictorySequenceView.Skip();
			PillarsCutsceneView.DisableCanvasIfEnabled();
		}
		else
		{
			yield return VictorySequenceView.Skip();
		}
	}

	private void Start()
	{
		if (victorySequenceView == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogWarning("Missing reference for victorySequenceView, trying to get it using FindObjectOfType.");
			victorySequenceView = UnityEngine.Object.FindObjectOfType<VictorySequenceView>();
		}
		if (pillarsCutsceneView == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogWarning("Missing reference for pillarsCutsceneView, trying to get it using FindObjectOfType.");
			pillarsCutsceneView = UnityEngine.Object.FindObjectOfType<PillarsCutsceneView>();
		}
		if (tutorialSequenceView == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogWarning("Missing reference for tutorialSequenceView, trying to get it using FindObjectOfType.");
			tutorialSequenceView = UnityEngine.Object.FindObjectOfType<TutorialSequenceView>();
		}
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(23) && VictorySequenceView.CanBeSkipped())
		{
			VictorySequenceSkipped = true;
			TPSingleton<CutsceneManager>.Instance.StopAllCoroutines();
			StartCoroutine(SkipVictoryCutscene());
		}
	}

	public void UnregisterGenericCutscene(GenericCutsceneView cutsceneView)
	{
		genericCutsceneViews.Remove(cutsceneView);
		UnityEngine.Object.Destroy(cutsceneView.gameObject);
	}

	[DevConsoleCommand("PlayCutscene", Options = DevConsoleCommandOptions.ForceStatic)]
	private static void DebugPlayCutscene([StringConverter(typeof(StringToCutsceneIdConverter))] string cutsceneId = "")
	{
		GenericCutsceneView genericCutsceneView = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
		genericCutsceneView.Init(cutsceneId, new CutsceneData(TPSingleton<WorldMapCityManager>.Instance.SelectedCity));
		PlayCutscene(genericCutsceneView);
	}
}
