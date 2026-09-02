using System;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.GroundManagement;

public class GroundManagementPanel : SerializedMonoBehaviour
{
	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject fogPanel;

	[SerializeField]
	private TextMeshProUGUI fogPanelDescription;

	[SerializeField]
	private TextMeshProUGUI fogPanelTitle;

	[SerializeField]
	private Image fogPanelTitleIcon;

	[SerializeField]
	private Image fogPanelTitleBG;

	[SerializeField]
	private Sprite fogTitleIcon;

	[SerializeField]
	private Sprite lightFogTitleIcon;

	[SerializeField]
	private Sprite fogTitleBG;

	[SerializeField]
	private Sprite lightFogTitleBG;

	[SerializeField]
	private DataColor fogTitleColor;

	[SerializeField]
	private DataColor lightFogTitleColor;

	[SerializeField]
	private TextMeshProUGUI groundDescriptionText;

	[SerializeField]
	private TextMeshProUGUI groundIdText;

	[SerializeField]
	private Image groundPortraitImage;

	private Tile tile;

	public void Close()
	{
		tile = null;
		canvas.enabled = false;
		simpleFontLocalizedParent?.UnregisterChildren();
	}

	public void Open()
	{
		canvas.enabled = UIManager.DebugToggleUI != false;
		simpleFontLocalizedParent?.RegisterChildren();
		Refresh();
	}

	public void Refresh()
	{
		if (!TileObjectSelectionManager.HasTileSelected)
		{
			Close();
		}
		else if (TileObjectSelectionManager.SelectedTile != tile)
		{
			tile = TileObjectSelectionManager.SelectedTile;
			groundPortraitImage.sprite = tile.TileView.GetPortraitSprite();
			RefreshLocalizedTexts();
			bool flag = TPSingleton<FogManager>.Exist() && TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile) && TPSingleton<FogManager>.Instance.Fog.LightFogTiles[tile].Mode != Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			if (tile.HasFog || flag)
			{
				fogPanelTitle.color = (tile.HasFog ? fogTitleColor._Color : lightFogTitleColor._Color);
				fogPanelTitleBG.sprite = (tile.HasFog ? fogTitleBG : lightFogTitleBG);
				fogPanelTitleIcon.sprite = (tile.HasFog ? fogTitleIcon : lightFogTitleIcon);
				fogPanel.SetActive(value: true);
			}
			else if (fogPanel.activeSelf)
			{
				fogPanel.SetActive(value: false);
			}
		}
	}

	private void RefreshLocalizedTexts()
	{
		if (tile != null)
		{
			string text = "_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
			string text2 = "GroundName_" + tile.GroundDefinition.Id;
			string text3 = "GroundDescription_" + tile.GroundDefinition.Id;
			groundIdText.text = Localizer.Get(Localizer.ExistsInCurrentLanguage(text2 + text) ? (text2 + text) : text2);
			groundDescriptionText.text = Localizer.Get(Localizer.ExistsInCurrentLanguage(text3 + text) ? (text3 + text) : text3);
			bool flag = TPSingleton<FogManager>.Exist() && TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile) && TPSingleton<FogManager>.Instance.Fog.LightFogTiles[tile].Mode != Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			if (tile.HasFog || flag)
			{
				string text4 = (tile.HasFog ? "Fog" : "LightFog");
				fogPanelTitle.text = Localizer.Get("FogName_" + text4);
				fogPanelDescription.text = Localizer.Get("FogDescription_" + text4);
			}
		}
	}

	private void Start()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		Close();
	}

	private void OnLocalize()
	{
		if (canvas.enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}
}
