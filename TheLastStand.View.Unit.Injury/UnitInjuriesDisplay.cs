using System.Collections.Generic;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuriesDisplay : MonoBehaviour
{
	[SerializeField]
	private RectTransform injuriesParent;

	[SerializeField]
	private UnitInjuryDisplay injuryPrefab;

	[SerializeField]
	private bool shouldSetInjuriesBoxUnRaycastable;

	[SerializeField]
	private Selectable[] selectOnUp;

	[SerializeField]
	private Selectable[] selectOnDown;

	private TheLastStand.Model.Unit.Unit currentUnit;

	private List<UnitInjuryDisplay> injuries = new List<UnitInjuryDisplay>();

	public void Refresh(TheLastStand.Model.Unit.Unit newUnit = null)
	{
		if (currentUnit != newUnit)
		{
			currentUnit = newUnit;
			Init();
		}
		if (currentUnit != null)
		{
			int i = 0;
			for (int count = currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count; i < count; i++)
			{
				InjuryDefinition injuryDefinition = currentUnit.UnitTemplateDefinition.InjuryDefinitions[i];
				float xPos = Mathf.Ceil(currentUnit.UnitController.ComputeInjuryThresholdRatio(injuryDefinition) * injuriesParent.sizeDelta.x);
				injuries[i].Refresh(currentUnit.UnitStatsController.UnitStats.InjuryStage > i, xPos, injuryDefinition, i, currentUnit);
			}
		}
	}

	private void Init()
	{
		TransformExtensions.DestroyChildren(injuriesParent);
		injuries.Clear();
		if (currentUnit == null)
		{
			return;
		}
		for (int i = 0; i < currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count; i++)
		{
			UnitInjuryDisplay unitInjuryDisplay = Object.Instantiate(injuryPrefab, injuriesParent);
			injuries.Add(unitInjuryDisplay);
			if (shouldSetInjuriesBoxUnRaycastable)
			{
				unitInjuryDisplay.ToggleRaycaster(state: false);
			}
			else
			{
				unitInjuryDisplay.JoystickSelectable.SetSelectOnUp((selectOnUp.Length != 0) ? selectOnUp[0] : null);
				unitInjuryDisplay.JoystickSelectable.SetSelectOnDown((selectOnDown.Length != 0) ? selectOnDown[0] : null);
			}
			if (i != currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count - 1)
			{
				continue;
			}
			for (int j = 0; j < selectOnUp.Length; j++)
			{
				if (selectOnUp[j] != null)
				{
					selectOnUp[j].SetSelectOnDown(unitInjuryDisplay.JoystickSelectable);
				}
			}
			for (int k = 0; k < selectOnDown.Length; k++)
			{
				if (selectOnDown[k] != null)
				{
					selectOnDown[k].SetSelectOnUp(unitInjuryDisplay.JoystickSelectable);
				}
			}
		}
		for (int l = 0; l < injuries.Count; l++)
		{
			injuries[l].JoystickSelectable.SetMode(Navigation.Mode.Explicit);
			if (l > 0)
			{
				injuries[l].JoystickSelectable.SetSelectOnRight(injuries[l - 1].JoystickSelectable);
			}
			if (l < injuries.Count - 1)
			{
				injuries[l].JoystickSelectable.SetSelectOnLeft(injuries[l + 1].JoystickSelectable);
			}
		}
	}
}
