using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedSettings : SerializedContainer
{
	public bool AlwaysDisplayMaxStatValue;

	public bool EdgePan = true;

	public bool EdgePanOverUI = true;

	public bool FocusCamOnSelections = true;

	public SerializedScreenSettings ScreenSettings;

	public SerializedSoundSettings SoundSettings;

	public string Language;

	public bool[] EndTurnWarnings;

	public bool? ShowSkillsHotkeys;

	public float? SpeedScale;

	public bool SmartCast;

	public SettingsManager.E_SpeedMode SpeedMode;

	public SettingsManager.E_InputDeviceType InputDeviceType;

	public int CurrentProfile;

	public bool HideCompendium;

	public bool AlwaysDisplayUnitPortraitAttribute;

	public bool AllowDataCollection = true;

	public static void HandleUnknownXMLElement(object sender, XmlElementEventArgs e)
	{
		CLoggerManager.Log("Found unknown " + e.Element.Name + " element, trying to handle it with " + typeof(SerializedSettings).Name + "'s Unkown XML Element Handler...", LogType.Warning);
		if (!(e.ObjectBeingDeserialized is SerializedSettings serializedSettings))
		{
			CLoggerManager.Log(typeof(SerializedSettings).Name + "'s Unkown XML Element Handler has been used for an unkown type, aborting. Something wrong happened in the registration, this should not happen.", LogType.Warning);
			return;
		}
		XElement xElement = XElement.Parse(e.Element.OuterXml);
		if (!(xElement.Name.LocalName == "TurnEndWarningsEnabled"))
		{
			return;
		}
		try
		{
			bool flag = bool.Parse(xElement.Value);
			if (serializedSettings.EndTurnWarnings == null)
			{
				serializedSettings.EndTurnWarnings = new bool[Enum.GetNames(typeof(SettingsManager.E_EndTurnWarning)).Length];
				for (int i = 0; i < serializedSettings.EndTurnWarnings.Length; i++)
				{
					serializedSettings.EndTurnWarnings[i] = flag;
				}
			}
		}
		catch (Exception ex)
		{
			CLoggerManager.Log($"An error occured when manually handling {xElement.Name} :\n{ex.Message}", LogType.Error);
		}
	}

	public override byte GetSaveVersion()
	{
		return SaveManager.SettingsSaveVersion;
	}
}
