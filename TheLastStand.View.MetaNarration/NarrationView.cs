using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Meta;
using TheLastStand.View.MetaShops;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaNarration;

public class NarrationView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup replicasGroup;

	[SerializeField]
	private NarrationReplicaView[] replicaViews;

	[SerializeField]
	private RectTransform greetingPanelParent;

	[SerializeField]
	private RectTransform greetingPanelScaler;

	[SerializeField]
	private CanvasGroup greetingPanelGroup;

	[SerializeField]
	private TextTyper greetingTextTyper;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private RectTransform contentRoot;

	[SerializeField]
	private float greetingPanelInDuration = 0.3f;

	[SerializeField]
	private float greetingPanelOutDuration = 0.15f;

	[SerializeField]
	private Ease greetingPanelInEase = Ease.InOutSine;

	[SerializeField]
	private Ease greetingPanelOutEase = Ease.OutCubic;

	[SerializeField]
	private float greetingPanelOffset = -50f;

	[SerializeField]
	private float greetingPanelOffsetDuration = 0.3f;

	[SerializeField]
	private Ease greetingPanelOffsetEase = Ease.OutCirc;

	[SerializeField]
	private AudioClip onCharacterPrintedSound;

	[SerializeField]
	private float greetingPanelSmokeOffset = 235f;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	private int answersCounter;

	private int answersToDisplayCount;

	private bool dialogueOver;

	private bool isTypingText;

	private bool isWaitingForSkipInput;

	private NarrationReplicaView selectedReplica;

	public TheLastStand.Model.Meta.MetaNarration MetaNarration { get; set; }

	public bool HasNarrationToPlay
	{
		get
		{
			if (!MetaNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out var replicas))
			{
				if (!MetaNarrationsManager.NarrationDoneThisDay)
				{
					return MetaNarration.MetaNarrationController.TryGetValidReplicas(1, out replicas);
				}
				return false;
			}
			return true;
		}
	}

	public void DisplayShopGreeting(string greetingId)
	{
		if (greetingPanelGroup.alpha == 0f)
		{
			FadeInGreetingPanel();
		}
		greetingTextTyper.TypeText(Localizer.Get(MetaNarration.LocalizationGreetingPrefix + greetingId));
		greetingPanelScaler.anchoredPosition = new Vector2(0f, greetingPanelScaler.anchoredPosition.y);
	}

	private void FadeInGreetingPanel()
	{
		greetingPanelScaler.localScale = Vector3.one * ((Screen.height > 768) ? 1f : (2f / 3f));
		RefreshGreetingPanelSize();
		if (!(greetingPanelGroup.alpha >= 1f))
		{
			RefreshDisplayedName();
			greetingTextTyper.TypeText(" ");
			greetingPanelGroup.alpha = 0f;
			greetingPanelGroup.DOFade(1f, greetingPanelInDuration).SetEase(greetingPanelInEase);
		}
	}

	private void RefreshGreetingPanelSize()
	{
		Vector2 sizeDelta = greetingPanelScaler.sizeDelta;
		float num = greetingPanelParent.rect.width / greetingPanelScaler.localScale.x + greetingPanelSmokeOffset * 2f;
		float num2 = greetingPanelScaler.rect.width - sizeDelta.x;
		sizeDelta = new Vector2(num - num2, sizeDelta.y);
		greetingPanelScaler.sizeDelta = sizeDelta;
	}

	public IEnumerator GreetingSequenceCoroutine(string greetingId)
	{
		greetingPanelScaler.anchoredPosition = new Vector2(greetingPanelOffset, greetingPanelScaler.anchoredPosition.y);
		FadeInGreetingPanel();
		yield return SharedYields.WaitForSeconds(greetingPanelInDuration);
		greetingTextTyper.TypeText(Localizer.Get(MetaNarration.LocalizationGreetingPrefix + greetingId));
		isTypingText = true;
		yield return new WaitUntil(InputManager.GetSubmitButtonDown);
		DOTween.To(() => greetingPanelOffset, delegate(float x)
		{
			greetingPanelScaler.anchoredPosition = new Vector2(x, greetingPanelScaler.anchoredPosition.y);
		}, 0f, greetingPanelOffsetDuration).SetEase(greetingPanelOffsetEase);
	}

	public void Hide()
	{
		greetingPanelGroup.DOFade(0f, greetingPanelOutDuration).SetEase(greetingPanelOutEase);
		HideReplicas();
	}

	public IEnumerator NarrationSequenceCoroutine(List<MetaReplica> replicas)
	{
		replicasGroup.alpha = 0f;
		replicasGroup.DOFade(1f, 0.3f).SetDelay(0.2f).SetEase(Ease.InOutSine);
		SetReplicas(replicas);
		yield return new WaitUntil(() => dialogueOver);
		dialogueOver = false;
		HideReplicas();
	}

	public void OnCharacterPrinted()
	{
		SoundManager.PlayAudioClip(onCharacterPrintedSound);
	}

	public void OnReplicaSelected(NarrationReplicaView replicaView)
	{
		if (selectedReplica != null)
		{
			return;
		}
		if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
		{
			TPSingleton<DarkShopManager>.Instance.MetaShopView.ExitText.SetActive(value: true);
		}
		else
		{
			TPSingleton<LightShopManager>.Instance.MetaShopView.ExitText.SetActive(value: true);
		}
		selectedReplica = replicaView;
		MetaNarration.MetaNarrationController.MarkReplicaAsUsed(replicaView.Replica);
		FadeInGreetingPanel();
		for (int i = 0; i < replicaViews.Length; i++)
		{
			if (replicaViews[i] != replicaView)
			{
				replicaViews[i].HideWithoutDisabling();
			}
		}
		AllowReplicasInteraction(allowed: false);
		answersToDisplayCount = replicaView.Replica.MetaReplicaDefinition.AnswersCount;
		answersCounter = 0;
		DisplayNextAnswer();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public void RefreshDisplayedName()
	{
		nameText.text = (MetaNarration.MetaNarrationController.CanDisplayGoddessName() ? MetaNarration.GoddessName : Localizer.Get("MetaShops_UnknownGoddessName"));
	}

	public void RefreshLocalizedFonts()
	{
		fontLocalizedParent?.RefreshChildren();
	}

	private void AllowReplicasInteraction(bool allowed)
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].AllowInteraction(allowed);
		}
	}

	private void Awake()
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].NarrationView = this;
		}
		greetingTextTyper.PrintCompleted.AddListener(OnGreetingOrAnswerTypeCompleted);
	}

	private void DisplayNextAnswer()
	{
		greetingTextTyper.TypeText(Localizer.Get(string.Format(selectedReplica.Replica.LocalizationAnswerFormat, selectedReplica.Replica.Id, answersCounter)));
		isTypingText = true;
		answersCounter++;
	}

	private void HideReplicas()
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].Display(show: false);
		}
	}

	private void OnGreetingOrAnswerTypeCompleted()
	{
		isTypingText = false;
		if (selectedReplica != null)
		{
			isWaitingForSkipInput = true;
		}
	}

	private void OnNextButtonClicked()
	{
		if (answersCounter == answersToDisplayCount)
		{
			dialogueOver = true;
			selectedReplica = null;
			HideReplicas();
		}
		else
		{
			DisplayNextAnswer();
		}
	}

	private void SetReplicas(List<MetaReplica> replicas)
	{
		int i;
		for (i = 0; i < replicas.Count; i++)
		{
			NarrationReplicaView narrationReplicaView = replicaViews[i];
			narrationReplicaView.Button.onClick.RemoveAllListeners();
			narrationReplicaView.Button.onClick.AddListener(narrationReplicaView.OnClick);
			narrationReplicaView.SetReplica(replicas[i]);
		}
		for (; i < replicaViews.Length; i++)
		{
			replicaViews[i].Display(show: false);
		}
		AllowReplicasInteraction(allowed: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
		RefreshJoystickNavigation();
		StartCoroutine(SelectFirstReplicaJoystickEndOfFrame());
	}

	private void RefreshJoystickNavigation()
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].Button.SetMode(Navigation.Mode.Explicit);
			replicaViews[i].Button.ClearNavigation();
			if (replicaViews[i].gameObject.activeSelf)
			{
				if (i > 0)
				{
					replicaViews[i].Button.SetSelectOnUp(replicaViews[i - 1].Button);
				}
				if (i < replicaViews.Length - 1 && replicaViews[i + 1].gameObject.activeSelf)
				{
					replicaViews[i].Button.SetSelectOnDown(replicaViews[i + 1].Button);
				}
				continue;
			}
			break;
		}
	}

	private IEnumerator SelectFirstReplicaJoystickEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(replicaViews[0].gameObject);
		}
	}

	public void SkipNextNarration()
	{
		if (isTypingText)
		{
			greetingTextTyper.Skip();
			isTypingText = false;
		}
		if (isWaitingForSkipInput || selectedReplica != null)
		{
			isWaitingForSkipInput = false;
			dialogueOver = true;
			selectedReplica = null;
			HideReplicas();
		}
	}

	private void Update()
	{
		if ((!isTypingText && !isWaitingForSkipInput) || !InputManager.GetSubmitButtonDown())
		{
			return;
		}
		if (isTypingText)
		{
			greetingTextTyper.Skip();
			isTypingText = false;
			if (selectedReplica != null)
			{
				isWaitingForSkipInput = true;
			}
		}
		else if (isWaitingForSkipInput)
		{
			OnNextButtonClicked();
			isWaitingForSkipInput = false;
		}
		else if (selectedReplica != null)
		{
			selectedReplica.OnClick();
		}
	}
}
