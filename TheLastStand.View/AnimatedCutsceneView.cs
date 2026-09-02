using System.Collections;
using DG.Tweening;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Definition.AnimatedCutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.View;

public class AnimatedCutsceneView : TPSingleton<AnimatedCutsceneView>
{
	public class Constants
	{
		public const string AnimatorParameterNext = "Next";

		public const string SFXPathPrefix = "Sounds/SFX/AnimatedCutscene";

		public const string PrefabPathPrefix = "Prefab/AnimatedCutscene/AnimatedCutscene_";
	}

	[SerializeField]
	[Tooltip("Duration waited before starting the very first slide, during the initial black screen.")]
	private float startDelay = 1f;

	[SerializeField]
	[Tooltip("Duration waited during the black screen between each slides.")]
	private float sequenceOverDelay = 1f;

	[SerializeField]
	[Tooltip("Duration waited after the last slide sequence and starting the game (to avoid a too harsh cut after last slide).")]
	private float delayBetweenSlides = 0.5f;

	[SerializeField]
	[Tooltip("Duration waited after a text display to show the player the skip feedback.")]
	private float delayBeforeSkipFeedback = 1f;

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
	private Canvas canvas;

	[SerializeField]
	private Transform cutsceneContainer;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextTyper textTyper;

	[SerializeField]
	[Tooltip("GameObject enabled to show the player he can skip.")]
	private GameObject skipFeedback;

	[SerializeField]
	private PooledAudioSourceData cutsceneAudioSourceData;

	[SerializeField]
	private float fadeOutDuration = 1f;

	private AnimatedCutsceneDefinition animatedCutsceneDefinition;

	private Coroutine cutsceneCoroutine;

	private Animator[] slideAnimators;

	private AnimatedCutsceneSlideDefinition currentSlideDefinition;

	private Animator currentAnimator;

	private GameObject currentContainer;

	private string currentCutsceneId;

	private GameObject[] slideContainers;

	private AnimatedCutscenePrefab animatedCutscenePrefab;

	private bool isRunning;

	private AudioSource lastAudioSource;

	public IEnumerator Fade(bool fadeIn)
	{
		yield return SharedYields.WaitForSeconds(delayBeforeBlackFade);
		if (fadeIn)
		{
			CanvasFadeManager.FadeIn(blackFadeDuration, 99, blackFadeCurve);
		}
		else
		{
			CanvasFadeManager.FadeOut(blackFadeDuration, 99, blackFadeCurve);
		}
		yield return new WaitUntil(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver);
		yield return SharedYields.WaitForSeconds(delayAfterBlackFade);
	}

	public void RemoveAnimatedCutscene()
	{
		Object.Destroy(animatedCutscenePrefab.gameObject);
	}

	public void StartAnimatedCutScene(string cutsceneId)
	{
		currentCutsceneId = cutsceneId;
		AnimatedCutscenePrefab original = Resources.Load<AnimatedCutscenePrefab>("Prefab/AnimatedCutscene/AnimatedCutscene_" + cutsceneId);
		animatedCutscenePrefab = Object.Instantiate(original, Vector3.zero, Quaternion.identity, cutsceneContainer);
		animatedCutscenePrefab.transform.localPosition = Vector3.zero;
		slideContainers = animatedCutscenePrefab.Slides;
		canvas.enabled = true;
		Init();
		cutsceneCoroutine = StartCoroutine(PlayAnimatedCutscene());
		StartCoroutine(WaitForSkipFull());
	}

	public IEnumerator StartAnimatedCutSceneCoroutine(string cutsceneId)
	{
		currentCutsceneId = cutsceneId;
		AnimatedCutscenePrefab original = Resources.Load<AnimatedCutscenePrefab>("Prefab/AnimatedCutscene/AnimatedCutscene_" + cutsceneId);
		animatedCutscenePrefab = Object.Instantiate(original, Vector3.zero, Quaternion.identity, cutsceneContainer);
		animatedCutscenePrefab.transform.localPosition = Vector3.zero;
		slideContainers = animatedCutscenePrefab.Slides;
		canvas.enabled = true;
		Init();
		cutsceneCoroutine = StartCoroutine(PlayAnimatedCutscene());
		yield return StartCoroutine(WaitForSkipFull());
	}

