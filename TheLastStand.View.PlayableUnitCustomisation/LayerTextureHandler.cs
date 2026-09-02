using System.Collections.Generic;
using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class LayerTextureHandler : Handler
{
	public static class Constants
	{
		public const string DropDownTitleMaleFormatting = "HeroCustomization_TextureDropdownTitle_Male_Formatting";

		public const string DropDownTitleFemaleFormatting = "HeroCustomization_TextureDropdownTitle_Female_Formatting";
	}

	private const string IndexFormat = "00";

	private const string Space = " ";

	[SerializeField]
	private Commons.E_LayerType layerType;

	[SerializeField]
	private TMP_Dropdown dropDown;

	private int currentValue;

	public override bool IsDropdownOpen => DropDown.IsExpanded;

	public TMP_Dropdown DropDown => dropDown;

	public Commons.E_LayerType LayerType => layerType;

	public override void ChangeCurrentValue()
	{
		currentValue = dropDown.value;
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfLayerType(LayerType, dropDown.value);
		if (LayerType == Commons.E_LayerType.Hair)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshBeardHandler = true;
		}
		dropDown.RefreshShownValue();
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshPortraitParts = true;
	}

	public override void DecreaseCurrentValue()
	{
		currentValue = dropDown.value;
		int valueWithoutNotify = dropDown.options.Count - 1;
		if (currentValue != 0)
		{
			valueWithoutNotify = currentValue - 1;
		}
		dropDown.SetValueWithoutNotify(valueWithoutNotify);
		ChangeCurrentValue();
	}

	public override void IncreaseCurrentValue()
	{
		currentValue = dropDown.value;
		int valueWithoutNotify = 0;
		if (currentValue < dropDown.options.Count - 1)
		{
			valueWithoutNotify = currentValue + 1;
		}
		dropDown.SetValueWithoutNotify(valueWithoutNotify);
		ChangeCurrentValue();
	}

	public override void RandomizeValue(bool useWeights)
	{
		int num = 0;
		if (LayerType == Commons.E_LayerType.Beard)
		{
			Refresh();
		}
		if (dropDown.options.Count <= 0)
		{
			return;
		}
		if (useWeights)
		{
			switch (LayerType)
			{
			case Commons.E_LayerType.Beard:
				num = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.RandomWeightedIndex(RandomManager.GetRandomForCaller(TPSingleton<PlayableUnitCustomisationPanel>.Instance.GetType().Name));
				break;
			case Commons.E_LayerType.Hair:
			{
				string faceId = GetRandomFaceId(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender);
				List<HairLayer> availableLayers = LayerManagement.GetAvailableLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, Commons.E_LayerType.Hair);
				if (availableLayers.Find((HairLayer x) => x.FaceId == faceId) != null)
				{
					num = availableLayers.IndexOf(availableLayers.Find((HairLayer x) => x.FaceId == faceId));
				}
				break;
			}
			default:
				num = LayerManagement.GetAvailableLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType).RandomWeightedIndex(RandomManager.GetRandomForCaller(TPSingleton<PlayableUnitCustomisationPanel>.Instance.GetType().Name));
				break;
			}
		}
		else
		{
			num = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, (LayerType == Commons.E_LayerType.Beard) ? TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Count : LayerManagement.GetAvailableLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType).Count);
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfLayerType(LayerType, num);
		if (LayerType == Commons.E_LayerType.Hair)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshBeardHandler = true;
		}
		dropDown.SetValueWithoutNotify(num);
	}

	public void Refresh()
	{
		Commons.E_Gender e_Gender = ((LayerType != Commons.E_LayerType.Beard || !TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType) ? TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender : TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender);
		Commons.E_Gender e_Gender2 = e_Gender;
		List<SimpleLayer> list = new List<SimpleLayer>();
		List<SimpleLayer> list2 = new List<SimpleLayer>();
		switch (e_Gender2)
		{
		case Commons.E_Gender.Man:
			list.AddRange(LayerManagement.GetAvailableLayers<SimpleLayer>(e_Gender2, layerType));
			break;
		case Commons.E_Gender.Woman:
			list2.AddRange(LayerManagement.GetAvailableLayers<SimpleLayer>(e_Gender2, layerType));
			break;
		case Commons.E_Gender.Any:
			list.AddRange(LayerManagement.GetAvailableLayers<SimpleLayer>(Commons.E_Gender.Man, layerType));
			if (layerType != Commons.E_LayerType.Clothes)
			{
				list2.AddRange(LayerManagement.GetAvailableLayers<SimpleLayer>(Commons.E_Gender.Woman, layerType));
			}
			break;
		}
		string text = Localizer.Get("HeroCustomization_TextureDropdownTitle_" + LayerType);
		dropDown.onValueChanged.RemoveListener(onValueChanged);
		if (dropDown.options != null)
		{
			dropDown.ClearOptions();
		}
		List<TMP_Dropdown.OptionData> list3 = new List<TMP_Dropdown.OptionData>();
		if (LayerType == Commons.E_LayerType.Beard)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Clear();
			int layerIndex = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[Commons.E_LayerType.Hair].LayerIndex;
			List<HairLayer> allLayers = LayerManagement.GetAllLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[Commons.E_LayerType.Hair].Gender, Commons.E_LayerType.Hair);
			int count = allLayers.Count;
			HairLayer hairLayer = null;
			if (layerIndex != -1 && layerIndex < count)
			{
				hairLayer = allLayers[layerIndex];
			}
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType)
			{
				AddBeardsToCurrentBeards(list, list3, text, "HeroCustomization_TextureDropdownTitle_Male_Formatting", isBeardLockedToHair: true, hairLayer);
				AddBeardsToCurrentBeards(list2, list3, text, "HeroCustomization_TextureDropdownTitle_Female_Formatting", isBeardLockedToHair: true, hairLayer);
			}
			else
			{
				AddBeardsToCurrentBeards(list, list3, text, "HeroCustomization_TextureDropdownTitle_Male_Formatting", isBeardLockedToHair: false);
				AddBeardsToCurrentBeards(list2, list3, text, "HeroCustomization_TextureDropdownTitle_Female_Formatting", isBeardLockedToHair: false);
			}
		}
		else
		{
			int num = 0;
			foreach (SimpleLayer item in list)
			{
				string text2 = ((layerType != Commons.E_LayerType.Clothes) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Male_Formatting", text, (item.LayerGlobalIndex + 1).ToString("00") + GetLayerItemDLCIcon(item)) : (text + " " + (item.LayerGlobalIndex + 1).ToString("00") + GetLayerItemDLCIcon(item)));
				list3.Add(new TMP_Dropdown.OptionData(text2));
				num++;
			}
			num = 0;
			foreach (SimpleLayer item2 in list2)
			{
				string text3 = ((layerType != Commons.E_LayerType.Clothes) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Female_Formatting", text, (item2.LayerGlobalIndex + 1).ToString("00") + GetLayerItemDLCIcon(item2)) : (text + " " + (item2.LayerGlobalIndex + 1).ToString("00") + GetLayerItemDLCIcon(item2)));
				list3.Add(new TMP_Dropdown.OptionData(text3));
				num++;
			}
		}
		if (list3.Count > 0)
		{
			SwitchHandlerLockState(state: true);
			dropDown.AddOptions(list3);
			dropDown.onValueChanged.AddListener(onValueChanged);
		}
		else
		{
			SwitchHandlerLockState(state: false);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[LayerType].SetIndex(-1);
		}
	}

	public override void SwitchHandlerLockState(bool state)
	{
		base.SwitchHandlerLockState(state);
		dropDown.interactable = state;
		dropDown.image.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
	}

	private void AddBeardsToCurrentBeards(List<SimpleLayer> genderedLayers, List<TMP_Dropdown.OptionData> optionsData, string localizedLayerType, string localizeFinalTextKey, bool isBeardLockedToHair, HairLayer hairLayer = null)
	{
		int num = 0;
		foreach (SimpleLayer genderedLayer in genderedLayers)
		{
			bool flag = true;
			if (isBeardLockedToHair && hairLayer != null)
			{
				flag = ((BeardLayers)genderedLayer).FaceIds.Contains(hairLayer.FaceId);
			}
			if (flag)
			{
				TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Add(genderedLayer);
				string text = Localizer.Format(localizeFinalTextKey, localizedLayerType, (genderedLayer.LayerGlobalIndex + 1).ToString("00") + GetLayerItemDLCIcon(genderedLayer));
				optionsData.Add(new TMP_Dropdown.OptionData(text));
				num++;
			}
		}
	}

	private string GetLayerItemDLCIcon(SimpleLayer layer)
	{
		if (layer.IsLinkedToDLC && TPSingleton<DLCManager>.Exist())
		{
			return "  " + TPSingleton<DLCManager>.Instance.GetDLCFromId(layer.DLCId).TextIconForPortraits;
		}
		return string.Empty;
	}

	private string GetRandomFaceId(Commons.E_Gender gender)
	{
		UnitFaceIdDefinitions[] array = new UnitFaceIdDefinitions[2];
		switch (gender)
		{
		case Commons.E_Gender.Woman:
			array[1] = PlayableUnitDatabase.PlayableFemaleUnitFaceIds;
			break;
		case Commons.E_Gender.Man:
			array[0] = PlayableUnitDatabase.PlayableMaleUnitFaceIds;
			break;
		case Commons.E_Gender.Any:
			array[0] = PlayableUnitDatabase.PlayableMaleUnitFaceIds;
			array[1] = PlayableUnitDatabase.PlayableFemaleUnitFaceIds;
			break;
		}
		string result = string.Empty;
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				for (int j = 0; j < array[i].Count; j++)
				{
					num += array[i][j].Weight;
				}
			}
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, num);
		int num2 = 0;
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] == null)
			{
				continue;
			}
			int num3 = 0;
			while (num3 < array[k].Count)
			{
				if (randomRange < num2 || randomRange >= array[k][num3].Weight + num2)
				{
					num2 += array[k][num3].Weight;
					num3++;
					continue;
				}
				goto IL_00c5;
			}
			continue;
			IL_00c5:
			result = array[k][num3].FaceId;
			break;
		}
		return result;
	}

	private void Start()
	{
		onValueChanged = delegate
		{
			ChangeCurrentValue();
		};
	}
}
