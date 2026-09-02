using DG.Tweening;
using UnityEngine;

namespace TheLastStand.Model;

[CreateAssetMenu(fileName = "New Joystick HUD Navigation Config", menuName = "TLS/Joystick Config/HUD Navigation")]
public class JoystickHUDNavigationConfig : ScriptableObject
{
	[SerializeField]
	[Tooltip("When leaving a popup back to world view, should the user stay in HUD navigation mode (true) or be reset in world navigation mode (false).")]
	private bool stayInHUDOnPopupExit;

	[SerializeField]
	[Tooltip("Allow the user to use the same input for entering and leaving HUD.")]
	private bool canLeaveHUDUsingEnterInput = true;

	[SerializeField]
	[Tooltip("When starting navigating through UI elements that can display a tooltip, should the first hover display the tooltip by itself or wait for user input.")]
	private bool tooltipsToggledInit;

	[SerializeField]
	[Tooltip("Allows the user to select next/previous hero while selecting an equipment slot (will cancel slot selection).")]
	private bool canChangeHeroWhileEquipmentSlotSelection;

	[SerializeField]
	[Tooltip("Allows the user to open Level up panel while in inventory tab.")]
	private bool canLevelUpInInventoryTab;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Duration of the highlight tween between selectables.")]
	private float highlightTweenDuration = 0.2f;

	[SerializeField]
	[Tooltip("Ease of the highlight tween between selectables.")]
	private Ease highlightTweenEase = Ease.Linear;

	[SerializeField]
	[Tooltip("Always shows the tooltip on playables skill (hover only), even if disabled.")]
	private bool alwaysShowTooltipOnPlayableSkillHovering;

	[SerializeField]
	[Tooltip("Always shows the tooltip on the buildings skills.")]
	private bool alwaysShowTooltipOnBuildingSkill;

	[SerializeField]
	[Tooltip("When opening a building, the first skill is selected.")]
	private bool selectFirstBuildingCapacity;

	[SerializeField]
	[Tooltip("Deselect the building skill or action completely after execution.")]
	private bool deselectBuildingSkillAfterExecution;

	[SerializeField]
	[Tooltip("When changing selected unit while in perks panel, should select the first perk or keep the currently hovered one.")]
	private bool selectFirstPerkOnUnitChange;

	[SerializeField]
	[Tooltip("When selecting the construction building button, he is not placed directly.")]
	private bool hoverBeforePlacingConstruction;

	[SerializeField]
	[Tooltip("Always shows the tooltip on the construction button.")]
	private bool alwaysShowTooltipOnConstruction;

	[SerializeField]
	[Tooltip("Reselect the building button after cancelling the construction.")]
	private bool selectBuildingButtonAfterConstruction;

	[SerializeField]
	[Tooltip("Should select the last journal button when coming from bottom left panel.")]
	private bool selectLastButtonFromBottomPanel;

	[SerializeField]
	[Tooltip("Should select the recruit button when changing selected unit.")]
	private bool selectRecruitButtonOnUnitChanged;

	[SerializeField]
	[Tooltip("Should select the next omen (if it exists) when clicking on a selected omen to remove it. Else, select the previous one (if it exists).")]
	private bool selectNextOmenOnUnselect = true;

	public bool StayInHUDOnPopupExit => stayInHUDOnPopupExit;

	public bool CanLeaveHUDUsingEnterInput => canLeaveHUDUsingEnterInput;

	public bool TooltipsToggledInit => tooltipsToggledInit;

	public float HighlightTweenDuration => highlightTweenDuration;

	public Ease HighlightTweenEase => highlightTweenEase;

	public bool CanChangeHeroWhileEquipmentSlotSelection => canChangeHeroWhileEquipmentSlotSelection;

	public bool CanLevelUpInInventoryTab => canLevelUpInInventoryTab;

	public bool AlwaysShowTooltipOnPlayableSkillHovering => alwaysShowTooltipOnPlayableSkillHovering;

	public bool AlwaysShowTooltipOnBuildingSkill => alwaysShowTooltipOnBuildingSkill;

	public bool SelectFirstBuildingCapacity => selectFirstBuildingCapacity;

	public bool DeselectBuildingSkillAfterExecution => deselectBuildingSkillAfterExecution;

	public bool SelectFirstPerkOnUnitChange => selectFirstPerkOnUnitChange;

	public bool HoverBeforePlacingConstruction => hoverBeforePlacingConstruction;

	public bool AlwaysShowTooltipOnConstruction => alwaysShowTooltipOnConstruction;

	public bool SelectBuildingButtonAfterConstruction => selectBuildingButtonAfterConstruction;

	public bool SelectLastButtonFromBottomPanel => selectLastButtonFromBottomPanel;

	public bool SelectRecruitButtonOnUnitChanged => selectRecruitButtonOnUnitChanged;

	public bool SelectNextOmenOnUnselect => selectNextOmenOnUnselect;
}
