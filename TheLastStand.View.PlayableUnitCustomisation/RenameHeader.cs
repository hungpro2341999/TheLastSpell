using System.Collections.Generic;
using PortraitAPI;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class RenameHeader : RandomizableCustomizationElement
{
	[SerializeField]
	private TextMeshProUGUI heroNameText;

	[SerializeField]
	private BetterButton renameButton;

	[SerializeField]
	private RenamePopup renamePopup;

	[HideInInspector]
	public bool IsEditingName;

	private PlayableUnit playableUnit;

	public string CurrentName { get; private set; } = string.Empty;

	public RenamePopup RenamePopup => renamePopup;

	public override void RandomizeValue(bool useWeight)
	{
		List<string> list = new List<string>();
		switch (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender)
		{
		case Commons.E_Gender.Man:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Male"));
			break;
		case Commons.E_Gender.Woman:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Female"));
			break;
		case Commons.E_Gender.Any:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Male"));
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Female"));
			break;
		}
		CurrentName = RandomManager.GetRandomElement(TPSingleton<PlayableUnitCustomisationPanel>.Instance, list);
		heroNameText.text = CurrentName;
	}

	public void Refresh(PlayableUnit playableUnit)
	{
		this.playableUnit = playableUnit;
		CurrentName = playableUnit.PlayableUnitName;
		heroNameText.text = CurrentName;
	}

	public void Refresh(string name)
	{
		CurrentName = name;
		heroNameText.text = CurrentName;
	}

	private void OnRenameButtonClicked()
	{
		renamePopup.PlayableUnitName = ((CurrentName != string.Empty) ? CurrentName : playableUnit.PlayableUnitName);
		IsEditingName = true;
		renamePopup.Open();
	}

	private void Start()
	{
		renameButton.onClick.AddListener(OnRenameButtonClicked);
	}

	private void OnDestroy()
	{
		renameButton.onClick.RemoveAllListeners();
	}
}
