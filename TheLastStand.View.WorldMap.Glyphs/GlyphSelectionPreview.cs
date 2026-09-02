using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphSelectionPreview : MonoBehaviour
{
	[SerializeField]
	private PreviewedGlyphDisplay previewedGlyphDisplayPrefab;

	[SerializeField]
	private RectTransform glyphDisplayContainer;

	[SerializeField]
	private GlyphsHeader glyphsHeader;

	[SerializeField]
	private Button editButton;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private TextMeshProUGUI lockedCityText;

	private readonly List<PreviewedGlyphDisplay> glyphDisplays = new List<PreviewedGlyphDisplay>();

	public PreviewedGlyphDisplay FirstPreviewedGlyphDisplay
	{
		get
		{
			if (glyphDisplays.Count <= 0)
			{
				return null;
			}
			return glyphDisplays[0];
		}
	}

	public Button EditButton => editButton;

	public void Refresh()
	{
		glyphsHeader.InitCityPointImages();
		glyphsHeader.RefreshCityPoints();
		SetGlyphDisplayContainerVisible(isVisible: true);
		List<GlyphDefinition> selectedGlyphs = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs;
		while (glyphDisplays.Count > selectedGlyphs.Count)
		{
			if (EventSystem.current.currentSelectedGameObject == glyphDisplays[0].gameObject && TPSingleton<GlyphManager>.Instance.GlyphTooltip.Displayed && InputManager.IsLastControllerJoystick)
			{
				TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
			}
			Object.DestroyImmediate(glyphDisplays[0].gameObject);
			glyphDisplays.RemoveAt(0);
		}
		while (glyphDisplays.Count < selectedGlyphs.Count)
		{
			glyphDisplays.Add(Object.Instantiate(previewedGlyphDisplayPrefab, glyphDisplayContainer));
		}
		for (int num = selectedGlyphs.Count - 1; num >= 0; num--)
		{
			glyphDisplays[num].Init(selectedGlyphs[num]);
		}
		layoutNavigationInitializer.InitNavigation(reset: true);
		foreach (PreviewedGlyphDisplay glyphDisplay in glyphDisplays)
		{
			if (glyphDisplay.JoystickSelectable.navigation.selectOnUp == null)
			{
				glyphDisplay.JoystickSelectable.SetSelectOnUp(EditButton);
			}
			if (glyphDisplay.JoystickSelectable.navigation.selectOnDown == null)
			{
				glyphDisplay.JoystickSelectable.SetSelectOnDown(TPSingleton<GameConfigurationsView>.Instance.ApocalypseSelectionPreview.gameObject.activeSelf ? TPSingleton<GameConfigurationsView>.Instance.ApocalypseSelectionPreview.EditButtonSelectable : null);
			}
		}
	}

	public void RefreshLockedUI(WorldMapCity city)
	{
		lockedCityText.gameObject.SetActive(value: false);
		if (city.IsUnlocked)
		{
			SetEditButtonVisible(isVisible: true);
			SetGlyphDisplayContainerVisible(isVisible: true);
			return;
		}
		SetEditButtonVisible(isVisible: false);
		SetGlyphDisplayContainerVisible(isVisible: false);
		if (city.CityDefinition.HasLinkedDLC && !city.IsLinkedDLCOwned)
		{
			SetLockedCityText(city.GetMissingDLCText());
		}
		else
		{
			SetLockedCityText(city.GetLockedCityText());
		}
	}

	private void SetEditButtonVisible(bool isVisible)
	{
		editButton.gameObject.SetActive(isVisible);
	}

	private void SetGlyphDisplayContainerVisible(bool isVisible)
	{
		glyphDisplayContainer.gameObject.SetActive(isVisible);
	}

	private void SetLockedCityText(string lockedCityTextContent)
	{
		lockedCityText.gameObject.SetActive(value: true);
		lockedCityText.text = lockedCityTextContent;
	}
}
