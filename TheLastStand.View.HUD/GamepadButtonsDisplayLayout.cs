using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class GamepadButtonsDisplayLayout : MonoBehaviour
{
	[SerializeField]
	private Image gamepadInputDisplayPrefab;

	[SerializeField]
	private LayoutGroup gamepadInputDisplaysLayoutGroup;

	[SerializeField]
	private float heightMultiplier = 10f;

	[HideInInspector]
	public Vector3 TargetPosition;

	private UnityEngine.Camera mainCamera;

	private RectTransform rectTransform;

	private E_GamepadButtonType[] gamepadButtonTypesValues;

	private readonly List<Image> gamepadInputDisplays = new List<Image>();

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Init()
	{
		mainCamera = UnityEngine.Camera.main;
		rectTransform = base.transform as RectTransform;
		gamepadButtonTypesValues = Enum.GetValues(typeof(E_GamepadButtonType)).Cast<E_GamepadButtonType>().ToArray();
	}

	public void ToggleLayoutGroup(bool state)
	{
		gamepadInputDisplaysLayoutGroup.enabled = state;
	}

	public void RefreshGamepadInputDisplays(E_GamepadButtonType gamepadButtonTypes)
	{
		for (int num = gamepadInputDisplays.Count - 1; num >= 0; num--)
		{
			gamepadInputDisplays[num].gameObject.SetActive(value: false);
		}
		int num2 = 0;
		E_GamepadButtonType[] array = gamepadButtonTypesValues;
		foreach (E_GamepadButtonType e_GamepadButtonType in array)
		{
			if (e_GamepadButtonType != E_GamepadButtonType.NONE && (gamepadButtonTypes & e_GamepadButtonType) != E_GamepadButtonType.NONE)
			{
				if (num2 + 1 > gamepadInputDisplays.Count)
				{
					AddInputDisplay();
				}
				Image image = gamepadInputDisplays[num2];
				GamepadButtonsSet setForButtonType = InputManager.JoystickConfig.GamepadButtonsSetsTable.GetSetForButtonType(e_GamepadButtonType);
				image.sprite = setForButtonType.GetIcon();
				image.gameObject.SetActive(value: true);
				num2++;
			}
		}
	}

	private void AddInputDisplay()
	{
		Image item = UnityEngine.Object.Instantiate(gamepadInputDisplayPrefab, gamepadInputDisplaysLayoutGroup.transform);
		gamepadInputDisplays.Add(item);
	}

	private void Update()
	{
		Vector3 vector = mainCamera.WorldToViewportPoint(TargetPosition);
		if (vector.y < 0f)
		{
			float num = heightMultiplier * ((float)Screen.height / 1080f);
			rectTransform.anchoredPosition = new Vector2(0f, (0f - vector.y) * num);
		}
		else
		{
			rectTransform.anchoredPosition = Vector2.zero;
		}
	}
}
