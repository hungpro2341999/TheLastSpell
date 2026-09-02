using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.HUD.BottomScreenPanel.UnitManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class ModifiersLayoutView : MonoBehaviour
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private LayoutGroup containerLayout;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private GuardianIconDisplay guardianIconDisplay;

	[SerializeField]
	private RectTransform enemiesAffixesParent;

	[SerializeField]
	private LayoutGroup enemiesAffixesLayoutGroup;

	[SerializeField]
	private EliteAffixIconDisplay enemyAffixIconDisplayPrefab;

	[SerializeField]
	private EnemyAffixSeparator enemyAffixSeparatorPrefab;

	[SerializeField]
	private InjuryIconDisplay injuryIconDisplayPrefab;

	[SerializeField]
	private RectTransform statusesParent;

	[SerializeField]
	private RectTransform statusesBackground;

	[SerializeField]
	private LayoutGroup statusesLayoutGroup;

	[SerializeField]
	private Status.E_StatusType[] statusesDisplayOrder = new Status.E_StatusType[9]
	{
		Status.E_StatusType.Debuff,
		Status.E_StatusType.Stun,
		Status.E_StatusType.Poison,
		Status.E_StatusType.Contagion,
		Status.E_StatusType.Invulnerable,
		Status.E_StatusType.Buff,
		Status.E_StatusType.Charged,
		Status.E_StatusType.AllNegativeImmunity,
		Status.E_StatusType.All
	};

	[SerializeField]
	private StatusIconDisplay statusIconDisplayPrefab;

	[SerializeField]
	private RectTransform statusesHover;

	[SerializeField]
	private int statusesBackgroundWidthBase = 46;

	[SerializeField]
	private int statusesBackgroundWidthIncrement = 36;

	[SerializeField]
	private MomentumIconDisplay momentumIconDisplay;

	[SerializeField]
	private RectTransform perksParent;

	[SerializeField]
	private GameObject perksSpacing;

	[SerializeField]
	private LayoutGroup[] perksLayoutGroups;

	[SerializeField]
	private ModifiersLayoutViewAllPerksView allPerksView;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private RectTransform joystickTargetFeedback;

	[SerializeField]
	private float joystickTargetFeedbackMinWidth = 116f;

	[SerializeField]
	private float joystickTargetFeedbackMinHeight = 110f;

	[SerializeField]
	private float joystickTargetFeedbackTwoLinesLayoutHeight = 190f;

	private List<EliteAffixIconDisplay> enemyAffixIconDisplays = new List<EliteAffixIconDisplay>();

	private List<EnemyAffixSeparator> enemyAffixSeparators = new List<EnemyAffixSeparator>();

	private int displayedStatusesCount;

	private TheLastStand.Model.Unit.Unit unit;

	private InjuryIconDisplay injuryIconDisplay;

	private readonly ConcurrentQueue<StatusIconDisplay> statusIconDisplayPool = new ConcurrentQueue<StatusIconDisplay>();

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public bool IsDisplayed()
	{
		if ((!(guardianIconDisplay != null) || !guardianIconDisplay.gameObject.activeSelf) && (enemyAffixIconDisplays == null || !enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => icon.gameObject.activeSelf)) && (!(injuryIconDisplay != null) || !injuryIconDisplay.gameObject.activeSelf) && !statusIconDisplayPool.Any((StatusIconDisplay o) => o.gameObject.activeSelf))
		{
			return allPerksView.IsDisplayed();
		}
		return true;
	}

	public void Refresh()
	{
		unit = TileObjectSelectionManager.SelectedUnit;
		RefreshGuardian();
		RefreshEliteAffixes();
		RefreshInjuries();
		RefreshStatuses();
		RefreshMomentum();
		RefreshStatusesBackground();
		RefreshPerks();
		ToggleLayouts(state: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(container);
		ToggleLayouts(state: false);
		RefreshJoystickNavigation();
		simpleFontLocalizedParent?.RefreshChildren();
	}

	public void RefreshPerks()
	{
		if (unit is PlayableUnit playableUnit)
		{
			List<Perk> list = new List<Perk>();
			List<Perk> list2 = new List<Perk>();
			List<Perk> list3 = new List<Perk>();
			foreach (Perk value in playableUnit.Perks.Values)
			{
				if (value.Unlocked && value.DisplayInHUD(out var _))
				{
					if (value.IsFromRace)
					{
						list.Add(value);
					}
					else if (value.IsNative)
					{
						list3.Add(value);
					}
					else if (value.OnlyUnlockedByItem)
					{
						list2.Add(value);
					}
					else
					{
						list.Add(value);
					}
				}
			}
			allPerksView.PerksTreeView.Refresh(list);
			allPerksView.PerksItemView.Refresh(list2);
			allPerksView.PerksOmenView.Refresh(list3);
			perksParent.gameObject.SetActive(allPerksView.IsDisplayed());
		}
		else
		{
			perksParent.gameObject.SetActive(value: false);
			allPerksView.HideAllPerkDisplays();
		}
		perksSpacing.SetActive((enemyAffixIconDisplays != null && enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => icon.gameObject.activeSelf)) || displayedStatusesCount > 0);
	}

	private void Awake()
	{
		momentumIconDisplay.Hovered += OnStatusIconDisplayHovered;
		momentumIconDisplay.Unhovered += OnStatusIconDisplayUnhovered;
		TileObjectSelectionManager.OnUnitSelectionChange += OnNewUnitSelected;
	}

	private void OnDestroy()
	{
		if (momentumIconDisplay != null)
		{
			momentumIconDisplay.Hovered -= OnStatusIconDisplayHovered;
			momentumIconDisplay.Unhovered -= OnStatusIconDisplayUnhovered;
		}
		TileObjectSelectionManager.OnUnitSelectionChange -= OnNewUnitSelected;
	}

	private void OnStatusIconDisplayHovered(UnitModifiersIconDisplay statusIconDisplay)
	{
		statusesHover.gameObject.SetActive(value: true);
		statusesHover.position = (statusIconDisplay.transform as RectTransform).position;
	}

	private void OnStatusIconDisplayUnhovered(UnitModifiersIconDisplay statusIconDisplay)
	{
		statusesHover.gameObject.SetActive(value: false);
	}

	private void RefreshEliteAffixes()
	{
		if (!(unit is EnemyUnit enemyUnit))
		{
			foreach (EliteAffixIconDisplay enemyAffixIconDisplay in enemyAffixIconDisplays)
			{
				enemyAffixIconDisplay.Hide();
			}
			return;
		}
		while (enemyAffixIconDisplays.Count < enemyUnit.Affixes.Count)
		{
			if (enemyAffixIconDisplays.Count > 0)
			{
				enemyAffixSeparators.Add(Object.Instantiate(enemyAffixSeparatorPrefab, enemiesAffixesParent));
			}
			enemyAffixIconDisplays.Add(Object.Instantiate(enemyAffixIconDisplayPrefab, enemiesAffixesParent));
		}
		int i;
		for (i = 0; i < enemyUnit.Affixes.Count; i++)
		{
			if (i > 0)
			{
				enemyAffixSeparators[i - 1].Toggle(toggle: true);
			}
			enemyAffixIconDisplays[i].Display(enemyUnit.Affixes[i], (i == 0 && enemyUnit is EliteEnemyUnit) ? EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite : enemyUnit.Affixes[i].EnemyAffixDefinition.BoxType);
		}
		for (; i < enemyAffixIconDisplays.Count; i++)
		{
			if (i > 0)
			{
				enemyAffixSeparators[i - 1].Toggle(toggle: false);
			}
			enemyAffixIconDisplays[i].Hide();
		}
	}

	private void RefreshGuardian()
	{
		if (unit is EnemyUnit { IsGuardian: not false } enemyUnit)
		{
			guardianIconDisplay.Display(enemyUnit.LinkedBuilding == null);
		}
		else
		{
			guardianIconDisplay.Hide();
		}
	}

	private void RefreshMomentum()
	{
		if (!(unit is PlayableUnit { MomentumTilesActive: not 0 } playableUnit) || playableUnit.MomentumSkills.Count == 0)
		{
			momentumIconDisplay.Hide();
			return;
		}
		momentumIconDisplay.Display();
		momentumIconDisplay.Refresh(playableUnit);
	}

	private void RefreshStatuses()
	{
		if (unit == null)
		{
			return;
		}
		List<StatusIconDisplay> list = new List<StatusIconDisplay>();
		Status.E_StatusType e_StatusType = unit.StatusOwned;
		displayedStatusesCount = 0;
		int statusInOrderIndex = 0;
		while (statusInOrderIndex < statusesDisplayOrder.Length)
		{
			e_StatusType &= ~statusesDisplayOrder[statusInOrderIndex];
			Status[] array = unit.StatusList.FindAll((Status o) => statusesDisplayOrder[statusInOrderIndex].HasFlag(o.StatusType) && !o.IsFromInjury).ToArray();
			if (array.Length != 0)
			{
				if (!statusIconDisplayPool.TryDequeue(out var result))
				{
					result = Object.Instantiate(statusIconDisplayPrefab, statusesParent);
					simpleFontLocalizedParent.AddChilds(result.LocalizedFonts);
					result.Hovered += OnStatusIconDisplayHovered;
					result.Unhovered += OnStatusIconDisplayUnhovered;
				}
				result.Display();
				result.Refresh(statusesDisplayOrder[statusInOrderIndex], array);
				list.Add(result);
				displayedStatusesCount++;
			}
			if (e_StatusType == Status.E_StatusType.None)
			{
				break;
			}
			int num = statusInOrderIndex + 1;
			statusInOrderIndex = num;
		}
		StatusIconDisplay result2;
		while (statusIconDisplayPool.TryDequeue(out result2))
		{
			list.Add(result2);
			result2.Hide();
		}
		foreach (StatusIconDisplay item in list)
		{
			statusIconDisplayPool.Enqueue(item);
		}
		momentumIconDisplay.transform.SetAsLastSibling();
	}

	private void RefreshStatusesBackground()
	{
		int num = displayedStatusesCount;
		if (injuryIconDisplay != null && injuryIconDisplay.gameObject.activeSelf)
		{
			num++;
		}
		if (momentumIconDisplay != null && momentumIconDisplay.gameObject.activeSelf)
		{
			num++;
		}
		if (num > 0)
		{
			statusesParent.gameObject.SetActive(value: true);
			statusesBackground.sizeDelta = new Vector2(statusesBackgroundWidthBase + statusesBackgroundWidthIncrement * (num - 1), statusesBackground.sizeDelta.y);
		}
		else
		{
			statusesParent.gameObject.SetActive(value: false);
		}
		statusesHover.SetAsLastSibling();
	}

	private void RefreshInjuries()
	{
		if (unit == null)
		{
			injuryIconDisplay?.Hide();
		}
		else if (unit.UnitStatsController.UnitStats.InjuryStage != 0)
		{
			if (injuryIconDisplay == null)
			{
				injuryIconDisplay = Object.Instantiate(injuryIconDisplayPrefab, statusesParent);
				injuryIconDisplay.Hovered += OnStatusIconDisplayHovered;
				injuryIconDisplay.Unhovered += OnStatusIconDisplayUnhovered;
			}
			injuryIconDisplay.transform.SetAsFirstSibling();
			statusesBackground.SetAsFirstSibling();
			injuryIconDisplay.Refresh(unit);
			injuryIconDisplay.Display();
		}
		else
		{
			injuryIconDisplay?.Hide();
		}
	}

	private void RefreshJoystickNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		if (guardianIconDisplay != null && guardianIconDisplay.gameObject.activeSelf)
		{
			list.Add(guardianIconDisplay.JoystickSelectable);
		}
		for (int i = 0; i < enemyAffixIconDisplays.Count && enemyAffixIconDisplays[i].gameObject.activeSelf; i++)
		{
			list.Add(enemyAffixIconDisplays[i].JoystickSelectable);
		}
		if (injuryIconDisplay != null && injuryIconDisplay.gameObject.activeSelf)
		{
			list.Add(injuryIconDisplay.JoystickSelectable);
		}
		foreach (StatusIconDisplay item in statusIconDisplayPool)
		{
			if (item.gameObject.activeSelf)
			{
				list.Add(item.JoystickSelectable);
			}
		}
		if (momentumIconDisplay.gameObject.activeSelf)
		{
			list.Add(momentumIconDisplay.JoystickSelectable);
		}
		bool flag = list.Count > 0;
		for (int j = 0; j < list.Count; j++)
		{
			Selectable selectable = list[j];
			selectable.SetMode(Navigation.Mode.Explicit);
			selectable.ClearNavigation();
			if (j > 0)
			{
				selectable.SetSelectOnLeft(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				selectable.SetSelectOnRight(list[j + 1]);
			}
		}
		bool flag2 = allPerksView.IsDisplayed();
		allPerksView.RefreshJoystickNavigation();
		if (flag && flag2)
		{
			Selectable selectable2 = allPerksView.GetBottomLeftPerkIconDisplay()?.JoystickSelectable;
			if (selectable2 != null)
			{
				list[^1].SetSelectOnRight(selectable2);
				selectable2.SetSelectOnLeft(list[^1]);
			}
		}
		JoystickTarget.NavigationEnabled = IsDisplayed();
		JoystickTarget.ClearSelectables();
		List<Selectable> list2 = list;
		list2.AddRange(allPerksView.GetAllSelectables());
		JoystickTarget.AddSelectables(list2);
		if (flag || list2.Count > 0)
		{
			float y = joystickTargetFeedbackMinHeight;
			if (allPerksView.HasTwoPerksLines())
			{
				y = joystickTargetFeedbackTwoLinesLayoutHeight;
			}
			float num = 0f;
			float x;
			if (flag)
			{
				x = list[0].transform.position.x;
				num = list[^1].transform.position.x;
			}
			else
			{
				x = allPerksView.GetBottomLeftPerkIconDisplay().transform.position.x;
			}
			PerkIconDisplay farRightPerkIconDisplay = allPerksView.GetFarRightPerkIconDisplay();
			if (farRightPerkIconDisplay != null && farRightPerkIconDisplay.transform.position.x > num)
			{
				num = farRightPerkIconDisplay.transform.position.x;
			}
			joystickTargetFeedback.sizeDelta = new Vector2(joystickTargetFeedbackMinWidth + num - x, y);
		}
	}

	private void ToggleLayouts(bool state)
	{
		containerLayout.enabled = state;
		enemiesAffixesLayoutGroup.enabled = state;
		statusesLayoutGroup.enabled = state;
		LayoutGroup[] array = perksLayoutGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = state;
		}
	}

	private void OnNewUnitSelected()
	{
		if (!InputManager.IsLastControllerJoystick || !joystickTargetFeedback.gameObject.activeSelf)
		{
			return;
		}
		if (joystickTarget.IsSelectable())
		{
			Selectable selectable = joystickTarget.GetSelectionInfo().Selectable;
			if (selectable != null)
			{
				EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			}
			return;
		}
		if (TileObjectSelectionManager.HasPlayableUnitSelected && TPSingleton<PlayableUnitManagementView>.Instance.Displayed && TPSingleton<PlayableUnitManagementView>.Instance.JoystickTarget.IsSelectable())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<PlayableUnitManagementView>.Instance.JoystickTarget.GetSelectionInfo());
		}
		if (TileObjectSelectionManager.HasEnemyUnitSelected && TPSingleton<EnemyUnitManagementView>.Instance.Displayed && TPSingleton<EnemyUnitManagementView>.Instance.JoystickTarget.IsSelectable())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<EnemyUnitManagementView>.Instance.JoystickTarget.GetSelectionInfo());
		}
	}
}