	private void ChangeMusic(AnimatedCutsceneSlideChangeMusicDefinition changeMusicDefinition)
	{
		string text = "Sounds/SFX/AnimatedCutscene/" + currentCutsceneId + "/" + changeMusicDefinition.ClipAssetName;
		AudioClip audioClip = ResourcePooler.LoadOnce<AudioClip>(text);
		if (audioClip == null)
		{
			TPSingleton<AnimatedCutsceneManager>.Instance.LogError("No clip found at Resources path " + text + " during slide " + currentSlideDefinition.Id + "! Aborting music change.");
		}
		else
		{
			TPSingleton<SoundManager>.Instance.FadeMusic(audioClip);
		}
	}

	private bool CheckSkipInput()
	{
		return InputManager.GetSubmitButtonDown();
	}

	private void Init()
	{
		animatedCutsceneDefinition = AnimatedCutsceneDatabase.AnimatedCutsceneDefinitions[currentCutsceneId];
		slideAnimators = new Animator[slideContainers.Length];
		for (int i = 0; i < slideContainers.Length; i++)
		{
			slideAnimators[i] = slideContainers[i].GetComponent<Animator>() ?? slideContainers[i].GetComponentInChildren<Animator>();
		}
		textTyper.Init();
		text.text = string.Empty;
		skipFeedback.SetActive(value: false);
		for (int num = slideContainers.Length - 1; num >= 0; num--)
		{
			slideContainers[num].SetActive(value: false);
		}
	}

