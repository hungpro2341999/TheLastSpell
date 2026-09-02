using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.View.Camera;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitCodePopup : ACustomizationPopup
{
	public override void Close()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.PortraitCodePanel.IsEditingCode = false;
		base.Close();
	}

	public override void OnCloseButtonClicked()
	{
		Close();
	}

	public override void OnValidateButtonClicked()
	{
		if (CodeGenerator.TryDecode(inputField.text, out var codeData))
		{
			if (inputField.text.Length == 12)
			{
				codeData.SetColorDatas(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType.Clone());
			}
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.UpdateTextureHandlersOnChangePortraitCode(codeData);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.UpdateColorHandlersOnChangePortraitCode(codeData);
			CameraView.AttenuateWorldForPopupFocus(TPSingleton<PlayableUnitCustomisationPanel>.Instance);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.PortraitCodePanel.IsEditingCode = false;
			Close();
		}
	}

	public override void OnValueChanged(string value)
	{
		string text = value;
		foreach (char c in text)
		{
			if (!char.IsNumber(c) && !char.IsLetter(c))
			{
				value = previousValue;
				inputField.SetTextWithoutNotify(value);
				break;
			}
		}
		previousValue = value;
		base.OnValueChanged(value);
	}

	public void Refresh(string code)
	{
		inputField.text = code;
	}

	protected override E_ErrorCause CheckValidity(string value)
	{
		if (value.Length < 30 && value.Length != 12)
		{
			errorText.text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.MinSize, 30);
			return E_ErrorCause.MinSize;
		}
		E_ErrorCause e_ErrorCause = base.CheckValidity(value);
		if (e_ErrorCause != E_ErrorCause.None)
		{
			return e_ErrorCause;
		}
		if (!CodeGenerator.TryDecode(value, out var codeData))
		{
			errorText.text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue);
			return E_ErrorCause.InvalidLayerValue;
		}
		for (int i = 0; i < codeData.CodeSectionDatas.Count; i++)
		{
			Commons.E_LayerType e_LayerType = (Commons.E_LayerType)i;
			if (codeData.CodeSectionDatas[e_LayerType].LayerIndex != -1 && !LayerManagement.IsThisIndexAvailable(codeData.CodeSectionDatas[e_LayerType].Gender, e_LayerType, codeData.CodeSectionDatas[e_LayerType].LayerIndex))
			{
				errorText.text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue);
				return E_ErrorCause.InvalidLayerValue;
			}
		}
		for (int j = 0; j < codeData.CodeColorDatas.Count; j++)
		{
			Commons.E_ColorTypes e_ColorTypes = (Commons.E_ColorTypes)j;
			bool flag = true;
			switch (e_ColorTypes)
			{
			case Commons.E_ColorTypes.Skin:
				if (codeData.CodeColorDatas[e_ColorTypes].Index >= PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.Count || codeData.CodeColorDatas[e_ColorTypes].Index < 0)
				{
					flag = false;
				}
				break;
			case Commons.E_ColorTypes.Hair:
				if (codeData.CodeColorDatas[e_ColorTypes].Index >= PlayableUnitDatabase.PlayableUnitHairColorDefinitions.Count || codeData.CodeColorDatas[e_ColorTypes].Index < 0)
				{
					flag = false;
				}
				break;
			case Commons.E_ColorTypes.Eyes:
				if (codeData.CodeColorDatas[e_ColorTypes].Index >= PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.Count || codeData.CodeColorDatas[e_ColorTypes].Index < 0)
				{
					flag = false;
				}
				break;
			case Commons.E_ColorTypes.Background:
				if (codeData.CodeColorDatas[e_ColorTypes].Index >= PlayableUnitDatabase.PortraitBackgroundColors.Count || codeData.CodeColorDatas[e_ColorTypes].Index < 0)
				{
					flag = false;
				}
				break;
			}
			if (!flag)
			{
				errorText.text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue);
				return E_ErrorCause.InvalidLayerValue;
			}
		}
		return e_ErrorCause;
	}
}
