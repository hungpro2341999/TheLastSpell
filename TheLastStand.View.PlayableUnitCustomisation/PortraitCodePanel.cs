using System.Collections;
using DG.Tweening;
using PortraitAPI.Misc;
using TMPro;
using TPLib.Yield;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitCodePanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI codeText;

	[SerializeField]
	private BetterButton editCodeButton;

	[SerializeField]
	private BetterButton copyCodeButton;

	[SerializeField]
	private PortraitCodePopup portraitCodePopup;

	[SerializeField]
	private CanvasGroup copyFeedbackCanvasGroup;

	public bool IsEditingCode;

	private Tween copyFeedbackApparitionTween;

	private Tween copyFeedbackDisappearTween;

	public CodeGenerator.CodeData CurrentCode { get; private set; }

	public PortraitCodePopup PortraitCodePopup => portraitCodePopup;

	public void Refresh(CodeGenerator.CodeData codeData)
	{
		CurrentCode = codeData;
		codeText.text = CurrentCode.ToString();
	}

	public void Refresh(string code)
	{
		if (CodeGenerator.TryDecode(code, out var codeData))
		{
			CurrentCode = codeData;
			codeText.text = CurrentCode.ToString();
		}
	}

	private void OnCopyCodeButtonClicked()
	{
		GUIUtility.systemCopyBuffer = codeText.text;
		Tween tween = copyFeedbackApparitionTween;
		if (tween != null && tween.IsPlaying())
		{
			copyFeedbackApparitionTween.Kill();
		}
		Tween tween2 = copyFeedbackDisappearTween;
		if (tween2 != null && tween2.IsPlaying())
		{
			copyFeedbackDisappearTween.Kill();
		}
		copyFeedbackApparitionTween = copyFeedbackCanvasGroup.DOFade(1f, 0.25f).OnComplete(delegate
		{
			StartCoroutine(WaitThenPlayDisappear());
		});
	}

	private void OnEditCodeButtonClicked()
	{
		IsEditingCode = true;
		portraitCodePopup.Open();
		portraitCodePopup.Refresh(codeText.text);
	}

	private void Start()
	{
		editCodeButton.onClick.AddListener(OnEditCodeButtonClicked);
		copyCodeButton.onClick.AddListener(OnCopyCodeButtonClicked);
	}

	private void OnDestroy()
	{
		editCodeButton.onClick.RemoveListener(OnEditCodeButtonClicked);
		copyCodeButton.onClick.RemoveListener(OnCopyCodeButtonClicked);
	}

	private IEnumerator WaitThenPlayDisappear()
	{
		yield return SharedYields.WaitForSeconds(1f);
		copyFeedbackDisappearTween = copyFeedbackCanvasGroup.DOFade(0f, 0.25f);
	}
}
