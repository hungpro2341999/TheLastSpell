using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingCapacitiesGroupPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas panelCanvas;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	[FormerlySerializedAs("buildingSkillsGroupParent")]
	private RectTransform buildingCapacitiesGroupParent;

	[SerializeField]
	[FormerlySerializedAs("buildingSkills")]
	private List<BuildingCapacityPanel> buildingCapacities = new List<BuildingCapacityPanel>();

	[SerializeField]
	private HorizontalLayoutGroup buildingSkillsGroupLayout;

	public List<BuildingCapacityPanel> BuildingCapacities => buildingCapacities;

	public bool IsDisplayed()
	{
		return panelCanvas.enabled;
	}

	public void Display(bool show = true)
	{
		panelCanvas.enabled = show;
		layoutElement.ignoreLayout = !show;
		if (show)
		{
			Refresh();
		}
	}

	private void Refresh()
	{
		if (BuildingCapacities.Count == 0)
		{
			return;
		}
		float num = 0f;
		int i = 0;
		for (int count = BuildingCapacities.Count; i < count; i++)
		{
			if (BuildingCapacities[i].gameObject.activeInHierarchy && buildingSkillsGroupLayout != null)
			{
				num += BuildingCapacities[i].BuildingCapacityRect.sizeDelta.x + buildingSkillsGroupLayout.spacing;
			}
			BuildingCapacities[i].Refresh();
		}
		if (buildingSkillsGroupLayout != null)
		{
			num += (float)(buildingSkillsGroupLayout.padding.left + buildingSkillsGroupLayout.padding.right) - buildingSkillsGroupLayout.spacing;
			buildingCapacitiesGroupParent.sizeDelta = new Vector2(num, buildingCapacitiesGroupParent.sizeDelta.y);
			LayoutRebuilder.ForceRebuildLayoutImmediate(buildingCapacitiesGroupParent);
		}
	}
}
