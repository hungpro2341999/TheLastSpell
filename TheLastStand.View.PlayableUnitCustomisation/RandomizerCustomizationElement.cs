using System.Collections.Generic;
using TPLib;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

[RequireComponent(typeof(Button))]
public class RandomizerCustomizationElement : MonoBehaviour
{
	[SerializeField]
	private List<RandomizableCustomizationElement> randomizableCustomizationElements = new List<RandomizableCustomizationElement>();

	[SerializeField]
	private bool ignoreLockState;

	[SerializeField]
	private bool useWeights;

	private Button button;

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(RandomizeAll);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveListener(RandomizeAll);
	}

	private void RandomizeAll()
	{
		for (int i = 0; i < randomizableCustomizationElements.Count; i++)
		{
			RandomizableCustomizationElement randomizableCustomizationElement = randomizableCustomizationElements[i];
			if (!randomizableCustomizationElement.IsLocked || ignoreLockState)
			{
				randomizableCustomizationElement.RandomizeValue(useWeights);
			}
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshPortraitParts = true;
	}
}
