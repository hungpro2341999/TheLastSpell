using TPLib;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class ShowSkillsHotkeysPanel : MonoBehaviour
{
	[SerializeField]
	private BetterToggle showSkillsHotkeysToggle;

	public void OnValueChanged()
	{
		SettingsController.SetShowSkillsHotkeys(showSkillsHotkeysToggle.isOn);
	}

	public void Refresh()
	{
		showSkillsHotkeysToggle.isOn = TPSingleton<SettingsManager>.Instance.Settings.ShowSkillsHotkeys;
	}
}
