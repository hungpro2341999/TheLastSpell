using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PlayableUnitCustomisationPanel : TPSingleton<PlayableUnitCustomisationPanel>, IOverlayUser
{
	[SerializeField]
	private Material colorSwapMaterial;

	[SerializeField]
	private Material colorSwapPortraitMaterial;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private BetterButton resetButton;

	[SerializeField]
	private Image imageBG;

	[SerializeField]
	private PortraitPartsDisplay portraitPartsDisplay;

	[SerializeField]
	private AppearanceToggle apperanceToggle;

	[SerializeField]
	private List<LayerTextureHandler> textureHandlers = new List<LayerTextureHandler>();

	[SerializeField]
	private BetterToggle beardLockToHairToggle;

	[SerializeField]
	private TextMeshProUGUI beardLockToHairToggleText;

	[SerializeField]
	private List<ColorHandler> colorHandlers = new List<ColorHandler>();

	[SerializeField]
	private RenameHeader renameHeader;

	[SerializeField]
	private PortraitCodePanel portraitCodePanel;

	[SerializeField]
	private Image unitAvatarImage;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	private static readonly int SwapTex = Shader.PropertyToID("_SwapTex");

	private Texture2D currentColorTexture;

	private Tween fadeTween;

	private Handler currentJoystickSelectedHandler;

	private bool initialized;

	private bool openedThisFrame;

	private string initialCode = string.Empty;

	private string initialFaceId = string.Empty;

	private string initialName = string.Empty;

	[HideInInspector]
	public bool RefreshBeardHandler;

	[HideInInspector]
	public bool RefreshColors;

	[HideInInspector]
	public bool RefreshPortraitParts;

	[HideInInspector]
	public Commons.E_Gender CurrentGender;

	private PlayableUnit playableUnit;

	private Commons.E_Gender initialGender;

	private readonly Dictionary<Commons.E_LayerType, SimpleLayer> currentLayers = new Dictionary<Commons.E_LayerType, SimpleLayer>();

	private readonly Dictionary<Commons.E_LayerKind, Material> layerMaterials = new Dictionary<Commons.E_LayerKind, Material>();

	private readonly Dictionary<Commons.E_Gender, Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData>> previousIndexByLayerType = new Dictionary<Commons.E_Gender, Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData>>();

	public Dictionary<Commons.E_ColorTypes, Dictionary<string, ColorSwapPaletteDefinition>> AllPalettesByColorType { get; } = new Dictionary<Commons.E_ColorTypes, Dictionary<string, ColorSwapPaletteDefinition>>();

	public bool BeardIsLockToHairType { get; private set; } = true;

	public Material ColorSwapMaterial => colorSwapMaterial;

	public List<SimpleLayer> CurrentBeards { get; } = new List<SimpleLayer>();

	public Dictionary<Commons.E_ColorTypes, CodeGenerator.CodeColorData> CurrentIndexByColorType { get; } = new Dictionary<Commons.E_ColorTypes, CodeGenerator.CodeColorData>();

	public Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData> CurrentIndexByLayerType { get; private set; } = new Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData>();

	public int OverlaySortingOrder => canvas.sortingOrder - 2;

	public PortraitCodePanel PortraitCodePanel => portraitCodePanel;

	public RenameHeader RenameHeader => renameHeader;

	public Dictionary<Commons.E_ColorTypes, Dictionary<string, Texture2D>> TexturesByColorType { get; } = new Dictionary<Commons.E_ColorTypes, Dictionary<string, Texture2D>>();

	public void ChangeIndexOfColorType(Commons.E_ColorTypes colorType, int valueToSet)
	{
		CurrentIndexByColorType[colorType].SetIndex(valueToSet);
		UpdateColorHandler(colorType);
		RefreshColors = true;
	}

	public void ChangeIndexOfLayerType(Commons.E_LayerType layerType, int dropdownValue)
	{
		Commons.E_Gender currentGender = CurrentGender;
		Commons.E_Gender layerGender;
		SimpleLayer simpleLayerFromDropdownValue = GetSimpleLayerFromDropdownValue(layerType, currentGender, dropdownValue, out layerGender);
		UpdateCurrentIndex(layerType, layerGender, simpleLayerFromDropdownValue?.LayerGlobalIndex ?? (-1));
	}

	public SimpleLayer GetSimpleLayerFromDropdownValue(Commons.E_LayerType layerType, Commons.E_Gender selectedGender, int dropdownValue, out Commons.E_Gender layerGender)
	{
		if (dropdownValue == -1)
		{
			layerGender = Commons.E_Gender.Man;
			return null;
		}
		if (layerType == Commons.E_LayerType.Beard && BeardIsLockToHairType)
		{
			SimpleLayer simpleLayer = CurrentBeards[dropdownValue];
			layerGender = CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender;
			if (LayerManagement.GetAvailableLayers<SimpleLayer>(layerGender, Commons.E_LayerType.Beard).IndexOf(simpleLayer) != -1)
			{
				return simpleLayer;
			}
		}
		else
		{
			List<SimpleLayer> availableLayers = LayerManagement.GetAvailableLayers<SimpleLayer>(selectedGender, layerType);
			if (dropdownValue < availableLayers.Count)
			{
				SimpleLayer simpleLayer2 = availableLayers[dropdownValue];
				layerGender = simpleLayer2.Gender;
				return simpleLayer2;
			}
		}
		layerGender = Commons.E_Gender.Man;
		return null;
	}

	public int GetDropdownValueFromSimpleLayerIndex(Commons.E_LayerType layerType, Commons.E_Gender layerGender, int layerIndex, Commons.E_Gender uiSelectedGender)
	{
		if (layerIndex == -1)
		{
			return -1;
		}
		List<SimpleLayer> allLayers = LayerManagement.GetAllLayers<SimpleLayer>(layerGender, layerType);
		if (layerIndex >= allLayers.Count)
		{
			return -1;
		}
		SimpleLayer item = allLayers[layerIndex];
		if (layerType == Commons.E_LayerType.Beard)
		{
			int num = CurrentBeards.IndexOf(item);
			if (CurrentBeards.Count > 0 && num == -1)
			{
				num = 0;
			}
			return num;
		}
		return LayerManagement.GetAvailableLayers<SimpleLayer>(uiSelectedGender, layerType).IndexOf(item);
	}

	public void Init()
	{
		if (initialized)
		{
			return;
		}
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		resetButton.onClick.AddListener(OnResetButtonClicked);
		for (int i = 0; i < Enum.GetValues(typeof(Commons.E_ColorTypes)).Length; i++)
		{
			Commons.E_ColorTypes e_ColorTypes = (Commons.E_ColorTypes)i;
			Dictionary<string, ColorSwapPaletteDefinition> value = e_ColorTypes switch
			{
				Commons.E_ColorTypes.Skin => PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, 
				Commons.E_ColorTypes.Hair => PlayableUnitDatabase.PlayableUnitHairColorDefinitions, 
				Commons.E_ColorTypes.Eyes => PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, 
				_ => null, 
			};
			AllPalettesByColorType.Add(e_ColorTypes, value);
		}
		if (CurrentIndexByLayerType.Count == 0)
		{
			for (int j = 0; j < Enum.GetValues(typeof(Commons.E_LayerType)).Length; j++)
			{
				Commons.E_LayerType key = (Commons.E_LayerType)j;
				CurrentIndexByLayerType.Add(key, new CodeGenerator.CodeLayerData(CurrentGender, 1));
			}
		}
		if (CurrentIndexByColorType.Count == 0)
		{
			for (int k = 0; k < Enum.GetValues(typeof(Commons.E_ColorTypes)).Length; k++)
			{
				Commons.E_ColorTypes key2 = (Commons.E_ColorTypes)k;
				CurrentIndexByColorType.Add(key2, new CodeGenerator.CodeColorData(0));
			}
		}
		TexturesByColorType.Add(Commons.E_ColorTypes.Hair, new Dictionary<string, Texture2D>());
		TexturesByColorType.Add(Commons.E_ColorTypes.Skin, new Dictionary<string, Texture2D>());
		TexturesByColorType.Add(Commons.E_ColorTypes.Eyes, new Dictionary<string, Texture2D>());
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitHairColorDefinition in PlayableUnitDatabase.PlayableUnitHairColorDefinitions)
		{
			Texture2D colorSwapTex = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
			{
				filterMode = FilterMode.Point
			};
			for (int l = 0; l < colorSwapTex.width; l++)
			{
				colorSwapTex.SetPixel(l, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitHairColorDefinition.Value, ref colorSwapTex);
			colorSwapTex.Apply();
			TexturesByColorType[Commons.E_ColorTypes.Hair].Add(playableUnitHairColorDefinition.Key, colorSwapTex);
		}
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitEyesColorDefinition in PlayableUnitDatabase.PlayableUnitEyesColorDefinitions)
		{
			Texture2D colorSwapTex2 = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
			{
				filterMode = FilterMode.Point
			};
			for (int m = 0; m < colorSwapTex2.width; m++)
			{
				colorSwapTex2.SetPixel(m, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitEyesColorDefinition.Value, ref colorSwapTex2);
			colorSwapTex2.Apply();
			TexturesByColorType[Commons.E_ColorTypes.Eyes].Add(playableUnitEyesColorDefinition.Key, colorSwapTex2);
		}
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitSkinColorDefinition in PlayableUnitDatabase.PlayableUnitSkinColorDefinitions)
		{
			Texture2D colorSwapTex3 = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
			{
				filterMode = FilterMode.Point
			};
			for (int n = 0; n < colorSwapTex3.width; n++)
			{
				colorSwapTex3.SetPixel(n, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitSkinColorDefinition.Value, ref colorSwapTex3);
			colorSwapTex3.Apply();
			TexturesByColorType[Commons.E_ColorTypes.Skin].Add(playableUnitSkinColorDefinition.Key, colorSwapTex3);
		}
		initialized = true;
	}

	public void OnBeardLockToHairAvailabilityChanged(bool availabilityState)
	{
		beardLockToHairToggle.interactable = !availabilityState;
		beardLockToHairToggle.image.CrossFadeAlpha(availabilityState ? 0.35f : 1f, 0.25f, ignoreTimeScale: false);
		beardLockToHairToggleText.DOFade(availabilityState ? 0.35f : 1f, 0.25f);
		if (beardLockToHairToggle.isOn)
		{
			beardLockToHairToggle.isOn = false;
		}
	}

	public void OnCancelButtonClicked()
	{
		if (!InputManager.IsLastControllerJoystick || !openedThisFrame)
		{
			OnResetButtonClicked();
			Close();
		}
	}

	public void Open(PlayableUnit playableUnit)
	{
		Init();
		canvas.enabled = true;
		fadeTween?.Kill();
		fadeTween = canvasGroup.DOFade(1f, 0.25f);
		fadeTween.Play();
		CameraView.AttenuateWorldForPopupFocus(this);
		if (playableUnit != null)
		{
			this.playableUnit = playableUnit;
			CodeGenerator.CodeData portraitCodeData = playableUnit.PortraitCodeData;
			unitAvatarImage.sprite = playableUnit.UiSprite;
			SetInitialsValues(this.playableUnit, portraitCodeData);
			renameHeader.Refresh(playableUnit);
			portraitCodePanel.Refresh(portraitCodeData);
			PopulateTextureHandlers(portraitCodeData);
			PopulateColorHandlers(portraitCodeData);
			currentColorTexture = (Texture2D)playableUnit.PlayableUnitView.ColorSwapPortraitMaterial.GetTexture(SwapTex);
			layerMaterials.Clear();
			for (int i = 0; i < Enum.GetValues(typeof(Commons.E_LayerKind)).Length; i++)
			{
				Commons.E_LayerKind key = (Commons.E_LayerKind)i;
				Material material = new Material(colorSwapPortraitMaterial)
				{
					name = "Copy - " + key
				};
				material.SetTexture(SwapTex, currentColorTexture);
				layerMaterials.Add(key, material);
			}
			imageBG.sprite = playableUnit.PortraitBackgroundSprite;
			imageBG.color = playableUnit.PortraitColor._Color;
			apperanceToggle.SelectState(CurrentGender);
			apperanceToggle.OnAppearanceChanged.AddListener(OnAppearanceChange);
			beardLockToHairToggle.onValueChanged.AddListener(delegate
			{
				SwitchBeardIsLockToHairType();
			});
			if (fontLocalizedParent != null)
			{
				fontLocalizedParent.RefreshChildren();
			}
			RefreshPortraitParts = true;
			openedThisFrame = true;
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			}
			PortraitCodePanel.gameObject.SetActive(!InputManager.IsLastControllerJoystick);
		}
	}

	public void SetHandlerAsJoystickTarget(Handler handler)
	{
		currentJoystickSelectedHandler = handler;
	}

	public void SwitchBeardIsLockToHairType()
	{
		BeardIsLockToHairType = beardLockToHairToggle.isOn;
		LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == Commons.E_LayerType.Beard);
		if (!BeardIsLockToHairType)
		{
			int num = ((CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex > -1) ? CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex : 0);
			Commons.E_Gender gender = CurrentIndexByLayerType[Commons.E_LayerType.Beard].Gender;
			SimpleLayer simpleLayer = null;
			List<BeardLayers> allLayers = LayerManagement.GetAllLayers<BeardLayers>(gender, Commons.E_LayerType.Beard);
			if (num < allLayers.Count)
			{
				simpleLayer = allLayers[num];
			}
			layerTextureHandler.Refresh();
			int valueWithoutNotify = -1;
			if (simpleLayer != null)
			{
				valueWithoutNotify = ((gender == Commons.E_Gender.Woman) ? CurrentBeards.LastIndexOf(simpleLayer) : CurrentBeards.IndexOf(simpleLayer));
				UpdateCurrentIndex(Commons.E_LayerType.Beard, simpleLayer.Gender, simpleLayer.LayerGlobalIndex);
			}
			else
			{
				UpdateCurrentIndex(Commons.E_LayerType.Beard, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(valueWithoutNotify);
		}
		else if ((bool)beardLockToHairToggle && CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex != -1)
		{
			layerTextureHandler.Refresh();
			int num2 = -1;
			if (CurrentBeards.Count != 0)
			{
				Commons.E_Gender gender2 = CurrentIndexByLayerType[Commons.E_LayerType.Beard].Gender;
				List<BeardLayers> allLayers2 = LayerManagement.GetAllLayers<BeardLayers>(gender2, Commons.E_LayerType.Beard);
				SimpleLayer item = allLayers2[Mathf.Clamp(CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex, 0, allLayers2.Count - 1)];
				num2 = (CurrentBeards.Contains(item) ? ((gender2 == Commons.E_Gender.Woman) ? CurrentBeards.LastIndexOf(item) : CurrentBeards.IndexOf(item)) : 0);
			}
			if (num2 > -1)
			{
				SimpleLayer simpleLayer2 = CurrentBeards[num2];
				UpdateCurrentIndex(Commons.E_LayerType.Beard, simpleLayer2.Gender, simpleLayer2.LayerGlobalIndex);
			}
			else
			{
				UpdateCurrentIndex(Commons.E_LayerType.Beard, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(num2);
		}
		else
		{
			layerTextureHandler.Refresh();
			if (CurrentBeards.Count > 0)
			{
				SimpleLayer simpleLayer3 = CurrentBeards[0];
				UpdateCurrentIndex(Commons.E_LayerType.Beard, simpleLayer3.Gender, simpleLayer3.LayerGlobalIndex);
			}
			else
			{
				UpdateCurrentIndex(Commons.E_LayerType.Beard, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify((CurrentBeards.Count <= 0) ? (-1) : 0);
		}
		RefreshPortraitParts = true;
	}

	public void UpdateBackgroundTexture()
	{
		Texture2D texture2D = BackgroundCreater.WriteTexture(LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers, Color.white, Color.black));
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), PortraitAPIManager.GetTexturesData().PivotPoint, PortraitAPIManager.GetTexturesData().PixelPerUnit);
		imageBG.sprite = sprite;
	}

	public void UpdateTextureHandlersOnChangePortraitCode(CodeGenerator.CodeData codeData)
	{
		previousIndexByLayerType[CurrentGender] = CurrentIndexByLayerType.Clone();
		Commons.E_Gender gender = codeData.CodeFooterData.Gender;
		if (CurrentGender != gender)
		{
			apperanceToggle.SelectStateWithoutNotify(gender);
		}
		CurrentIndexByLayerType = codeData.CodeSectionDatas.Clone();
		textureHandlers.ForEach(delegate(LayerTextureHandler x)
		{
			x.Refresh();
		});
		if (codeData.CodeSectionDatas[Commons.E_LayerType.Beard].LayerIndex != -1)
		{
			HairLayer hairLayer = LayerManagement.GetAllLayers<HairLayer>(CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender, Commons.E_LayerType.Hair)[CurrentIndexByLayerType[Commons.E_LayerType.Hair].LayerIndex];
			BeardLayers beardLayers = LayerManagement.GetAllLayers<BeardLayers>(CurrentIndexByLayerType[Commons.E_LayerType.Beard].Gender, Commons.E_LayerType.Beard)[codeData.CodeSectionDatas[Commons.E_LayerType.Beard].LayerIndex];
			beardLockToHairToggle.isOn = beardLayers.FaceIds.Contains(hairLayer.FaceId);
			BeardIsLockToHairType = beardLayers.FaceIds.Contains(hairLayer.FaceId);
		}
		else
		{
			beardLockToHairToggle.isOn = true;
			BeardIsLockToHairType = true;
		}
		for (int num = 0; num < codeData.CodeSectionDatas.Count; num++)
		{
			Commons.E_LayerType layerType = (Commons.E_LayerType)num;
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == layerType);
			if (CurrentIndexByLayerType[layerType].LayerIndex <= -1)
			{
				continue;
			}
			if (CurrentGender == Commons.E_Gender.Any && CurrentIndexByLayerType[layerType].Gender == Commons.E_Gender.Woman)
			{
				int dropdownValueFromSimpleLayerIndex = GetDropdownValueFromSimpleLayerIndex(layerType, CurrentIndexByLayerType[layerType].Gender, CurrentIndexByLayerType[layerType].LayerIndex, CurrentGender);
				layerTextureHandler.DropDown.SetValueWithoutNotify(dropdownValueFromSimpleLayerIndex);
				continue;
			}
			int valueWithoutNotify = -1;
			if (layerType == Commons.E_LayerType.Beard)
			{
				SimpleLayer item = LayerManagement.GetAllLayers<SimpleLayer>(CurrentIndexByLayerType[layerType].Gender, layerType)[CurrentIndexByLayerType[layerType].LayerIndex];
				if (CurrentBeards.Count != 0)
				{
					if (CurrentBeards.Contains(item))
					{
						valueWithoutNotify = CurrentBeards.IndexOf(item);
					}
					else
					{
						valueWithoutNotify = 0;
						CurrentIndexByLayerType[layerType].SetIndex(CurrentBeards[0].LayerGlobalIndex);
					}
				}
			}
			else
			{
				valueWithoutNotify = GetDropdownValueFromSimpleLayerIndex(layerType, CurrentIndexByLayerType[layerType].Gender, CurrentIndexByLayerType[layerType].LayerIndex, CurrentGender);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(valueWithoutNotify);
		}
		PortraitCodePanel.Refresh(codeData);
		RefreshPortraitParts = true;
	}

	public void UpdateColorHandlersOnChangePortraitCode(CodeGenerator.CodeData codeData)
	{
		for (int i = 0; i < Enum.GetValues(typeof(Commons.E_ColorTypes)).Length; i++)
		{
			Commons.E_ColorTypes e_ColorTypes = (Commons.E_ColorTypes)i;
			ChangeIndexOfColorType(e_ColorTypes, codeData.CodeColorDatas[e_ColorTypes].Index);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Close()
	{
		previousIndexByLayerType.Clear();
		apperanceToggle.OnAppearanceChanged?.RemoveListener(OnAppearanceChange);
		beardLockToHairToggle.onValueChanged?.RemoveListener(delegate
		{
			SwitchBeardIsLockToHairType();
		});
		CameraView.AttenuateWorldForPopupFocus(TPSingleton<CharacterSheetPanel>.Instance);
		fadeTween?.Kill();
		fadeTween = canvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			canvas.enabled = false;
			GameController.SetState(Game.E_State.CharacterSheet);
		});
		fadeTween.Play();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private void OnAppearanceChange(Commons.E_Gender gender)
	{
		Commons.E_Gender currentGender = CurrentGender;
		CurrentGender = gender;
		if (previousIndexByLayerType.ContainsKey(gender) && previousIndexByLayerType[gender].Count != 0)
		{
			Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData> dico = CurrentIndexByLayerType.Clone();
			CurrentIndexByLayerType = previousIndexByLayerType[gender].Clone();
			if (!previousIndexByLayerType.ContainsKey(currentGender))
			{
				previousIndexByLayerType.Add(currentGender, dico.Clone());
			}
			else
			{
				previousIndexByLayerType[currentGender] = dico.Clone();
			}
			foreach (LayerTextureHandler textureHandler in textureHandlers)
			{
				textureHandler.Refresh();
				if (textureHandler.LayerType == Commons.E_LayerType.Beard && textureHandler.IsLocked && CurrentIndexByLayerType[textureHandler.LayerType].LayerIndex == -1)
				{
					textureHandler.LockToggle.isOn = false;
					beardLockToHairToggle.isOn = true;
				}
				int layerIndex = CurrentIndexByLayerType[textureHandler.LayerType].LayerIndex;
				int dropdownValueFromSimpleLayerIndex = GetDropdownValueFromSimpleLayerIndex(textureHandler.LayerType, CurrentIndexByLayerType[textureHandler.LayerType].Gender, layerIndex, CurrentGender);
				textureHandler.DropDown.SetValueWithoutNotify(dropdownValueFromSimpleLayerIndex);
			}
		}
		else
		{
			if (!previousIndexByLayerType.ContainsKey(currentGender))
			{
				previousIndexByLayerType.Add(currentGender, new Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData>());
			}
			foreach (KeyValuePair<Commons.E_LayerType, CodeGenerator.CodeLayerData> item in CurrentIndexByLayerType)
			{
				if (!previousIndexByLayerType[currentGender].ContainsKey(item.Key))
				{
					previousIndexByLayerType[currentGender].Add(item.Key, item.Value.Clone());
				}
				else
				{
					previousIndexByLayerType[currentGender][item.Key] = item.Value.Clone();
				}
			}
			if (!previousIndexByLayerType[currentGender].ContainsKey(Commons.E_LayerType.Beard))
			{
				previousIndexByLayerType[currentGender].Add(Commons.E_LayerType.Beard, new CodeGenerator.CodeLayerData(CurrentGender, 0));
			}
			foreach (LayerTextureHandler textureHandler2 in textureHandlers)
			{
				textureHandler2.Refresh();
				if (textureHandler2.DropDown.options.Count == 0)
				{
					UpdateCurrentIndex(textureHandler2.LayerType, CurrentGender, -1);
					textureHandler2.DropDown.SetValueWithoutNotify(-1);
					continue;
				}
				List<SimpleLayer> allLayers = LayerManagement.GetAllLayers<SimpleLayer>(currentGender, textureHandler2.LayerType);
				SimpleLayer simpleLayer = ((allLayers.Count > 0) ? allLayers[Mathf.Clamp(CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, 0, allLayers.Count - 1)] : null);
				List<SimpleLayer> availableLayers = LayerManagement.GetAvailableLayers<SimpleLayer>(CurrentGender, textureHandler2.LayerType);
				int num = 0;
				if (availableLayers.Contains(simpleLayer) && simpleLayer != null)
				{
					num = availableLayers.IndexOf(simpleLayer);
					UpdateCurrentIndex(textureHandler2.LayerType, simpleLayer.Gender, simpleLayer.LayerGlobalIndex);
				}
				else if (textureHandler2.LayerType == Commons.E_LayerType.Beard)
				{
					num = 0;
					CurrentIndexByLayerType[textureHandler2.LayerType].SetIndex(CurrentBeards[0].LayerGlobalIndex);
					CurrentIndexByLayerType[textureHandler2.LayerType].SetGender(Commons.E_Gender.Man);
				}
				else
				{
					int dropdownValueFromSimpleLayerIndex2 = GetDropdownValueFromSimpleLayerIndex(textureHandler2.LayerType, CurrentIndexByLayerType[textureHandler2.LayerType].Gender, CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, CurrentGender);
					SimpleLayer simpleLayer2 = null;
					if (dropdownValueFromSimpleLayerIndex2 == -1)
					{
						simpleLayer2 = TryGetSimpleLayerInAnotherGenderFromLayerIndex(textureHandler2.LayerType, CurrentIndexByLayerType[textureHandler2.LayerType].Gender, CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, CurrentGender);
						if (simpleLayer2 != null)
						{
							dropdownValueFromSimpleLayerIndex2 = GetDropdownValueFromSimpleLayerIndex(textureHandler2.LayerType, simpleLayer2.Gender, simpleLayer2.LayerGlobalIndex, CurrentGender);
						}
						if (simpleLayer2 == null && CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex != -1)
						{
							dropdownValueFromSimpleLayerIndex2 = GetDropdownValueFromSimpleLayerIndex(textureHandler2.LayerType, CurrentIndexByLayerType[textureHandler2.LayerType].Gender, CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, CurrentIndexByLayerType[textureHandler2.LayerType].Gender);
						}
					}
					num = Mathf.Clamp(dropdownValueFromSimpleLayerIndex2, 0, textureHandler2.DropDown.options.Count - 1);
					if (simpleLayer2 == null)
					{
						simpleLayer2 = GetSimpleLayerFromDropdownValue(textureHandler2.LayerType, CurrentGender, num, out var _);
					}
					if (simpleLayer2 != null)
					{
						UpdateCurrentIndex(textureHandler2.LayerType, simpleLayer2.Gender, simpleLayer2.LayerGlobalIndex);
					}
					else
					{
						UpdateCurrentIndex(textureHandler2.LayerType, CurrentGender, -1);
					}
				}
				textureHandler2.DropDown.SetValueWithoutNotify(num);
			}
		}
		RefreshPortraitParts = true;
	}

	private void OnConfirmButtonClicked()
	{
		HairLayer hairLayer = LayerManagement.GetAllLayers<HairLayer>(CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender, Commons.E_LayerType.Hair)[CurrentIndexByLayerType[Commons.E_LayerType.Hair].LayerIndex];
		playableUnit.FaceId = hairLayer.FaceId;
		playableUnit.PlayableUnitName = RenameHeader.CurrentName;
		playableUnit.PlayableUnitView.name = playableUnit.PlayableUnitName;
		playableUnit.PlayableUnitView.UnitHUD.name = playableUnit.PlayableUnitName + " HUD";
		playableUnit.PortraitCodeData = PortraitCodePanel.CurrentCode.Clone();
		SetNewTextures();
		playableUnit.PlayableUnitView.ColorSwapPortraitMaterial.SetTexture(SwapTex, currentColorTexture);
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshName(TileObjectSelectionManager.SelectedPlayableUnit);
		TPSingleton<CharacterSheetPanel>.Instance.RefreshPortrait(TileObjectSelectionManager.SelectedPlayableUnit);
		GameView.CharacterDetailsView.IsDirty = true;
		TileObjectSelectionManager.SelectedUnitFeedback.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
		TPSingleton<GameOverPanel>.Instance.RefreshPlayables();
		TPSingleton<NightReportPanel>.Instance.RefreshPlayables();
		TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_CUSTOMIZE_HERO);
		Close();
	}

	private void OnResetButtonClicked()
	{
		playableUnit.FaceId = initialFaceId;
		playableUnit.Gender = ((initialGender == Commons.E_Gender.Man) ? "Male" : "Female");
		playableUnit.PlayableUnitName = initialName;
		RenameHeader.Refresh(initialName);
		CodeGenerator.TryDecode(initialCode, out var codeData);
		playableUnit.PortraitCodeData = codeData;
		UpdateTextureHandlersOnChangePortraitCode(codeData);
		CurrentIndexByColorType[Commons.E_ColorTypes.Skin].SetIndex(codeData.CodeColorDatas[Commons.E_ColorTypes.Skin].Index);
		CurrentIndexByColorType[Commons.E_ColorTypes.Hair].SetIndex(codeData.CodeColorDatas[Commons.E_ColorTypes.Hair].Index);
		CurrentIndexByColorType[Commons.E_ColorTypes.Eyes].SetIndex(codeData.CodeColorDatas[Commons.E_ColorTypes.Eyes].Index);
		CurrentIndexByColorType[Commons.E_ColorTypes.Background].SetIndex(codeData.CodeColorDatas[Commons.E_ColorTypes.Background].Index);
		int length = Enum.GetValues(typeof(Commons.E_ColorTypes)).Length;
		for (int i = 0; i < length; i++)
		{
			Commons.E_ColorTypes colorType = (Commons.E_ColorTypes)i;
			ColorHandler colorHandler = colorHandlers.Find((ColorHandler x) => x.ColorType == colorType);
			if (colorHandler != null)
			{
				switch (colorType)
				{
				case Commons.E_ColorTypes.Skin:
				case Commons.E_ColorTypes.Hair:
				case Commons.E_ColorTypes.Eyes:
					colorHandler.Refresh();
					break;
				case Commons.E_ColorTypes.Background:
					colorHandler.Refresh(PlayableUnitDatabase.PortraitBackgroundColors);
					break;
				}
			}
			UpdateColorHandler(colorType);
		}
		playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
	}

	private void PopulateColorHandlers(CodeGenerator.CodeData codeData)
	{
		int length = Enum.GetValues(typeof(Commons.E_ColorTypes)).Length;
		for (int i = 0; i < length; i++)
		{
			Commons.E_ColorTypes colorType = (Commons.E_ColorTypes)i;
			ColorHandler colorHandler = colorHandlers.Find((ColorHandler x) => x.ColorType == colorType);
			CurrentIndexByColorType[colorType].SetIndex(codeData.CodeColorDatas[colorType].Index);
			if (colorHandler != null)
			{
				switch (colorType)
				{
				case Commons.E_ColorTypes.Skin:
				case Commons.E_ColorTypes.Hair:
				case Commons.E_ColorTypes.Eyes:
					colorHandler.Refresh();
					break;
				case Commons.E_ColorTypes.Background:
					colorHandler.Refresh(PlayableUnitDatabase.PortraitBackgroundColors);
					break;
				}
			}
		}
	}

	private void PopulateTextureHandlers(CodeGenerator.CodeData datas)
	{
		CurrentGender = datas.CodeFooterData.Gender;
		BeardLayers beardLayers = null;
		if (datas.GetSectionData(Commons.E_LayerType.Beard).LayerIndex != -1)
		{
			HairLayer hairLayer = LayerManagement.GetAllLayers<HairLayer>(datas.GetSectionData(Commons.E_LayerType.Hair).Gender, Commons.E_LayerType.Hair)[datas.GetSectionData(Commons.E_LayerType.Hair).LayerIndex];
			beardLayers = LayerManagement.GetAllLayers<BeardLayers>(datas.GetSectionData(Commons.E_LayerType.Beard).Gender, Commons.E_LayerType.Beard)[datas.GetSectionData(Commons.E_LayerType.Beard).LayerIndex];
			beardLockToHairToggle.isOn = beardLayers.FaceIds.Contains(hairLayer.FaceId);
			BeardIsLockToHairType = beardLayers.FaceIds.Contains(hairLayer.FaceId);
		}
		int length = Enum.GetValues(typeof(Commons.E_LayerType)).Length;
		for (int i = 0; i < length; i++)
		{
			Commons.E_LayerType layerType = (Commons.E_LayerType)i;
			CurrentIndexByLayerType[layerType].SetIndex(datas.GetSectionData(layerType).LayerIndex);
			CurrentIndexByLayerType[layerType].SetGender(datas.GetSectionData(layerType).Gender);
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == layerType);
			if (!(layerTextureHandler != null))
			{
				continue;
			}
			layerTextureHandler.Refresh();
			if (layerType == Commons.E_LayerType.Beard && datas.GetSectionData(layerType).LayerIndex != -1)
			{
				if (CurrentBeards.Count != 0)
				{
					if (CurrentBeards.Contains(beardLayers))
					{
						CurrentIndexByLayerType[layerType].SetIndex(beardLayers.LayerGlobalIndex);
						layerTextureHandler.DropDown.SetValueWithoutNotify(CurrentBeards.IndexOf(beardLayers));
					}
					else
					{
						CurrentIndexByLayerType[layerType].SetIndex(CurrentBeards[0].LayerGlobalIndex);
						layerTextureHandler.DropDown.SetValueWithoutNotify(0);
					}
				}
				else
				{
					CurrentIndexByLayerType[layerType].SetIndex(-1);
				}
			}
			else if (CurrentIndexByLayerType[layerType].LayerIndex > -1)
			{
				int dropdownValueFromSimpleLayerIndex = GetDropdownValueFromSimpleLayerIndex(layerType, CurrentIndexByLayerType[layerType].Gender, CurrentIndexByLayerType[layerType].LayerIndex, CurrentGender);
				layerTextureHandler.DropDown.SetValueWithoutNotify(dropdownValueFromSimpleLayerIndex);
			}
		}
	}

	private void Update()
	{
		if (!canvas.enabled)
		{
			return;
		}
		if (RefreshBeardHandler)
		{
			RefreshBeardHandler = false;
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == Commons.E_LayerType.Beard);
			SimpleLayer simpleLayer = null;
			if (LayerManagement.GetAvailableLayers<SimpleLayer>(CurrentIndexByLayerType[Commons.E_LayerType.Beard].Gender, Commons.E_LayerType.Beard).Count > 0 && CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex > -1)
			{
				simpleLayer = LayerManagement.GetAllLayers<SimpleLayer>(CurrentIndexByLayerType[Commons.E_LayerType.Beard].Gender, Commons.E_LayerType.Beard)[CurrentIndexByLayerType[Commons.E_LayerType.Beard].LayerIndex];
			}
			layerTextureHandler.Refresh();
			if (layerTextureHandler.DropDown.options.Count > 0)
			{
				if (simpleLayer != null && CurrentBeards.Contains(simpleLayer))
				{
					layerTextureHandler.DropDown.SetValueWithoutNotify(CurrentBeards.IndexOf(simpleLayer));
					return;
				}
				int layerGlobalIndex = CurrentBeards[0].LayerGlobalIndex;
				CurrentIndexByLayerType[Commons.E_LayerType.Beard].SetIndex(layerGlobalIndex);
				layerTextureHandler.DropDown.SetValueWithoutNotify(0);
			}
			else
			{
				CurrentIndexByLayerType[Commons.E_LayerType.Beard].SetIndex(-1);
			}
		}
		if (RefreshColors)
		{
			RefreshColors = false;
			playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			unitAvatarImage.sprite = playableUnit.UiSprite;
			RefreshCodePanel(CurrentIndexByLayerType);
		}
		if (RefreshPortraitParts)
		{
			RefreshPortraitParts = false;
			Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData> dictionary = CurrentIndexByLayerType.Clone();
			currentLayers.Clear();
			for (int num = 0; num < dictionary.Count; num++)
			{
				if (dictionary.ElementAt(num).Value.LayerIndex >= 0)
				{
					Commons.E_LayerType e_LayerType = (Commons.E_LayerType)num;
					SimpleLayer value = LayerManagement.GetAllLayers<SimpleLayer>(dictionary[e_LayerType].Gender, e_LayerType)[dictionary[e_LayerType].LayerIndex];
					currentLayers.Add(e_LayerType, value);
				}
			}
			portraitPartsDisplay.UpdateValues(currentLayers, CurrentGender, playableUnit, layerMaterials);
			UpdateBackgroundTexture();
			HairLayer hairLayer = LayerManagement.GetAllLayers<HairLayer>(dictionary[Commons.E_LayerType.Hair].Gender, Commons.E_LayerType.Hair)[dictionary[Commons.E_LayerType.Hair].LayerIndex];
			RefreshCodePanel(dictionary);
			playableUnit.FaceId = hairLayer.FaceId;
			playableUnit.Gender = ((CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender == Commons.E_Gender.Man) ? "Male" : "Female");
			playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			unitAvatarImage.sprite = playableUnit.UiSprite;
		}
		if (InputManager.GetButtonDown(29) || (InputManager.GetButtonDown(80) && !openedThisFrame))
		{
			if (currentJoystickSelectedHandler == null || !currentJoystickSelectedHandler.IsDropdownOpen)
			{
				if (PortraitCodePanel.IsEditingCode)
				{
					PortraitCodePanel.PortraitCodePopup.Close();
				}
				else if (RenameHeader.IsEditingName)
				{
					RenameHeader.RenamePopup.Close();
				}
				else
				{
					OnResetButtonClicked();
					Close();
				}
			}
		}
		else if (InputManager.GetButtonDown(66) || InputManager.GetButtonDown(112))
		{
			if (PortraitCodePanel.IsEditingCode)
			{
				PortraitCodePanel.PortraitCodePopup.OnValidateButtonClicked();
			}
			else if (RenameHeader.IsEditingName)
			{
				RenameHeader.RenamePopup.OnValidateButtonClicked();
			}
			else
			{
				OnConfirmButtonClicked();
			}
		}
		else if (InputManager.GetButtonDown(113))
		{
			if (currentJoystickSelectedHandler != null)
			{
				currentJoystickSelectedHandler.IncreaseCurrentValue();
			}
		}
		else if (InputManager.GetButtonDown(114) && currentJoystickSelectedHandler != null)
		{
			currentJoystickSelectedHandler.DecreaseCurrentValue();
		}
		if (openedThisFrame)
		{
			openedThisFrame = false;
		}
	}

	private void RefreshCodePanel(Dictionary<Commons.E_LayerType, CodeGenerator.CodeLayerData> indexes)
	{
		CodeGenerator.CodeFooterData footerData = new CodeGenerator.CodeFooterData(CurrentGender, PortraitAPIManager.PortraitAPIVersion);
		CodeGenerator.CodeData codeData = new CodeGenerator.CodeData();
		codeData.SetFooterData(footerData);
		codeData.SetSectionDatas(indexes);
		codeData.SetColorDatas(CurrentIndexByColorType);
		portraitCodePanel.Refresh(codeData);
	}

	private void UpdateColorHandler(Commons.E_ColorTypes colorType)
	{
		switch (colorType)
		{
		case Commons.E_ColorTypes.Skin:
		case Commons.E_ColorTypes.Hair:
		case Commons.E_ColorTypes.Eyes:
			colorHandlers.Find((ColorHandler x) => x.ColorType == colorType).Refresh();
			SwapColorsForPalette(AllPalettesByColorType[colorType].ToList()[CurrentIndexByColorType[colorType].Index].Value, ref currentColorTexture);
			currentColorTexture.Apply();
			{
				foreach (KeyValuePair<Commons.E_LayerKind, Material> layerMaterial in layerMaterials)
				{
					layerMaterial.Value.SetTexture(SwapTex, currentColorTexture);
				}
				break;
			}
		case Commons.E_ColorTypes.Background:
			colorHandlers.Find((ColorHandler x) => x.ColorType == colorType).Refresh(PlayableUnitDatabase.PortraitBackgroundColors, clearOptions: false);
			imageBG.color = PlayableUnitDatabase.PortraitBackgroundColors[CurrentIndexByColorType[colorType].Index]._Color;
			break;
		}
	}

	private void UpdateCurrentIndex(Commons.E_LayerType layerType, Commons.E_Gender gender, int newIndex)
	{
		switch (gender)
		{
		case Commons.E_Gender.Man:
		case Commons.E_Gender.Woman:
			CurrentIndexByLayerType[layerType].SetIndex(Mathf.Clamp(newIndex, -1, LayerManagement.GetAllLayers<SimpleLayer>(gender, layerType).Count - 1));
			CurrentIndexByLayerType[layerType].SetGender(gender);
			break;
		case Commons.E_Gender.Any:
			CurrentIndexByLayerType[layerType].SetGender(Commons.E_Gender.Man);
			CurrentIndexByLayerType[layerType].SetIndex(newIndex);
			break;
		}
	}

	private void SetInitialsValues(PlayableUnit playableUnit, CodeGenerator.CodeData codeData)
	{
		initialFaceId = playableUnit.FaceId;
		initialCode = playableUnit.PortraitCodeData.ToString();
		initialGender = codeData.CodeFooterData.Gender;
		initialName = playableUnit.PlayableUnitName;
	}

	private void SetNewTextures()
	{
		Color[,] array = LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers);
		Texture2D texture2D = BackgroundCreater.WriteTexture(LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers, Color.white, Color.black));
		Texture2D texture2D2 = PortraitAPIManager.CreateEmptyTexture();
		TexturesData texturesData = PortraitAPIManager.GetTexturesData();
		for (int i = 0; i < texturesData.Width; i++)
		{
			for (int j = 0; j < texturesData.Height; j++)
			{
				texture2D2.SetPixel(i, j, array[i, j]);
			}
		}
		texture2D2.Apply();
		Sprite portraitSprite = Sprite.Create(texture2D2, new Rect(0f, 0f, texturesData.Width, texturesData.Height), texturesData.PivotPoint, texturesData.PixelPerUnit);
		Sprite portraitBackgroundSprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), texturesData.PivotPoint, texturesData.PixelPerUnit);
		playableUnit.PortraitSprite = portraitSprite;
		playableUnit.PortraitBackgroundSprite = portraitBackgroundSprite;
	}

	private void SwapColor(int index, Color color, ref Texture2D colorSwapTex)
	{
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void SwapColorsForPalette(ColorSwapPaletteDefinition paletteDefinition, ref Texture2D colorSwapTex)
	{
		if (paletteDefinition != null)
		{
			int i = 0;
			for (int count = paletteDefinition.ColorSwapDefinitions.Count; i < count; i++)
			{
				SwapColor(paletteDefinition.ColorSwapDefinitions[i].Index, paletteDefinition.ColorSwapDefinitions[i].OutputColor, ref colorSwapTex);
			}
		}
	}

	private SimpleLayer TryGetSimpleLayerInAnotherGenderFromLayerIndex(Commons.E_LayerType layerType, Commons.E_Gender layerGender, int layerGlobalIndex, Commons.E_Gender otherGender)
	{
		List<SimpleLayer> allLayers = LayerManagement.GetAllLayers<SimpleLayer>(layerGender, layerType);
		SimpleLayer simpleLayer = null;
		if (layerGlobalIndex < allLayers.Count && layerGlobalIndex != -1)
		{
			simpleLayer = allLayers[layerGlobalIndex];
			if (layerGender == otherGender)
			{
				return simpleLayer;
			}
			List<SimpleLayer> availableLayers = LayerManagement.GetAvailableLayers<SimpleLayer>(otherGender, layerType);
			if (availableLayers.Contains(simpleLayer))
			{
				return simpleLayer;
			}
			return availableLayers.Find((SimpleLayer aLayer) => aLayer.LayerGlobalIndex == layerGlobalIndex && aLayer.Gender != layerGender);
		}
		return null;
	}
}
