using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand;

public class LocalizedLabelWithActions : LocalizedLabel
{
	[SerializeField]
	private List<RewiredAction> actions = new List<RewiredAction>();

	protected override void OnLocalize()
	{
		if (string.IsNullOrEmpty(key))
		{
			TMP_Text component = GetComponent<TMP_Text>();
			if (component != null)
			{
				key = component.text;
			}
		}
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < actions.Count; i++)
		{
			string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction(actions[i].RewiredLabel);
			if (localizedHotkeysForAction != null && localizedHotkeysForAction.Length != 0)
			{
				string text = localizedHotkeysForAction[0];
				if (actions[i].ShowAllInputs)
				{
					for (int j = 1; j < localizedHotkeysForAction.Length; j++)
					{
						text = text + "&" + localizedHotkeysForAction[j];
					}
				}
				else if (actions[i].SpecificIndex >= localizedHotkeysForAction.Length)
				{
					CLoggerManager.Log($"You tried to access the index {actions[i].SpecificIndex} butAction \"{actions[i]}\" only have {localizedHotkeysForAction.Length} hotkeys assiociated.", LogType.Warning);
				}
				else
				{
					text = localizedHotkeysForAction[actions[i].SpecificIndex];
				}
				list.Add(text);
			}
			else
			{
				CLoggerManager.Log("Action \"" + actions[i].RewiredLabel + "\" corresponds to no hotkey", LogType.Warning);
			}
		}
		if (list.Count == actions.Count)
		{
			string format = Localizer.Get(key);
			object[] args = list.ToArray();
			base.Value = string.Format(format, args);
		}
	}
}
