using TPLib;
using TPLib.Localization;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class RenamePopup : ACustomizationPopup
{
	public string PlayableUnitName = string.Empty;

	public override void Close()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RenameHeader.IsEditingName = false;
		base.Close();
	}

	public override void OnCloseButtonClicked()
	{
		Close();
	}

	public override void OnValidateButtonClicked()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RenameHeader.Refresh(inputField.text);
		Close();
	}

	public override void Open()
	{
		if (PlayableUnitName != string.Empty)
		{
			base.Open();
			inputField.SetTextWithoutNotify(PlayableUnitName);
			previousValue = PlayableUnitName;
		}
	}

	protected override E_ErrorCause CheckValidity(string value)
	{
		if (value.Length < 1)
		{
			errorText.text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.MinSize, 1);
			return E_ErrorCause.MinSize;
		}
		return base.CheckValidity(value);
	}
}
