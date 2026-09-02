using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitPortraitPanel;

public class UnitPortraitsPanel : MonoBehaviour
{
	private UnitPortraitView previousPortraitCursorIsHover;

	private UnitPortraitView currentPortraitCursorIsHover;

	[SerializeField]
	private Canvas portraitsCanvas;

	[SerializeField]
	private float leftBorderWidth = 59f;

	[SerializeField]
	private float rightBorderWidth = 43f;

	[SerializeField]
	private Vector2 widthBoundaries = new Vector2(266f, 640f);

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private UnitPortraitPanel unitPortraitPanelPrefab;

	[SerializeField]
	private RectTransform unitPortraitsParent;

	[SerializeField]
	private ToggleGroup unitPortraitsToggleGroup;

	[SerializeField]
	private GameObject leftArrowGameObject;

	[SerializeField]
	private GameObject rightArrowGameObject;

	[SerializeField]
	private ScrollRect portraitsScrollRect;

	private List<UnitPortraitView> unitPortraits = new List<UnitPortraitView>();

	public Canvas PortraitsCanvas => portraitsCanvas;

	public bool CursorIsHoverPortrait => currentPortraitCursorIsHover != null;

	public bool TargettedPortraitHasChanged => currentPortraitCursorIsHover != previousPortraitCursorIsHover;

	public UnitPortraitPanel AddPortrait(PlayableUnit playableUnit)
	{
		UnitPortraitPanel newUnitPortrait = Object.Instantiate(unitPortraitPanelPrefab, unitPortraitsParent);
		unitPortraits.Add(newUnitPortrait);
		newUnitPortrait.PlayableUnit = playableUnit;
		newUnitPortrait.RefreshPortrait();
		newUnitPortrait.RefreshStats();
		newUnitPortrait.UnitPortraitToggle.onValueChanged.AddListener(delegate
		{
			newUnitPortrait.OnUnitPortraitClick();
		});
		newUnitPortrait.UnitPortraitToggle.OnPointerEnterEvent.AddListener(delegate
		{
			newUnitPortrait.OnUnitPortraitHoverEnter();
		});
		newUnitPortrait.UnitPortraitToggle.OnPointeExitEvent.AddListener(delegate
		{
			newUnitPortrait.OnUnitPortraitHoverExit();
		});
		unitPortraitsToggleGroup.RegisterToggle(newUnitPortrait.UnitPortraitToggle);
		newUnitPortrait.UnitPortraitToggle.group = unitPortraitsToggleGroup;
		StartCoroutine(RefreshSize());
		return newUnitPortrait;
	}

	public void DeselectAll()
	{
		unitPortraitsToggleGroup.SetAllTogglesOff();
	}

	public void Display(bool show)
	{
		portraitsScrollRect.enabled = show && UIManager.DebugToggleUI != false;
		portraitsCanvas.enabled = show && UIManager.DebugToggleUI != false;
	}

	public UnitPortraitView GetPortraitIsHovered()
	{
		return currentPortraitCursorIsHover;
	}

	public UnitPortraitView GetPreviousPortraitWasHovered()
	{
		return previousPortraitCursorIsHover;
	}

	public void RefreshPortraits()
	{
		for (int i = 0; i < unitPortraits.Count; i++)
		{
			unitPortraits[i].RefreshPortrait();
		}
	}

	public void RefreshPortraitsStats()
	{
		for (int i = 0; i < unitPortraits.Count; i++)
		{
			unitPortraits[i].RefreshStats();
		}
	}

	public void RemovePortrait(int unitIndex)
	{
		unitPortraitsToggleGroup.UnregisterToggle(unitPortraits[unitIndex].GetComponent<Toggle>());
		Object.Destroy(unitPortraits[unitIndex].gameObject);
		unitPortraits.RemoveAt(unitIndex);
		StartCoroutine(RefreshSize());
	}

	public IEnumerator RefreshSize()
	{
		yield return SharedYields.WaitForFrames(1);
		float x = Mathf.Clamp(leftBorderWidth + unitPortraitsParent.sizeDelta.x + rightBorderWidth, widthBoundaries.x, widthBoundaries.y);
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
		ToggleArrows();
	}

	public void SetPortraitIsHovered(UnitPortraitView unitPortraitView = null)
	{
		currentPortraitCursorIsHover = unitPortraitView;
	}

	public void ToggleSelectedUnit(int unitIndex)
	{
		unitPortraits[unitIndex].UnitPortraitToggle.isOn = true;
	}

	public void ToggleSelectedUnit(PlayableUnit unit)
	{
		UnitPortraitView unitPortraitView = unitPortraits.Find((UnitPortraitView o) => o.PlayableUnit == unit);
		if (unitPortraitView == null)
		{
			TPSingleton<UIManager>.Instance.LogWarning("Told to toggle portrait for unit " + unit.Name + ", but they have no portrait! What's up?");
		}
		else
		{
			unitPortraitView.UnitPortraitToggle.isOn = true;
		}
	}

	public void ToggleUnselectedUnit(PlayableUnit unit)
	{
		UnitPortraitView unitPortraitView = unitPortraits.Find((UnitPortraitView o) => o.PlayableUnit == unit);
		if (unitPortraitView == null)
		{
			TPSingleton<UIManager>.Instance.LogWarning("Told to toggle portrait for unit " + unit.Name + ", but they have no portrait! What's up?");
		}
		else
		{
			unitPortraitView.UnitPortraitToggle.isOn = false;
		}
	}

	private void ToggleArrows()
	{
		if (leftBorderWidth + unitPortraitsParent.sizeDelta.x + rightBorderWidth > rectTransform.sizeDelta.x)
		{
			leftArrowGameObject.SetActive(value: true);
			rightArrowGameObject.SetActive(value: true);
		}
		else
		{
			leftArrowGameObject.SetActive(value: false);
			rightArrowGameObject.SetActive(value: false);
		}
	}

	private void Awake()
	{
		GetComponent<Canvas>().sortingOrder = 0;
	}

	private void Update()
	{
		previousPortraitCursorIsHover = currentPortraitCursorIsHover;
	}
}
