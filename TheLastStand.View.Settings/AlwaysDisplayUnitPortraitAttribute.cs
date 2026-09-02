using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class AlwaysDisplayUnitPortraitAttribute : MonoBehaviour
{
	[SerializeField]
	private BetterToggle alwaysDisplayUnitPortraitAttributeToggle;

	public void OnValueChanged()
	{
		SettingsController.SetAlwaysDisplayUnitPortraitAttribute(alwaysDisplayUnitPortraitAttributeToggle.isOn);
	}

	public void Refresh()
	{
		alwaysDisplayUnitPortraitAttributeToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute;
	}
}