	private IEnumerator PlayAnimatedCutscene()
	{
		isRunning = true;
		yield return SharedYields.WaitForSeconds(startDelay);
		int slideIndex = 0;
		int slidesCount = animatedCutsceneDefinition.SlidesDefinitions.Count;
		while (slideIndex < slidesCount)
		{
			currentSlideDefinition = animatedCutsceneDefinition.SlidesDefinitions[slideIndex];
			currentAnimator = slideAnimators[slideIndex];
			currentContainer = slideContainers[slideIndex];
			int nextTextIndex = 0;
			currentContainer.SetActive(value: true);
			int slideItemIndex = 0;
			int itemsCount = currentSlideDefinition.SlideElementsDefinitions.Count;
			int num;
			while (slideItemIndex < itemsCount)
			{
				AnimatedCutsceneSlideItemDefinition animatedCutsceneSlideItemDefinition = currentSlideDefinition.SlideElementsDefinitions[slideItemIndex];
				if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideDelayDefinition delayDefinition))
				{
					if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideClearTextDefinition))
					{
						if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideNextAnimationDefinition nextAnimationDefinition))
						{
							if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideAppendNextTextDefinition appendNextTextDefinition))
							{
								if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlidePlaySoundDefinition playSoundDefinition))
								{
									if (!(animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideChangeMusicDefinition changeMusicDefinition))
									{
										if (animatedCutsceneSlideItemDefinition is AnimatedCutsceneSlideReplaceCommanderViewDefinition)
										{
											ReplaceCommanderView();
										}
									}
									else
									{
										ChangeMusic(changeMusicDefinition);
									}
								}
								else
								{
									PlaySoundEffect(playSoundDefinition);
								}
							}
							else
							{
								yield return AppendNextTextCoroutine(appendNextTextDefinition, currentSlideDefinition.Id, nextTextIndex++);
							}
						}
						else
						{
							yield return PlayNextAnimationCoroutine(nextAnimationDefinition);
						}
					}
					else
					{
						yield return ClearTextCoroutine();
					}
				}
				else
				{
					yield return WaitForDelay(delayDefinition);
				}
				num = slideItemIndex + 1;
				slideItemIndex = num;
			}
			yield return ClearTextCoroutine();
			currentContainer?.SetActive(value: false);
			if (slideIndex < slidesCount - 1)
			{
				yield return SharedYields.WaitForSeconds(delayBetweenSlides);
			}
			num = slideIndex + 1;
			slideIndex = num;
		}
		yield return SharedYields.WaitForSeconds(sequenceOverDelay);
		canvas.enabled = false;
		yield return AnimatedCutsceneManager.OnIntroductionOver();
		isRunning = false;
	}

	private void PlaySoundEffect(AnimatedCutsceneSlidePlaySoundDefinition playSoundDefinition)
	{
		string text = "Sounds/SFX/AnimatedCutscene/" + currentCutsceneId + "/" + playSoundDefinition.ClipAssetName;
		AudioClip audioClip = ResourcePooler.LoadOnce<AudioClip>(text);
		if (audioClip == null)
		{
			TPSingleton<AnimatedCutsceneManager>.Instance.LogError("No clip found at Resources path " + text + " during slide " + currentSlideDefinition.Id + "! Aborting SFX play.");
		}
		else
		{
			if (lastAudioSource != null && lastAudioSource.gameObject.activeSelf)
			{
				SoundManager.FadeOutAudioSource(lastAudioSource, fadeOutDuration);
			}
			lastAudioSource = SoundManager.PlayAudioClip(audioClip, cutsceneAudioSourceData, playSoundDefinition.Delay);
		}
	}

	private void ReplaceCommanderView()
	{
		BuildingManager.MagicCircle.MagicCircleView.ReplaceCommanderView();
	}

	private IEnumerator AppendNextTextCoroutine(AnimatedCutsceneSlideAppendNextTextDefinition appendNextTextDefinition, string textId, int textIndex)
	{
		string key = $"{currentCutsceneId}_{textId}_{textIndex}";
		string text = "<delay=0>" + this.text.text + "</delay>";
		if (textIndex > 0 && !appendNextTextDefinition.NoNewLine)
		{
			text += "\n";
		}
		text += Localizer.Get(key);
		textTyper.TypeText(text);
		yield return new WaitUntil(() => !textTyper.IsTyping || CheckSkipInput());
		yield return WaitForTwoFrames();
		textTyper.Skip();
		if (appendNextTextDefinition.DontWaitForSkipInput)
		{
			yield break;
		}
		float waitForSkipTimer = 0f;
		while (true)
		{
			waitForSkipTimer += Time.deltaTime;
			if (waitForSkipTimer > delayBeforeSkipFeedback)
			{
				skipFeedback.SetActive(value: true);
			}
			if (CheckSkipInput())
			{
				break;
			}
			yield return null;
		}
		skipFeedback.SetActive(value: false);
		yield return WaitForTwoFrames();
	}

	private IEnumerator ClearTextCoroutine()
	{
		text.text = string.Empty;
		yield return null;
	}

	private IEnumerator PlayNextAnimationCoroutine(AnimatedCutsceneSlideNextAnimationDefinition nextAnimationDefinition)
	{
		currentAnimator.SetTrigger(nextAnimationDefinition.CustomParameter ?? "Next");
		yield return SharedYields.WaitForEndOfFrame;
		if (!nextAnimationDefinition.WaitForClipDuration)
		{
			yield break;
		}
		for (float t = 0f; t < currentAnimator.GetCurrentAnimatorStateInfo(0).length; t += Time.deltaTime)
		{
			if (CheckSkipInput())
			{
				currentAnimator.Play(currentAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 1f);
				break;
			}
			yield return null;
		}
	}

	private IEnumerator WaitForDelay(AnimatedCutsceneSlideDelayDefinition delayDefinition)
	{
		if (delayDefinition.Unskippable)
		{
			yield return SharedYields.WaitForSeconds(delayDefinition.Delay);
			yield break;
		}
		for (float t = 0f; t < delayDefinition.Delay; t += Time.deltaTime)
		{
			if (CheckSkipInput())
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator WaitForSkipFull()
	{
		while (isRunning)
		{
			if (InputManager.GetButtonDown(23))
			{
				StopCoroutine(cutsceneCoroutine);
				canvas.enabled = false;
				yield return AnimatedCutsceneManager.OnIntroductionOver();
				break;
			}
			yield return null;
		}
	}

	private IEnumerator WaitForTwoFrames()
	{
		int i = 0;
		while (i < 2)
		{
			yield return SharedYields.WaitForEndOfFrame;
			int num = i + 1;
			i = num;
		}
	}
}
