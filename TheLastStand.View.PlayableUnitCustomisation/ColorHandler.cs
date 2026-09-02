using System.Collections.Generic;
using System.Linq;
using PortraitAPI;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class ColorHandler : Handler
{
	[SerializeField]
	private ColorDropdown colorDropdown;

	[SerializeField]
	private Commons.E_ColorTypes colorType = Commons.E_ColorTypes.Eyes;

	[SerializeField]
	private Texture2D colorTexture;

	public Commons.E_ColorTypes ColorType => colorType;

	public override bool IsDropdownOpen => colorDropdown.IsExpanded;

	public override void ChangeCurrentValue()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, colorDropdown.value);
	}

	public override void DecreaseCurrentValue()
	{
		int valueToSet = 0;
		switch (ColorType)
		{
		case Commons.E_ColorTypes.Skin:
		case Commons.E_ColorTypes.Hair:
		case Commons.E_ColorTypes.Eyes:
			valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count - 1;
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index != 0)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index - 1;
			}
			break;
		case Commons.E_ColorTypes.Background:
			valueToSet = PlayableUnitDatabase.PortraitBackgroundColors.Count - 1;
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index != 0)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index - 1;
			}
			break;
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public override void IncreaseCurrentValue()
	{
		int valueToSet = 0;
		switch (ColorType)
		{
		case Commons.E_ColorTypes.Skin:
		case Commons.E_ColorTypes.Hair:
		case Commons.E_ColorTypes.Eyes:
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count - 1)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index + 1;
			}
			break;
		case Commons.E_ColorTypes.Background:
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index < PlayableUnitDatabase.PortraitBackgroundColors.Count - 1)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index + 1;
			}
			break;
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public override void RandomizeValue(bool useWeights)
	{
		int valueToSet = 0;
		switch (ColorType)
		{
		case Commons.E_ColorTypes.Skin:
		case Commons.E_ColorTypes.Eyes:
			if (useWeights)
			{
				int num = 0;
				for (int i = 0; i < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count; i++)
				{
					num += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(i).Value.Weight;
				}
				int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, num);
				int num2 = 0;
				for (int j = 0; j < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count; j++)
				{
					if (j == 0)
					{
						if (randomRange >= 0 && randomRange < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight)
						{
							valueToSet = j;
							break;
						}
						num2 += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight;
					}
					else
					{
						if (randomRange >= num2 && randomRange < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight + num2)
						{
							valueToSet = j;
							break;
						}
						num2 += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight;
					}
				}
			}
			else
			{
				valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count);
			}
			break;
		case Commons.E_ColorTypes.Hair:
			if (useWeights)
			{
				string key = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[Commons.E_ColorTypes.Skin].ElementAt(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[Commons.E_ColorTypes.Skin].Index).Key;
				string randomHairColorId = PlayableUnitDatabase.UnitLinkHairSkin.GetRandomHairColorId(key);
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[Commons.E_ColorTypes.Hair].Keys.ToList().IndexOf(randomHairColorId);
			}
			else
			{
				valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count);
			}
			break;
		case Commons.E_ColorTypes.Background:
			valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, PlayableUnitDatabase.PortraitBackgroundColors.Count);
			break;
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public void Refresh(List<DataColor> portraitBackgroundColors, bool clearOptions = true)
	{
		colorDropdown.onValueChanged.RemoveListener(onValueChanged);
		if (colorDropdown.options != null)
		{
			colorDropdown.ClearOptions();
		}
		List<ColorDropdown.ColorOptionData> list = new List<ColorDropdown.ColorOptionData>();
		foreach (DataColor portraitBackgroundColor in portraitBackgroundColors)
		{
			Sprite image = Sprite.Create(colorTexture, new Rect(new Vector2(0f, 0f), new Vector2(colorTexture.width, colorTexture.height)), new Vector2(0.5f, 0.5f));
			string text = Localizer.Get("HeroCustomization_ColorDropdownItem_" + ColorType.ToString() + "_" + portraitBackgroundColor.name.Replace(" ", string.Empty));
			list.Add(new ColorDropdown.ColorOptionData(text, image, portraitBackgroundColor._Color));
		}
		colorDropdown.AddOptions(list);
		colorDropdown.value = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index;
		colorDropdown.onValueChanged.AddListener(onValueChanged);
	}

	public void Refresh()
	{
		colorDropdown.onValueChanged.RemoveListener(onValueChanged);
		if (colorDropdown.options != null)
		{
			colorDropdown.ClearOptions();
		}
		List<ColorDropdown.ColorOptionData> list = new List<ColorDropdown.ColorOptionData>();
		foreach (KeyValuePair<string, Texture2D> item in TPSingleton<PlayableUnitCustomisationPanel>.Instance.TexturesByColorType[ColorType])
		{
			Sprite image = Sprite.Create(colorTexture, new Rect(Vector2.zero, new Vector2(colorTexture.width, colorTexture.height)), new Vector2(0.5f, 0.5f));
			Material material = new Material(TPSingleton<PlayableUnitCustomisationPanel>.Instance.ColorSwapMaterial);
			material.SetTexture("_SwapTex", item.Value);
			string text = Localizer.Get($"HeroCustomization_ColorDropdownItem_{ColorType}_{item.Key}");
			list.Add(new ColorDropdown.ColorOptionData(text, image, material));
		}
		colorDropdown.AddOptions(list);
		colorDropdown.value = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index;
		colorDropdown.onValueChanged.AddListener(onValueChanged);
	}

	private void Start()
	{
		onValueChanged = delegate
		{
			ChangeCurrentValue();
		};
	}
}
