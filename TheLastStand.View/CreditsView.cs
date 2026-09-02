using System.Collections;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using UnityEngine;

namespace TheLastStand.View;

public class CreditsView : Manager<CreditsView>
{
	[SerializeField]
	private RectTransform creditsRectTransform;

	[SerializeField]
	[Min(100f)]
	[Tooltip("Scroll speed must be adjusted using a screen height of 1080p as reference.")]
	private float scrollSpeed = 100f;

	private bool isQuitting;

	private void GoBackToMenu()
	{
		if (!isQuitting)
		{
			ApplicationManager.Application.ApplicationController.SetState("GameLobby");
			isQuitting = true;
		}
	}

	private IEnumerator DisplayCreditsCoroutine()
	{
		float num = Screen.height;
		float adjustedScrollSpeed = scrollSpeed * num / 1080f;
		creditsRectTransform.anchorMin = new Vector2(0.5f, 1f);
		creditsRectTransform.anchorMax = new Vector2(0.5f, 1f);
		creditsRectTransform.pivot = new Vector2(creditsRectTransform.pivot.x, 0f);
		creditsRectTransform.anchoredPosition -= new Vector2(creditsRectTransform.anchoredPosition.x, creditsRectTransform.rect.size.y + (float)Screen.height);
		while (creditsRectTransform.anchoredPosition.y < 0f)
		{
			Vector2 anchoredPosition = creditsRectTransform.anchoredPosition;
			anchoredPosition.y += adjustedScrollSpeed * Time.deltaTime;
			creditsRectTransform.anchoredPosition = anchoredPosition;
			yield return null;
		}
		GoBackToMenu();
	}

	private void Start()
	{
		TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.CreditsMusic);
		StartCoroutine(DisplayCreditsCoroutine());
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(23))
		{
			GoBackToMenu();
		}
	}
}
