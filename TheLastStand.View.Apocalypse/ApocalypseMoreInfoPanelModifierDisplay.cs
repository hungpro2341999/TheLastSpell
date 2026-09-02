using System.Text;
using TMPro;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.View.HUD;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseMoreInfoPanelModifierDisplay : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Image separatorImage;

	[SerializeField]
	private TextMeshProUGUI modifierDescription;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private StringBuilder modifierDescriptionStr = new StringBuilder();

	private ApocalypseModifierDefinition modifierDefinition;

	private ApocalypseModifierStepDefinition modifierStepDefinition;

	private bool isInitialized;

	private bool isLastEntry;

	private bool showBackground;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Init(ApocalypseModifierStepDefinition apocalypseModifierStepDefinition, bool isLastEntryDisplay, bool mustShowBackground)
	{
		isInitialized = false;
		modifierStepDefinition = apocalypseModifierStepDefinition;
		if (!ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(modifierStepDefinition, out var value))
		{
			Debug.LogError("Couldn't find the definition of the modifier from the step: " + modifierStepDefinition.Id + " in ApocalypseMoreInfoPanelModifierDisplay");
			return;
		}
		modifierDefinition = value;
		isLastEntry = isLastEntryDisplay;
		showBackground = mustShowBackground;
		isInitialized = true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (TPSingleton<ApocalypseMoreInfoPanel>.Instance != null)
		{
			TPSingleton<ApocalypseMoreInfoPanel>.Instance.AdjustScrollView(base.transform as RectTransform);
		}
	}

	public void Refresh()
	{
		if (isInitialized)
		{
			separatorImage.gameObject.SetActive(!isLastEntry);
			backgroundImage.enabled = showBackground;
			int modifierStepIndexFromDefinition = modifierDefinition.GetModifierStepIndexFromDefinition(modifierStepDefinition);
			modifierDescriptionStr.Clear();
			modifierDescriptionStr.Append("<style=Bad>• " + modifierDefinition.GetLocalizedTitle() + " " + ApocalypseEffectsTooltip.GetModifierStepDotsStr(modifierDefinition, modifierStepIndexFromDefinition) + " : </style>");
			modifierDescriptionStr.Append(modifierDefinition.GetLocalizedDescription(modifierStepIndexFromDefinition) ?? "");
			modifierDescription.text = modifierDescriptionStr.ToString();
		}
	}
}
