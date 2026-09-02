using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Skill.UI;

public class SkillTooltipDisplayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Tooltip("If null, will try to automatically find a sourceSkillDisplay on its gameObject")]
	[SerializeField]
	private SkillDisplay sourceSkillDisplay;

	[Tooltip("If null, will use the one defined in Manager.Unit.PlayableUnitManager.StatTooltipPanel")]
	[SerializeField]
	private SkillTooltip targetTooltip;

	[Tooltip("If true, a message will be displayed on SkillDisplay if the skill is not valid in current phase.")]
	[SerializeField]
	private bool displayInvalidPhase;

	[SerializeField]
	private FollowElement.FollowDatas followData = new FollowElement.FollowDatas();

	public bool HasFocus { get; private set; }

	public void DisplayTooltip(bool display)
	{
		if (sourceSkillDisplay.Skill == null)
		{
			TPDebug.LogError("SkillDisplay' skill can't be null! Aborting...", this);
			base.enabled = false;
		}
		else if (display)
		{
			targetTooltip.FollowElement.ChangeFollowDatas(followData);
			targetTooltip.SetContent(sourceSkillDisplay.Skill, sourceSkillDisplay.SkillOwner);
			targetTooltip.DisplayInvalidityPanel = displayInvalidPhase;
			targetTooltip.CompendiumFollowRight = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet;
			targetTooltip.Display();
		}
		else
		{
			targetTooltip.Hide();
		}
	}

	public void Init(SkillTooltip skillTooltip)
	{
		if ((object)targetTooltip == null)
		{
			targetTooltip = skillTooltip;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		HasFocus = true;
		DisplayTooltip(display: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (HasFocus)
		{
			HasFocus = false;
			DisplayTooltip(display: false);
		}
	}

	private void Awake()
	{
		if (targetTooltip == null && TPSingleton<SkillManager>.Exist())
		{
			targetTooltip = SkillManager.SkillInfoPanel;
		}
		if (sourceSkillDisplay == null)
		{
			sourceSkillDisplay = GetComponent<SkillDisplay>();
		}
		sourceSkillDisplay.SkillChangedEvent += OnSkillChanged;
	}

	private void OnDestroy()
	{
		if (sourceSkillDisplay != null)
		{
			sourceSkillDisplay.SkillChangedEvent -= OnSkillChanged;
		}
	}

	private void OnDisable()
	{
		if (HasFocus)
		{
			HasFocus = false;
			DisplayTooltip(display: false);
		}
	}

	private void OnSkillChanged(TheLastStand.Model.Skill.Skill skill)
	{
		if (HasFocus)
		{
			DisplayTooltip(display: true);
		}
	}
}
